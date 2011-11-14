using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcSharp.Net.Paket
{
    public abstract class Paket
    {
        public const string ServerCrLf = "\r\n";

        public abstract void Read(byte[] reader);
        public abstract void Write();

        protected PaketWriter Writer;

        public class NickPaket : Paket
        {
            public string Username { get; set; }
            
            public override void Read(byte[] reader)
            {
                Username = Encoding.UTF8.GetString(reader).Substring(4);
            }

            public override void Write()
            {
                StringBuilder builder = new StringBuilder("NICK ");
                builder.Append(Username);
                Writer.Write(builder + ServerCrLf);
            }
        }

        public class UserPaket : Paket
        {
            public string Username { get; set; }
            public string Hostname { get; set; }
            public string ServerName { get; set; }
            public string RealName { get; set; }

            public override void Read(byte[] reader)
            {
                throw new NotImplementedException();
            }

            public override void Write()
            {
                throw new NotImplementedException();
            }
        }
    }
}
