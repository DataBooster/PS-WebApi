// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;

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

		private readonly StringBuilder _sbStandardOutput, _sbStandardError;
		private readonly ManualResetEventSlim _waitStandardOutput, _waitStandardError;
		private readonly WaitHandle[] _waitRedirectionHandles;

		public CmdProcess(string filePath, string arguments = null)
		{
			if (filePath != null)
				filePath = filePath.Trim();

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("fileName");

			_processStartInfo = new ProcessStartInfo(filePath) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true, WorkingDirectory = Path.GetDirectoryName(filePath) };

			if (!string.IsNullOrWhiteSpace(arguments))
				_processStartInfo.Arguments = arguments;

			_sbStandardOutput = new StringBuilder();
			_sbStandardError = new StringBuilder();
			_waitStandardOutput = new ManualResetEventSlim(false);
			_waitStandardError = new ManualResetEventSlim(false);
			_waitRedirectionHandles = new WaitHandle[] { _waitStandardOutput.WaitHandle, _waitStandardError.WaitHandle };

			_process = new Process() { StartInfo = _processStartInfo };
			_started = _disposed = false;

			_process.OutputDataReceived += (sender, e) =>
				{
					if (e.Data == null)
						_waitStandardOutput.Set();
					else
						_sbStandardOutput.AppendLine(e.Data);
				};
			_process.ErrorDataReceived += (sender, e) =>
				{
					if (e.Data == null)
						_waitStandardError.Set();
					else
						_sbStandardError.AppendLine(e.Data);
				};
		}

		/// <summary>
		/// Starts the associated process and makes the current thread wait until the associated process terminates or times out.
		/// </summary>
		/// <param name="timeoutSeconds">The amount of time, in seconds, to wait for the associated process to exit. The default is Timeout.Infinite(-1).</param>
		/// <returns>The exit code that the associated process specified when it terminated. If the process cannot be completed within the timeoutSeconds, a TimeoutException is thrown.</returns>
		public int Execute(int timeoutSeconds = Timeout.Infinite)
		{
			if (!_started)
			{
				_process.Start();
				_process.BeginOutputReadLine();
				_process.BeginErrorReadLine();
				_started = true;
			}

			int millisecondsTimeout = (timeoutSeconds == Timeout.Infinite) ? Timeout.Infinite : timeoutSeconds * 1000;

			_process.WaitForExit(millisecondsTimeout);
			WaitHandle.WaitAll(_waitRedirectionHandles, millisecondsTimeout);

			if (_process.HasExited)
				return _process.ExitCode;
			else
				throw new TimeoutException(string.Format("\"{0}\" timed out in {1} seconds.", Path.GetFileName(_processStartInfo.FileName), timeoutSeconds));
		}

		public string GetStandardOutput()
		{
			return _sbStandardOutput.ToString();
		}

		public string GetStandardError()
		{
			return _sbStandardError.ToString();
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

					_waitStandardOutput.Dispose();
					_waitStandardError.Dispose();
				}

				_disposed = true;
			}
		}
		#endregion
	}
}
