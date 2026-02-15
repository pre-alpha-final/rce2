using PubSub;

namespace Rce2;

public class Rce2Sync<T> : IDisposable
{
    private readonly SemaphoreSlim _sync = new(0, 1);
    private readonly ValidateFunc _validate;
    private readonly int _timeout;

    public Rce2Sync(ValidateFunc validate, int timeout = 3000)
    {
        _validate = validate;
        _timeout = timeout;

        Hub.Default.Subscribe<Rce2Message>(this, OnRce2Message);
    }

    public delegate Task<bool> ValidateFunc(T payload, Rce2Message rce2Message);
    public Rce2Message? Result { get; set; }

    public void Dispose()
    {
        Hub.Default.Unsubscribe(this);
    }

    public async Task WaitAsync()
    {
        var timeoutTask = Task.Delay(_timeout);
        var responseTask = _sync.WaitAsync();

        await Task.WhenAny(timeoutTask, responseTask);
    }

    private async Task OnRce2Message(Rce2Message rce2Message)
    {
        try
        {
            var payloadData = (T)rce2Message.Payload?["data"]?.ToObject(typeof(T))!;
            if (await _validate(payloadData, rce2Message))
            {
                Result = rce2Message;
                _sync.Release();
            }
        }
        catch
        {
            // ignore
        }
    }
}
