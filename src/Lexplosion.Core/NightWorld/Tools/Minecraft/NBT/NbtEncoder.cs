using System;
using System.Collections.Generic;
using System.Text;

namespace NightWorld.Tools.Minecraft.NBT
{
	class NbtEncoder
	{
		private List<byte> _buffer;

		public byte[] Encode(INbtNode element)
		{
			_buffer = new List<byte>();
			PushTagHead(element);
			PushElementContent(element);

			return _buffer.ToArray();
		}

		private void PushElementContent(INbtNode element)
		{
			switch (element.Type)
			{
				case NbtTagType.Byte:
					PushByte(((NbtByte)element).Content);
					break;
				case NbtTagType.Short:
					PushShort(((NbtShort)element).Content);
					break;
				case NbtTagType.Int:
					PushInt(((NbtInt)element).Content);
					break;
				case NbtTagType.Long:
					PushLong(((NbtLong)element).Content);
					break;
				case NbtTagType.ByteArray:
					PushByteArray(((NbtByteArray)element).Content);
					break;
				case NbtTagType.IntArray:
					PushIntArray(((NbtIntArray)element).Content);
					break;
				case NbtTagType.LongArray:
					PushLongArray(((NbtLongArray)element).Content);
					break;
				case NbtTagType.String:
					PushString(((NbtString)element).Content);
					break;
				case NbtTagType.Float:
					PushFloat(((NbtFloat)element).Content);
					break;
				case NbtTagType.Double:
					PushDouble(((NbtDouble)element).Content);
					break;
				case NbtTagType.List:
					PushList((NbtList)element);
					break;
				case NbtTagType.Compound:
					PushCompound((NbtCompound)element);
					break;
			}
		}

		private void PushTagHead(INbtNode tag)
		{
			_buffer.Add((byte)tag.Type);
			byte[] name = Encoding.UTF8.GetBytes(tag.Name);
			byte[] size = new byte[2];
			ByteConverter.BigEndian.ToBytes(size, 0, (short)name.Length);
			_buffer.AddRange(size);
			_buffer.AddRange(name);
		}

		private void PushByte(byte num)
		{
			_buffer.Add(num);
		}

		private void PushShort(short num)
		{
			byte[] buff = new byte[2];
			ByteConverter.BigEndian.ToBytes(buff, 0, num);
			_buffer.AddRange(buff);
		}

		private void PushInt(int num)
		{
			byte[] buff = new byte[4];
			ByteConverter.BigEndian.ToBytes(buff, 0, num);
			_buffer.AddRange(buff);
		}

		private void PushLong(long num)
		{
			byte[] buff = new byte[8];
			ByteConverter.BigEndian.ToBytes(buff, 0, num);
			_buffer.AddRange(buff);
		}

		private void PushFloat(float num)
		{
			byte[] buff = BitConverter.GetBytes(num);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(buff);
			}
			_buffer.AddRange(buff);
		}

		private void PushDouble(double num)
		{
			byte[] buff = BitConverter.GetBytes(num);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(buff);
			}
			_buffer.AddRange(buff);
		}

		private void PushString(string str)
		{
			if (str.Length > short.MaxValue) str = str.Substring(0, short.MaxValue);

			byte[] buff = Encoding.UTF8.GetBytes(str);
			PushShort((short)buff.Length);
			_buffer.AddRange(buff);
		}

		private void PushByteArray(byte[] array)
		{
			PushInt(array.Length);
			_buffer.AddRange(array);
		}

		private void PushIntArray(int[] array)
		{
			PushInt(array.Length);
			foreach (int item in array)
			{
				PushInt(item);
			}
		}

		private void PushLongArray(long[] array)
		{
			PushInt(array.Length);
			foreach (long item in array)
			{
				PushLong(item);
			}
		}

		private void PushList(NbtList list)
		{
			_buffer.Add((byte)list.ListContentType);
			PushInt(list.Count);

			// TODO: я изначально знаю тип элементов, поэтому можно сразу же использовать метод для нужного типа элемента, а не PushElement
			foreach (INbtNode elem in list)
			{
				PushElementContent(elem);
			}
		}

		private void PushCompound(NbtCompound compound)
		{
			foreach (INbtNode elem in compound.Values)
			{
				PushTagHead(elem);
				PushElementContent(elem);
			}

			_buffer.Add((byte)NbtTagType.End);
		}
	}
}
