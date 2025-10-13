using System.Collections.Generic;
using System.Linq;

namespace System
{
    static class RandomExtensions
    {
        const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";

        public static string GenerateString(this Random random, int length)
        {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GenerateBytes(this Random random, int lenght)
        {
            byte[] bytes = new byte[lenght];
            for (int i = 0; i < lenght; i++)
            {
                bytes[i] = (byte)random.Next(0, 256);
            }

            return bytes;
        }

        public static void Shuffle<T>(this Random random, IList<T> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                int targetIndex = random.Next(collection.Count);

                T temp = collection[i];
                collection[i] = collection[targetIndex];
                collection[targetIndex] = temp;
            }
        }
    }
}
