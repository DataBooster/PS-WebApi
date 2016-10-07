using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace DataBooster.PSWebApi
{
	public class ConsoleProcess
	{
		private readonly ProcessStartInfo _processStartInfo;

		public Encoding OutputEncoding
		{
			get { return _processStartInfo.StandardOutputEncoding; }
			set { _processStartInfo.StandardErrorEncoding = _processStartInfo.StandardOutputEncoding = value; }
		}

		private Process _process;

		public ConsoleProcess(string filePath)
		{
			if (filePath != null)
				filePath = filePath.Trim();

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("fileName");

			_processStartInfo = new ProcessStartInfo(filePath) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true, WorkingDirectory = Path.GetDirectoryName(filePath) };
		}


	}
}
