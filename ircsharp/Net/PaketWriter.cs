using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IrcSharp.Net
{
    public class PaketWriter
    {

        private MemoryStream _stream;
        public MemoryStream UnderlyingStream
        {
            get { return _stream; }
            set { _stream = value; }
        }

        public void Write(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            _stream.Write(data, 0, data.Length);
        }

    }
}
