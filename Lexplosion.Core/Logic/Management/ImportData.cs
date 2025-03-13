using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Tools;
using static Lexplosion.Logic.Management.ImportInterruption;

namespace Lexplosion.Logic.Management
{
	public struct ImportData
	{
		public readonly DynamicStateHandler<ImportInterruption, InterruptionType> InterruptionHandler;
		public readonly Guid ImportId;
		public readonly CancellationToken CancelToken;

		private readonly CancellationTokenSource _cancellationTokenSource;

		public ImportData(DynamicStateHandler<ImportInterruption, InterruptionType> interruptionHandler)
		{
			InterruptionHandler = interruptionHandler;
			ImportId = Guid.NewGuid();
			_cancellationTokenSource = new CancellationTokenSource();
			CancelToken = _cancellationTokenSource.Token;
		}

		public void CancelImport()
		{
			_cancellationTokenSource.Cancel();
		}
	}
}
