#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Text;

namespace IrcSharp.Net
{
	public class ByteQueue
	{
		private int _mHead;
		private int _mTail;
		private int _mSize;

		private byte[] _mBuffer;

		public int Length{ get{ return _mSize; } }

		public byte[] UnderlyingBuffer
		{
			get { return _mBuffer; }
            set { _mBuffer = value; }
		}

		public int Head
		{
			get { return _mHead; }
		}

		public int Tail
		{
			get { return _mTail; }
		}

		public int Size
		{
			get { return _mSize; }
		}

		public ByteQueue()
		{
			_mBuffer = new byte[2048];
		}

		public void Clear()
		{
			_mHead = 0;
			_mTail = 0;
			_mSize = 0;
		}

		public void SetCapacity( int capacity, bool alwaysNewBuffer ) 
		{
            if(!alwaysNewBuffer)
            {
                if(_mBuffer == null || _mBuffer.Length < capacity)
                    _mBuffer = new byte[capacity]; 
            }
            else
            {
                var newBuffer = new byte[capacity];

                if ( _mSize > 0 )
                {
                    if ( _mHead < _mTail )
                    {
                        Buffer.BlockCopy( _mBuffer, _mHead, newBuffer, 0, _mSize );
                    }
                    else
                    {
                        Buffer.BlockCopy( _mBuffer, _mHead, newBuffer, 0, _mBuffer.Length - _mHead );
                        Buffer.BlockCopy( _mBuffer, 0, newBuffer, _mBuffer.Length - _mHead, _mTail );
                    }
                }

                _mBuffer = newBuffer;
            }

			_mHead = 0;
			_mTail = _mSize;
			
		}

        public string GetPaket()
        {
            string line = Encoding.UTF8.GetString(_mBuffer);
            line = line.Split('\n')[0];

            

            return "";
        }

        public string[] GetCommands()
        {
            string[] line = Encoding.UTF8.GetString(_mBuffer).Split('\n');

            return line;

        }

		public byte GetPacketId()
		{
		   //string test = Encoding.UTF8.GetString(_mBuffer);

            string line2 = Encoding.UTF8.GetString(_mBuffer);
            string[] line = line2.Split('\n');

            if(line.Length > 1) {
		        int i = Encoding.UTF8.GetBytes(line[0]).Length;
		        byte[] bla = new byte[i+1];

		        Dequeue(bla, 0, bla.Length);
                string line3 = Encoding.UTF8.GetString(bla);
                Console.WriteLine(line3);
            }
           // Enqueue(Encoding.UTF8.GetBytes(line), );

		//	if ( _mSize >= 1 )
			//	return _mBuffer[_mHead];

			return 0xF0;
		}

        public int CopyAll(byte[] buffer)
        {
            if (_mHead < _mTail)
            {
                Buffer.BlockCopy(_mBuffer, _mHead, buffer, 0, _mSize);
            }
            else
            {
                int rightLength = (_mBuffer.Length - _mHead);

                if (rightLength >= _mSize)
                {
                    Buffer.BlockCopy(_mBuffer, _mHead, buffer, 0, _mSize);
                }
                else
                {
                    Buffer.BlockCopy(_mBuffer, _mHead, buffer, 0, rightLength);
                    Buffer.BlockCopy(_mBuffer, 0, buffer, 0 + rightLength, _mSize - rightLength);
                }
            }

            return _mSize;
        }

		public int Dequeue( byte[] buffer, int offset, int size )
		{
			if ( size > _mSize )
				size = _mSize;

			if ( size == 0 )
				return 0;

			if ( _mHead < _mTail )
			{
				Buffer.BlockCopy( _mBuffer, _mHead, buffer, offset, size );
			}
			else
			{
				int rightLength = ( _mBuffer.Length - _mHead );

				if ( rightLength >= size )
				{
					Buffer.BlockCopy( _mBuffer, _mHead, buffer, offset, size );
				}
				else
				{
					Buffer.BlockCopy( _mBuffer, _mHead, buffer, offset, rightLength );
					Buffer.BlockCopy( _mBuffer, 0, buffer, offset + rightLength, size - rightLength );
				}
			}

			_mHead = ( _mHead + size ) % _mBuffer.Length;
			_mSize -= size;

			if ( _mSize == 0 )
			{
				_mHead = 0;
				_mTail = 0;
			}

			return size;
		}

		public void Enqueue( byte[] buffer, int offset, int size )
		{
			if ( (_mSize + size) > _mBuffer.Length )
				SetCapacity( (_mSize + size + 2047) & ~2047, true );

			if ( _mHead < _mTail )
			{
				int rightLength = ( _mBuffer.Length - _mTail );

				if ( rightLength >= size )
				{
					Buffer.BlockCopy( buffer, offset, _mBuffer, _mTail, size );
				}
				else
				{
					Buffer.BlockCopy( buffer, offset, _mBuffer, _mTail, rightLength );
					Buffer.BlockCopy( buffer, offset + rightLength, _mBuffer, 0, size - rightLength );
				}
			}
			else
			{
				Buffer.BlockCopy( buffer, offset, _mBuffer, _mTail, size );
			}

			_mTail = ( _mTail + size ) % _mBuffer.Length;
			_mSize += size;
		}
	}
}