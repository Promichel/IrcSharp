using System;
using System.Text;

namespace IrcSharp.Net.Paket.Response
{
    public class WelcomeResponse : Paket
    {
        public string Nickname { get; set; }
        public string Username { get; set; }
        public string Host { get; set; }

        public override void Read(byte[] reader)
        {
            throw new NotImplementedException();
        }

        public override void Write()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(NumericFormat, (int) ResponseType.Welcome);
            builder.Append(" ");
            builder.Append("Welcome to the Internet Relay Network");
            builder.Append(" ");
            builder.Append(Nickname);
            builder.Append("!");
            builder.Append(Username);
            builder.Append("@");
            builder.Append(Host);

            Writer.Write(builder + ServerCrLf);
        }
    }
}
