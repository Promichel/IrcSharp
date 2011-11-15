using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcSharp.Net.Paket.Response
{
    public class NickNameInUseResponse : Paket
    {
        public string NickName { get; set; }

        public override void Read(byte[] reader)
        {
            throw new NotImplementedException();
        }

        public override void Write()
        {
            var builder = new StringBuilder(DefaultResponsePrefix);
            builder.AppendFormat(NumericFormat, (int)ResponseType.NickNameInUse);
            builder.Append(NickName);
            builder.Append(" ");
            builder.Append(":Nickname already in use.");
            Writer.Write(builder + ServerCrLf);
        }
    }
}
