using System.Collections.Concurrent;

namespace Orders.Application.Services;

public class StockValidationResponseManager
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<(bool IsValid, string Message)>> _pendingValidations;

    public StockValidationResponseManager()
    {
        _pendingValidations = new ConcurrentDictionary<Guid, TaskCompletionSource<(bool IsValid, string Message)>>();
    }

    public TaskCompletionSource<(bool IsValid, string Message)> CreatePendingValidation(Guid requestId)
    {
        var tcs = new TaskCompletionSource<(bool IsValid, string Message)>();
        _pendingValidations.TryAdd(requestId, tcs);
        return tcs;
    }

    public bool TryCompleteValidation(Guid requestId, bool isValid, string message)
    {
        if (_pendingValidations.TryRemove(requestId, out var tcs))
        {
            tcs.SetResult((isValid, message));
            return true;
        }
        return false;
    }

    public void CancelPendingValidation(Guid requestId)
    {
        if (_pendingValidations.TryRemove(requestId, out var tcs))
        {
            tcs.SetCanceled();
        }
    }
}
