using System.IO;
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

        public PaketWriter()
        {
            _stream = new MemoryStream();
        }

        public void Write(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            _stream.Write(data, 0, data.Length);
        }

    }
}
