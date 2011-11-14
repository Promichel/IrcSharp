using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using IrcSharp.Net;

namespace IrcSharp
{
    public partial class Client
    {

        public int TimesEnqueuedForRecv;
        private readonly object _QueueSwapLock = new object();

        private void Recv_Start()
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
                    Recv_Completed(null, _recvSocketEvent);
            }
            catch (Exception e)
            {
             //   Server.Logger.Log(Chraft.Logger.LogLevel.Error, e.Message);
             //   Stop();
            }

        }

        private void Recv_Completed(object sender, SocketAsyncEventArgs e)
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
                Recv_Process(e);
            }
        }

        private void Recv_Process(SocketAsyncEventArgs e)
        {
            lock (_QueueSwapLock)
                _currentBuffer.Enqueue(e.Buffer, 0, e.BytesTransferred);

            int newValue = Interlocked.Increment(ref TimesEnqueuedForRecv);

            if ((newValue - 1) == 0)
                IrcServer.RecvClientQueue.Enqueue(this);

            Server.NetworkSignal.Set();

            Recv_Start();
        }

        public ByteQueue GetBufferToProcess()
        {
            lock (_QueueSwapLock)
            {
                ByteQueue temp = _currentBuffer;
                _currentBuffer = _processedBuffer;
                _processedBuffer = temp;
            }

            return _processedBuffer;
        }

    }
}
