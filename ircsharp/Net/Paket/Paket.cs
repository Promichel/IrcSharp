using System;
using System.IO;
using System.Text;

namespace IrcSharp.Net.Paket
{
    public abstract class Paket
    {
        public const string ServerCrLf = "\r\n";
        public const string NumericFormat = "{0:000}";
        public static readonly string DefaultResponsePrefix = ":"+Settings.Default.ServerHost + " ";

        public abstract void Read(byte[] reader);
        public abstract void Write();

        protected PaketWriter Writer;

        public MemoryStream Stream
        {
            get { return Writer.UnderlyingStream; }
        }

        protected Paket()
        {
            Writer = new PaketWriter();
        }
    }

    public class NickPaket : Paket
    {
        public string Nickname { get; set; }

        public override void Read(byte[] reader)
        {
            Nickname = Encoding.UTF8.GetString(reader).Substring(5);
        }

        public override void Write()
        {
            var builder = new StringBuilder("NICK ");
            builder.Append(Nickname);
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
            string userCommand = Encoding.UTF8.GetString(reader).Substring(5);
            int index = userCommand.IndexOf(':');
            string[] parameters = userCommand.Substring(0, index).Split(' ');
            if (parameters.Length != 4)
            {
                //todo: implement Paket
                throw new Exception("Not enough parameters!");
            }
            Username = parameters[0];
            Hostname = parameters[1];
            ServerName = parameters[2];
            RealName = userCommand.Substring(index + 1);
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }
    }

    public class CapPaket : Paket
    {
        public override void Read(byte[] reader)
        {
            
        }

        public override void Write()
        {
            
        }
    }

    public class PongPaket : Paket
    {
        public string Message { get; set; }

        public override void Read(byte[] reader)
        {
            Message = Encoding.UTF8.GetString(reader).Substring(5);
        }

        public override void Write()
        {
            var builder = new StringBuilder("PING ");
            builder.Append(Message);

            Writer.Write(builder + ServerCrLf);
        }
    }

    public class UserHostPaket : Paket
    {
        public override void Read(byte[] reader)
        {
            //throw new NotImplementedException();
        }

        public override void Write()
        {
            //throw new NotImplementedException();
        }
    }
}
