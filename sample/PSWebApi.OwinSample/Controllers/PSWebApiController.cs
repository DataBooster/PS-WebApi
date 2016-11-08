using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample.Controllers
{
	[CustomAuthorize]
	public class PSWebApiController : ApiController
	{
		[AcceptVerbs("GET", "POST", "PUT", "DELETE")]
		public HttpResponseMessage InvokePS(string script, Dictionary<string, object> parametersFromBody)
		{
			IEnumerable<KeyValuePair<string, object>> allParameters = this.Request.GatherInputParameters(parametersFromBody);

			return this.InvokePowerShell(script.LocalFullPath(), allParameters);
		}

		[AcceptVerbs("GET", "POST", "PUT", "DELETE")]
		public HttpResponseMessage InvokeCMD(string script, JToken argumentsFromBody)
		{
			string physicalFullPath = script.LocalFullPath();
			string allArguments = this.Request.BuildCmdArguments(argumentsFromBody, (string arg) =>
				EscapeCmdArgument(Path.GetExtension(physicalFullPath), arg, ConfigHelper.CmdForceArgumentQuote));

			return this.InvokeCmd(physicalFullPath, allArguments, ConfigHelper.CmdTimeoutSeconds);
		}

		private string EscapeCmdArgument(string fileExtension, string arg, bool forceQuote)
		{
			if (string.IsNullOrWhiteSpace(arg))
				return "\"" + (arg ?? string.Empty) + "\"";

			switch (fileExtension.ToUpperInvariant())
			{
				case "EXE": return CmdArgumentsBuilder.EscapeExeArgument(arg, forceQuote);
				case "BAT": return CmdArgumentsBuilder.EscapeExeArgument(arg, forceQuote);
				default: return arg;
			}
		}
	}
}
