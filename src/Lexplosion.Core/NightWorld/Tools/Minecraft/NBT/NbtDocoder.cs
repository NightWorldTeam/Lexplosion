using System;
using System.Collections.Generic;
using System.Text;

namespace NightWorld.Tools.Minecraft.NBT
{
	class NbtDocoder
	{
		private int _nextIndex = 0;

		public INbtNode Load(byte[] data)
		{
			string name = ParseTagHead(data, out NbtTagType tagType);

			switch (tagType)
			{
				case NbtTagType.Byte:
					{
						byte value = ParseByteTagBody(data);
						return new NbtByte(name, value);
					}
				case NbtTagType.Short:
					{
						short value = ParseShortTagBody(data);
						return new NbtShort(name, value);
					}
				case NbtTagType.Int:
					{
						int value = ParseIntTagBody(data);
						return new NbtInt(name, value);
					}
				case NbtTagType.Long:
					{
						long value = ParseLongTagBody(data);
						return new NbtLong(name, value);
					}
				case NbtTagType.Float:
					{
						float value = ParseFloatTagBody(data);
						return new NbtFloat(name, value);
					}
				case NbtTagType.Double:
					{
						double value = ParseDoubleTagBody(data);
						return new NbtDouble(name, value);
					}
				case NbtTagType.String:
					{
						string value = ParseStringTagBody(data);
						return new NbtString(name, value);
					}
				case NbtTagType.List:
					{
						List<INbtNode> value = ParseListTagBody(data);
						return new NbtList(name, value, tagType);
					}
				case NbtTagType.Compound:
					{
						Dictionary<string, INbtNode> value = ParseCompoundTagBody(data);
						return new NbtCompound(name, value);
					}
				case NbtTagType.ByteArray:
					{
						byte[] value = ParseByteArrayTagBody(data);
						return new NbtByteArray(name, value);
					}
				case NbtTagType.IntArray:
					{
						int[] value = ParseIntArrayTagBody(data);
						return new NbtIntArray(name, value);
					}
				case NbtTagType.LongArray:
					{
						long[] value = ParseLongArrayTagBody(data);
						return new NbtLongArray(name, value);
					}
				default:
					return null;

			}
		}

		private string ParseTagHead(byte[] buffer, out NbtTagType tagType)
		{
			if (_nextIndex > buffer.Length - 1)
			{
				tagType = NbtTagType.End;
				return string.Empty;
			}
			tagType = (NbtTagType)buffer[_nextIndex];
			if (tagType == NbtTagType.End)
			{
				_nextIndex++;
				return string.Empty;
			}
			ushort nameSize = ByteConverter.BigEndian.ToUShort(buffer, _nextIndex + 1);
			string name = Encoding.UTF8.GetString(buffer, _nextIndex + 3, nameSize);
			_nextIndex = _nextIndex + 3 + nameSize;

			return name;
		}

		private byte ParseByteTagBody(byte[] buffer)
		{
			var index = _nextIndex;
			_nextIndex++;
			return buffer[index];
		}

		private short ParseShortTagBody(byte[] buffer)
		{
			var index = _nextIndex;
			_nextIndex += 2;
			return ByteConverter.BigEndian.ToShort(buffer, index);
		}

		private int ParseIntTagBody(byte[] buffer)
		{
			var index = _nextIndex;
			_nextIndex += 4;
			return ByteConverter.BigEndian.ToInt(buffer, index);
		}

		private long ParseLongTagBody(byte[] buffer)
		{
			var index = _nextIndex;
			_nextIndex += 8;
			return ByteConverter.BigEndian.ToLong(buffer, index);
		}

		private float ParseFloatTagBody(byte[] buffer)
		{
			float value;
			if (BitConverter.IsLittleEndian)
			{
				value = BitConverter.ToSingle(new byte[]
				{
					buffer[_nextIndex + 3],
					buffer[_nextIndex + 2],
					buffer[_nextIndex + 1],
					buffer[_nextIndex]
				}, 0);
			}
			else
			{
				value = BitConverter.ToSingle(buffer, _nextIndex);
			}

			_nextIndex += 4;
			return value;
		}

		private double ParseDoubleTagBody(byte[] buffer)
		{
			double value;
			if (BitConverter.IsLittleEndian)
			{
				value = BitConverter.ToDouble(new byte[]
				{
					buffer[_nextIndex + 7],
					buffer[_nextIndex + 6],
					buffer[_nextIndex + 5],
					buffer[_nextIndex + 4],
					buffer[_nextIndex + 3],
					buffer[_nextIndex + 2],
					buffer[_nextIndex + 1],
					buffer[_nextIndex]
				}, 0);
			}
			else
			{
				value = BitConverter.ToDouble(buffer, _nextIndex);
			}

			_nextIndex += 8;
			return value;
		}

		private string ParseStringTagBody(byte[] buffer)
		{
			short strSize = ParseShortTagBody(buffer);
			string str = Encoding.UTF8.GetString(buffer, _nextIndex, strSize);
			_nextIndex += strSize;

			return str;
		}

		private byte[] ParseByteArrayTagBody(byte[] buffer)
		{
			int arraySize = ParseIntTagBody(buffer);
			byte[] resultArray = new byte[arraySize];
			Buffer.BlockCopy(buffer, _nextIndex, resultArray, 0, arraySize);
			_nextIndex += arraySize;

			return resultArray;
		}

		private int[] ParseIntArrayTagBody(byte[] buffer)
		{
			int arraySize = ParseIntTagBody(buffer);
			int[] resultArray = new int[arraySize];
			for (int i = 0; i < arraySize; i++)
			{
				resultArray[i] = ParseIntTagBody(buffer);
			}

			return resultArray;
		}

