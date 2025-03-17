using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Configuration;
using Xunit;

public class TokenService_test
{
    private readonly TokenService _tokenService;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
    private readonly JwtSettings _jwtSettings;

    public TokenService_test()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsASecretKeyForTestingOnly123!", // Should be at least 256 bits
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60
        };

        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockJwtSettings.Setup(o => o.Value).Returns(_jwtSettings);

        _tokenService = new TokenService(_mockJwtSettings.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        string username = "testUser";
        string role = "Admin";

        // Act
        string token = _tokenService.GenerateToken(username, role);

        // Assert
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token));

        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtSettings.Audience, jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == username);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [Fact]
    public void GenerateToken_ShouldContainValidClaims()
    {
        // Arrange
        string username = "testUser";
        string role = "User";

        // Act
        string token = _tokenService.GenerateToken(username, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        Assert.NotNull(usernameClaim);
        Assert.Equal(username, usernameClaim.Value);

        Assert.NotNull(roleClaim);
        Assert.Equal(role, roleClaim.Value);

        Assert.NotNull(jtiClaim);
    }

    [Fact]
    public void GenerateToken_ShouldExpireAfterConfiguredTime()
    {
        // Arrange
        string username = "testUser";
        string role = "User";

        // Act
        string token = _tokenService.GenerateToken(username, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.NotNull(jwtToken.ValidTo);
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow); // Should be in the future
        Assert.True(jwtToken.ValidTo <= DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes));
    }

    [Fact]
    public void Constructor_ShouldThrowExceptionIfJwtSettingsNotConfigured()
    {
        // Arrange
        var mockJwtOptions = new Mock<IOptions<JwtSettings>>();
        mockJwtOptions.Setup(o => o.Value).Returns((JwtSettings)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TokenService(mockJwtOptions.Object));
    }
}
