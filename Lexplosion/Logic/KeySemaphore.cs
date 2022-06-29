using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic
{
    class KeySemaphore<T>
    {
        private class SemophoreData
        {
            public Semaphore semaphore;
            public int users;
        }

        private Dictionary<T, SemophoreData> _semaphores = new Dictionary<T, SemophoreData>();
        private object _locker = new object();

        public void WaitOne(T key)
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

        public void Release(T key)
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
