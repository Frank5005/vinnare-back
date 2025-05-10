public interface ITokenService
{
    string GenerateToken(string username, string role);
    string? GetEmailFromToken(string token);
}
