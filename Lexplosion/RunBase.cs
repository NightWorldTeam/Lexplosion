namespace Lexplosion
{
    internal static class RunBase
    {

        public static StopTask TaskRun(ThreadStart ThreadFunc)
        {
            threads.Wait();

            int key = threads.Add(null);

            var thread = new Thread(delegate ()
            {
                int threadKey = key;

                ThreadFunc();

                threads.RemoveAt(threadKey);
            });

            threads[key] = thread;

            thread.Start();
            threads.Release();

            return delegate ()
            {
                thread.Abort();
                int threadKey = key;
                threads.RemoveAt(threadKey);
            };
        }
    }
}