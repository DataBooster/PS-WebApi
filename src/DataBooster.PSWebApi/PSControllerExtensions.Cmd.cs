using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;

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

		public static HttpResponseMessage InvokeCmd(this ApiController apiController, string scriptPath, IEnumerable<string> arguments)
		{
			return null;
		}

		#endregion
	}
}
