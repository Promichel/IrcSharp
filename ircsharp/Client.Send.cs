using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace IrcSharp
{
    public partial class Client
    {

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            /*
            if (e.Buffer[0] == (byte)PacketType.Disconnect)
                e.Completed -= Disconnected;
            if (!Running)
                DisposeSendSystem();
            else if (e.SocketError != SocketError.Success)
            {
                MarkToDispose();
                DisposeSendSystem();
                _nextActivityCheck = DateTime.MinValue;
            }
            else
            {
                if (DateTime.Now + TimeSpan.FromSeconds(5) > _nextActivityCheck)
                    _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(5);
                Send_Start();
            }
             */
        }

    }
}
