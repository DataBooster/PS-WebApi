// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DataBooster.PSWebApi
{
	public class CmdArgumentsBuilder
	{
		private const string _argSeparator = " ";
		private static readonly Regex _rgxNoneedQuotes_Exe = new Regex(@"^((\\\S|\\$|[^""\s\\])|(""(\\.|""""|[^""\\])*""))+$");
		private static readonly Regex _rgxEscapeBackslash_Exe = new Regex(@"(\\+)(?=""|$)");
		private readonly List<string> _rawArguments;
		public Collection<string> RawArguments { get; private set; }

		public CmdArgumentsBuilder()
		{
			_rawArguments = new List<string>();
			RawArguments = new Collection<string>(_rawArguments);
		}

		public CmdArgumentsBuilder Add(string arg)
		{
			_rawArguments.Add(arg);
			return this;
		}

		public CmdArgumentsBuilder Add(IEnumerable<string> args)
		{
			if (args != null)
				_rawArguments.AddRange(args);

			return this;
		}

		public CmdArgumentsBuilder Add<T>(IEnumerable<KeyValuePair<string, T>> parameters)
		{
			string key;

			if (parameters == null)
				return this;

			foreach (var kvp in parameters)
			{
				key = (kvp.Key == null) ? string.Empty : kvp.Key.Trim();

				if (key.Length > 0)
					_rawArguments.Add(key);

				if (key.Length > 0 || !IsNullValue(kvp.Value))
					_rawArguments.Add((kvp.Value == null) ? string.Empty : kvp.Value.ToString());
			}

			return this;
		}

		public CmdArgumentsBuilder Add(JToken argsFromBody)
		{
			if (argsFromBody != null)
			{
				JArray jArray = argsFromBody as JArray;

				if (jArray != null)
					Add(jArray.Select(a => a.ToString()));
				else
				{
					JObject jObject = argsFromBody as JObject;

					if (jObject != null)
						Add<JToken>(jObject);
					else
					{
						JValue jValue = argsFromBody as JValue;

						if (jValue != null)
							Add(jValue.ToString());
						else
							throw new InvalidCastException(argsFromBody.GetType().ToString());
					}
				}
			}

			return this;
		}

		protected virtual bool IsNullValue<T>(T value)
		{
			if (value == null)
				return true;
			else
			{
				JToken jValue = value as JToken;
				return (jValue != null && jValue.Type == JTokenType.Null);
			}
		}

		public CmdArgumentsBuilder AddFromQueryString(HttpRequestMessage httpRequestMessage)
		{
			return httpRequestMessage == null ? this : Add(httpRequestMessage.GetQueryNameValuePairs());
		}

		public string ToString(Func<string, string> escapeArgument)
		{
			if (escapeArgument == null)
				escapeArgument = (string arg) => arg ?? string.Empty;

			return (_rawArguments.Count == 0) ? string.Empty :
				string.Join(_argSeparator, _rawArguments.Select(escapeArgument));
		}

		// https://msdn.microsoft.com/en-us/library/17w5ykft.aspx
		// https://msdn.microsoft.com/en-us/library/a1y7w461.aspx
		public static string EscapeExeArgument(string arg, bool forceQuote)
		{
			if (string.IsNullOrWhiteSpace(arg))
				return "\"" + (arg ?? string.Empty) + "\"";

			if (forceQuote == false && _rgxNoneedQuotes_Exe.IsMatch(arg))
				return arg;

			string escArg = _rgxEscapeBackslash_Exe.Replace(arg, m => m.Groups[1].Value + m.Groups[1].Value);

			return "\"" + escArg.Replace("\"", "\\\"") + "\"";
		}

		public override string ToString()
		{
			return ToString(null);
		}
	}
}
