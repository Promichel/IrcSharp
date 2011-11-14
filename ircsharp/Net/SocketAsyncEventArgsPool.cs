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
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace IrcSharp.Net
{
	public class SocketAsyncEventArgsPool
	{
		private readonly ConcurrentStack<SocketAsyncEventArgs> _mEventsPool;

		public SocketAsyncEventArgsPool(int numConnection)
		{
			_mEventsPool = new ConcurrentStack<SocketAsyncEventArgs>();
		}

		public SocketAsyncEventArgs Pop()
		{		
			if(_mEventsPool.IsEmpty)
				return new SocketAsyncEventArgs();

			SocketAsyncEventArgs popped;
			_mEventsPool.TryPop(out popped);

			return popped;			
		}

		public void Push(SocketAsyncEventArgs item)
		{
			if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
			
            _mEventsPool.Push(item);			
		}

		public int Count
		{
			get{return _mEventsPool.Count;}
		}
		
		public void Dispose()
		{
			foreach (SocketAsyncEventArgs e in _mEventsPool)
			{
				e.Dispose();
			}

			_mEventsPool.Clear();
		}
	}
}
