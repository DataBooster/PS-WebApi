// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// Encapsulates the detail operations (synchronous and asynchronous) on executing a local Process.
	/// Please note that every instance of this class can be executed only once.
	/// </summary>
	public class CmdProcess : IDisposable
	{
		private readonly ProcessStartInfo _processStartInfo;

		/// <summary>
		/// Gets or sets the encoding of StandardOutput and StandardError.
		/// Require this encoding to exactly match the Console.OutputEncoding within the process if it's set.
		/// </summary>
		public Encoding OutputEncoding
		{
			get { return _processStartInfo.StandardOutputEncoding; }
			set { _processStartInfo.StandardErrorEncoding = _processStartInfo.StandardOutputEncoding = value; }
		}
		private readonly Process _process;
		private volatile bool _started, _disposed, _canceled;
		private volatile TaskCompletionSource<int> _workingTaskCompletionSource;

		private readonly StringBuilder _sbStandardOutput, _sbStandardError;
		private readonly ManualResetEventSlim _waitStandardOutput, _waitStandardError;
		private readonly WaitHandle[] _waitRedirectionHandles;

		/// <summary>
		/// Initializes a new instance of the CmdProcess class with the name of an application and a set of command-line arguments.
		/// </summary>
		/// <param name="filePath">The path of an application file to run in the process.</param>
		/// <param name="arguments">Command-line arguments to pass when starting the process.</param>
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
			_started = _disposed = _canceled = false;

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
		/// Synchronously starts the associated process and makes the current thread wait until the associated process terminates or times out.
		/// Please note that every instance of this class can be executed only once.
		/// </summary>
		/// <param name="timeoutSeconds">The amount of time, in seconds, to wait for the associated process to exit. The default is Timeout.Infinite(-1).</param>
		/// <returns>The exit code that the associated process specified when it terminated. If the process cannot be completed within the timeoutSeconds, a TimeoutException is thrown.</returns>
		public int Execute(int timeoutSeconds = Timeout.Infinite)
		{
			if (_started)
				throw new InvalidOperationException();

			_process.Start();
			_started = true;
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();

			int millisecondsTimeout = (timeoutSeconds == Timeout.Infinite) ? Timeout.Infinite : timeoutSeconds * 1000;

			_process.WaitForExit(millisecondsTimeout);
			WaitHandle.WaitAll(_waitRedirectionHandles, millisecondsTimeout);

			if (_process.HasExited)
				return _process.ExitCode;
			else
				throw new TimeoutException(string.Format("\"{0}\" timed out in {1} seconds.", Path.GetFileName(_processStartInfo.FileName), timeoutSeconds));
		}

		/// <summary>
		/// Starts the associated process with a cancellation token as an asynchronous operation.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the process.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public Task<int> ExecuteAsync(CancellationToken cancellationToken)
		{
			if (_started)
				throw new InvalidOperationException();

			_workingTaskCompletionSource = new TaskCompletionSource<int>();
			CancellationTokenRegistration ctr;

			if (cancellationToken.CanBeCanceled)
			{
				if (cancellationToken.IsCancellationRequested)
					return cancellationToken.AsCanceledTask<int>(_workingTaskCompletionSource);

				ctr = cancellationToken.Register(() =>
					{
						lock (_process)
						{
							if (_started)
								_process.Kill();
							_canceled = true;
						}
					});
			}
			else
				ctr = new CancellationTokenRegistration();

			if (!_process.EnableRaisingEvents)
			{
				_process.EnableRaisingEvents = true;
				_process.Exited += OnProcess_Exited;
			}

			_workingTaskCompletionSource.Task.ContinueWith((antecedent) => { try { ctr.Dispose(); } catch { } });

			lock (_process)
			{
				if (_canceled)
					_workingTaskCompletionSource.TrySetCanceled(/*cancellationToken*/);
				else
				{
					_process.Start();
					_started = true;
					_process.BeginOutputReadLine();
					_process.BeginErrorReadLine();
				}
			}

			return _workingTaskCompletionSource.Task;
		}

		private void OnProcess_Exited(object sender, EventArgs e)
		{
			if (_workingTaskCompletionSource != null)
			{
				if (_canceled && _process.ExitCode == -1)
					_workingTaskCompletionSource.TrySetCanceled();
				else
				{
					WaitHandle.WaitAll(_waitRedirectionHandles);
					_workingTaskCompletionSource.TrySetResult(_process.ExitCode);
				}
			}
		}

		/// <summary>
		/// Get the textual output of the application.
		/// </summary>
		/// <returns>A string whose value is the textual output of the application.</returns>
		public string GetStandardOutput()
		{
			return _sbStandardOutput.ToString();
		}

		/// <summary>
		/// Get the error output of the application.
		/// </summary>
		/// <returns>A string whose value is the error output of the application.</returns>
		public string GetStandardError()
		{
			return _sbStandardError.ToString();
		}

		#region IDisposable Members
		/// <summary>
		/// Releases all resources used by the CmdProcess.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// <para>This API supports the product infrastructure and is not intended to be used directly from your code.</para>
		/// <para>Release all resources used by this CmdProcess.</para>
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
