using Microsoft.Extensions.Options;
using Moq;
using Services.Utils;
using Shared.Configuration;
using Xunit;

public class PasswordHasher_test
{
    private readonly PasswordHasher _passwordHasher;
    private readonly Mock<IOptions<SecuritySettings>> _mockOptions;

    public PasswordHasher_test()
    {
        _mockOptions = new Mock<IOptions<SecuritySettings>>();
        _mockOptions.Setup(o => o.Value).Returns(new SecuritySettings { PasswordPepper = "testPepper" });

        _passwordHasher = new PasswordHasher(_mockOptions.Object);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedValue()
    {
        // Arrange
        string password = "testPassword";

        // Act
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.Contains(".", hashedPassword); // Ensures salt and hash are separated
        var parts = hashedPassword.Split('.');
        Assert.Equal(2, parts.Length); // Ensures correct format
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        string password = "testPassword";
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        bool isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        string password = "testPassword";
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        bool isValid = _passwordHasher.VerifyPassword("wrongPassword", hashedPassword);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForInvalidStoredValueFormat()
    {
        // Act
        bool isValid = _passwordHasher.VerifyPassword("testPassword", "invalidFormat");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Constructor_ShouldThrowExceptionIfPepperNotConfigured()
    {
        // Arrange
        var mockOptionsWithoutPepper = new Mock<IOptions<SecuritySettings>>();
        mockOptionsWithoutPepper.Setup(o => o.Value).Returns(new SecuritySettings { PasswordPepper = null });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PasswordHasher(mockOptionsWithoutPepper.Object));
    }
}
