namespace NightWorld.Tools
{
    public partial class ByteConverter
    {
        public static class BigEndian
        {
            public static short ToShort(byte[] bytes, int index)
            {
                short result = bytes[index];
                result = (short)(result << 8);
                return (short)(result | bytes[index + 1]);
            }

            public static int ToInt(byte[] bytes, int index)
            {
                int result = bytes[index];
                result <<= 8;
                result = (result | bytes[index + 1]) << 8;
                result = (result | bytes[index + 2]) << 8;

                return result | bytes[index + 3];
            }

            public static long ToLong(byte[] bytes, int index)
            {
                long result = bytes[index];
                result <<= 8;
                result = (result | bytes[index + 1]) << 8;
                result = (result | bytes[index + 2]) << 8;
                result = (result | bytes[index + 3]) << 8;
                result = (result | bytes[index + 4]) << 8;
                result = (result | bytes[index + 5]) << 8;
                result = (result | bytes[index + 6]) << 8;

                return result | bytes[index + 7];
            }

            public static void ToBytes(byte[] buffer, int startIndex, short value)
            {
                buffer[startIndex] = (byte)(value >> 8);
                buffer[startIndex + 1] = (byte)value;
            }

            public static void ToBytes(byte[] buffer, int startIndex, int value)
            {
                buffer[startIndex] = (byte)(value >> 24);
                buffer[startIndex + 1] = (byte)(value >> 16);
                buffer[startIndex + 2] = (byte)(value >> 8);
                buffer[startIndex + 3] = (byte)value;
            }

            public static void ToBytes(byte[] buffer, int startIndex, long value)
            {
                buffer[startIndex] = (byte)(value >> 56);
                buffer[startIndex + 1] = (byte)(value >> 48);
                buffer[startIndex + 2] = (byte)(value >> 40);
                buffer[startIndex + 3] = (byte)(value >> 32);
                buffer[startIndex + 4] = (byte)(value >> 24);
                buffer[startIndex + 5] = (byte)(value >> 16);
                buffer[startIndex + 6] = (byte)(value >> 8);
                buffer[startIndex + 7] = (byte)value;
            }

            public static ushort ToUShort(byte[] bytes, int index) => (ushort)ToShort(bytes, index);

            public static uint ToUInt(byte[] bytes, int index) => (uint)ToInt(bytes, index);

            public static ulong ToULong(byte[] bytes, int index) => (ulong)ToLong(bytes, index);

            public static void ToBytes(byte[] buffer, int startIndex, ushort value) => ToBytes(buffer, startIndex, (short)value);

            public static void ToBytes(byte[] buffer, int startIndex, uint value) => ToBytes(buffer, startIndex, (int)value);
            public static void ToBytes(byte[] buffer, int startIndex, ulong value) => ToBytes(buffer, startIndex, (long)value);
        }
    }
}
