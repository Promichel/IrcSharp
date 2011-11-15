using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IrcSharp.Entities;
using IrcSharp.Net;

namespace IrcSharp
{
    public partial class Client
    {

        public IrcServer Server;
        private readonly Socket _socket;

        public volatile bool Running = true;

        private ByteQueue _currentBuffer;
        private ByteQueue _processedBuffer;

        public static SocketAsyncEventArgsPool SendSocketEventPool = new SocketAsyncEventArgsPool(10);
        public static SocketAsyncEventArgsPool RecvSocketEventPool = new SocketAsyncEventArgsPool(10);
        public static BufferPool RecvBufferPool = new BufferPool("Receive", 2048, 2048);

        private byte[] _recvBuffer;
        private SocketAsyncEventArgs _sendSocketEvent;
        private SocketAsyncEventArgs _recvSocketEvent;

        private bool _sendSystemDisposed;
        private bool _recvSystemDisposed;

        private readonly object _disposeLock = new object();

        public int SessionId { get; private set; }
        public ClientInfo ClientInfo { get; set; }
        private DateTime _nextActivityCheck;

        /// <summary>
        /// A reference to the server logger.
        /// </summary>
        public Logger Logger { get { return Server.Logger; } }

        public ByteQueue FragPackets { get; set; }

        public Client(int sessionId, IrcServer server, Socket socket)
        {

            SessionId = sessionId;
            Server = server;
            _socket = socket;

            ClientInfo = new ClientInfo();

            _currentBuffer = new ByteQueue();
            _processedBuffer = new ByteQueue();
            FragPackets = new ByteQueue();

            _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(30);
        }

        public void Start()
        {
            Running = true;
            _sendSocketEvent = SendSocketEventPool.Pop();
            _recvSocketEvent = RecvSocketEventPool.Pop();
            _recvBuffer = RecvBufferPool.AcquireBuffer();

            _recvSocketEvent.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);
            _recvSocketEvent.Completed += RecvCompleted;
            _sendSocketEvent.Completed += Send_Completed;

            Task.Factory.StartNew(RecvStart);
        }

        public void DisposeSendSystem()
        {
            lock (_disposeLock)
            {
                if (!_sendSystemDisposed)
                {
                    _sendSystemDisposed = true;
                    if (_recvSystemDisposed)
                    {
                        IrcServer.ClientsToDispose.Enqueue(this);
                        Server.NetworkSignal.Set();
                    }
                }
            }
        }

    }
}
