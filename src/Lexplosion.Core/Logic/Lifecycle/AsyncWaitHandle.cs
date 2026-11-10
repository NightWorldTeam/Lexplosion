using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Lifecycle
{
    public static class AsyncWaitHandle
    {
        public static async Task WaitAsync(WaitHandle handle, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (handle == null)
                throw new ArgumentNullException(nameof(handle));

            cancellationToken.ThrowIfCancellationRequested();
            if (handle.WaitOne(0))
                return;

            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, OnSignaled, completion, Timeout.Infinite, true);

            using (cancellationToken.Register(OnCancelled, completion))
            {
                try
                {
                    await completion.Task.ConfigureAwait(false);
                }
                finally
                {
                    registration.Unregister(null);
                }
            }
        }

        private static void OnSignaled(object state, bool timedOut)
        {
            var completion = (TaskCompletionSource<bool>)state;
            completion.TrySetResult(true);
        }

        private static void OnCancelled(object state)
        {
            var completion = (TaskCompletionSource<bool>)state;
            completion.TrySetCanceled();
        }
    }
}
