using System.Reflection;
using Xunit;

namespace GestionClientsAvalonia.Tests;

public class EmailValidatorTests
{
    [Fact]
    public void IsValid_WithValidEmail_ReturnsTrue()
    {

        string email = "marty@example.com";

        bool result = EmailValidator.IsValid(email);

        Assert.True(result);
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("marty")]
    [InlineData("marty@example")]
    [InlineData("Marty <marty@example.com>")]
    [InlineData("marty@example.com.")]
    public void IsValid_WithValidEmail_ReturnsFalse(string email)
    {
        bool result = EmailValidator.IsValid(email);

        Assert.False(result);
    }

    [Theory]
    [InlineData("marty@example.com")]
    [InlineData("   marty@example.com")]
    [InlineData("MARTY@EXAMPLE.COM")]

    public void IsValid_WithValidEmailVariants_ReturnsTrue(string email)
    {
        bool result = EmailValidator.IsValid(email);

        Assert.True(result);
    }

}
