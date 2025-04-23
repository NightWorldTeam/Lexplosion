using System.Collections.Generic;

namespace Lexplosion.Logic.Network.SMP
{
	partial class SmpClient : IClientTransmitter
	{
		private struct DataFlags
		{
			public const byte None = 0;
			public const byte NotFull = 1;
		}

		private struct Flags
		{
			public const byte None = 0;
			public const byte NeedConfirm = 1;
		}

		/// <summary>
		/// Позиции в заголовке пакета данных.
		/// </summary>
		private struct DataPackageHeaders
		{
			public const byte Code = 0; // Код пакета
			public const byte Id_1 = 1; // Первая часть айдишника
			public const byte Id_2 = 2; // Вторая часть
			public const byte IdsRange = 3; // номер пространсва айдишников. Можно сказать второй айди
			public const byte Flag = 4; // Флаг пакета
			public const byte LastId_1 = 5; // id последнего пакета в данном окне. Первая часть
			public const byte LastId_2 = 6; // Вторая часть
			public const byte AttemptsCounts = 7; // Количество попыток отправки данного пакета

			public const byte FirstDataByte = AttemptsCounts + 1; // Позиция первого байта вне заголовка и соотвественно размер ебучего хэадера
		}

		private struct FailedListHeaders
		{
			public const byte Code = 0; // Код пакета
			public const byte IdsRange = 1; // номер пространсва айдишников. Можно сказать второй айди
			public const byte LastId_1 = 2; // id последнего пакета в данном окне. Первая часть
			public const byte LastId_2 = 3; // Вторая часть

			public const byte HeadersSize = LastId_2 + 1;
		}

		private struct ConfirmDataDeliveryHeaders
		{
			public const byte Code = 0; // Код пакета
			public const byte IdsRange = 1; // номер пространсва айдишников. Можно сказать второй айди
			public const byte LastId_1 = 2; // id последнего пакета в данном окне. Первая часть
			public const byte LastId_2 = 3; // Вторая часть

			public const byte HeadersSize = LastId_2 + 1;
		}

		private struct PackgeCodes
		{
			public const byte MtuRequest = 0x00;
			public const byte PingRequest = 0x01;
			public const byte PingResponse = 0x02;
			public const byte Data = 0x03;
			public const byte ConfirmDataDelivery = 0x04;
			public const byte ConnectionClose = 0x05;
			public const byte FailedList = 0x06;
			public const byte MtuResponse = 0x07;
			public const byte MtuInfo = 0x08;
			public const byte MtuInfoConfirm = 0x09;
			public const byte ConnectRequest = 0x0a;
			public const byte ConnectAnswer = 0x0b;
		}

		private class Package
		{
			public List<byte[]> Segments = new List<byte[]>();
			public int Size;
			public bool lastSegmentIsFull = true;
		}

		public class Message
		{
			public byte[] data;
			public bool IsFull;
			public long offset;
		}

		private struct RttCalculator
		{
			private const int DeltesCount = 50;
			private long[] _deltes;
			private int _lastElement = 0;
			private long _rtt;
			private long _deltesSum;

			private int _maxDeltaIndex = 0;
			private int _minDeltaIndex = 1;

			public RttCalculator(long firstRtt)
			{
				_deltes = new long[DeltesCount];

				for (int i = 0; i < DeltesCount; i++)
				{
					_deltes[i] = firstRtt;
				}

				_deltesSum = firstRtt * DeltesCount;
				_rtt = firstRtt;
			}

			public void AddDelta(long delta)
			{
				_deltesSum = _deltesSum + delta - _deltes[_lastElement]; // обновляем сумму всех значений: прибавляем новое, и вичитаем старое (то, что будет заменено новым)
				double average = (double)_deltesSum / DeltesCount;

				if (delta >= _deltes[_maxDeltaIndex])
				{
					_maxDeltaIndex = _lastElement;
				}
				else if (_maxDeltaIndex == _lastElement)
				{
					long maxValue = 0;
					for (int i = _lastElement + 1; i < DeltesCount; i++)
					{
						if (_deltes[i] > maxValue)
						{
							maxValue = _deltes[i];
							_maxDeltaIndex = i;
						}
					}
				}

				if (delta <= _deltes[_minDeltaIndex])
				{
					_minDeltaIndex = _lastElement;
				}
				else if (_minDeltaIndex == _lastElement)
				{
					long minValue = 0;
					for (int i = _lastElement + 1; i < DeltesCount; i++)
					{
						if (_deltes[i] < minValue)
						{
							minValue = _deltes[i];
							_minDeltaIndex = i;
						}
					}
				}

				_deltes[_lastElement] = delta;
				_lastElement++;
				if (_lastElement == DeltesCount)
				{
					_lastElement = 0;
				}

				_rtt = (long)(_deltes[_maxDeltaIndex] - _deltes[_minDeltaIndex] + average);
			}

			public long GetRtt
			{
				get
				{
					if (_rtt < 1)
					{
						return 1;
					}

					return _rtt;
				}
			}
		}
	}
}
