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
using System.Collections.Generic;

namespace IrcSharp.Net
{
    public class BufferPool
    {
        private static List<BufferPool> _mPools = new List<BufferPool>();

        public static List<BufferPool> Pools { get { return _mPools; } set { _mPools = value; } }

        private readonly string _mName;

        private readonly int _mInitialCapacity;
        private readonly int _mBufferSize;

        private int _mMisses;

        private readonly Queue<byte[]> _mFreeBuffers;

        public void GetInfo(out string name, out int freeCount, out int initialCapacity, out int currentCapacity, out int bufferSize, out int misses)
        {
            lock (this)
            {
                name = _mName;
                freeCount = _mFreeBuffers.Count;
                initialCapacity = _mInitialCapacity;
                currentCapacity = _mInitialCapacity * (1 + _mMisses);
                bufferSize = _mBufferSize;
                misses = _mMisses;
            }
        }

        public BufferPool(string name, int initialCapacity, int bufferSize)
        {
            _mName = name;

            _mInitialCapacity = initialCapacity;
            _mBufferSize = bufferSize;

            _mFreeBuffers = new Queue<byte[]>(initialCapacity);

            for (int i = 0; i < initialCapacity; ++i)
                _mFreeBuffers.Enqueue(new byte[bufferSize]);

            lock (_mPools)
                _mPools.Add(this);
        }

        public byte[] AcquireBuffer()
        {
            lock (this)
            {
                if (_mFreeBuffers.Count > 0)
                    return _mFreeBuffers.Dequeue();

                ++_mMisses;

                for (int i = 0; i < _mInitialCapacity; ++i)
                    _mFreeBuffers.Enqueue(new byte[_mBufferSize]);

                return _mFreeBuffers.Dequeue();
            }
        }

        public void ReleaseBuffer(byte[] buffer)
        {
            if (buffer == null)
                return;

            lock (this)
                _mFreeBuffers.Enqueue(buffer);
        }

        public void Free()
        {
            lock (_mPools)
                _mPools.Remove(this);
        }
    }
}