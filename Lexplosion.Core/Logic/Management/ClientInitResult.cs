using System.Collections.Generic;

namespace Lexplosion.Logic.Management
{
	public struct ClientInitResult
	{
		public InstanceInit State;
		public List<string> DownloadErrors;

		public ClientInitResult(InstanceInit state, List<string> downloadErrors)
		{
			State = state;
			DownloadErrors = downloadErrors;
		}
	}
}
