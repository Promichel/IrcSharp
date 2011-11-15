using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcSharp;

namespace IrcServerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var svc = new MainService();
            svc.Run(args);

        }
    }
}
