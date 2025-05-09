using System.Collections.Generic;

namespace Lexplosion.Tools
{
	/// <summary>
	/// Реализация убогого хэша, используемого курсфорджем для генерации фингерпринтов файлов
	/// </summary>
	static class StupidHash
	{
		private const uint m = 0x5bd1e995;
		private const int r = 24;

		private static uint Hash(byte[] data, uint seed, int length)
		{
			if (length == 0)
				return 0;

			uint h = seed ^ (uint)length;
			int currentIndex = 0;
			while (length >= 4)
			{
				uint k = (uint)(data[currentIndex++] | data[currentIndex++] << 8 | data[currentIndex++] << 16 | data[currentIndex++] << 24);
				k *= m;
				k ^= k >> r;
				k *= m;

				h *= m;
				h ^= k;
				length -= 4;
			}

			switch (length)
			{
				case 3:
					h ^= (ushort)(data[currentIndex++] | data[currentIndex++] << 8);
					h ^= (uint)(data[currentIndex] << 16);
					h *= m;
					break;
				case 2:
					h ^= (ushort)(data[currentIndex++] | data[currentIndex] << 8);
					h *= m;
					break;
				case 1:
					h ^= data[currentIndex];
					h *= m;
					break;
			}

			h ^= h >> 13;
			h *= m;
			h ^= h >> 15;

			return h;
		}

		public static uint Compute(byte[] array)
		{
			List<byte> normalArray = new();

			for (int i = 0; i < array.Length; i++)
			{
				byte b = array[i];

				if (!(b == 9 || b == 10 || b == 13 || b == 32))
				{
					normalArray.Add(b);
				}
			}

			return Hash(normalArray.ToArray(), 1, normalArray.Count);
		}
	}
}
