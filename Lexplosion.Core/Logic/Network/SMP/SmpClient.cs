using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
	/// <summary>
	/// Клиент Stream Messages Protocol. Работает над udp. 
	/// Устанавливает соединение, контролириует доставку пакетов. Заточен для работы с Udp hole pushing.
	/// Может соединяться только с одним хостом.
	/// </summary>
	partial class SmpClient : IClientTransmitter
	{
		/* Я и из будующего, предлагаю тебе пойти нахуй, я не собираюсь комментировать код, мне лень */
		private readonly ConcurrentQueue<List<Package>> _sendingBuffer = new ConcurrentQueue<List<Package>>(); // Буфер пакетов на отправку
		private readonly ConcurrentQueue<Message> _receivingBuffer = new ConcurrentQueue<Message>();
		private readonly ConcurrentDictionary<ushort, byte[]> _packagesBuffer = new ConcurrentDictionary<ushort, byte[]>(); //буфер принятых пакетов

		private readonly object _sendLocker = new object();
		private readonly AutoResetEvent _waitSendData = new AutoResetEvent(false);
		private readonly AutoResetEvent _deliveryWait = new AutoResetEvent(false);
		private readonly Semaphore _repeatDeliveryBlock = new Semaphore(1, 1);
		private readonly object _closeLocker = new object();
		private readonly AutoResetEvent _receiveWait = new AutoResetEvent(false);
		private readonly ManualResetEvent _sendingCycleDetector = new ManualResetEvent(false);

		private ushort _sendingPointer = 0;
		private ushort _receivingPointer = 0;
		private byte _sendingIdsRange = 0;
		private byte _receivingIdsRange = 0;
		private int _lastPackage = -1;
		private short _lastPackageIdsRange = -1;
		private List<ushort> _repeatDeliveryList = null;
		private bool _sendigIsConfirmed = false;

		private int _maxPackagesCount = 400;
		private long _rtt = -1; // пинг в обе стороны (время ожидание ответа)
		private int _mtu = 68; // максимальный размер пакета
		private int _hostMtu = -1; // mtu удалённого хоста
		private byte _selfSessionId = 0; // наш id сессии. Его мы задаем при подключении и будем отправлять при отключении
		private byte _hostSessionId = 0; //id сесии хоста. Его мы будем проверять, если хост отправит запрос на отключение

		private RttCalculator _rttCalculator;

		private IPEndPoint _point = null;
		private readonly Socket _socket = null;

		private Thread _serviceSend;
		private Thread _serviceReceive;
		private Thread _connectionControl;

		private bool _workPing = false; // когда работает метод, вычислящий rtt эта переменная становится true
		private readonly long[] _times = new long[20]; // этот массив тоже нужен для метода вычисления пинга
		private long _pingPackagesDelay = 0;
		private readonly AutoResetEvent _pingWait = new AutoResetEvent(false);

		private readonly AutoResetEvent _mtuWait = new AutoResetEvent(false); // ожидание ответа при вычислении mtu
		private int _mtuPackageId = -1; // айди mtu пакета

		private readonly AutoResetEvent _mtuInfoWait = new AutoResetEvent(false); // ожидание ответа при отправке своего mtu
		private readonly Semaphore _calculateMtuBlock = new Semaphore(1, 1);

		public event Action MessageReceived;

		private bool _inStopping = false; // это флаг чтобы в процессе закрытия соединения нельзя было вызвать метод send
		private long _lastTime = 0; //время отправки последнего пакета
		private readonly int[] _delayMultipliers = new int[15] //этот массив хранит множители rtt при отправке сообщений.
        { 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1 };      //Ключ - номер неуспешной попытки отправки. Будет сипользовано на следующий итерации.
															  //То есть если на нулевой итеарции не получилось доставить, то rtt будет домножен на нулвой множитель и полученное число будет использоваться как задержка на первой итерации.

		private byte[] _connectAnswerPackage;

		/// <summary>
		/// Активно ли соединение. 
		/// Соединение может быть активно еще в течении некотрого времени после вызова Close, в случае если идёт отправка пакетов, оставшихся в буфере
		/// </summary>
		public bool IsConnected { get; private set; } = false;

		/// <summary>
		/// Если мы израсходуем все попытки отправки этот флаг будет true 
		/// </summary>
		private bool _sendingProblems = false;

		/// <summary>
		/// Было ли закрыто соединение. После вызова Close сразу же принимает значение true
		/// </summary>
		public bool IsClosed { get; private set; } = false;

		public long Ping
		{
			get
			{
				return _rtt / 2;
			}
		}

		public SmpClient(IPEndPoint point)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_socket.Bind(point);

			// TODO: можеть быть вырезать эту хуйню
			var sioUdpConnectionReset = -1744830452;
			var inValue = new byte[] { 0 };
			var outValue = new byte[] { 0 };
			_socket.IOControl(sioUdpConnectionReset, inValue, outValue);
		}

		public bool Connect(IPEndPoint remoteIp, byte[] connectCode)
		{
			var connectionWait = new AutoResetEvent(false);

			_selfSessionId = (byte)(new Random()).Next(0, 0xff);

			_connectAnswerPackage = new byte[connectCode.Length + 3];
			_connectAnswerPackage[0] = PackgeCodes.ConnectAnswer;
			_connectAnswerPackage[1] = (byte)connectCode.Length; //вставлям размер кода подключения. Он не может быть больше 256 байт
			_connectAnswerPackage[_connectAnswerPackage.Length - 1] = _selfSessionId; //вставляем наш id сессии.
			Buffer.BlockCopy(connectCode, 0, _connectAnswerPackage, 2, (byte)connectCode.Length); //копируем код подключения в пакет подключения

			var thread = new Thread(delegate ()
			{
				const int bufferSize = 65536;
				byte[] buffer = new byte[bufferSize];
				bool pointDefined = false;
				EndPoint anyPoint = new IPEndPoint(IPAddress.Any, 0);

				while (!IsConnected)
				{
					try
					{
						int dataLength = _socket.ReceiveFrom(buffer, bufferSize, SocketFlags.None, ref anyPoint);
						IPEndPoint senderPoint = (IPEndPoint)anyPoint;

						if (dataLength > 0)
						{
							if (buffer[0] == PackgeCodes.MtuRequest && buffer.Length > 2) // если это пакет с вычислением mtu - отвечаем на него
							{
								if (pointDefined || remoteIp.Equals(senderPoint))
									_socket.Send(new byte[2] { PackgeCodes.MtuResponse, buffer[1] }, 2, SocketFlags.None);
							}
							else if (buffer[0] == PackgeCodes.PingRequest) // если это пакет пинга то отвечаем на него
							{
								if (pointDefined || remoteIp.Equals(senderPoint))
									_socket.Send(new byte[2] { PackgeCodes.PingResponse, buffer[1] }, 2, SocketFlags.None);
							}
							else if (buffer[0] == PackgeCodes.PingResponse) // если это ответ на пинг, то обрабатываем его
							{
								if (pointDefined)
									PingProcessing(buffer, dataLength);
							}
							else if ((buffer[0] == PackgeCodes.ConnectRequest || buffer[0] == PackgeCodes.ConnectAnswer) && dataLength > 3 && remoteIp.Address.Equals(senderPoint?.Address))
							{
								byte codeSize = buffer[1];
								if (codeSize + 3 == dataLength)
								{
									byte[] recivedCode = new byte[codeSize];
									Buffer.BlockCopy(buffer, 2, recivedCode, 0, codeSize);
									if (connectCode.SequenceEqual(recivedCode))
									{
										if (!pointDefined)
										{
											remoteIp = senderPoint;
											pointDefined = true;
											_hostSessionId = buffer[dataLength - 1]; // устанавливаем id сессии хоста
											Runtime.DebugConsoleWrite("_hostSessionId " + _hostSessionId);
											connectionWait.Set();
										}

										if (buffer[0] == PackgeCodes.ConnectRequest)
											_socket.SendTo(_connectAnswerPackage, 0, _connectAnswerPackage.Length, SocketFlags.None, senderPoint);
									}
								}
							}
						}
					}
					catch { }
				}
			});
			thread.Start();

			//формируем пакет запроса подключения
			byte[] connectPackage = new byte[connectCode.Length + 3];
			connectPackage[0] = PackgeCodes.ConnectRequest;
			connectPackage[1] = (byte)connectCode.Length; //вставлям размер кода подключения. Он не может быть больше 256 байт
			connectPackage[connectPackage.Length - 1] = _selfSessionId; //херачим id для сессии.
			Buffer.BlockCopy(connectCode, 0, connectPackage, 2, (byte)connectCode.Length); //копируем код подключения в пакет подключения

			int i = 0;
			bool successfulConnect = false;
			while (!successfulConnect && i < 20)
			{
				_socket.SendTo(connectPackage, 0, connectPackage.Length, SocketFlags.None, remoteIp);
				i++;
				successfulConnect = connectionWait.WaitOne(200);
			}

			if (!successfulConnect)
			{
				Runtime.DebugConsoleWrite("Point error");
				return false;
			}
			Runtime.DebugConsoleWrite("Point is defined");

			_socket.Connect(remoteIp);

			_rtt = CalculateRTT(); //измеряем rtt
			_rttCalculator = new RttCalculator(_rtt);
			Runtime.DebugConsoleWrite("RTT " + _rtt);

			if (_rtt != -1) // если -1, значит ответные пакеты не дошли. Соединение установить не удалось
			{
				IsConnected = true;
				_point = remoteIp;

				SafeThreadAbort(thread);
				_serviceSend = new Thread(ServiceSend);
				_serviceReceive = new Thread(ServiceReceive);
				_connectionControl = new Thread(ConnectionControl);

				_lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000;

				_serviceReceive.Start();
				_serviceSend.Start();
				_connectionControl.Start();

				ThreadPool.QueueUserWorkItem(delegate (object state)
				{
					_mtu = CalculateMTU(); // измеряем mtu
					if (!IsConnected) return;
					SendMTUInfo(); // отправляем наш mtu хосту
					_calculateMtuBlock.WaitOne();

					if (_hostMtu != -1) // пакет с инфой об mtu хоста уже был получен
					{
						// если mtu хоста меньше, то обновляем наш mtu
						if (_hostMtu < _mtu)
						{
							_mtu = _hostMtu;
						}
					}
					else
					{
						_hostMtu = -2; // устанавливаем -2 чтобы при получении пакета с инфой наш mtu был обновлён
					}
					_calculateMtuBlock.Release();

					if (!IsConnected) return;

					try
					{
						_socket.ReceiveBufferSize = _maxPackagesCount * _mtu;
					}
					catch { }

					Runtime.DebugConsoleWrite("MTU " + _mtu);
				});

				return true;
			}
			else
			{
				return false;
			}
		}

		private int CalculateMTU()
		{
			int thisData = 10;
			int lostData = 1500;

			byte packageId = 0;
			int difference = 1490;
			while (difference > 1 && thisData > 0 && IsConnected)
			{
				difference = lostData - thisData;

				_mtuPackageId = packageId;
				byte[] data = new byte[thisData];
				data[0] = PackgeCodes.MtuRequest;
				data[1] = packageId;

				int j;
				for (j = 0; j < 5 && IsConnected; j++) // пробуем отправить 5 раз
				{
					try
					{
						_socket.DontFragment = true;
						_socket.Send(data, thisData, SocketFlags.None);
						_socket.DontFragment = false;

						if (_mtuWait.WaitOne((int)_rtt * 2) && _mtuPackageId == packageId)
						{
							break;
						}
					}
					catch { }
				}

				if (j == 5 || thisData < 1) // пакет не дошёл 
				{
					// TODO: если первый пакет не дойдёт, то наверное закрывать соединение
					int thisData_ = thisData;
					thisData -= (difference / 2) + (difference % 2);
					lostData = thisData_;
				}
				else // покет дошёл
				{
					thisData += difference / 2;
				}

				packageId++;
			}

			_mtuPackageId = -1;

			return thisData;
		}

		private void SendMTUInfo()
		{
			byte[] payload = BitConverter.GetBytes((ushort)_mtu);
			byte[] data = new byte[3];

			Buffer.BlockCopy(payload, 0, data, 1, 2);
			data[0] = PackgeCodes.MtuInfo;

			for (int j = 0; j < 5 && IsConnected; j++) // пробуем отправить 5 раз
			{
				try
				{
					_socket.Send(data, data.Length, SocketFlags.None);

					if (_mtuInfoWait.WaitOne((int)_rtt))
					{
						break;
					}
				}
				catch { }
			}
		}

		private long CalculateRTT()
		{
			long rttSum = 0;

			try
			{
				byte i = 0;
				for (int j = 0; j < 5; j++) // сий процесс повторяем 5 раз
				{
					_workPing = true;

					bool successful = false;
					while (!successful && i < 20)
					{
						_times[i] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						Runtime.DebugConsoleWrite("SEND");
						_socket.Send(new byte[2] { PackgeCodes.PingRequest, i }, 2, SocketFlags.None);
						i++;

						successful = _pingWait.WaitOne(200);
					}

					if (!successful)
					{
						return -1;
					}

					rttSum += _pingPackagesDelay;
				}
			}
			catch
			{
				return -1;
			}

			Runtime.DebugConsoleWrite("RTT " + ((rttSum / 5) + 1));

			// вычиляем среднее значение и возвращаем его
			return (rttSum / 5) + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PingProcessing(byte[] data, int dataLength)
		{
			if (dataLength == 2 && data[1] < 21 && _workPing)
			{
				_pingPackagesDelay = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _times[data[1]]; //вчитаем из данного времени время отправки пакета
				_workPing = false;
				_pingWait.Set();
			}
		}

		/// <summary>
		/// Метод работающий всегда, поддерживает соединение, если данные не отправляются 10 и более секунд
		/// </summary>
		protected void ConnectionControl()
		{ // TODO: потом на сервере этот метод как-то занести в один поток для всех клиентов
			while (IsConnected)
			{
				if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastTime >= 10000) //проверяем что последний пакет был отправлен более 2 секунд назад
				{
					if (CalculateRTT() == -1) //проверяем ответил ли хост
					{
						Runtime.DebugConsoleWrite("Connection is dead");
						ThreadPool.QueueUserWorkItem(delegate (object state)
						{
							StopWork();
							ClientClosing?.Invoke(_point);
						});
					}
				}

				Thread.Sleep(10000);
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FormingSendDatagrams(out ushort lastPackageId, out byte lastPackageIdsRange, out SortedDictionary<ushort, byte[]> datagrams)
		{
			// ждём появления сообщений в буфере
			while (_sendingBuffer.Count == 0)
			{
				_waitSendData.WaitOne();
			}

			lastPackageIdsRange = _sendingIdsRange;
			bool acquiredLock = false;

			try
			{
				Monitor.Enter(_sendLocker, ref acquiredLock);

				datagrams = new();
				_sendingBuffer.TryPeek(out List<Package> packagesHeap); // получаем кучу пакетов

				int lastPackageId_ = packagesHeap.Count + _sendingPointer - 1;
				lastPackageId = (ushort)(lastPackageId_ > 65535 ? 65535 : lastPackageId_); // id последнего пакета в этом сегменте отправки
				int i = 0;
				// проходимся по всем пакетам
				foreach (Package packageInfo in packagesHeap)
				{
					byte[] package = new byte[packageInfo.Size];

					byte[] id = BitConverter.GetBytes(_sendingPointer);
					package[DataPackageHeaders.Code] = PackgeCodes.Data; //код пакета
					package[DataPackageHeaders.Id_1] = id[0]; //первая часть его айдишника
					package[DataPackageHeaders.Id_2] = id[1]; //вторая
					package[DataPackageHeaders.IdsRange] = _sendingIdsRange;

					id = BitConverter.GetBytes(lastPackageId);
					package[DataPackageHeaders.LastId_1] = id[0]; // первая часть id последнего пакета
					package[DataPackageHeaders.LastId_2] = id[1]; // вторая часть
					package[DataPackageHeaders.AttemptsCounts] = 0; // этот байт отвечает за номер попытки отправки

					int offset = DataPackageHeaders.FirstDataByte;
					int lastFlagIndex = 0;
					// проходимся по каждому сегменту
					foreach (byte[] payload in packageInfo.Segments)
					{
						package[offset] = DataFlags.None; // это флаг данного сегмента данных. 0 - значит нихуя не делать
						lastFlagIndex = offset;

						int payloadSize = payload.Length;
						byte[] size = BitConverter.GetBytes(payloadSize);
						package[offset + 1] = size[0]; // первая часть размера сегмента данных
						package[offset + 2] = size[1]; // вторая часть
						offset += 3;

						Buffer.BlockCopy(payload, 0, package, offset, payloadSize);
						offset += payloadSize;
					}

					if (!packageInfo.lastSegmentIsFull) // последний сегмент данных не полный и надо поставить флаг что его необходимо склеить
					{
						package[lastFlagIndex] = DataFlags.NotFull;
					}

					datagrams[_sendingPointer] = package;
					_sendingPointer++;
					i++;

					if (_sendingPointer == 0)
					{
						_sendingIdsRange++;
						break;
					}
				}

				if (packagesHeap.Count == i) // если все пакеты из кучи были поставлены на отправку, то убираем эту кучу из буфера
				{
					_sendingBuffer.TryDequeue(out _);
				}
				else // если нет, то тогда убираем из кучи поставленные на отправку пакеты. Оставшиеся пакеты отправим на следующей итерации
				{
					for (int j = 0; j < i; j++)
					{
						packagesHeap.RemoveAt(0);
					}
				}
			}
			finally
			{
				if (acquiredLock) Monitor.Exit(_sendLocker);
			}
		}

		private void ServiceSend()
		{
			while (IsConnected)
			{
				try
				{
					_sendingCycleDetector.Reset();

					FormingSendDatagrams(out ushort lastPackageId, out byte lastPackageIdsRange, out SortedDictionary<ushort, byte[]> datagrams);

					_repeatDeliveryBlock.WaitOne();
					_lastPackage = lastPackageId;
					_lastPackageIdsRange = lastPackageIdsRange;
					_sendigIsConfirmed = false;
					_deliveryWait.Reset();
					_repeatDeliveryBlock.Release();

					byte attemptCount = 0;
					int delay = (int)(_rtt/* + _rtt / 10*/);
					long lastTime = 0;
					bool repeated = false;
					bool isTimeout = false;

					// цикл отправки
					while (IsConnected && attemptCount < 15)
					{
#if DEBUG
						if (attemptCount > 0)
						{
							Runtime.DebugConsoleWrite("AXAXAXAXAXAX " + attemptCount + " " + lastPackageId + ", RTT " + _rtt + ", packages count: " + datagrams.Count + ", delay " + delay);
						}
#endif
						if (!isTimeout)
						{
							foreach (ushort id in datagrams.Keys)
							{
								datagrams[id][DataPackageHeaders.AttemptsCounts] = attemptCount; // увставляем номер попытки
								_socket.Send(datagrams[id], datagrams[id].Length, SocketFlags.None);
							}
						}
						else // если в прошлый раз был тймаут, то отправляем только псоледний пакет. Хост потом просто отправит список нехватающих пакетов
						{
							ushort id = datagrams.Keys.Last();
							datagrams[id][DataPackageHeaders.AttemptsCounts] = attemptCount; // увставляем номер попытки
							_socket.Send(datagrams[id], datagrams[id].Length, SocketFlags.None);
						}

						_lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
						if (attemptCount == 0 || repeated)
						{
							lastTime = _lastTime;
							repeated = false;
						}

					Begin:
						if (!_deliveryWait.WaitOne(delay)) // истекло время ожидания
						{
							delay *= _delayMultipliers[attemptCount];
							attemptCount++;
							isTimeout = true;
						}
						else // либо пришло подтверждение доставки, либо пришел запрос на повторную доставку
						{
							isTimeout = false;
							_repeatDeliveryBlock.WaitOne();
							if (_sendigIsConfirmed) // пакеты удачно доставлены
							{
								//рассчитываем задержку
								long deltaTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTime;
								_rttCalculator.AddDelta(deltaTime);
								_rtt = _rttCalculator.GetRtt;

								_repeatDeliveryBlock.Release();
								break;
							}
							else // хост просит повторить отправку некоторых пакетов
							{
								// оставляем в списке только те айдишники, которые надо повторить
								var datagrams_ = new SortedDictionary<ushort, byte[]>();
								bool isValid = true;
								ushort maxId = 0;
								foreach (ushort repeatId in _repeatDeliveryList)
								{
									if (datagrams.ContainsKey(repeatId))
									{
										maxId = repeatId;
										datagrams_[repeatId] = datagrams[repeatId];
									}
									else
									{
										isValid = false;
										break;
									}
								}

								if (isValid)
								{
									datagrams_[maxId][DataPackageHeaders.Flag] = Flags.NeedConfirm;

									repeated = true;
									datagrams = datagrams_;

									_repeatDeliveryBlock.Release();
								}
								else
								{
									_repeatDeliveryBlock.Release();
									goto Begin;
								}
							}
						}
					}

					_lastPackage = -1;
					_lastPackageIdsRange = -1;

					if (attemptCount == 15)
					{
						Runtime.DebugConsoleWrite("PIZDETS!!!!");
						_sendingProblems = true;
						ThreadPool.QueueUserWorkItem(delegate (object state)
						{
							Close();
							ClientClosing?.Invoke(_point);
						});

						return;
					}
				}
				catch
				{
					break;
				}
				finally
				{
					_sendingCycleDetector.Set();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddToReceivingBuffer(byte[] buffer, int packageLength)
		{
			int offset = DataPackageHeaders.FirstDataByte;
			while (offset < packageLength - 3)
			{
				byte flag = buffer[offset];
				ushort size = BitConverter.ToUInt16(new byte[2] { buffer[offset + 1], buffer[offset + 2] }, 0);
				offset += 3;

				byte[] payload = new byte[size];
				Buffer.BlockCopy(buffer, offset, payload, 0, size);
				offset += size;

				bool isFull = (flag == DataFlags.None);

				_receivingBuffer.Enqueue(new Message
				{
					data = payload,
					IsFull = isFull
				});

				if (isFull)
				{
					MessageReceived?.Invoke();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SendConfirmPackage(ushort lastId, byte receivingIdsRange)
		{
			byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);

			byte[] confirmPackage = new byte[ConfirmDataDeliveryHeaders.HeadersSize];
			confirmPackage[ConfirmDataDeliveryHeaders.Code] = PackgeCodes.ConfirmDataDelivery;
			confirmPackage[ConfirmDataDeliveryHeaders.IdsRange] = receivingIdsRange;
			confirmPackage[ConfirmDataDeliveryHeaders.LastId_1] = neEbyKakNazvat[0];
			confirmPackage[ConfirmDataDeliveryHeaders.LastId_2] = neEbyKakNazvat[1];
			_socket.Send(confirmPackage, ConfirmDataDeliveryHeaders.HeadersSize, SocketFlags.None);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DataPackcageProcessing(byte[] buffer, int dataLength, ref int waitingLastPackage, ref int attemptSendCounts, ref ushort lastMissedPacket, ref short lastIdRange)
		{
			if (dataLength <= DataPackageHeaders.FirstDataByte) return;

			ushort id = BitConverter.ToUInt16(new byte[2]
			{
				buffer[DataPackageHeaders.Id_1],
				buffer[DataPackageHeaders.Id_2]
			}, 0);

			byte packageIdRange = buffer[DataPackageHeaders.IdsRange];

			ushort lastId = BitConverter.ToUInt16(new byte[2]
			{
				buffer[DataPackageHeaders.LastId_1],
				buffer[DataPackageHeaders.LastId_2]
			}, 0);

			if (id >= _receivingPointer && packageIdRange == _receivingIdsRange)
			{
				lastIdRange = _receivingIdsRange;
				waitingLastPackage = lastId;
				byte receivingIdsRange = _receivingIdsRange;

				if (id == _receivingPointer)
				{
					AddToReceivingBuffer(buffer, dataLength);
					_packagesBuffer.TryRemove(_receivingPointer, out _);
					_receiveWait.Set();

					_receivingPointer++;
					if (_receivingPointer == 0) _receivingIdsRange++;
				}
				else
				{
					byte[] package = new byte[dataLength];
					Buffer.BlockCopy(buffer, 0, package, 0, dataLength);

					_packagesBuffer[id] = package;
				}

				// проходимся по буферу в поисках пакетов, которые мы уже получили. Если пакет найден - пихаем  в буфер
				while (_packagesBuffer.ContainsKey(_receivingPointer))
				{
					byte[] package = _packagesBuffer[_receivingPointer];
					AddToReceivingBuffer(package, package.Length);
					_packagesBuffer.TryRemove(_receivingPointer, out _);
					_receiveWait.Set();

					_receivingPointer++;
					if (_receivingPointer == 0)
					{
						_receivingIdsRange++;
						break;
					}
				}

				// проверяем все ли пакеты были получены
				if (_receivingPointer == (ushort)(lastId + 1))
				{
					SendConfirmPackage(lastId, receivingIdsRange);

					attemptSendCounts = -1;
					waitingLastPackage = -1;
				}
				else if (_receivingPointer != (ushort)(id + 1))
				{
					var package = new List<byte>
					{
						PackgeCodes.FailedList,
						receivingIdsRange,
						buffer[DataPackageHeaders.LastId_1],
						buffer[DataPackageHeaders.LastId_2]
					};

					bool flag = true;
					bool needRepeat = true;
					for (int i = _receivingPointer; i <= lastId; i++)
					{
						if (!_packagesBuffer.ContainsKey((ushort)i))
						{
							if (i == lastMissedPacket && buffer[DataPackageHeaders.AttemptsCounts] <= attemptSendCounts)
							{
								needRepeat = false;
								break;
							}

							if (flag)
							{
								lastMissedPacket = (ushort)i;
								flag = false;
							}

							byte[] packageId = BitConverter.GetBytes((ushort)i);
							package.Add(packageId[0]);
							package.Add(packageId[1]);
						}
					}

					if (needRepeat)
					{
						byte[] array = package.ToArray();
						_socket.Send(array, array.Length, SocketFlags.None);
						for (int h = 3; h < array.Length - 1; h += 2)
						{
							var idg = BitConverter.ToUInt16(new byte[2]
							{
								array[h],
								array[h + 1]
							}, 0);
						}
					}

					attemptSendCounts = buffer[DataPackageHeaders.AttemptsCounts];
				}
			}
			else if (packageIdRange == _receivingIdsRange)
			{
				if (waitingLastPackage == lastId && buffer[DataPackageHeaders.AttemptsCounts] > attemptSendCounts)
				{
					// TODO: это оптимизировать. Я по сути впустую формирую failedList
					var package = new List<byte>
					{
						PackgeCodes.FailedList,
						_receivingIdsRange,
						buffer[DataPackageHeaders.LastId_1],
						buffer[DataPackageHeaders.LastId_2]
					};

					bool needRepeat = false;
					for (int i = _receivingPointer; i <= lastId; i++)
					{
						if (!_packagesBuffer.ContainsKey((ushort)i))
						{
							needRepeat = true;
							byte[] packageId = BitConverter.GetBytes((ushort)i);
							package.Add(packageId[0]);
							package.Add(packageId[1]);
						}
					}

					if (needRepeat)
					{
						byte[] array = package.ToArray();
						_socket.Send(array, array.Length, SocketFlags.None);
					}
					else
					{
						SendConfirmPackage(lastId, _receivingIdsRange);
					}

					attemptSendCounts = buffer[DataPackageHeaders.AttemptsCounts];
				}
				else if ((id == lastId || buffer[DataPackageHeaders.Flag] == Flags.NeedConfirm) && lastId != waitingLastPackage)
				{
					SendConfirmPackage(lastId, _receivingIdsRange);
				}
			}
			else if (packageIdRange == lastIdRange && (id == lastId || buffer[DataPackageHeaders.Flag] == Flags.NeedConfirm))
			{
				SendConfirmPackage(lastId, packageIdRange);
			}
		}

		private void ServiceReceive()
		{
			int waitingLastPackage = -1;
			int attemptSendCounts = -1;
			ushort lastMissedPacket = 0;
			short lastIdRange = -1;

			try
			{
				const int bufferSize = 65536;
				byte[] buffer = new byte[bufferSize];

				while (IsConnected)
				{
					int dataLength = _socket.Receive(buffer, bufferSize, SocketFlags.None);

					if (dataLength < 1) continue;

					_lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

					switch (buffer[0])
					{
						case PackgeCodes.ConnectRequest:
							//здесь уже не проверяем код подключения, ведь он был уже проверен в методе connect и ip отправителя зафиксирован
							if (dataLength > 2)
							{
								_socket.Send(_connectAnswerPackage, _connectAnswerPackage.Length, SocketFlags.None);
							}
							break;
						case PackgeCodes.MtuRequest: // пришел пакет с вычислением mtu
							if (dataLength > 2)
							{
								_socket.Send(new byte[2] { PackgeCodes.MtuResponse, buffer[1] }, 2, SocketFlags.None);
							}
							break;
						case PackgeCodes.PingRequest: //пришел пакет с пингом
							if (dataLength == 2)
							{
								_socket.Send(new byte[2] { PackgeCodes.PingResponse, buffer[1] }, 2, SocketFlags.None);
							}
							break;
						case PackgeCodes.PingResponse: //пришел ответ на пинг
							PingProcessing(buffer, dataLength);
							break;
						case PackgeCodes.Data: //пришел пакет данных
							DataPackcageProcessing(buffer, dataLength, ref waitingLastPackage, ref attemptSendCounts, ref lastMissedPacket, ref lastIdRange);
							break;
						case PackgeCodes.ConfirmDataDelivery: // пришло подтверждение доставки пакета
							if (dataLength == ConfirmDataDeliveryHeaders.HeadersSize)
							{
								ushort id = BitConverter.ToUInt16(new byte[2]
								{
									buffer[ConfirmDataDeliveryHeaders.LastId_1],
									buffer[ConfirmDataDeliveryHeaders.LastId_2]
								}, 0);

								byte packageIdRange = buffer[ConfirmDataDeliveryHeaders.IdsRange];

								_repeatDeliveryBlock.WaitOne();
								if (id == _lastPackage && packageIdRange == _lastPackageIdsRange)
								{
									_sendigIsConfirmed = true;
									_deliveryWait.Set();
								}
								_repeatDeliveryBlock.Release();
							}
							break;
						case PackgeCodes.ConnectionClose: // обрыв соединения
							Runtime.DebugConsoleWrite("StopWork!!!!");
							if (dataLength == 2 && buffer[1] == _hostSessionId)
							{
								Runtime.DebugConsoleWrite("StopWork, _hostSessionId: " + _hostSessionId);
								ThreadPool.QueueUserWorkItem(delegate (object state)
								{
									StopWork();
									ClientClosing?.Invoke(_point);
								});
							}
							break;
						case PackgeCodes.FailedList: // пришел пакет со списком пакетов, которые нужно переотправить
													 //проверяем валидность этого пакета. пакет должен содержать список айдишников. каждый id занимает 2 байта. первый байт - код. то есть размер должен быть нечетным
							if (dataLength > 5 && ((dataLength & 1) == 0))
							{
								_repeatDeliveryBlock.WaitOne();
								ushort packageId = BitConverter.ToUInt16(new byte[2] { buffer[FailedListHeaders.LastId_1], buffer[FailedListHeaders.LastId_2] }, 0);
								byte packageIdRange = buffer[FailedListHeaders.IdsRange];
								if (packageId == _lastPackage && packageIdRange == _lastPackageIdsRange) // проверяем не старый ли это запрос на повторную отправку
								{
									List<ushort> ids = new List<ushort>();
									int i = FailedListHeaders.HeadersSize;
									while (i < dataLength)
									{
										ushort id = BitConverter.ToUInt16(new byte[2] { buffer[i], buffer[i + 1] }, 0);
										ids.Add(id);
										i += 2;
									}

									_repeatDeliveryList = ids;
									_deliveryWait.Set();
								}
								_repeatDeliveryBlock.Release();
							}
							break;
						case PackgeCodes.MtuResponse: // пришел ответ на вычисление mtu
							if (dataLength == 2)
							{
								_mtuPackageId = buffer[1];
								_mtuWait.Set();
							}
							break;
						case PackgeCodes.MtuInfo: // пришёл пакет с инфой об mtu
							if (dataLength == 3)
							{
								ushort hostMtu_ = BitConverter.ToUInt16(new byte[2] { buffer[1], buffer[2] }, 0);
								_socket.Send(new byte[1] { PackgeCodes.MtuInfoConfirm }, 1, SocketFlags.None);

								_calculateMtuBlock.WaitOne();
								if (_hostMtu == -1) // метод Connect ещё не отработал
								{
									_hostMtu = hostMtu_;
								}
								else // connect уже отработал, можно обновлять mtu
								{
									// если mtu, отправленный хостом меньше, который вычислили мы, то обновляем его
									if (hostMtu_ < _mtu)
									{
										_mtu = hostMtu_;
									}
								}
								_calculateMtuBlock.Release();
							}
							break;
						case PackgeCodes.MtuInfoConfirm: // пришёл ответ на пакет с инфой об mtu
							_mtuInfoWait.Set();
							break;

					}
				}
			}
			catch { }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DistributeByPackcages(byte[] inputData, ref List<Package> packagesHeap, int mtu, int maxPackagesCount)
		{
			const int serviceDataLenght = DataPackageHeaders.FirstDataByte + 3;

			if (inputData.Length + serviceDataLenght <= mtu)
			{
				// inputData вмещается в один пакет, создаем пакет и вставляем его в кучу пакетов
				Package package = new Package
				{
					Size = inputData.Length + serviceDataLenght
				};
				package.Segments.Add(inputData);
				packagesHeap.Add(package);
			}
			else
			{
				// inputData не вмещается в один пакет, разбиваем на сегменты и рассовываем в несколько пакетов
				int offset = 0;
				while (offset < inputData.Length)
				{
					int lenght = (inputData.Length - offset) > (mtu - serviceDataLenght) ? mtu - serviceDataLenght : inputData.Length - offset;
					byte[] part = new byte[lenght];
					Buffer.BlockCopy(inputData, offset, part, 0, lenght);

					Package package = new Package
					{
						Size = lenght + serviceDataLenght,
						lastSegmentIsFull = false
					};
					package.Segments.Add(part);

					if (packagesHeap.Count >= maxPackagesCount)
					{
						packagesHeap = new List<Package>();
						_sendingBuffer.Enqueue(packagesHeap);
					}

					packagesHeap.Add(package);

					offset += lenght;
				}

				packagesHeap[packagesHeap.Count - 1].lastSegmentIsFull = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DistributeBySegments(Package package, byte[] inputData, ref List<Package> packagesHeap, int mtu, int maxPackagesCount)
		{
			int freeSpace = mtu - package.Size - 3; // оставшееся место в пакете
			if (freeSpace > 0)
			{
				// в пакете есть место, часть inputData пихаем в этот пакет, а часть в новые пакеты
				byte[] part = new byte[freeSpace];
				Buffer.BlockCopy(inputData, 0, part, 0, freeSpace);

				package.Segments.Add(part);
				package.Size += freeSpace + 3;
				package.lastSegmentIsFull = false;

				int partSize = inputData.Length - freeSpace;
				part = new byte[partSize];
				Buffer.BlockCopy(inputData, freeSpace, part, 0, partSize);

				DistributeByPackcages(part, ref packagesHeap, mtu, maxPackagesCount);
			}
			else
			{
				// в пакете места нет, пихаем данные в новые пакеты
				DistributeByPackcages(inputData, ref packagesHeap, mtu, maxPackagesCount);
			}
		}

		public void Send(byte[] inputData)
		{
			if (_inStopping || !IsConnected) return;
			Begin:
			bool acquiredLock = false;
			Monitor.Enter(_sendLocker, ref acquiredLock);

			int mtu = _mtu;
			int maxPackagesCount = _maxPackagesCount;

			List<Package> packagesHeap;
			if (_sendingBuffer.Count > 0)
			{
				if (_sendingBuffer.Count > 1)
				{
					if (acquiredLock) Monitor.Exit(_sendLocker);
					_sendingCycleDetector.WaitOne();

					if (_inStopping || !IsConnected) return;
					goto Begin;
				}

				_sendingBuffer.TryPeek(out packagesHeap);
			}
			else
			{
				packagesHeap = new List<Package>();
				_sendingBuffer.Enqueue(packagesHeap);
			}

			if (packagesHeap.Count > maxPackagesCount)
			{
				if (acquiredLock) Monitor.Exit(_sendLocker);
				_sendingCycleDetector.WaitOne();
				goto Begin;
			}

			if (packagesHeap.Count > 0)
			{
				Package lastElement = packagesHeap[packagesHeap.Count - 1];
				if (lastElement.Size + inputData.Length + 3 > mtu)
				{
					if (packagesHeap.Count >= maxPackagesCount)
					{
						if (acquiredLock) Monitor.Exit(_sendLocker);
						_sendingCycleDetector.WaitOne();

						if (_inStopping || !IsConnected) return;
						goto Begin;
					}

					DistributeBySegments(lastElement, inputData, ref packagesHeap, mtu, maxPackagesCount);

					_waitSendData.Set();
					if (acquiredLock) Monitor.Exit(_sendLocker);
					return;
				}

				lastElement.Segments.Add(inputData);
				lastElement.Size += inputData.Length + 3;
			}
			else
			{
				DistributeByPackcages(inputData, ref packagesHeap, mtu, maxPackagesCount);
			}

			_waitSendData.Set();
			if (acquiredLock) Monitor.Exit(_sendLocker);
		}

		public bool TrySend(byte[] inputData)
		{
			if (_inStopping || !IsConnected) return false;

			lock (_sendLocker)
			{
				int mtu = _mtu;
				int maxPackagesCount = _maxPackagesCount;

				List<Package> packagesHeap;
				if (_sendingBuffer.Count > 0)
				{
					if (_sendingBuffer.Count > 1) return false;
					_sendingBuffer.TryPeek(out packagesHeap);
				}
				else
				{
					packagesHeap = new List<Package>();
					_sendingBuffer.Enqueue(packagesHeap);
				}

				if (packagesHeap.Count > maxPackagesCount) return false;

				if (packagesHeap.Count > 0)
				{
					Package lastElement = packagesHeap[packagesHeap.Count - 1];
					if (lastElement.Size + inputData.Length + 3 > mtu)
					{
						if (packagesHeap.Count >= maxPackagesCount) return false;
						DistributeBySegments(lastElement, inputData, ref packagesHeap, mtu, maxPackagesCount);

						_waitSendData.Set();
						return true;
					}

					lastElement.Segments.Add(inputData);
					lastElement.Size += inputData.Length + 3;
				}
				else
				{
					DistributeByPackcages(inputData, ref packagesHeap, mtu, maxPackagesCount);
				}

				_waitSendData.Set();
				return true;
			}
		}

		private void FormingMessage(out byte[] data)
		{
			List<byte[]> buffer = new List<byte[]>();
			int messageSize = 0;

			_receivingBuffer.TryDequeue(out Message segment);
			buffer.Add(segment.data);
			messageSize += segment.data.Length;

			while (!segment.IsFull && (IsConnected || _receivingBuffer.Count > 0))
			{
				if (_receivingBuffer.Count > 0)
				{
					_receivingBuffer.TryDequeue(out segment);
					buffer.Add(segment.data);
					messageSize += segment.data.Length;
				}
				else
				{
					_receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты
				}
			}

			data = new byte[messageSize];
			int offset = 0;
			foreach (byte[] segmentBytes in buffer)
			{
				int len = segmentBytes.Length;
				Buffer.BlockCopy(segmentBytes, 0, data, offset, len);
				offset += len;
			}
		}

		public bool Receive(out byte[] data)
		{
			if (_receivingBuffer.Count > 0)
			{
				FormingMessage(out data);
				return true;
			}
			else //буфер пуст
			{
				while (IsConnected)
				{
					_receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

					if (_receivingBuffer.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
					{
						FormingMessage(out data);
						return true;
					}
				}
			}

			Runtime.DebugConsoleWrite("SMP CLIENT STOP WORK");
			data = Array.Empty<byte>();
			return false;
		}

		private void StopWork()
		{
			Runtime.DebugConsoleWrite("StopWork() SMP CLIENT");
			IsClosed = true;
			IsConnected = false;
			SafeThreadAbort(_connectionControl);
			//serviceReceive.Abort();
			SafeThreadAbort(_serviceSend);
			try
			{
				_socket.Close();
			}
			catch { }
			_receiveWait.Set();
		}

		private void SafeThreadAbort(Thread thread)
		{
			try
			{
				thread.Abort();
			}
			catch (Exception ex)
			{
				Runtime.DebugConsoleWrite("Exception " + ex);
			}
		}

		public void Close()
		{
			IsClosed = true;
			ThreadPool.QueueUserWorkItem((_) =>
			{
				lock (_closeLocker)
				{
					if (!IsConnected) return;

					try
					{
						bool acquiredLock = false;
						Monitor.Enter(_sendLocker, ref acquiredLock);
						_inStopping = true; // ставим флаг чтобы send нельзя было вызвать ещё раз
						if (acquiredLock) Monitor.Exit(_sendLocker);

						while (!_sendingProblems && _sendingBuffer.Count != 0) // ждём когда все пакеты из буфера будут доставлены
						{
							_sendingCycleDetector.WaitOne();
						}

						Runtime.DebugConsoleWrite("_sendingBuffer.Count " + _sendingBuffer.Count);
					}
					catch
					{
						return;
					}

					try
					{
						for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
						{
							_socket.Send(new byte[2] { PackgeCodes.ConnectionClose, _selfSessionId }, 2, SocketFlags.None);
						}
					}
					catch { }

					StopWork();
				}
			});
		}

		public event PointHandle ClientClosing;
	}
}