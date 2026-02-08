using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network.TURN
{
	class TurnBridgeServer : IServerTransmitter
	{
		class ClientData
		{
			public AutoResetEvent SendDataEvent = new AutoResetEvent(false);
			public Socket Sock;
			public int BufferSize = 0;
			public ConcurrentQueue<byte[]> Buffer = new ConcurrentQueue<byte[]>();
			public ClientDesc Point;
			public Thread SendThread;
		}

		private const int MAX_BUFFER_SIZE = 10 * 1024 * 1024;

		private List<Socket> _sockets = new List<Socket>();

		private object _waitDeletingLoocker = new object();
		private ManualResetEvent _waitConnections = new ManualResetEvent(false); // блокировка метода Receive, если нет клиентов
		private ConcurrentDictionary<ClientDesc, ClientData> _clients = new();
		private ConcurrentDictionary<Socket, ClientDesc> _clientsPoints = new();

		private bool IsWork = true;

		private byte[] _selfTurnId;
		private char _groupPrefix;
		private IPEndPoint _serverPoint;

		/// <param name="uuid">UUID с которым мы подключаемся к серверу. Не должен быть больше 32-х символов.</param>
		/// <param name="turnGroup">Этот символ будет вставлен перед uuid при подключении к серверу.
		/// Он описывает группу, к которой относится это подключение.
		/// </param>
		public TurnBridgeServer(string uuid, char turnGroup, IPEndPoint controlServerPoint)
		{
			_selfTurnId = Encoding.UTF8.GetBytes(turnGroup + uuid);
			_groupPrefix = turnGroup;

			_serverPoint = controlServerPoint;
		}

		/// <summary>
		/// Выполняет соединение с хостом.
		/// </summary>
		/// <param name="hostUUID">UUID хоста. не должен быть больше 32-х символов.</param>
		/// <param name="clientDesc">Поинт, присвоенный этому клиенту. С помощью этого поинта можно взаимодействоать с склиентом.</param>
		/// <returns></returns>
		public bool Connect(string hostUUID, out ClientDesc clientDesc)
		{
			try
			{
				byte[] data = new byte[66];
				byte[] bhostUUID = Encoding.UTF8.GetBytes(_groupPrefix + hostUUID);

				Buffer.BlockCopy(_selfTurnId, 0, data, 0, _selfTurnId.Length);
				Buffer.BlockCopy(bhostUUID, 0, data, 33, bhostUUID.Length);

				Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				sock.Connect(_serverPoint);
				sock.Send(data);

				clientDesc = new ClientDesc(hostUUID, (IPEndPoint)sock.LocalEndPoint);

				lock (_waitDeletingLoocker)
				{
					_sockets.Add(sock);
				}

				var clientData = new ClientData()
				{
					Sock = sock,
					Point = clientDesc
				};

				var sendThread = new Thread(delegate ()
				{
					ServiceSend(clientData);
				});

				clientData.SendThread = sendThread;
				_clients[clientDesc] = clientData;
				_clientsPoints[sock] = clientDesc;
				sendThread.Start();

				_waitConnections.Set();

				Runtime.DebugConsoleWrite("CONNECTED FGDSGFSD");
			}
			catch
			{
				clientDesc = ClientDesc.Empty;
				return false;
			}

			return true;
		}

		private void ServiceSend(ClientData data)
		{
			AutoResetEvent sendDataEvent = data.SendDataEvent;
			Socket sock = data.Sock;
			ConcurrentQueue<byte[]> buffer = data.Buffer;
			ClientDesc point = data.Point;

			while (IsWork)
			{
				try
				{
					sendDataEvent.WaitOne();
					while (buffer.Count > 0 && IsWork)
					{
						buffer.TryDequeue(out byte[] package);
						int sendCount = sock.Send(package);
						data.BufferSize -= package.Length;
					}
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex)
				{
					Runtime.DebugConsoleWrite("ServiceSend Exception " + ex);
					ThreadPool.QueueUserWorkItem(delegate (object state)
					{
						Close(point);
						ClientClosing?.Invoke(point);
					});
				}
			}

			Runtime.DebugConsoleWrite("ServiceSend end");
		}

		public ClientDesc Receive(out byte[] data)
		{
			while (IsWork)
			{
				_waitConnections.WaitOne(); // тут метод остановится, если нет ни одного клиента

				List<Socket> sockets_;
				lock (_waitDeletingLoocker)
					sockets_ = new List<Socket>(_sockets);

				Socket.Select(sockets_, null, null, -1);
				if (sockets_.Count < 1)
				{
					continue;
				}

				Socket sock = sockets_[0];

				ClientDesc point;
				// При получении ClientDesc может быть исключение. В этом случае мы продалжаем цикл и снова пытаемся считать данные
				try
				{
					point = _clientsPoints[sock];
				}
				catch (Exception e)
				{
					Runtime.DebugConsoleWrite("Turn Receive exception " + e);
					continue;
				}

				try
				{
					data = new byte[sock.Available];
					int recvCount = sock.Receive(data);
				}
				catch (Exception e)
				{
					Runtime.DebugConsoleWrite("Turn Receive exception " + e);
					data = new byte[0];
				}

				return point;
			}

			Runtime.DebugConsoleWrite("Turn Receive stop");
			data = new byte[0];
			return ClientDesc.Empty;
		}

		public void Send(byte[] inputData, ClientDesc clientDesc)
		{
			ClientData clientData = _clients[clientDesc];
			int dataLenght = inputData.Length;

			if (clientData.BufferSize + dataLenght <= MAX_BUFFER_SIZE)
			{
				clientData.Buffer.Enqueue(inputData);
				clientData.BufferSize += dataLenght;
				clientData.SendDataEvent.Set();
			}
			else
			{
				ThreadPool.QueueUserWorkItem(delegate (object state)
				{
					Close(clientDesc);
					ClientClosing?.Invoke(clientDesc);
				});
			}
		}

		public void StopWork()
		{
			IsWork = false;
			lock (_waitDeletingLoocker)
			{
				foreach (var client in _clients.Values)
				{
					try
					{
						client.SendThread.Abort();
					}
					catch { }
				}

				foreach (var socket in _sockets)
				{
					socket.Close();
				}
			}
		}

		public bool Close(ClientDesc clientDesc)
		{
			Runtime.DebugConsoleWrite("TURN CLOSE ");
			lock (_waitDeletingLoocker)
			{
				// может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
				if (IsWork && _clients.ContainsKey(clientDesc))
				{
					Runtime.DebugConsoleWrite("TRUN CLOSE GSFSDGF");
					_clients.TryRemove(clientDesc, out ClientData clientData);
					_sockets.Remove(clientData.Sock);

					if (_sockets.Count == 0) // если не осталось клиентов, то стопаем метод Receive
					{
						_waitConnections.Reset();
					}

					try
					{
						clientData.SendThread.Abort();
					}
					catch { }

					clientData.Sock.Close();
				}
			}
			Runtime.DebugConsoleWrite("TURN END CLOSE ");

			return true;
		}

		private HashSet<ClientDesc> _sendAvailableClients = new();

		public IReadOnlyCollection<ClientDesc> WaitSendAvailable()
		{
			_sendAvailableClients.Clear();

			while (IsWork)
			{
				_waitConnections.WaitOne(); // тут метод остановится, если нет ни одного клиента

				try
				{
					List<Socket> sockets_;
					lock (_waitDeletingLoocker)
						sockets_ = new List<Socket>(_sockets);

					Socket.Select(null, sockets_, null, -1);
					if (sockets_.Count < 1) continue;

					foreach (var socket in sockets_)
					{
						if (_clientsPoints.TryGetValue(socket, out ClientDesc value)) _sendAvailableClients.Add(value);
					}
				}
				catch { }

				return _sendAvailableClients;
			}

			return _sendAvailableClients;
		}

		public event ClientPointHandle ClientClosing;
	}
}
