using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NightWorld.Tools
{
    public partial class ByteConverter
    {
        public static class LittleEndian
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static short ToShort2<T>(T bytes, int index) where T : IList<byte>
            {
                short result = bytes[index + 1];
                result = (short)(result << 8);
                return (short)(result | bytes[index]);
            }

            public static short ToShort(byte[] bytes, int index)
            {
                short result = bytes[index + 1];
                result = (short)(result << 8);
                return (short)(result | bytes[index]);
            }

            public static ushort ToUShort(byte[] bytes, int index)
            {
                return (ushort)ToShort(bytes, index);
            }
        }

    }
}
