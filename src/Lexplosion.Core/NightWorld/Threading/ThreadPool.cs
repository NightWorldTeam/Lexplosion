using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NightWorld.Threading
{
	public class ThreadPool
	{
		public int BaseTreadsCount { get; }
		public int MaxThreadsCount { get; }

		private int _currentWorkersCount = 0;
		private Thread[] _baseThreads;
		private ConcurrentQueue<Action> _tasks = new();
		private object _taskGetlocker = new();
		private AutoResetEvent _taskWaiter = new AutoResetEvent(false);
		private bool _isWork = true;

		public ThreadPool(int baseTreadsCount, int maxThreadsCount)
		{
			BaseTreadsCount = baseTreadsCount;
			MaxThreadsCount = maxThreadsCount;

			_baseThreads = new Thread[BaseTreadsCount];
			for (int i = 0; i < BaseTreadsCount; i++)
			{
				_baseThreads[i] = new Thread(ThreadWorkCycle)
				{
					IsBackground = true,
					Name = $"ThreadPool({GetHashCode()}) thread {i}"
				};

				_baseThreads[i].Start();
			}
		}

		public void Enqueue(Action action)
		{
			lock (_taskGetlocker)
			{
				_tasks.Enqueue(action);
				_taskWaiter.Set();
				if (_currentWorkersCount >= BaseTreadsCount && (MaxThreadsCount < 0 || _currentWorkersCount < MaxThreadsCount))
				{
					new Thread(SupportThreadWorkCycle)
					{
						IsBackground = true,
						Name = $"ThreadPool({GetHashCode()}) support thread"
					}.Start();
				}
			}
		}

		private void ThreadWorkCycle()
		{
			Action task;
			while (_isWork && (task = GetAction()) != null)
			{
				try
				{
					task();
				}
				catch { }

				lock (_taskGetlocker) _currentWorkersCount--;
			}
		}

		private void SupportThreadWorkCycle()
		{
			Action task;
			while (_isWork && (task = GetActionWithoutWaiting()) != null)
			{
				try
				{
					task();
				}
				catch { }

				lock (_taskGetlocker) _currentWorkersCount--;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private Action GetAction()
		{
			while (_isWork)
			{
				lock (_taskGetlocker)
				{
					if (_tasks.Count > 0 && _tasks.TryDequeue(out Action action))
					{
						_currentWorkersCount++;
						return action;
					}
				}

				_taskWaiter.WaitOne();
			}

			return null;
		}

		private Action GetActionWithoutWaiting()
		{
			lock (_taskGetlocker)
			{
				if (_tasks.Count < 1 || !_tasks.TryDequeue(out Action action)) return null;

				_currentWorkersCount++;
				return action;
			}
		}
	}
}
