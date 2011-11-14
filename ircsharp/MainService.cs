using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace IrcSharp
{
    public class MainService : ServiceBase
    {

        private Task ServerRunTask { get; set; }
        private IrcServer IrcServer;
        private bool IsStopping { get; set; }

        public void Run(string[] args)
        {
            OnStart(args);
            while(true)
            {
                
            }
        }

        protected override void OnStart(string[] args)
        {
            foreach (string arg in args)
            {
                switch(arg)
                {
                    case "-port":
                        //todo: Set Port
                        break;
                    case "-ip":
                        //todo: Set different Ip
                        break;
                }
            }

            ServerRunTask = Task.Factory.StartNew(RunServer);
        }

        private void RunServer()
        {
            IsStopping = false;
            while (!IsStopping)
            {
                (IrcServer = new IrcServer()).Run();
            }
        }

    }
}
