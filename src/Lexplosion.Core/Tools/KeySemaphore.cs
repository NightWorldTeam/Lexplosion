using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Tools
{
	public class KeySemaphore<T>
	{
		private class SemophoreData
		{
			public Semaphore Sem;
			public int Users;
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
						Sem = new Semaphore(1, 1),
						Users = 1
					};

					_semaphores[key] = sem;
				}
				else
				{
					sem = _semaphores[key];
					sem.Users++;
				}
			}

			sem.Sem.WaitOne();
		}

		public void Release(T key)
		{
			lock (_locker)
			{
				if (_semaphores.ContainsKey(key) && _semaphores[key].Users > 0)
				{
					_semaphores[key].Users--;
					if (_semaphores[key].Users < 1)
					{
						_semaphores[key].Sem.Release();
						_semaphores.Remove(key);
					}
					else
					{
						_semaphores[key].Sem.Release();
					}
				}
			}
		}
	}
}
