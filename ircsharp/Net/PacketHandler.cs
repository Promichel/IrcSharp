using IrcSharp.Net.Paket;

namespace IrcSharp.Net
{
    public delegate void OnPacketReceive(Client client, byte[] data);
    public class PacketHandler
    {

        private readonly OnPacketReceive _onReceive;
        private readonly PacketType _packetId;

        public PacketType PacketId
        {
            get { return _packetId; }
        }

        public OnPacketReceive OnReceive
        {
            get { return _onReceive; }
        }

        public PacketHandler(PacketType type, OnPacketReceive onReceive)
        {
            _packetId = type;
            _onReceive = onReceive;
        }

    }
}
