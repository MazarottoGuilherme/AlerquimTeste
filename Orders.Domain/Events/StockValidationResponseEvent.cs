namespace Orders.Domain.Events;

public class StockValidationResponseEvent
{
    public Guid RequestId { get; }
    public bool IsValid { get; }
    public string Message { get; }

    public StockValidationResponseEvent(Guid requestId, bool isValid, string message)
    {
        RequestId = requestId;
        IsValid = isValid;
        Message = message;
    }
}
