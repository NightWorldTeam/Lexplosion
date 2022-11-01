using System;

namespace Lexplosion.Logic.Network.WebSockets
{
    abstract class WebSocket
    {
        /// <summary>
        /// Кодирует данные. Адекватно работает, только если их количество меньше 256
        /// </summary>
        /// <param name="payload">Данные блять</param>
        /// <returns>Кодированные данный нахуй</returns>
        protected byte[] EncodeFrame(byte[] payload)
        {
            byte[] data = new byte[payload.Length + 2];
            data[0] = 129;
            data[1] = (byte)payload.Length;
            Array.Copy(payload, 0, data, 2, payload.Length);

            return data;
        }

        protected byte[] DecodeFrame(byte[] frame)
        {
            try
            {
                bool mask = (frame[1] & 0b10000000) != 0;
                int msglen = frame[1] - 128, offset = 2;

                if (msglen == 126)
                {
                    msglen = BitConverter.ToUInt16(new byte[] { frame[3], frame[2] }, 0);
                    offset = 4;
                }
                else if (msglen == 127)
                {
                    return null;
                }

                if (mask && msglen != 0)
                {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = new byte[4] { frame[offset], frame[offset + 1], frame[offset + 2], frame[offset + 3] };
                    offset += 4;

                    for (int i = 0; i < msglen; ++i)
                        decoded[i] = (byte)(frame[offset + i] ^ masks[i % 4]);

                    return decoded;
                }
                else
                {
                    byte[] decoded = new byte[frame.Length - 2];
                    Array.Copy(frame, 2, decoded, 0, frame.Length - 2);

                    return decoded;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
