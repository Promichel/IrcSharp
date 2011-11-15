using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using IrcSharp.Net;
using IrcSharp.Net.Paket;
using IrcSharp.Net.Paket.Response;

namespace IrcSharp
{
    public partial class Client
    {
        public ConcurrentQueue<Paket> PacketsToBeSent = new ConcurrentQueue<Paket>();

        private int _timesEnqueuedForSend;

        public void SendPacket(Paket packet)
        {
            if (!Running)
                return;

            PacketsToBeSent.Enqueue(packet);

            int newValue = Interlocked.Increment(ref _timesEnqueuedForSend);

            if (newValue == 1)
            {
                IrcServer.SendClientQueue.Enqueue(this);

            }

            Server.NetworkSignal.Set();
         
        }

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

        internal void SendStart()
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }

            Paket packet = null;
            try
            {
                var byteQueue = new ByteQueue();
                int length = 0;
                while (!PacketsToBeSent.IsEmpty && length <= 1024)
                {
                    if (!PacketsToBeSent.TryDequeue(out packet))
                    {
                        Interlocked.Exchange(ref _timesEnqueuedForSend, 0);
                        return;
                    }

                    packet.Write();

                    byte[] packetBuffer = packet.Stream.GetBuffer();
                    length += packetBuffer.Length;

                    byteQueue.Enqueue(packetBuffer, 0, packetBuffer.Length);

                }

                if (byteQueue.Length > 0)
                {
                    var data = new byte[length];
                    byteQueue.Dequeue(data, 0, data.Length);
                    SendAsync(data);
                }
                else
                {
                    Interlocked.Exchange(ref _timesEnqueuedForSend, 0);

                    if (!PacketsToBeSent.IsEmpty)
                    {
                        int newValue = Interlocked.Increment(ref _timesEnqueuedForSend);

                        if (newValue == 1)
                        {
                            IrcServer.SendClientQueue.Enqueue(this);
                            Server.NetworkSignal.Set();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DisposeSendSystem();
                if (packet != null)
                    Logger.Log(Logger.LogLevel.Error, "Sending packet: {0}", packet.ToString());
                Logger.Log(Logger.LogLevel.Error, e.ToString());

                // TODO: log something?
            }

        }

        private void SendAsync(byte[] data)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }

            _sendSocketEvent.SetBuffer(data, 0, data.Length);
            bool pending = _socket.SendAsync(_sendSocketEvent);
            if (!pending)
                Send_Completed(null, _sendSocketEvent);
        }

        private static void RegisterUser(Client client)
        {
            if(client.ClientInfo.IsRegistered && client.ClientInfo.Nickname != null)
                client.SendPacket(new WelcomeResponse {Host = client.ClientInfo.Host, Nickname = client.ClientInfo.Nickname, Username = client.ClientInfo.Username});
        }

    }
}
