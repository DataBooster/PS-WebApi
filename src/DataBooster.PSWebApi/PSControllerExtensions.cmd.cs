// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace DataBooster.PSWebApi
{
	public static partial class PSControllerExtensions
	{
		/// <summary>
		/// Consolidates all the arguments from uri query string and HTTP POST body (if any), as a single string of Command-line arguments.
		/// </summary>
		/// <param name="request">The HTTP request where the uri query string part of arguments will be extracted from. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="argsFromBody">A set of raw arguments from HTTP POST body.</param>
		/// <param name="funcQuoteArgument">A transform function to apply to each raw argument.</param>
		/// <returns>A single string of Command-line arguments.</returns>
		public static string BuildCmdArguments(this HttpRequestMessage request, IEnumerable<string> argsFromBody, Func<string, string> funcQuoteArgument)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.AddFromQueryString(request).Add(argsFromBody).ToString(funcQuoteArgument);
		}

		/// <summary>
		/// Consolidates all the arguments from uri query string and HTTP POST body (if any), as a single string of Command-line arguments.
		/// </summary>
		/// <param name="request">The HTTP request where the uri query string part of arguments will be extracted from. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="argsFromBody">A set of raw arguments from HTTP POST body.</param>
		/// <param name="funcQuoteArgument">A transform function to apply to each raw argument.</param>
		/// <returns>A single string of Command-line arguments.</returns>
		public static string BuildCmdArguments(this HttpRequestMessage request, IEnumerable<KeyValuePair<string, object>> argsFromBody, Func<string, string> funcQuoteArgument)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.AddFromQueryString(request).Add(argsFromBody).ToString(funcQuoteArgument);
		}

		/// <summary>
		/// Consolidates all the arguments from uri query string and HTTP POST body (if any), as a single string of Command-line arguments.
		/// </summary>
		/// <param name="request">The HTTP request where the uri query string part of arguments will be extracted from. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="argsFromBody">A set of raw arguments from HTTP POST body (JSON).</param>
		/// <param name="funcQuoteArgument">A transform function to apply to each raw argument.</param>
		/// <returns>A single string of Command-line arguments.</returns>
		public static string BuildCmdArguments(this HttpRequestMessage request, JToken argsFromBody, Func<string, string> funcQuoteArgument)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.AddFromQueryString(request).Add(argsFromBody).ToString(funcQuoteArgument);
		}

		/// <summary>
		/// Synchronously invokes a Windows batch file or executable file by using a set of command-line arguments.
		/// </summary>
		/// <param name="apiController">The ApiController. This is an extension method to ApiController, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="scriptPath">The fully qualified location of an application file (batch file or executable file) to be executed.</param>
		/// <param name="arguments">Command-line arguments to pass when starting the process.</param>
		/// <param name="timeoutSeconds">The time in seconds to wait for the command to execute before terminating the attempt to execute a command and generating an error.</param>
		/// <returns>A complete HttpResponseMessage contains the standard output (stdout) if the application runs successfully, Otherwise, the standard error (stderr).</returns>
		public static HttpResponseMessage InvokeCmd(this ApiController apiController, string scriptPath, string arguments, int timeoutSeconds = Timeout.Infinite)
		{
			PSContentNegotiator contentNegotiator = new PSContentNegotiator(apiController.Request);
			Encoding encoding = contentNegotiator.NegotiatedEncoding;

			using (CmdProcess cmd = new CmdProcess(scriptPath, arguments) { OutputEncoding = encoding })
			{
				try
				{
					int exitCode = cmd.Execute(timeoutSeconds);
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
	}
}
