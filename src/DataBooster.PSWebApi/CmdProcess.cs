// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace DataBooster.PSWebApi
{
	public class CmdProcess : IDisposable
	{
		private readonly ProcessStartInfo _processStartInfo;

		public Encoding OutputEncoding
		{
			get { return _processStartInfo.StandardOutputEncoding; }
			set { _processStartInfo.StandardErrorEncoding = _processStartInfo.StandardOutputEncoding = value; }
		}

		private readonly Process _process;
		private bool _started, _disposed;

		public CmdProcess(string filePath, string arguments = null)
		{
			if (filePath != null)
				filePath = filePath.Trim();

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("fileName");

			_processStartInfo = new ProcessStartInfo(filePath) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true, WorkingDirectory = Path.GetDirectoryName(filePath) };

			if (!string.IsNullOrWhiteSpace(arguments))
				_processStartInfo.Arguments = arguments;

			_process = new Process() { StartInfo = _processStartInfo };
			_started = _disposed = false;
		}

		public CmdProcess(string filePath, IEnumerable<string> args, bool forceArgumentQuote = false)
			: this(filePath)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();

			argsBuilder.Add(args);
			_processStartInfo.Arguments = argsBuilder.ToString(forceArgumentQuote);
		}

		public int Execute(int timeoutSeconds = Timeout.Infinite)
		{
			if (!_started)
			{
				_process.Start();
				_started = true;
			}

			_process.WaitForExit(timeoutSeconds * 1000);

			return _process.HasExited ? _process.ExitCode : int.MinValue;
		}

		public string ReadStandardOutput()
		{
			return _process.StandardOutput.ReadToEnd();
		}

		public string ReadStandardError()
		{
			return _process.StandardError.ReadToEnd();
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_started && !_process.HasExited)
						_process.Kill();

					_process.Dispose();
				}

				_disposed = true;
			}
		}
		#endregion
	}
}
