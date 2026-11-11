namespace BlogManager.API.Services;

public interface IAuthService
{
    bool ValidateCredentials(string username, string password);
    string CreateToken(string username);
}
