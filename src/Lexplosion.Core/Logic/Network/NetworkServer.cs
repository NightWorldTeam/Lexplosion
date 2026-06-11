using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using LumiSoft.Net.STUN.Client;

using ByteConverter = NightWorld.Tools.ByteConverter;


namespace Lexplosion.Logic.Network
{
	using SMP;
	using TURN;

	abstract class NetworkServer
	{
		protected Thread AcceptingThread;
		protected Thread ReadingThread;
		protected Thread SendingThread;
		private Thread MaintainingThread;

		protected Semaphore AcceptingBlock = new Semaphore(1, 1); //блокировка во время приёма подключения
		protected ManualResetEvent ConnectionWait = new(false); // блокируется на время работы метода PerformConnect

		private AutoResetEvent _controlConnectionBlock = new AutoResetEvent(false); // чтобы методы MaintainingConnection и Accepting одновременно не обраащлись к управляющему серверу
		private ManualResetEvent _threadsStartWait = new(false);

		private Socket _controlConnection;
		protected IServerTransmitter Server;
		protected bool IsWork = false;

		protected string UUID;
		protected string _sessionToken;
		protected bool SmpConnection;
		protected ControlServerData ControlServer;

		public event Action<string> ConnectingUser;
		public event Action<string> DisconnectedUser;

		// тут хранится список клиентов. В одном соответсвие uuid и ip, в другом наоборот
		private ConcurrentDictionary<string, IPEndPoint> _uuidPointPair = new();
		private ConcurrentDictionary<IPEndPoint, string> _pointUuidPair = new();

		protected HashSet<string> KickedClients = new HashSet<string>(); //тут хранятся выкинутые клиенты

		protected (string, int) SelectedStunServer;
		protected (string, int)[] StunServers;

		public NetworkServer(string uuid, string sessionToken, string serverType, bool directConnection, ControlServerData controlServer)
		{
			UUID = uuid;
			_sessionToken = sessionToken;
			IsWork = true;
			ControlServer = controlServer;
			StunServers = controlServer.StunServers;
			SelectedStunServer = controlServer.StunServers[0];

			_controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			AcceptingThread = new Thread(delegate () //поток принимающий новые подключения
			{
				TransmitterPrepear(directConnection, serverType);
				_threadsStartWait.Set();
				Accepting(serverType);
			});

			//этот поток читает сообщения от клиента
			ReadingThread = new Thread(delegate ()
			{
				_threadsStartWait.WaitOne();
				Reading();
			});

			//этот поток отправляет сообщения клиенту
			SendingThread = new Thread(delegate ()
			{
				_threadsStartWait.WaitOne();
				Sending();
			});

			MaintainingThread = new Thread(MaintainingConnection); //поток отправляющий управляющему серверу пустые пакеты для поддержиния соединения
		}

		protected void StartThreads()
		{
			AcceptingThread.Start();
			SendingThread.Start();
			ReadingThread.Start();
		}

