using Xunit;

namespace Api.Utils
{
    public class EmailValidator_test
    {
        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("string", false)]
        [InlineData("invalid-email", false)]
        [InlineData("", false)]
        [InlineData("user@domain", false)]
        [InlineData("user@sub.domain.com", true)]
        public void IsValidEmail_ShouldValidateCorrectly(string email, bool expected)
        {
            // Act
            bool result = EmailValidator.IsValidEmail(email);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