		private long[] ParseLongArrayTagBody(byte[] buffer)
		{
			int arraySize = ParseIntTagBody(buffer);
			long[] resultArray = new long[arraySize];
			for (int i = 0; i < arraySize; i++)
			{
				resultArray[i] = ParseLongTagBody(buffer);
			}

			return resultArray;
		}

		private Dictionary<string, INbtNode> ParseCompoundTagBody(byte[] buffer)
		{
			var list = new Dictionary<string, INbtNode>();
			while (true)
			{
				string childName = ParseTagHead(buffer, out NbtTagType tagType);
				INbtNode child;
				switch (tagType)
				{
					case NbtTagType.Byte:
						{
							byte value = ParseByteTagBody(buffer);
							child = new NbtByte(childName, value);
						}
						break;
					case NbtTagType.Short:
						{
							short value = ParseShortTagBody(buffer);
							child = new NbtShort(childName, value);
						}
						break;
					case NbtTagType.Int:
						{
							int value = ParseIntTagBody(buffer);
							child = new NbtInt(childName, value);
						}
						break;
					case NbtTagType.Long:
						{
							long value = ParseLongTagBody(buffer);
							child = new NbtLong(childName, value);
						}
						break;
					case NbtTagType.Float:
						{
							float value = ParseFloatTagBody(buffer);
							child = new NbtFloat(childName, value);
						}
						break;
					case NbtTagType.Double:
						{
							double value = ParseDoubleTagBody(buffer);
							child = new NbtDouble(childName, value);
						}
						break;
					case NbtTagType.String:
						{
							string value = ParseStringTagBody(buffer);
							child = new NbtString(childName, value);
						}
						break;
					case NbtTagType.List:
						{
							NbtTagType listContentType = (NbtTagType)buffer[_nextIndex];
							List<INbtNode> value = ParseListTagBody(buffer);
							child = new NbtList(childName, value, listContentType);
						}
						break;
					case NbtTagType.Compound:
						{
							Dictionary<string, INbtNode> value = ParseCompoundTagBody(buffer);
							child = new NbtCompound(childName, value);
						}
						break;
					case NbtTagType.ByteArray:
						{
							byte[] value = ParseByteArrayTagBody(buffer);
							child = new NbtByteArray(childName, value);
						}
						break;
					case NbtTagType.IntArray:
						{
							int[] value = ParseIntArrayTagBody(buffer);
							child = new NbtIntArray(childName, value);
						}
						break;
					case NbtTagType.LongArray:
						{
							long[] value = ParseLongArrayTagBody(buffer);
							child = new NbtLongArray(childName, value);
						}
						break;

					default:
						goto EndWhile;

				}

				list[childName] = child;
			}

		EndWhile:

			return list;
		}

		private List<INbtNode> ParseListTagBody(byte[] buffer)
		{
			NbtTagType listContentType = (NbtTagType)buffer[_nextIndex];
			var list = new List<INbtNode>();
			_nextIndex++;
			int elementsCount = ParseIntTagBody(buffer);

			switch (listContentType)
			{
				case NbtTagType.Byte:
					for (int i = 0; i < elementsCount; i++)
					{
						byte value = ParseByteTagBody(buffer);
						list.Add(new NbtByte(string.Empty, value));
					}
					break;
				case NbtTagType.Short:
					for (int i = 0; i < elementsCount; i++)
					{
						short value = ParseShortTagBody(buffer);
						list.Add(new NbtShort(string.Empty, value));
					}
					break;
				case NbtTagType.Int:
					for (int i = 0; i < elementsCount; i++)
					{
						int value = ParseIntTagBody(buffer);
						list.Add(new NbtInt(string.Empty, value));
					}
					break;
				case NbtTagType.Long:
					for (int i = 0; i < elementsCount; i++)
					{
						long value = ParseLongTagBody(buffer);
						list.Add(new NbtLong(string.Empty, value));
					}
					break;
				case NbtTagType.Float:
					for (int i = 0; i < elementsCount; i++)
					{
						float value = ParseFloatTagBody(buffer);
						list.Add(new NbtFloat(string.Empty, value));
					}
					break;
				case NbtTagType.Double:
					for (int i = 0; i < elementsCount; i++)
					{
						double value = ParseDoubleTagBody(buffer);
						list.Add(new NbtDouble(string.Empty, value));
					}
					break;
				case NbtTagType.String:
					for (int i = 0; i < elementsCount; i++)
					{
						string value = ParseStringTagBody(buffer);
						list.Add(new NbtString(string.Empty, value));
					}
					break;
				case NbtTagType.List:
					for (int i = 0; i < elementsCount; i++)
					{
						List<INbtNode> value = ParseListTagBody(buffer);
						list.Add(new NbtList(string.Empty, value, listContentType));
					}
					break;
				case NbtTagType.Compound:
					for (int i = 0; i < elementsCount; i++)
					{
						Dictionary<string, INbtNode> value = ParseCompoundTagBody(buffer);
						list.Add(new NbtCompound(string.Empty, value));
					}
					break;

				case NbtTagType.ByteArray:
					for (int i = 0; i < elementsCount; i++)
					{
						byte[] value = ParseByteArrayTagBody(buffer);
						list.Add(new NbtByteArray(string.Empty, value));
					}
					break;
				case NbtTagType.IntArray:
					for (int i = 0; i < elementsCount; i++)
					{
						int[] value = ParseIntArrayTagBody(buffer);
						list.Add(new NbtIntArray(string.Empty, value));
					}
					break;
				case NbtTagType.LongArray:
					for (int i = 0; i < elementsCount; i++)
					{
						long[] value = ParseLongArrayTagBody(buffer);
						list.Add(new NbtLongArray(string.Empty, value));
					}
					break;

				default:
					return list;

			}


			return list;
		}
	}
}