		private void TransmitterPrepear(bool directConnectionIsPriority, string serverType)
		{
			// если стоит парметр установки прямого соединения, то проверяем, возможно ли его вообще установить. если нет - переходим на TURN
			if (directConnectionIsPriority)
			{
				STUN_Result result = null;
				foreach (var stunServ in StunServers)
				{
					Runtime.DebugConsoleWrite("Check stun server: " + stunServ);
					try
					{
						result = STUN_Client.Query(stunServ.Item1, stunServ.Item2, new IPEndPoint(IPAddress.Any, 0)); //получем наш внешний адрес
						Runtime.DebugConsoleWrite("NatType " + result?.NetType.ToString());

						if (result != null && result.NetType != STUN_NetType.UdpBlocked)
						{
							Runtime.DebugConsoleWrite("Selected stun server: " + stunServ);
							SelectedStunServer = stunServ;
							break;
						}
					}
					catch { }
				}

				if (result != null && result.NetType != STUN_NetType.UdpBlocked && result.NetType != STUN_NetType.Symmetric && result.NetType != STUN_NetType.SymmetricUdpFirewall)
				{
					SmpConnection = true;
					Server = new SmpServer();
				}
				else
				{
					SmpConnection = false;
					Server = new TurnBridgeServer(UUID, serverType[0], ControlServer.TurnPoint);
				}
			}
			else
			{
				SmpConnection = false;
				Server = new TurnBridgeServer(UUID, serverType[0], ControlServer.TurnPoint);
			}

			Server.ClientClosing += ClientAbort;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PrepeareRepeat()
		{
			try
			{
				MaintainingThread.Abort();
			}
			catch { }

			MaintainingThread = new Thread(MaintainingConnection);

			try
			{
				WriteMessage(_controlConnection, ControlSrverCodes.Z);
			}
			catch { }
			finally
			{
				try { _controlConnection?.Close(); } catch { }
			}

			_controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Runtime.DebugConsoleWrite("Repeat connection to control server");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool SetConnection(string serverType)
		{
			for (int i = 0; i < 5; i++)
			{
				Runtime.DebugConsoleWrite("Сonnection attempt " + i);

				//подключаемся к управляющему серверу
				try
				{
					_controlConnection.Connect(ControlServer.HandshakeServerPoint);
				}
				catch (Exception ex)
				{
					//при ошибке ждем 10 секунд и пытаемся повторить
					Runtime.DebugConsoleWrite("Сonnection to control server error: " + ex);
					Thread.Sleep(10000);
					Runtime.DebugConsoleWrite("Repeat connection");

					try
					{
						_controlConnection.Close();
					}
					catch { }

					_controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					continue;
				}

				try
				{
					var st =
					"{\"UUID\" : \"" + UUID + "\"," +
					" \"type\": \"" + serverType + "\"," +
					" \"method\": \"" + (SmpConnection ? "STUN" : "TURN") + "\"," +
					" \"sessionToken\" : \"" + _sessionToken + "\"}";

					byte[] sendData = Encoding.UTF8.GetBytes(st);
					WriteMessage(_controlConnection, sendData); //авторизируемся на упрявляющем сервере

					byte[] answer = ReadMessage(_controlConnection, 1);

					// сервер должен вернуть либо ControlSrverCodes.Y - успех авторизации, либо ControlSrverCodes.Z - отказ в авторизации
					// если он вернул какую-то хуйню пробуем повторно
					if (answer == null || (answer[0] != ControlSrverCodes.Y && answer[0] != ControlSrverCodes.Z))
					{
						Runtime.DebugConsoleWrite("Auth answer error");
						Thread.Sleep(10000);
						PrepeareRepeat();

						continue;
					}

					// сервер отказал в акторизации, выходим
					if (answer[0] == ControlSrverCodes.Z)
					{
						Runtime.DebugConsoleWrite("Auth failed");
						return false;
					}
				}
				catch (Exception ex)
				{
					Runtime.DebugConsoleWrite("Сonnection to control server error: " + ex);
					Thread.Sleep(10000);
					PrepeareRepeat();
					continue;
				}

				MaintainingThread.Start();
				Runtime.DebugConsoleWrite("Control connection is established");

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PerformConnect(string clientUUID, string myPort, Socket udpSocket, string hostPointData, bool directConnectPossible)
		{
			bool isConected;
			IPEndPoint point = null;
			ConnectionWait.Reset();

			try
			{
				if (Server is SmpServer)
				{
					Runtime.DebugConsoleWrite("Udp connection");

					using (SHA1 sha = new SHA1Managed())
					{
						byte[] connectionCode;
						string hostPort;
						if (!directConnectPossible || hostPointData.EndsWith(",proxy"))
						{
							Runtime.DebugConsoleWrite("Udp proxy (" + directConnectPossible + ", " + hostPointData.EndsWith(",proxy") + ") " + ControlServer.SmpProxyPoint);
							point = ControlServer.SmpProxyPoint;
							hostPointData = hostPointData.Replace(",proxy", "");
							hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
						}
						else
						{
							Runtime.DebugConsoleWrite("Udp direct connection");
							hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
							string hostIp = hostPointData.Replace(":" + hostPort, "");
							point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
						}

						var strCode = UUID + "," + clientUUID + "," + myPort + "," + hostPort;
						Runtime.DebugConsoleWrite("Connection code: " + strCode);
						connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(strCode));

						var localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
						udpSocket.Close();
						Runtime.DebugConsoleWrite($"Client point {point}");
						isConected = ((SmpServer)Server).Connect(localPoint, new ClientDesc(clientUUID, point), connectionCode);
					}
				}
				else
				{
					Runtime.DebugConsoleWrite("Tcp Proxy");
					isConected = ((TurnBridgeServer)Server).Connect(clientUUID, out ClientDesc clientDesc);
					point = clientDesc.Point;
				}
			}
			catch (Exception ex)
			{
				isConected = false;
				Runtime.DebugConsoleWrite("Connect exception " + ex);
			}

			AcceptingBlock.WaitOne();

			if (isConected)
			{
				Runtime.DebugConsoleWrite("КОННЕКТ!!!");
				if (AfterConnect(new ClientDesc(clientUUID, point)))
				{
					Runtime.DebugConsoleWrite("After AfterConnect");
					_uuidPointPair[clientUUID] = point;
					_pointUuidPair[point] = clientUUID;

					try
					{
						ConnectingUser?.Invoke(clientUUID);
					}
					catch { }
				}
				else
				{
					Runtime.DebugConsoleWrite("Пиздец");
					AcceptingBlock.Release();
				}
			}
			else
			{
				Runtime.DebugConsoleWrite("Пиздец1");
				AcceptingBlock.Release();
			}

			ConnectionWait.Set();
		}

		protected void Accepting(string serverType)
		{
			// TODO: если управляющий есрерв откажет в подключении, то эта поябень будет его постоянно долбить запросами, пытаясь подключиться
			bool contolConnectionExists = true;
			while (IsWork && contolConnectionExists)
			{
				contolConnectionExists = SetConnection(serverType);
				bool needRepeat = false;

				while (IsWork && contolConnectionExists)
				{
					try
					{
						string clientUUID;

						Runtime.DebugConsoleWrite("ControlServerRecv");
						_controlConnectionBlock.Set(); // освобождаем семафор переда как начать слушать сокет. Ждать мы на Receive можем долго
						_controlConnection.ReceiveTimeout = -1; // делаем бесконечное ожидание

						byte[] message = null;
						try
						{
							message = ReadMessage(_controlConnection, 2);
						}
						catch (Exception ex)
						{
							Runtime.DebugConsoleWrite("Exception " + ex);
							needRepeat = true;
							break;
						}
						finally
						{
							_controlConnectionBlock.WaitOne(); // блочим семофор
							_controlConnection.ReceiveTimeout = 10000; //огрниччиваем ожидание до 10 секунд
							Runtime.DebugConsoleWrite("ControlServerEndRecv");
						}

						bool directConnectPossible = true;
						string myPort;
						Socket udpSocket = null;

						// проверяем поступил запрос на подключение. Если это не запрос на подключение
						// случился какой-то сбой ставим флаг для переподключения
						if (message == null || message[0] != ControlSrverCodes.A)
						{
							Runtime.DebugConsoleWrite("Repeat 2");
							needRepeat = true;
							break;
						}


						clientUUID = Encoding.UTF8.GetString(message, 1, message.Length - 1); // получаем UUID клиента

						// этот клиент был кикнут. послыем его нахуй
						lock (KickedClients)
						{
							if (KickedClients.Contains(clientUUID))
							{
								WriteMessage(_controlConnection, ControlSrverCodes.E); //отправляем серверу отказ
								continue;
							}
						}

						// такой клиент уже подключен. Значит обрываем прошлое соединение
						if (_uuidPointPair.ContainsKey(clientUUID))
						{
							_uuidPointPair.TryGetValue(clientUUID, out IPEndPoint point);
							if (point != null)
							{
								ClientAbort(new ClientDesc(clientUUID, point));
							}
						}

						WriteMessage(_controlConnection, ControlSrverCodes.A); //отправляем серверу соглашение

						message = null;
						message = ReadMessage(_controlConnection, 1);

						// проверяем запрашивает ли сервер порт. Сейчас он должен его запросить, если нет значит
						// случился какой-то сбой ставим флаг для переподключения
						if (message == null || message[0] != ControlSrverCodes.B)
						{
							Runtime.DebugConsoleWrite("Repeat 1");
							needRepeat = true;
							break;
						}

						byte[] portData;
						if (SmpConnection)
						{
							udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

							STUN_Result result = null;
							try
							{
								// TODO: сделать получения списка stun серверов с нашего сервера
								result = STUN_Client.Query(SelectedStunServer.Item1, SelectedStunServer.Item2, udpSocket); //получем наш внешний адрес
								Runtime.DebugConsoleWrite("NatType " + result.NetType.ToString());
							}
							catch { }

							//result = null;

							// TODO: наверное тут надо сделать проверку типа NAT.

							//парсим порт
							if (result?.PublicEndPoint != null)
							{
								myPort = result.PublicEndPoint.Port.ToString();
								portData = Encoding.UTF8.GetBytes(myPort.ToString());

								Runtime.DebugConsoleWrite("My EndPoint " + result.PublicEndPoint.ToString());
							}
							else // какая-то хуйня. будем устанавливать соединение через ретранслятор
							{
								Runtime.DebugConsoleWrite("My EndPoint error");
								var localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
								portData = Encoding.UTF8.GetBytes(localPoint.Port + ",proxy");
								myPort = localPoint.Port.ToString();
								directConnectPossible = false;
							}
						}
						else
						{
							myPort = "";
							portData = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
						}

						WriteMessage(_controlConnection, portData); //отправляем серверу наш порт

						byte[] pointData = ReadMessage(_controlConnection, 1); //получем ip клиента
						string pointDataStr = Encoding.UTF8.GetString(pointData);
						PerformConnect(clientUUID, myPort, udpSocket, pointDataStr, directConnectPossible);
					}
					catch (Exception ex)
					{
						Runtime.DebugConsoleWrite(ex);
					}
				}

				if (needRepeat)
				{
					PrepeareRepeat();
				}
				else
				{
					break;
				}
			}
		}

		public virtual void StopWork()
		{
			IsWork = false;

			AcceptingThread.Abort();
			MaintainingThread.Abort();
			try
			{
				WriteMessage(_controlConnection, ControlSrverCodes.Z); // отправляем управляющиму серверу сообщение что мы отключаемся
			}
			catch { }
			_controlConnection.Close(); //закрываем соединение с управляющим сервером

			SendingThread.Abort();
			ReadingThread.Abort();

			Server?.StopWork();
		}

		public void KickClient(string uuid)
		{
			try
			{
				if (uuid == "bbab3c32222e4f08a8b291d1e9b9267c" || uuid == "0920b1809fb09e14c2e0526a94fb7c93") return;

				lock (KickedClients)
				{
					KickedClients.Add(uuid);
				}

				IPEndPoint point = _uuidPointPair[uuid];
				ClientAbort(new ClientDesc(uuid, point));
			}
			catch { }
		}

		public void UnkickClient(string uuid)
		{
			lock (KickedClients)
			{
				if (KickedClients.Contains(uuid))
				{
					KickedClients.Remove(uuid);
				}
			}
		}

		protected ClientDesc ClientDescByUUID(string uuid)
		{
			_uuidPointPair.TryGetValue(UUID, out IPEndPoint point);
			if (point == null) return ClientDesc.Empty;
			return new ClientDesc(uuid, point);
		}

		protected string UuidByClientDesc(ClientDesc clientDesk)
		{
			if (clientDesk.IsEmpty) return null;
			_pointUuidPair.TryGetValue(clientDesk.Point, out string uuid);
			return uuid;
		}

		private void MaintainingConnection()
		{
			try
			{
				Thread.Sleep(120000); // ждём 2 минуты

				while (IsWork)
				{
					_controlConnectionBlock.WaitOne();
					WriteMessage(_controlConnection, ControlSrverCodes.Y);
					_controlConnectionBlock.Set();
					Thread.Sleep(120000); // ждём 2 минуты
				}
			}
			catch { }
		}

		protected virtual void ClientAbort(ClientDesc clientData) // мeтод который вызывается при обрыве соединения
		{
			try
			{
				_pointUuidPair.TryRemove(clientData.Point, out string clientUuid);

				if (clientUuid != null)
				{
					_uuidPointPair.TryRemove(clientUuid, out _);
					ThreadPool.QueueUserWorkItem((object obj) =>
					{
						DisconnectedUser?.Invoke(clientUuid);
					});
				}
			}
			catch { }
		}

		/// <summary>
		/// это метод который запускается после установления соединения
		/// </summary>
		protected virtual bool AfterConnect(ClientDesc clientData)
		{
			AcceptingBlock.Release();
			return true;
		}

		private void WriteMessage(Socket sock, byte[] data)
		{
			var message = new byte[data.Length + 2];
			ByteConverter.BigEndian.ToBytes(message, 0, (ushort)data.Length);

			Buffer.BlockCopy(data, 0, message, 2, data.Length);
			sock.Send(message);
		}

		private void WriteMessage(Socket sock, byte data)
		{
			var message = new byte[3];
			ByteConverter.BigEndian.ToBytes(message, 0, (ushort)1);

			message[2] = data;
			sock.Send(message);
		}

		private byte[] ReadMessage(Socket sock, int minLenght)
		{
			var messageLenghtBytes = new byte[2];

			int bytesCount = sock.Receive(messageLenghtBytes, 2, SocketFlags.None);
			if (bytesCount < 2) return null;

			ushort messageLenght = ByteConverter.BigEndian.ToUShort(messageLenghtBytes, 0);

			if (messageLenght < minLenght) return null;

			byte[] buffer = new byte[messageLenght];

			int lenght = sock.Receive(buffer, messageLenght, SocketFlags.None);
			if (lenght < messageLenght) return null;

			return buffer;
		}


		protected abstract void Sending(); // тут получаем данные от клиентов

		protected abstract void Reading(); // тут получаем данные из сети

	}
}
