using System.Collections.Generic;
using System.Net;

namespace Lexplosion.Logic.Network
{
    public delegate void ClientPointHandle(ClientDesc clientData);

    interface IServerTransmitter
    {
        ClientDesc Receive(out byte[] data);
        void Send(byte[] inputData, ClientDesc clientData);

        IReadOnlyCollection<ClientDesc> WaitSendAvailable();

        event ClientPointHandle ClientClosing;

        void StopWork();

        bool Close(ClientDesc clientData);
    }

    public struct ClientDesc
    {
        public readonly string Id;
        public readonly IPEndPoint Point;

        public ClientDesc(string id, IPEndPoint point)
        {
            Id = id;
            Point = point;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ClientDesc)) return false;
            return Id?.Equals(((ClientDesc)obj).Id) ?? false;
        }

        public override string ToString()
        {
            return "[ID-" + Id?.ToString() + ", Point-" + Point + "]";
        }

        public bool IsEmpty
        {
            get
            {
                return Id == null && Point == null;
            }
        }

        public static readonly ClientDesc Empty = new(null, null);
    }

}
