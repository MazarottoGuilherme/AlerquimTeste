namespace Identity.Domain;

public sealed class Email
{
    public string Value { get; private set; }

    private Email() { } 
    
    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email é obrigatorio");

        if (!value.Contains("@"))
            throw new DomainException("Email invalido");

        return new Email(value.ToLowerInvariant());
    }
}
