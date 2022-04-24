using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic
{
    class KeySemaphore
    {
        private class SemophoreData
        {
            public Semaphore semaphore;
            public int users;
        }

        private Dictionary<string, SemophoreData> _semaphores = new Dictionary<string, SemophoreData>();
        private object _locker = new object();

        public void WaitOne(string key)
        {
            SemophoreData sem;
            lock (_locker)
            {
                if (!_semaphores.ContainsKey(key))
                {
                    sem = new SemophoreData
                    {
                        semaphore = new Semaphore(1, 1),
                        users = 1
                    };

                    _semaphores[key] = sem;
                }
                else
                {
                    sem = _semaphores[key];
                    sem.users++;
                }
            }

            sem.semaphore.WaitOne();
        }

        public void Release(string key)
        {
            lock (_locker)
            {
                _semaphores[key].users--;
                if (_semaphores[key].users < 1)
                {
                    _semaphores[key].semaphore.Release();
                    _semaphores.Remove(key);
                }
                else
                {
                    _semaphores[key].semaphore.Release();
                }
            }
        }
    }
}
