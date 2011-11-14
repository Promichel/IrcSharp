using IrcSharp.Net.Paket;

namespace IrcSharp.Net
{
    public class PacketHandlers
    {


        private static PacketHandler[] m_Handlers;

        public static PacketHandler[] Handlers
        {
            get { return m_Handlers; }
        }

        static PacketHandlers()
        {
            m_Handlers = new PacketHandler[0x100];

            Register(PacketType.NICK, ReadNick);
        }

        public static void Register(PacketType packetId, OnPacketReceive onReceive)
        {
            m_Handlers[(int) packetId] = new PacketHandler(packetId, onReceive);
        }

        public static PacketHandler GetHandler(PacketType packetId)
        {
            return m_Handlers[(byte)packetId];
        }

        public static void ReadNick(Client client, byte[] data)
        {
            
        }
    }
}
