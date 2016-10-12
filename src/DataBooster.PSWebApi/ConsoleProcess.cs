// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DataBooster.PSWebApi
{
	public class ConsoleProcess : IDisposable
	{
		private static readonly Regex _rgxNoneedQuotes = new Regex(@"^((\\\S|\\$|[^""\s\\])|(""(\\.|""""|[^""\\])*""))+$");
		private static readonly Regex _rgxEscapeBackslash = new Regex(@"(\\+)(?=""|$)");
		private readonly ProcessStartInfo _processStartInfo;

		public Encoding OutputEncoding
		{
			get { return _processStartInfo.StandardOutputEncoding; }
			set { _processStartInfo.StandardErrorEncoding = _processStartInfo.StandardOutputEncoding = value; }
		}

		private readonly Process _process;
		private bool _started, _disposed;

		public ConsoleProcess(string filePath, string arguments = null)
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

		public ConsoleProcess(string filePath, IEnumerable<string> args, bool forceArgumentQuote = false)
			: this(filePath, JoinArguments(args, forceArgumentQuote))
		{
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

		private static string JoinArguments(IEnumerable<string> args, bool forceQuote = false)
		{
			if (args == null)
				return null;

			return string.Join(" ", args.Select(a => EscapeArgument(a, forceQuote)));
		}

		private static string EscapeArgument(string arg, bool forceQuote = false)
		{
			if (string.IsNullOrWhiteSpace(arg))
				return "\"" + (arg ?? string.Empty) + "\"";

			if (forceQuote == false && _rgxNoneedQuotes.IsMatch(arg))
				return arg;

			string escArg = _rgxEscapeBackslash.Replace(arg, m => m.Groups[1].Value + m.Groups[1].Value);

			return "\"" + escArg.Replace("\"", "\\\"") + "\"";
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
