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

            Register(PacketType.CAP, ReadCap);
            Register(PacketType.NICK, ReadNick);
            Register(PacketType.USER, ReadUser);
            Register(PacketType.PONG, ReadPong);
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
            NickPaket np = new NickPaket();
            np.Read(data);

            if(np.Nickname != null)
            {
                Client.HandlePacketNick(client, np);
            }
        }

        public static void ReadUser(Client client, byte[] data)
        {

            UserPaket up = new UserPaket();
            up.Read(data);

            if(up.RealName != null)
            {
                Client.HandlePacketUser(client, up);
            }

        }

        public static void ReadCap(Client client, byte[] data)
        {
            
        }

        public static void ReadPong(Client client, byte[] data)
        {
            PongPaket pp = new PongPaket();
            pp.Read(data);

            if(pp.Message != null)
            {
                Client.HandlePacketPong(client, pp);
            }
        }
    }
}
