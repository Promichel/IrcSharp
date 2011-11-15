using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrcSharp.Net;
using IrcSharp.Net.Paket;

namespace IrcSharp
{
    public class IrcServer
    {

        private int _nextSessionId;
        private readonly Socket _listener;
        private readonly SocketAsyncEventArgs _acceptEventArgs;

        public static ConcurrentQueue<Client> RecvClientQueue = new ConcurrentQueue<Client>();
        public static ConcurrentQueue<Client> SendClientQueue = new ConcurrentQueue<Client>();

        public static ConcurrentQueue<Client> ClientsToDispose = new ConcurrentQueue<Client>();

        public Logger Logger { get; private set; }
        private bool _running = true;

        public Dictionary<string, Client> Nicknames;

        public int MaxClientConnections;

        /// <summary>
        /// Invoked prior to a client being accepted, before any data is transcieved.
        /// </summary>
        public event EventHandler<TcpEventArgs> BeforeAccept;

        /// <summary>
        /// Gets a thread-safe dictionary of clients.  Use GetClients for an array version.
        /// </summary>
        public ConcurrentDictionary<int, Client> Clients { get; private set; }

        private int _clientDictChanges;

        public IrcServer()
        {
            //todo: Make it dynamic :)
            MaxClientConnections = 10;

            Logger = new Logger(Settings.Default.LogFile);
            Clients = new ConcurrentDictionary<int, Client>();
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _acceptEventArgs = new SocketAsyncEventArgs();
            _acceptEventArgs.Completed += AcceptCompletion;
            Nicknames = new Dictionary<string, Client>();
        }

        public void Run()
        {
            Logger.Log(Logger.LogLevel.Info, "Starting IrcSharp...");

            for (int i = 0; i < 10; ++i)
            {
                Client.SendSocketEventPool.Push(new SocketAsyncEventArgs());
                Client.RecvSocketEventPool.Push(new SocketAsyncEventArgs());
            }

            while (_running)
                RunProc();
        }

        private void RunProc()
        {
            Logger.Log(Logger.LogLevel.Info, "Using IP Address {0}.", Settings.Default.IPAddress);
            Logger.Log(Logger.LogLevel.Info, "Listening on port {0}.", Settings.Default.Port);

            IPAddress address = IPAddress.Parse(Settings.Default.IPAddress);
            var ipEndPoint = new IPEndPoint(address, Settings.Default.Port);

            _listener.Bind(ipEndPoint);
            _listener.Listen(5);

            RunNetwork();

            if (_running)
            {
                Logger.Log(Logger.LogLevel.Info, "Waiting one second before restarting network.");
                Thread.Sleep(1000);
            }
        }

        public AutoResetEvent NetworkSignal = new AutoResetEvent(true);
        private int _asyncAccepts;
        private Task _readClientsPackets;
        private Task _sendClientPackets;

        private void RunNetwork()
        {
            while (NetworkSignal.WaitOne())
            {
                if (TryTakeConnectionSlot())
                    _listener.AcceptAsync(_acceptEventArgs);

                if (!RecvClientQueue.IsEmpty && (_readClientsPackets == null || _readClientsPackets.IsCompleted))
                {
                    _readClientsPackets = Task.Factory.StartNew(ProcessReadQueue);
                }

                /*

                if (!ClientsToDispose.IsEmpty && (_disposeClients == null || _disposeClients.IsCompleted))
                {
                    _disposeClients = Task.Factory.StartNew(DisposeClients);
                }
                 * */
                if (!SendClientQueue.IsEmpty && (_sendClientPackets == null || _sendClientPackets.IsCompleted))
                {
                    _sendClientPackets = Task.Factory.StartNew(ProcessSendQueue);
                }
                
            }
        }

        public bool TryTakeConnectionSlot()
        {
            int accepts = Interlocked.Exchange(ref _asyncAccepts, 1);
            if (accepts == 0)
            {
                int count = Interlocked.Decrement(ref MaxClientConnections);

                if (count >= 0)
                    return true;

                _asyncAccepts = 0;

                Interlocked.Increment(ref MaxClientConnections);
            }

            return false;

        }

        public static void ProcessReadQueue()
        {
            int count = RecvClientQueue.Count;

            Parallel.For(0, count, i =>
            {
                Client client;
                if (!RecvClientQueue.TryDequeue(out client))
                    return;

                if (!client.Running)
                    return;

                Interlocked.Exchange(ref client.TimesEnqueuedForRecv, 0);
                ByteQueue bufferToProcess = client.GetBufferToProcess();

                string[] commands = bufferToProcess.GetCommands();
                
                if(commands.Length > 1)
                {
                    for(int u = 0; u < commands.Length - 1; u++)
                    {
                        client.Logger.Log(Logger.LogLevel.Debug, commands[u]);
                        int index = commands[u].IndexOf(' ');
                        var myEnum = (PacketType)Enum.Parse(typeof(PacketType), commands[u].Substring(0,index));

                        PacketHandler handler = PacketHandlers.GetHandler(myEnum);

                        if(handler != null)
                            handler.OnReceive(client, Encoding.UTF8.GetBytes(commands[u]));
                        else
                        {
                                client.Logger.Log(Logger.LogLevel.Error, "Command unknown: " + commands[u]);
                        }
                    }
                }
            });
        }

        public static void ProcessSendQueue()
        {
            int count = SendClientQueue.Count;

            Parallel.For(0, count, i =>
            {
                Client client;
                if (!SendClientQueue.TryDequeue(out client))
                    return;

                if (!client.Running)
                {
                    client.DisposeSendSystem();
                    return;
                }

                client.SendStart();
            });
        }

        private bool OnBeforeAccept(Socket socket)
        {
            if (BeforeAccept != null)
            {
                var e = new TcpEventArgs(socket);
                BeforeAccept.Invoke(this, e);
                return !e.Cancelled;
            }
            return true;
        }

        private void AcceptCompletion(object sender, SocketAsyncEventArgs e)
        {
            AcceptProcess(e);
        }

        private void AcceptProcess(SocketAsyncEventArgs e)
        {
            if (OnBeforeAccept(e.AcceptSocket))
            {
                Interlocked.Increment(ref _nextSessionId);
                var c = new Client(_nextSessionId, this, e.AcceptSocket);

                c.Start();

                AddClient(c);
                Logger.Log(Logger.LogLevel.Info, "Clients online: {0}", Clients.Count);
            }
            else
            {
                if (e.AcceptSocket.Connected)
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                e.AcceptSocket.Close();
            }
            _acceptEventArgs.AcceptSocket = null;
            Interlocked.Exchange(ref _asyncAccepts, 0);
            NetworkSignal.Set();
        }

        public void AddClient(Client client)
        {
            Clients.TryAdd(client.SessionId, client);
            Interlocked.Increment(ref _clientDictChanges);
        }

        public void DisconnectClient(Client client)
        {
            Clients.TryRemove(client.SessionId, out client);
            client = null;
        }

        public Client GetClientByNickname(string nickname)
        {
            if(Nicknames.ContainsKey(nickname)) {
                return Nicknames[nickname.ToUpper()];
            }
            return null;
        }
    }
}
