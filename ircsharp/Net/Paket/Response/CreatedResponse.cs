using System;
using System.Text;

namespace IrcSharp.Net.Paket.Response
{
    public class CreatedResponse : Paket
    {
        public string Date { get; set; }

        public override void Read(byte[] reader)
        {
            throw new NotImplementedException();
        }

        public override void Write()
        {
            var builder = new StringBuilder(DefaultResponsePrefix);
            builder.AppendFormat(NumericFormat, (int)ResponseType.Created);
            builder.Append(" :This server was created ");
            builder.Append(Date);

            Writer.Write(builder + ServerCrLf);
        }
    }
}
