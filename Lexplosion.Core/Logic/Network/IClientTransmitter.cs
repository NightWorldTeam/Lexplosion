namespace Lexplosion.Logic.Network
{
    interface IClientTransmitter
    {
        void Send(byte[] inputData);

        bool Receive(out byte[] data);

        event PointHandle ClientClosing;

        bool IsConnected { get; }

        void Close();
    }

}
