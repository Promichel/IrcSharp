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
using System.IO;

namespace IrcSharp
{
	public class Logger
	{
		private readonly StreamWriter _writeLog;
        private IrcServer _server;

		internal Logger(IrcServer server, string file)
		{
            _server = server;
			try
			{
				_writeLog = new StreamWriter(file, true);
				_writeLog.AutoFlush = true;
			}
			catch
			{
				_writeLog = null;
			}
		}

		~Logger()
		{
			try
			{
				_writeLog.Close();
			}
			catch
			{
			}
		}

        public void LogOnOneLine(LogLevel level, string format, bool header, params object[] arguments)
        {
            LogOnOneLine(level, string.Format(format, arguments), header);
        }

        public void LogOnOneLine(LogLevel level, string message, bool header)
        {
            LogToConsole(level, message, false, header);
            LogToFile(level, message, false, header);
        }

	    public void Log(LogLevel level, string format, params object[] arguments)
        {
			Log(level, string.Format(format, arguments));
        }

        public void Log(LogLevel level, string message)
		{
            LogToConsole(level, message, true);
			LogToFile(level, message, true);
		}

		private void LogToConsole(LogLevel level, string message, bool newLine, bool header = true)
		{
            if ((int)level >= Settings.Default.LogConsoleLevel)
            {
                if (newLine)
                    Console.WriteLine(Settings.Default.LogConsoleFormat, DateTime.Now, level.ToString().ToUpper(), message);
                else
                {
                    if (header)
                        Console.Write(Settings.Default.LogConsoleFormat, DateTime.Now, level.ToString().ToUpper(), message);
                    else
                        Console.Write("{0}", message);
                }
            }
		}

		private void LogToFile(LogLevel level, string message, bool newLine, bool header = true)
		{
            if ((int)level >= Settings.Default.LogFileLevel && _writeLog != null)
            {
                if (newLine)
                    _writeLog.WriteLine(Settings.Default.LogFileFormat, DateTime.Now, level.ToString().ToUpper(), message);
                else
                {
                    if (header)
                        _writeLog.Write(Settings.Default.LogFileFormat, DateTime.Now, level.ToString().ToUpper(), message);
                    else
                        _writeLog.Write("{0}", message);
                }
            }
		}

		public void Log(Exception ex)
		{
            Log(LogLevel.Debug, ex.ToString());
		}


		public enum LogLevel : int
		{
			Trivial = -1,
			Debug = 0,
			Info = 1,
			Warning = 2,
			Caution = 3,
			Notice = 4,
			Error = 5,
			Fatal = 6
		}
	}
}