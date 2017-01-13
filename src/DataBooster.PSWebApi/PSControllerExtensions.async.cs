// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;
using System.ComponentModel;

namespace DataBooster.PSWebApi
{
	public static partial class PSControllerExtensions
	{
		/// <summary>
		/// Asynchronously invokes a PowerShell script by using the supplied input parameters.
		/// </summary>
		/// <param name="apiController">The ApiController. This is an extension method to ApiController, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="scriptPath">The fully qualified location of the PowerShell script to be run.</param>
		/// <param name="parameters">A set of parameters to the PowerShell script. The parameter names and values are taken from the keys and values of a collection.</param>
		/// <param name="cancellationToken">The cancellation token can be used to request that the operation be abandoned before completing the execution. Exceptions will be reported via the returned Task object.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async static Task<HttpResponseMessage> InvokePowerShellAsync(this ApiController apiController, string scriptPath, IEnumerable<KeyValuePair<string, object>> parameters, CancellationToken cancellationToken)
		{
			PSContentNegotiator contentNegotiator = new PSContentNegotiator(apiController.Request);
			PSConverterRegistry converter = contentNegotiator.NegotiatedPsConverter;
			Encoding encoding = contentNegotiator.NegotiatedEncoding;

			if (converter == null)
				throw new HttpResponseException(HttpStatusCode.NotAcceptable);

			using (PowerShell ps = PowerShell.Create())
			{
				ps.RunspacePool = _runspacePool;
				ps.AddCommand("Set-Location").AddParameter("LiteralPath", Path.GetDirectoryName(scriptPath));
				ps/*.AddStatement()*/.AddCommand(scriptPath, true).Commands.AddParameters(parameters);

				if (!string.IsNullOrWhiteSpace(converter.ConversionCmdlet))
					ps.AddCommand(converter.ConversionCmdlet, true).Commands.AddParameters(converter.CmdletParameters);

				try
				{
					string stringResult = GetPsResult(await ps.InvokeAsync(cancellationToken).ConfigureAwait(false), encoding);

					ps.CheckErrors(cancellationToken);

					StringContent responseContent = new StringContent(stringResult, encoding, contentNegotiator.NegotiatedMediaType.MediaType);

					responseContent.Headers.SetContentHeader(ps.Streams);

					return new HttpResponseMessage(string.IsNullOrEmpty(stringResult) ? HttpStatusCode.NoContent : HttpStatusCode.OK) { Content = responseContent };
				}
				catch (CommandNotFoundException)
				{
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}
			}
		}

		private static Task<IList<PSObject>> InvokeAsync(this PowerShell ps, CancellationToken cancellationToken)
		{
			CancellationTokenRegistration ctr;

			if (cancellationToken.CanBeCanceled)
			{
				if (cancellationToken.IsCancellationRequested)
					return cancellationToken.AsCanceledTask<IList<PSObject>>();

				ctr = cancellationToken.Register(p => { ((PowerShell)p).BeginStop((ar) => { }, null); }, ps);
			}
			else
				ctr = new CancellationTokenRegistration();

			var taskFactory = new TaskFactory<IList<PSObject>>(cancellationToken);
			var task = taskFactory.FromAsync((callback, state) => ps.BeginInvoke<object>(null, null, callback, state), ps.EndInvoke, null);

			task.ContinueWith((antecedent) => { try { ctr.Dispose(); } catch { } });

			return task;
		}

		/// <summary>
		/// Asynchronously invokes a Windows batch file or executable file by using a set of command-line arguments.
		/// </summary>
		/// <param name="apiController">The ApiController. This is an extension method to ApiController, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="scriptPath">The fully qualified location of an application file (batch file or executable file) to be executed.</param>
		/// <param name="arguments">Command-line arguments to pass when starting the process.</param>
		/// <param name="cancellationToken">The cancellation token can be used to request that the operation be abandoned before completing the execution. Exceptions will be reported via the returned Task object.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public static async Task<HttpResponseMessage> InvokeCmdAsync(this ApiController apiController, string scriptPath, string arguments, CancellationToken cancellationToken)
		{
			PSContentNegotiator contentNegotiator = new PSContentNegotiator(apiController.Request);
			Encoding encoding = contentNegotiator.NegotiatedEncoding;

			using (CmdProcess cmd = new CmdProcess(scriptPath, arguments) { OutputEncoding = encoding })
			{
				try
				{
					int exitCode = await cmd.ExecuteAsync(cancellationToken).ConfigureAwait(false);
					string responseString = cmd.GetStandardError();
					HttpStatusCode httpStatusCode;

					if (exitCode == 0 && string.IsNullOrEmpty(responseString))
					{
						responseString = cmd.GetStandardOutput();
						httpStatusCode = string.IsNullOrEmpty(responseString) ? HttpStatusCode.NoContent : HttpStatusCode.OK;
					}
					else
						httpStatusCode = HttpStatusCode.InternalServerError;

					StringContent responseContent = new StringContent(responseString, encoding, contentNegotiator.NegotiatedMediaType.MediaType);
					responseContent.Headers.Add("Exit-Code", exitCode.ToString());

					return new HttpResponseMessage(httpStatusCode) { Content = responseContent };
				}
				catch (Win32Exception e)
				{
					switch (e.NativeErrorCode)
					{
						case 2:		// ERROR_FILE_NOT_FOUND
						case 267:	// ERROR_DIRECTORY
							return new HttpResponseMessage(HttpStatusCode.NotFound);
						default:
							throw;
					}
				}
			}
		}

		internal static Task<TResult> AsCanceledTask<TResult>(this CancellationToken cancellationToken, TaskCompletionSource<TResult> taskCompletionSource = null)
		{
			if (!cancellationToken.IsCancellationRequested)
				throw new InvalidOperationException();

			if (taskCompletionSource == null)
				taskCompletionSource = new TaskCompletionSource<TResult>();

			taskCompletionSource.TrySetCanceled(/*cancellationToken*/);

			return taskCompletionSource.Task;
		}
	}
}
