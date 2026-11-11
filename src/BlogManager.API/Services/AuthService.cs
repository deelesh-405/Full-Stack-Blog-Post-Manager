using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BlogManager.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidateCredentials(string username, string password)
    {
        var configuredUsername = _configuration["Auth:Username"] ?? "admin";
        var configuredPassword = _configuration["Auth:Password"] ?? "Password123!";

        return string.Equals(username, configuredUsername, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, configuredPassword, StringComparison.Ordinal);
    }

    public string CreateToken(string username)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "ThisIsA32CharacterLongSecretKey123";
        var issuer = _configuration["Jwt:Issuer"] ?? "Fieldnotes";
        var audience = _configuration["Jwt:Audience"] ?? "FieldnotesClient";
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
