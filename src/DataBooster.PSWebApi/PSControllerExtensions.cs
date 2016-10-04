// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DataBooster.PSWebApi
{
	public static class PSControllerExtensions
	{
		private static readonly RunspacePool _runspacePool;
		private static readonly string _escapedNewLine;

		static PSControllerExtensions()
		{
			_runspacePool = RunspaceFactory.CreateRunspacePool();
			_runspacePool.Open();
			_escapedNewLine = Uri.EscapeDataString(Environment.NewLine).ToLower();
		}

		public static IEnumerable<KeyValuePair<string, object>> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody)
		{
			var queryStrings = request.GetQueryNameValuePairs().Select(entry => new KeyValuePair<string, object>(entry.Key, entry.Value));

			return (parametersFromBody == null) ? queryStrings : parametersFromBody.Concat(queryStrings);
		}

		public static HttpResponseMessage InvokePowerShell(this ApiController apiController, string scriptPath, IEnumerable<KeyValuePair<string, object>> parameters)
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
				ps.AddStatement().AddCommand(scriptPath, true).Commands.AddParameters(parameters);

				if (!string.IsNullOrWhiteSpace(converter.ConversionCmdlet))
					ps.AddCommand(converter.ConversionCmdlet, true).Commands.AddParameters(converter.CmdletParameters);

				string stringResult = GetPsResult(ps.Invoke(), encoding);
				StringContent responseContent = new StringContent(stringResult, encoding, contentNegotiator.NegotiatedMediaType.MediaType);

				responseContent.Headers.SetContentHeader(ps.Streams);

				return new HttpResponseMessage(string.IsNullOrEmpty(stringResult) ? HttpStatusCode.NoContent : HttpStatusCode.OK) { Content = responseContent };
			}
		}

		private static string GetPsResult(Collection<PSObject> psResult, Encoding encoding)
		{
			if (psResult == null || psResult.Count == 0)
				return string.Empty;

			if (psResult.Count == 1)
				return GetOneResult(psResult[0], encoding);

			StringBuilder stringBuilder = new StringBuilder();
			string oneString;

			for (int i = 0; i < psResult.Count; i++)
			{
				oneString = GetOneResult(psResult[i], encoding);

				if (i > 0)
					stringBuilder.AppendLine();

				if (!string.IsNullOrEmpty(oneString))
					stringBuilder.Append(oneString);
			}

			return stringBuilder.ToString();
		}

		private static string GetOneResult(PSObject psObject, Encoding encoding)
		{
			if (psObject == null)
				return string.Empty;

			object baseObject = psObject.BaseObject;

			if (baseObject == null)
				return string.Empty;

			string stringResult = baseObject as string;

			if (stringResult != null)
				return stringResult;

			XmlDocument xml = baseObject as XmlDocument;

			if (xml != null)
			{
				using (XmlStringWriter sw = new XmlStringWriter(encoding))
				{
					xml.Save(sw);
					return sw.ToString();
				}
			}

			return psObject.ToString();
		}

		private static void SetContentHeader(this HttpContentHeaders headers, PSDataStreams invokedStatus)
		{
			if (invokedStatus == null)
				return;

			if (invokedStatus.Error != null && invokedStatus.Error.Count > 0)
			{
				if (invokedStatus.Error.Count == 1)
					throw invokedStatus.Error[0].Exception;
				else
					throw new AggregateException(string.Join(Environment.NewLine, invokedStatus.Error.Select(e => e.Exception.Message)),
						invokedStatus.Error.Select(e => e.Exception));
			}

			if (headers == null)
				return;

			if (invokedStatus.Warning != null && invokedStatus.Warning.Count > 0)
				headers.Add("Ps-Warning", invokedStatus.Warning.Select(w => w.ToString().EscapeNewLine()));

			if (invokedStatus.Verbose != null && invokedStatus.Verbose.Count > 0)
				headers.Add("Ps-Verbose", invokedStatus.Verbose.Select(v => v.ToString().EscapeNewLine()));

			if (invokedStatus.Debug != null && invokedStatus.Debug.Count > 0)
				headers.Add("Ps-Debug", invokedStatus.Debug.Select(d => d.ToString().EscapeNewLine()));
		}

		private static string EscapeNewLine(this string multiline)
		{
			return string.IsNullOrEmpty(multiline) ? multiline : multiline.Replace(Environment.NewLine, _escapedNewLine);
		}

		private static void AddParameters(this PSCommand psCommand, IEnumerable<KeyValuePair<string, object>> parameters)
		{
			if (parameters == null)
				return;

			foreach (var kvp in parameters)
			{
				if (string.IsNullOrWhiteSpace(kvp.Key))
				{
					if (kvp.Value != null)
						psCommand.AddArgument(kvp.Value);
				}
				else
					psCommand.AddParameter(kvp.Key, kvp.Value);
			}
		}

		public static string GetUserName(this ApiController apiController)
		{
			if (apiController == null)
				throw new ArgumentNullException("apiController");

			if (apiController.User == null || apiController.User.Identity == null)
				return null;

			return apiController.User.Identity.Name;
		}

		public static string GetUserName(this HttpActionContext actionContext)
		{
			ApiController apiController = actionContext.ControllerContext.Controller as ApiController;

			if (apiController == null)
				throw new ArgumentNullException("actionContext.ControllerContext.Controller");

			return GetUserName(apiController);
		}
	}
}
