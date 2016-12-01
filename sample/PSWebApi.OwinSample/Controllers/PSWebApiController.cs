using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample.Controllers
{
	[CustomAuthorize]
	public class PSWebApiController : ApiController
	{
		[AcceptVerbs("GET", "POST", "PUT", "DELETE")]
		public Task<HttpResponseMessage> InvokePS(string script, Dictionary<string, object> parametersFromBody, CancellationToken cancellationToken)
		{
			IEnumerable<KeyValuePair<string, object>> allParameters = this.Request.GatherInputParameters(parametersFromBody);

			return this.InvokePowerShellAsync(script.LocalFullPath(), allParameters, cancellationToken);
		}

		[AcceptVerbs("GET", "POST", "PUT", "DELETE")]
		public HttpResponseMessage InvokeCMD(string script, JToken argumentsFromBody)
		{
			string physicalFullPath = script.LocalFullPath();
			CmdArgumentResolver argResolver = new CmdArgumentResolver(Path.GetExtension(physicalFullPath));
			string allArguments = argResolver.GatherInputArguments(this.Request, argumentsFromBody, ConfigHelper.CmdForceArgumentQuote);

			return this.InvokeCmd(physicalFullPath, allArguments, ConfigHelper.CmdTimeoutSeconds);
		}
	}
}
