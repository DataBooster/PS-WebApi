// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DataBooster.PSWebApi
{
	public static partial class PSControllerExtensions
	{
		#region Command Line

		public static string BuildCmdArguments(this HttpRequestMessage request, IEnumerable<string> argsFromBody, bool forceArgumentQuote = false)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.Add(argsFromBody).AddFromQueryString(request).ToString(forceArgumentQuote);
		}

		public static string BuildCmdArguments(this HttpRequestMessage request, IEnumerable<KeyValuePair<string, object>> argsFromBody, bool forceArgumentQuote = false)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.Add(argsFromBody).AddFromQueryString(request).ToString(forceArgumentQuote);
		}

		public static string BuildCmdArguments(this HttpRequestMessage request, JToken argsFromBody, bool forceArgumentQuote = false)
		{
			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();
			return argsBuilder.Add(argsFromBody).AddFromQueryString(request).ToString(forceArgumentQuote);
		}

		public static HttpResponseMessage InvokeCmd(this ApiController apiController, string scriptPath, string arguments, int timeoutSeconds = Timeout.Infinite)
		{
			PSContentNegotiator contentNegotiator = new PSContentNegotiator(apiController.Request);
			Encoding encoding = contentNegotiator.NegotiatedEncoding;

			using (CmdProcess cmd = new CmdProcess(scriptPath, arguments))
			{
				int exitCode = cmd.Execute(timeoutSeconds);
				string responseString = cmd.ReadStandardError();
				HttpStatusCode httpStatusCode;

				if (exitCode == 0 && string.IsNullOrEmpty(responseString))
				{
					responseString = cmd.ReadStandardOutput();
					httpStatusCode = string.IsNullOrEmpty(responseString) ? HttpStatusCode.NoContent : HttpStatusCode.OK;
				}
				else
					httpStatusCode = HttpStatusCode.InternalServerError;

				StringContent responseContent = new StringContent(responseString, encoding, contentNegotiator.NegotiatedMediaType.MediaType);
				responseContent.Headers.Add("Exit-Code", exitCode.ToString());

				return new HttpResponseMessage(httpStatusCode) { Content = responseContent };
			}
		}

		#endregion
	}
}
