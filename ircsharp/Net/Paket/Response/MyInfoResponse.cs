using System;
using System.Text;

namespace IrcSharp.Net.Paket.Response
{
    public class MyInfoResponse : Paket
    {
        public string ServerName { get; set; }
        public string Version { get; set; }
        public string UserModes { get; set; }
        public string ChanModes { get; set; }

        public override void Read(byte[] reader)
        {
            throw new NotImplementedException();
        }

        public override void Write()
        {
            var builder = new StringBuilder(DefaultResponsePrefix);
            builder.AppendFormat(NumericFormat, (int)ResponseType.MyInfo);
            builder.Append(" ");
            builder.Append(ServerName);
            builder.Append(" ");
            builder.Append(Version);
            builder.Append(" ");
            builder.Append(UserModes);
            builder.Append(" ");
            builder.Append(ChanModes);

            Writer.Write(builder + ServerCrLf);
        }
    }
}
