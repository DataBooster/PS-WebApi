using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample
{
	public class CmdArgumentResolver
	{
		private readonly string _fileExtension;

		public CmdArgumentResolver(string fileExtension)
		{
			_fileExtension = (fileExtension == null) ? string.Empty : fileExtension.Trim().ToUpperInvariant();
		}

		public virtual string Quote(string rawArg, bool forceQuote = false)
		{
			if (string.IsNullOrWhiteSpace(rawArg))
				return "\"" + (rawArg ?? string.Empty) + "\"";

			switch (_fileExtension)
			{
				case ".EXE": return CmdArgumentsBuilder.QuoteExeArgument(rawArg, forceQuote);
				case ".BAT": return CmdArgumentsBuilder.QuoteBatArgument(rawArg, forceQuote);
				default: return rawArg;
			}
		}

		public string GatherInputArguments(HttpRequestMessage request, JToken argumentsFromBody, bool forceQuote)
		{
			if (request == null)
				throw new ArgumentNullException("request");

			CmdArgumentsBuilder argsBuilder = new CmdArgumentsBuilder();

			return argsBuilder.AddFromQueryString(request).Add(argumentsFromBody).ToString((string arg) => Quote(arg, forceQuote));
		}
	}
}