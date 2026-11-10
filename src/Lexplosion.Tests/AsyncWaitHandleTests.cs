using System;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.Lifecycle;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    [TestFixture]
    public sealed class AsyncWaitHandleTests
    {
        [Test]
        public async Task WaitAsyncCompletesWhenSignalled()
        {
            using (var signal = new ManualResetEvent(false))
            {
                var task = AsyncWaitHandle.WaitAsync(signal);
                Assert.That(task.IsCompleted, Is.False);

                signal.Set();
                await task;
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
        }

        [Test]
        public async Task WaitAsyncSupportsCancellation()
        {
            using (var signal = new ManualResetEvent(false))
            using (var source = new CancellationTokenSource())
            {
                var task = AsyncWaitHandle.WaitAsync(signal, source.Token);
                source.Cancel();

                try
                {
                    await task;
                    Assert.Fail("Wait should have been cancelled.");
                }
                catch (TaskCanceledException)
                {
                    Assert.That(task.IsCanceled, Is.True);
                }
            }
        }

        [Test]
        public async Task WaitAsyncFinishesImmediatelyForSignalledHandle()
        {
            using (var signal = new ManualResetEvent(true))
            {
                var task = AsyncWaitHandle.WaitAsync(signal);
                Assert.That(task.IsCompleted, Is.True);
                await task;
            }
        }
    }
}
