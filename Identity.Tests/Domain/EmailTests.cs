using Identity.Domain;

namespace Identity.Tests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("exemplodeteste@email.com")]
    [InlineData("usuario.nome@email.com.br")]
    [InlineData("usuario+tag@email.com")]
    [InlineData("MAIUSCULO@EMAIL.COM")]
    public void Create_ValidEmail_ShouldCreateEmail(string emailValue)
    {
        var email = Email.Create(emailValue);
        
        Assert.NotNull(email);
        Assert.Equal(emailValue.ToLowerInvariant(), email.Value);
    }
    
    [Fact]
    public void Create_EmailWithUppercase_ShouldConvertToLowercase()
    {
        var emailValue = "EXEMPLODETESTE@EMAIL.COM";
        
        var email = Email.Create(emailValue);
        
        Assert.Equal("exemplodeteste@email.com", email.Value);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespaceEmail_ShouldThrowDomainException(string invalidEmail)
    {
        var ex = Assert.Throws<DomainException>(() => Email.Create(invalidEmail));
        Assert.Equal("Email é obrigatorio", ex.Message);
    }
    

}

