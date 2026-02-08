using System.Collections.Generic;

namespace Lexplosion.Logic.Management
{
	public struct ClientInitResult
	{
		public InstanceInit State;
		public IReadOnlyCollection<string> DownloadErrors;

		public ClientInitResult(InstanceInit state, IReadOnlyCollection<string> downloadErrors)
		{
			State = state;
			DownloadErrors = downloadErrors;
		}

		public ClientInitResult(InstanceInit state)
		{
			State = state;
			DownloadErrors = new List<string>();
		}
	}
}
