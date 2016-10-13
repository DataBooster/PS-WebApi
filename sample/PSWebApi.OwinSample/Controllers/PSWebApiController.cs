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
			string allArguments = this.Request.BuildCmdArguments(argumentsFromBody, ConfigHelper.CmdForceArgumentQuote);

			return this.InvokeCmd(script.LocalFullPath(), allArguments, ConfigHelper.CmdTimeoutSeconds);
		}
	}
}
