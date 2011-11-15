using System;
using System.Text;

namespace IrcSharp.Net.Paket.Response
{
    public class YourHostResponse : Paket
    {

        public string ServerName { get; set; }
        public string Version { get; set; }

        public override void Read(byte[] reader)
        {
            throw new NotImplementedException();
        }

        public override void Write()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(NumericFormat, (int)ResponseType.YourHost);
            builder.Append(" ");
            builder.Append("Your host is");
            builder.Append(" ");
            builder.Append(ServerName);
            builder.Append(", running version");
            builder.Append(Version);

            Writer.Write(builder + ServerCrLf);
        }
    }
}
