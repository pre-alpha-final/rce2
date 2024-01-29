namespace FileSyncAgent.Extensions;

public static class SemaphoreSlimExtensions
{
    public static int SafeRelease(this SemaphoreSlim semaphoreSlim)
    {
        try
        {
            return semaphoreSlim.Release();
        }
        catch
        {
            // Ignore
        }

        return -1;
    }

    // IMPORTANT
    // Needs to be awaited in using() otherwise returns a task
    // instead of Disposable -> no Dispose = stuck
    public static async Task<IDisposable> DisposableWaitAsync(this SemaphoreSlim semaphoreSlim, TimeSpan timeout)
    {
        var milliseconds = (int)timeout.TotalMilliseconds;
        if (timeout == TimeSpan.MaxValue)
        {
            milliseconds = int.MaxValue; // Max for WaitAsync
        }
        await semaphoreSlim.WaitAsync(milliseconds).ConfigureAwait(false);

        return new Disposable(semaphoreSlim);
    }

    private class Disposable : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private bool _isDisposed;

        public Disposable(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            _semaphoreSlim.SafeRelease();
        }
    }
}
