// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;

namespace DataBooster.PSWebApi
{
	public static partial class PSControllerExtensions
	{
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
				ps.AddStatement().AddCommand(scriptPath, true).Commands.AddParameters(parameters);

				if (!string.IsNullOrWhiteSpace(converter.ConversionCmdlet))
					ps.AddCommand(converter.ConversionCmdlet, true).Commands.AddParameters(converter.CmdletParameters);

				string stringResult = GetPsResult(await ps.InvokeAsync(cancellationToken), encoding);

				ps.CheckErrors(cancellationToken.IsCancellationRequested);

				StringContent responseContent = new StringContent(stringResult, encoding, contentNegotiator.NegotiatedMediaType.MediaType);

				responseContent.Headers.SetContentHeader(ps.Streams);

				return new HttpResponseMessage(string.IsNullOrEmpty(stringResult) ? HttpStatusCode.NoContent : HttpStatusCode.OK) { Content = responseContent };
			}
		}

		private async static Task<IList<PSObject>> InvokeAsync(this PowerShell ps, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(p =>
				{
					try
					{
						((PowerShell)p).Stop();
					}
					catch
					{
					}
				}, ps))
			{
				return await Task.Run<IList<PSObject>>(() => ps.Invoke(), cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
