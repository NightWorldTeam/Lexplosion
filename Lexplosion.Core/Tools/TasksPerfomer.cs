using System.Threading;

namespace Lexplosion.Tools
{
	class TasksPerfomer
	{
		private Semaphore _sem; // этот семафор нужен чтобы за раз потоков не запустилось больше установленного количества
		private ManualResetEvent _endEvent = new ManualResetEvent(false); // эта хуйня сработает когда все потоки завершат работу

		private int _remainingTasksCount;

		public TasksPerfomer(int threadsCount, int totalTasksCount)
		{
			_sem = new Semaphore(threadsCount, threadsCount);
			_remainingTasksCount = totalTasksCount;
		}

		public void ExecuteTask(ThreadStart task)
		{
			_sem.WaitOne();
			try
			{
				var thread = new Thread(delegate ()
				{
					_sem.WaitOne();

					try
					{
						task();
					}
					finally
					{
						_remainingTasksCount--; // TODO: сделать Interlocked 
						if (_remainingTasksCount < 1)
						{
							_endEvent.Set();
						}

						_sem.Release();
					}

				});

#if DEBUG
				thread.Name = $"TasksPerfomer ({GetHashCode()}) thread {thread.GetHashCode()}";
#endif
				thread.Start();
			}
			finally
			{
				_sem.Release();
			}
		}

		public void WaitEnd() => _endEvent.WaitOne();
	}
}
