using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private ByteQueue _fragPackets;

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
        private DateTime _nextActivityCheck;

        /// <summary>
        /// A reference to the server logger.
        /// </summary>
        public Logger Logger { get { return Server.Logger; } }

        public ByteQueue FragPackets
        {
            get { return _fragPackets; }
            set { _fragPackets = value; }
        }

        public Client(int sessionId, IrcServer server, Socket socket)
        {

            SessionId = sessionId;
            Server = server;
            _socket = socket;

            _currentBuffer = new ByteQueue();
            _processedBuffer = new ByteQueue();
            _fragPackets = new ByteQueue();

            _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(30);
        }

        public void Start()
        {
            Running = true;
            _sendSocketEvent = SendSocketEventPool.Pop();
            _recvSocketEvent = RecvSocketEventPool.Pop();
            _recvBuffer = RecvBufferPool.AcquireBuffer();

            _recvSocketEvent.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);
            _recvSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recv_Completed);
            _sendSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);

            Task.Factory.StartNew(Recv_Start);
        }


    }
}
