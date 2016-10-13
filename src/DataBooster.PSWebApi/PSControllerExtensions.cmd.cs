// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Linq;
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

		public static IEnumerable<string> GatherCmdArguments(this HttpRequestMessage request, IEnumerable<KeyValuePair<string, object>> argsFromBody)
		{
			return GatherCmdArguments(request, TransformDictionaryToCmdArguments(argsFromBody));
		}

		public static IEnumerable<string> GatherCmdArguments(this HttpRequestMessage request, IEnumerable<string> argsFromBody)
		{
			var queryStrings = TransformDictionaryToCmdArguments(request.GetQueryNameValuePairs());

			return (argsFromBody == null) ? queryStrings : argsFromBody.Concat(queryStrings);
		}

		public static IEnumerable<string> GatherCmdArguments(this HttpRequestMessage request, JToken argsFromBody)
		{
			IEnumerable<string> args = null;

			if (argsFromBody != null)
			{
				JArray jArray = argsFromBody as JArray;

				if (jArray != null)
					args = jArray.Select(a => a.ToString());
				else
				{
					JObject jObject = argsFromBody as JObject;

					if (jObject != null)
						args = TransformDictionaryToCmdArguments<JToken>(jObject);
					else
					{
						JValue jValue = argsFromBody as JValue;

						if (jValue != null)
							args = new string[] { jValue.ToString() };
						else
							throw new InvalidCastException(argsFromBody.GetType().ToString());
					}
				}
			}

			var queryStrings = TransformDictionaryToCmdArguments(request.GetQueryNameValuePairs());

			return (args == null) ? queryStrings : args.Concat(queryStrings);
		}

		private static IEnumerable<string> TransformDictionaryToCmdArguments<T>(IEnumerable<KeyValuePair<string, T>> parameters)
		{
			if (parameters == null)
				yield break;

			foreach (var kvp in parameters)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Key))
					yield return kvp.Key;

				yield return kvp.Value.ToString();
			}
		}

		public static HttpResponseMessage InvokeCmd(this ApiController apiController, string scriptPath,
			IEnumerable<string> arguments, bool forceArgumentQuote = false, int timeoutSeconds = Timeout.Infinite)
		{
			PSContentNegotiator contentNegotiator = new PSContentNegotiator(apiController.Request);
			Encoding encoding = contentNegotiator.NegotiatedEncoding;

			using (CmdProcess cmd = new CmdProcess(scriptPath, arguments, forceArgumentQuote))
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
