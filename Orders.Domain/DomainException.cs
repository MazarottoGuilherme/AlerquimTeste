namespace Orders.Domain;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
