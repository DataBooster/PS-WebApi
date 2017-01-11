// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// Provides a set of static methods (extension methods) for invoking PowerShell scripts and batch/executable files from your Web API controller.
	/// </summary>
	public static partial class PSControllerExtensions
	{
		private static readonly RunspacePool _runspacePool;
		private static readonly string _escapedNewLine;

		static PSControllerExtensions()
		{
			_runspacePool = RunspaceFactory.CreateRunspacePool();
			_runspacePool.Open();
			_escapedNewLine = Uri.EscapeDataString(Environment.NewLine).ToLower();
		}

		/// <summary>
		/// Gathers input parameters from uri query string and concatenates with HTTP POST body.
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body.</param>
		/// <returns>An IEnumerable{KeyValuePair{string, object}} that contains the concatenated parameters from uri query string and POST body.</returns>
		public static IEnumerable<KeyValuePair<string, object>> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody)
		{
			var queryStrings = request.GetQueryNameValuePairs().Select(entry => new KeyValuePair<string, object>(entry.Key, entry.Value));

			return (parametersFromBody == null) ? queryStrings : queryStrings.Concat(parametersFromBody);
		}

		/// <summary>
		/// Synchronously invokes the PowerShell script by using the supplied input parameters.
		/// </summary>
		/// <param name="apiController">The ApiController. This is an extension method to ApiController, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="scriptPath">The fully qualified location of the PowerShell script to be run.</param>
		/// <param name="parameters">A set of parameters to the PowerShell script. The parameter names and values are taken from the keys and values of a collection.</param>
		/// <returns>A complete HttpResponseMessage contains result data returned by the PowerShell script.</returns>
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

				ps.CheckErrors();

				StringContent responseContent = new StringContent(stringResult, encoding, contentNegotiator.NegotiatedMediaType.MediaType);

				responseContent.Headers.SetContentHeader(ps.Streams);

				return new HttpResponseMessage(string.IsNullOrEmpty(stringResult) ? HttpStatusCode.NoContent : HttpStatusCode.OK) { Content = responseContent };
			}
		}

		private static string GetPsResult(IList<PSObject> psResult, Encoding encoding)
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

		private static void CheckErrors(this PowerShell ps, CancellationToken cancellationToken = default(CancellationToken))
		{
			PSDataCollection<ErrorRecord> errors = (ps.Streams == null) ? null : ps.Streams.Error;

			if (errors != null && errors.Count > 0)
			{
				if (errors.Count == 1)
					throw errors[0].Exception;
				else
					throw new AggregateException(string.Join(Environment.NewLine, errors.Select(e => e.Exception.Message)),
						errors.Select(e => e.Exception));
			}

			if (ps.HadErrors)
			{
				cancellationToken.ThrowIfCancellationRequested();	//	PipelineStoppedException();
				throw new InvalidPowerShellStateException();
			}
		}

		private static void SetContentHeader(this HttpContentHeaders headers, PSDataStreams invokedStatus)
		{
			if (invokedStatus == null || headers == null)
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
			string key;

			if (parameters == null)
				return;

			foreach (var kvp in parameters)
			{
				key = (kvp.Key == null) ? string.Empty : kvp.Key.Trim();

				if (key.Length > 0)
					psCommand.AddParameter(key, kvp.Value);
				else
					if (kvp.Value != null)
						psCommand.AddArgument(kvp.Value);
			}
		}

		/// <summary>
		/// Get current principal user name associated with this request.
		/// </summary>
		/// <param name="apiController">The ApiController. This is an extension method to ApiController, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <returns>The current principal user name associated with this request.</returns>
		public static string GetUserName(this ApiController apiController)
		{
			if (apiController == null)
				throw new ArgumentNullException("apiController");

			if (apiController.User == null || apiController.User.Identity == null)
				return null;

			return apiController.User.Identity.Name;
		}

		/// <summary>
		/// Get current principal user name associated with this request.
		/// </summary>
		/// <param name="actionContext">The HttpActionContext. This is an extension method to HttpActionContext, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <returns>The current principal user name associated with this request.</returns>
		public static string GetUserName(this HttpActionContext actionContext)
		{
			ApiController apiController = actionContext.ControllerContext.Controller as ApiController;

			if (apiController == null)
				throw new ArgumentNullException("actionContext.ControllerContext.Controller");

			return GetUserName(apiController);
		}

		/// <summary>
		/// Gets the value in current URL route data that is associated with a specified key.
		/// </summary>
		/// <param name="actionContext">The HttpActionContext. This is an extension method to HttpActionContext, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="urlPlaceholderName">The specified key in the URL route data.</param>
		/// <returns>The value that is associated with the specified key, or null if the key does not exist in URL route data.</returns>
		public static object GetRouteData(this HttpActionContext actionContext, string urlPlaceholderName)
		{
			if (string.IsNullOrWhiteSpace(urlPlaceholderName))
				throw new ArgumentNullException("urlPlaceholder");

			return actionContext.ControllerContext.RouteData.Values[urlPlaceholderName];
		}
	}
}
