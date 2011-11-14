using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private Socket _listener;
        private SocketAsyncEventArgs _acceptEventArgs;

        public static ConcurrentQueue<Client> RecvClientQueue = new ConcurrentQueue<Client>();
        public static ConcurrentQueue<Client> SendClientQueue = new ConcurrentQueue<Client>();

        public static ConcurrentQueue<Client> ClientsToDispose = new ConcurrentQueue<Client>();

        public Logger Logger { get; private set; }
        private bool _running = true;

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

            Logger = new Logger(this, Settings.Default.LogFile);
            Clients = new ConcurrentDictionary<int, Client>();
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _acceptEventArgs = new SocketAsyncEventArgs();
            _acceptEventArgs.Completed += Accept_Completion;
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
            IPEndPoint ipEndPoint = new IPEndPoint(address, Settings.Default.Port);

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
        private int _asyncAccepts = 0;
        private Task _readClientsPackets;
        private Task _sendClientPackets;
        private Task _disposeClients;

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

                if (!SendClientQueue.IsEmpty && (_sendClientPackets == null || _sendClientPackets.IsCompleted))
                {
                    _sendClientPackets = Task.Factory.StartNew(ProcessSendQueue);
                }
                 * */
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

                

                int length = client.FragPackets.Size + bufferToProcess.Size;

             //   int newLength = bufferToProcess.GetCommand();
                string[] commands = bufferToProcess.GetCommands();
                
                if(commands.Length > 1)
                {
                    for(int u = 0; u < commands.Length - 1; u++)
                    {
                        Console.WriteLine(commands[u]);
                    }
                }
            });
            Console.WriteLine("Done");
        }

        private static byte[] GetBufferToBeRead(ByteQueue processedBuffer, Client client, int length)
        {
            int availableData = client.FragPackets.Size + processedBuffer.Size;

            if (length > availableData)
                return null;

            int fromFrag;

            byte[] data = new byte[length];

            if (length >= client.FragPackets.Size)
                fromFrag = client.FragPackets.Size;
            else
                fromFrag = length;

            client.FragPackets.Dequeue(data, 0, fromFrag);

            int fromProcessed = length - fromFrag;

            processedBuffer.Dequeue(data, fromFrag, fromProcessed);

            return data;
        }

        private bool OnBeforeAccept(Socket socket)
        {
            if (BeforeAccept != null)
            {
                TcpEventArgs e = new TcpEventArgs(socket);
                BeforeAccept.Invoke(this, e);
                return !e.Cancelled;
            }
            return true;
        }

        private void Accept_Completion(object sender, SocketAsyncEventArgs e)
        {
            Accept_Process(e);
        }

        private void Accept_Process(SocketAsyncEventArgs e)
        {
            if (OnBeforeAccept(e.AcceptSocket))
            {
                Interlocked.Increment(ref _nextSessionId);
                Client c = new Client(_nextSessionId, this, e.AcceptSocket);

                c.Start();

                AddClient(c);
                Logger.Log(Logger.LogLevel.Info, "Clients online: {0}", Clients.Count);

                //Logger.Log(Chraft.Logger.LogLevel.Info, "Starting client");
                //OnJoined(c);
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

    }
}
