using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management
{
	public class ImportInterruption
	{
		private readonly ManualResetEvent _baseDataEvent = new(false);

		public enum InterruptionType
		{
			BasicDataRequired
		}

		public ImportInterruption(Guid importId)
		{
			ImportId = importId;
		}

		private BaseInstanceData _baseInstanceData;
		public BaseInstanceData BaseData
		{
			get
			{
				_baseDataEvent.WaitOne();
				return _baseInstanceData;
			}
			set
			{
				_baseInstanceData = value;
				_baseDataEvent.Set();
			}
		}

		public Guid ImportId { get; }
	}
}
