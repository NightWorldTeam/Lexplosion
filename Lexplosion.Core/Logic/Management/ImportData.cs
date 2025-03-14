using System;
using System.Threading;
using Lexplosion.Tools;
using static Lexplosion.Logic.Management.ImportInterruption;

namespace Lexplosion.Logic.Management
{
	public struct ImportData
	{
		public readonly Guid ImportId = Guid.NewGuid();
		public readonly CancellationToken CancelToken;
		public readonly DynamicStateHandler<ImportInterruption, InterruptionType> InterruptionHandler;

		private readonly CancellationTokenSource _cancellationTokenSource;

		public ImportData(DynamicStateHandler<ImportInterruption, InterruptionType> interruptionHandler)
		{
			InterruptionHandler = interruptionHandler;
			_cancellationTokenSource = new CancellationTokenSource();
			CancelToken = _cancellationTokenSource.Token;
		}

		public void CancelImport()
		{
			_cancellationTokenSource.Cancel();
		}
	}
}
