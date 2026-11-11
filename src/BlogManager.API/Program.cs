using System.Text;
using BlogManager.API.Data;
using BlogManager.API.Enums;
using BlogManager.API.Middleware;
using BlogManager.API.Models;
using BlogManager.API.Repositories;
using BlogManager.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsA32CharacterLongSecretKey123";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Fieldnotes";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FieldnotesClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blog Manager API",
        Version = "v1",
        Description = "REST API for managing blog posts."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

    if (builder.Environment.IsEnvironment("Testing") || useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("BlogManagerTests");
        return;
    }

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseSqlServer(connectionString);
        return;
    }

    options.UseInMemoryDatabase("BlogManagerInMemory");
});

builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        if (dbContext.Database.GetMigrations().Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    await SeedSamplePostsAsync(dbContext);
}

app.Run();

static async Task SeedSamplePostsAsync(ApplicationDbContext dbContext)
{
    if (await dbContext.Posts.AnyAsync())
    {
        return;
    }

    var now = DateTime.UtcNow;
    var posts = new[]
    {
        new BlogPost
        {
            Title = "Designing calm product rituals",
            Slug = "designing-calm-product-rituals",
            Excerpt = "Small routines help teams build better products without burning out.",
            Content = "A calm operating rhythm gives teams time to think, deliver, and adjust. We start by defining a weekly review, a clear owner for each decision, and a simple way to capture what we learn. The goal is not more process; it is more clarity. When rituals are lightweight and consistent, product work becomes easier to trust and easier to improve.",
            FeaturedImageUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=80",
            Status = PostStatus.Published,
            CreatedAt = now.AddDays(-12),
            UpdatedAt = now.AddDays(-2)
        },
        new BlogPost
        {
            Title = "Three ways to keep writing momentum",
            Slug = "three-ways-to-keep-writing-momentum",
            Excerpt = "Momentum is built by shipping small but useful work at a steady pace.",
            Content = "Writing is rarely powered by motivation alone. It is powered by repetition, constraints, and a manageable goal. Ship a short update, keep a visible list of ideas, and choose a fixed block of time to draft. Once you make the habit easier to begin, the work starts feeling lighter and more creative.",
            FeaturedImageUrl = "https://images.unsplash.com/photo-1455390582262-044cdead277a?auto=format&fit=crop&w=1200&q=80",
            Status = PostStatus.Published,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-5)
        },
        new BlogPost
        {
            Title = "A simple editorial backlog",
            Slug = "a-simple-editorial-backlog",
            Excerpt = "A backlog should make writing easier, not heavier.",
            Content = "Every editorial team benefits from a lightweight backlog. Capture topics, questions, and customer problems, then sort them by urgency and confidence. From there, the team can decide what to polish, what to publish, and what to let incubate. A simple list keeps the channel focused and prevents great ideas from being forgotten.",
            FeaturedImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80",
            Status = PostStatus.Draft,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = now.AddDays(-1)
        },
        new BlogPost
        {
            Title = "Building trust with public updates",
            Slug = "building-trust-with-public-updates",
            Excerpt = "Transparent progress updates help stakeholders stay aligned.",
            Content = "People do not need every detail, but they do need a steady signal that work is moving. Public updates are a proven way to reduce uncertainty and show that progress is real. Keep them short, honest, and specific about what changed and what comes next. That clarity creates trust over time.",
            FeaturedImageUrl = "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?auto=format&fit=crop&w=1200&q=80",
            Status = PostStatus.Published,
            CreatedAt = now.AddDays(-31),
            UpdatedAt = now.AddDays(-6)
        }
    };

    await dbContext.Posts.AddRangeAsync(posts);
    await dbContext.SaveChangesAsync();
}

public partial class Program { }
