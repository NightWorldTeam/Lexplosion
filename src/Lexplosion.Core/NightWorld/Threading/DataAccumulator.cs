using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NightWorld.Threading
{
	public class DataAccumulator<TResult, TInput>
	{
		private readonly int _waitingTime;
		private readonly Action<ICollection<TInput>, ConcurrentDictionary<TInput, TResult>> _executor;
		private List<TInput> _inputs = new();
		private State _state = State.Waiting;
		private ConcurrentDictionary<TInput, TResult> _results = null;
		private ManualResetEvent _event = new(false);
		private SemaphoreSlim _sem = new(1, 1);

		public DataAccumulator(int waitingTime, Action<ICollection<TInput>, ConcurrentDictionary<TInput, TResult>> executor)
		{
			_waitingTime = waitingTime;
			_executor = executor;
		}

		public TResult Perform(TInput input)
		{
			var results = WaitResults(input);

			results.TryGetValue(input, out var result);
			return result;
		}

		private ConcurrentDictionary<TInput, TResult> WaitResults(TInput input)
		{
		StartPoint:;
			_sem.Wait();
			if (_state == State.Waiting)
			{
				return ExecuteAccumulation(input);
			}
			else if (_state == State.DataAccumulation)
			{
				var res = _results;
				_inputs.Add(input);

				_sem.Release();
				_event.WaitOne();

				return res;
			}
			else //_state == State.Executing
			{
				_sem.Release();

				_event.WaitOne();
				goto StartPoint;
			}
		}

		private ConcurrentDictionary<TInput, TResult> ExecuteAccumulation(TInput input)
		{
			_results = new ConcurrentDictionary<TInput, TResult>();
			var res = _results;

			_state = State.DataAccumulation;
			_inputs.Add(input);

			_event.Reset();
			_sem.Release();
			if (_waitingTime > 0) Thread.Sleep(_waitingTime);

			_sem.Wait();
			_state = State.Executing;
			_sem.Release();

			_executor(_inputs, _results);

			_sem.Wait();
			_inputs = new();
			_state = State.Waiting;
			_event.Set();
			_sem.Release();

			return res;
		}

		public enum State
		{
			Waiting,
			DataAccumulation,
			Executing
		}

	}
}
