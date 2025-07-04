﻿using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Tools;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.Logic.Management.Import
{
	public struct ImportData
	{
		public readonly Guid ImportId = Guid.NewGuid();
		public readonly CancellationToken CancelToken;
		public readonly DynamicStateHandler<ImportInterruption, InterruptionType> InterruptionHandler;

		private readonly CancellationTokenSource _cancellationTokenSource;

		public ImportData(DynamicStateHandler<ImportInterruption, InterruptionType> interruptionHandler, Action<ClientInitResult> resultHandler)
		{
			InterruptionHandler = interruptionHandler;
			ResultHandler = resultHandler;
			_cancellationTokenSource = new CancellationTokenSource();
			CancelToken = _cancellationTokenSource.Token;
		}

		public Action<ClientInitResult> ResultHandler { get; }

		public void CancelImport()
		{
			_cancellationTokenSource.Cancel();
		}
	}
}
