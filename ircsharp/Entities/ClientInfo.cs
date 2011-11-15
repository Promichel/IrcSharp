using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcSharp.Entities
{
    public class ClientInfo
    {

        public bool IsRegistered;

        public string Nickname { get; set; }
        public string Username { get; set; }
        public string RealName { get; set; }
        public string Host { get; set; }

    }
}
