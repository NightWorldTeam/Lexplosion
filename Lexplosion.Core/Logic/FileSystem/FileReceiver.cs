using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.FileSystem
{
	public class FileReceiver
	{
		#region staticData
		private class RecieverInfo
		{
			public string Login;
			public string UserUUID;
			public string FileId;
			public string Parameters;
		}

		private static object _dataClientInitLocker = new object();

		public static List<FileReceiver> GetDistributors(string uuid, string sessionToken)
		{
			var data = new List<FileReceiver>();
			string anwer = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "getFileDistributions", new Dictionary<string, string>
			{
				["UUID"] = uuid,
				["sessionToken"] = sessionToken
			});

			if (anwer == null)
			{
				return data;
			}

			try
			{
				var recieversInfo = JsonConvert.DeserializeObject<List<RecieverInfo>>(anwer);

				if (recieversInfo != null)
				{
					foreach (RecieverInfo reciverInfo in recieversInfo)
					{
						var parameters = JsonConvert.DeserializeObject<DistributionData>(reciverInfo.Parameters);
						data.Add(new FileReceiver(uuid, sessionToken, reciverInfo.Login, reciverInfo.UserUUID, reciverInfo.FileId, parameters));
					}
				}
			}
			catch { }

			return data;
		}

		private static KeySemaphore<string> _downloadSemaphore = new();


		public enum DistributionState
		{
			InQueue,
			InConnect,
			InProcess
		}

		#endregion

		private string _ownerLogin;
		private string _ownerUUID;
		private string _fileId;

		private DataClient _dataClient = null;
		private DistributionData _info;

		public event Action<double> ProcentUpdate;
		public event Action<double> SpeedUpdate;
		public event Action StateChanged;

		private DistributionState _state;
		public DistributionState State
		{
			get => _state;
			private set
			{
				_state = value;
				StateChanged?.Invoke();
				Runtime.DebugWrite($"New state: {_state}");
			}
		}

		private object _locker = new object();

		public string OwnerLogin
		{
			get => _ownerLogin;
		}

		public string Name
		{
			get => _info?.Name;
		}

		public string Id
		{
			get => _fileId;
		}

		private string _myUUID;
		private string _mySessionToken;

		private FileReceiver(string myUUID, string mySessionToken, string ownerLogin, string ownerUUID, string fileId, DistributionData info)
		{
			_ownerLogin = ownerLogin;
			_ownerUUID = ownerUUID;
			_fileId = fileId;
			_info = info;

			_myUUID = myUUID;
			_mySessionToken = mySessionToken;

			_state = DistributionState.InQueue;
		}

		public void CancelDownload()
		{
			lock (_locker)
			{
				_dataClient?.Close();
				_dataClient = null;
			}
		}

		public FileRecvResult StartDownload(string fileName)
		{
			try
			{
				State = DistributionState.InQueue;

				_downloadSemaphore.WaitOne(_ownerUUID);
				lock (_locker)
				{
					var publicKey = Cryptography.DecodeRsaParams(_info.PublicRsaKey);
					var serverData = new ControlServerData(LaunсherSettings.ServerIp);

					_dataClient?.Close();
					_dataClient = new DataClient(publicKey, _info.ConfirmWord, serverData, fileName, _fileId);
					_dataClient.SpeedUpdate += SpeedUpdate;
					_dataClient.ProcentUpdate += ProcentUpdate;

					lock (_dataClientInitLocker)
					{
						State = DistributionState.InConnect;
						bool result = _dataClient.Initialization(_myUUID, _mySessionToken, _ownerUUID);
						if (!result)
						{
							_dataClient.Close();
							return FileRecvResult.ConnectionClose;
						}
					}

					State = DistributionState.InProcess;
				}

				return _dataClient?.WorkWait() ?? FileRecvResult.Canceled;
			}
			finally
			{
				_downloadSemaphore.Release(_ownerUUID);
			}
		}
	}
}
