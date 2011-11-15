using System;
using System.Net.Sockets;
using System.Threading;
using IrcSharp.Net;
using IrcSharp.Net.Paket.Response;

namespace IrcSharp
{
    public partial class Client
    {

        public int TimesEnqueuedForRecv;
        private readonly object _queueSwapLock = new object();

        private void RecvStart()
        {
            if (!Running)
            {
               // DisposeRecvSystem();
                return;
            }

            if (!_socket.Connected)
            {
              //  Stop();
                return;
            }

            try
            {
                bool pending = _socket.ReceiveAsync(_recvSocketEvent);

                if (!pending)
                    RecvCompleted(null, _recvSocketEvent);
            }
            catch
            {
            }

        }

        private void RecvCompleted(object sender, SocketAsyncEventArgs e)
        {
            //if (!Running)
             //   DisposeRecvSystem();
            /*else if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Client client;
                if (Server.AuthClients.TryGetValue(SessionID, out client))
                {
                    MarkToDispose();
                    DisposeRecvSystem();
                }
                _nextActivityCheck = DateTime.MinValue;
                //Logger.Log(Logger.LogLevel.Error, "Error receiving: {0}", e.SocketError);
            }*/
            if (Running)
            {
                if (DateTime.Now + TimeSpan.FromSeconds(5) > _nextActivityCheck)
                    _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(5);
                RecvProcess(e);
            }
        }

        private void RecvProcess(SocketAsyncEventArgs e)
        {
            lock (_queueSwapLock)
                _currentBuffer.Enqueue(e.Buffer, 0, e.BytesTransferred);

            int newValue = Interlocked.Increment(ref TimesEnqueuedForRecv);

            if ((newValue - 1) == 0)
                IrcServer.RecvClientQueue.Enqueue(this);

            Server.NetworkSignal.Set();

            RecvStart();
        }

        public ByteQueue GetBufferToProcess()
        {
            lock (_queueSwapLock)
            {
                ByteQueue temp = _currentBuffer;
                _currentBuffer = _processedBuffer;
                _processedBuffer = temp;
            }

            return _processedBuffer;
        }

        public static void HandlePacketNick(Client client, Net.Paket.NickPaket np)
        {
            var nickClient = client.Server.GetClientByNickname(np.Nickname.ToUpper());
            if(nickClient != null)
            {
                client.SendPacket(new NickNameInUseResponse {NickName = np.Nickname});
                return;
            }

            client.ClientInfo.Nickname = np.Nickname;
            client.Server.Nicknames.Add(np.Nickname.ToUpper(), client);
            RegisterUser(client);
        }

        public static void HandlePacketUser(Client client, Net.Paket.UserPaket up)
        {
            client.ClientInfo.Username = up.Username;
            client.ClientInfo.RealName = up.RealName;
            client.ClientInfo.Host = up.Hostname;
            if(client.ClientInfo.Nickname != null)
            {
                client.ClientInfo.IsRegistered = true;
                RegisterUser(client);
            }
        }

        public static void HandlePacketPong(Client client, Net.Paket.PongPaket pp)
        {
            if(pp.Message == Settings.Default.ServerName)
            {
                client._nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(30);
            }
        }
    }
}
