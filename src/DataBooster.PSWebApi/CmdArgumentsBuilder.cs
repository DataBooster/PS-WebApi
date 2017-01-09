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
	/// <summary>
	/// Assembling Windows command line arguments, and surrounding/escaping each argument by quotation marks if necessary.
	/// </summary>
	public class CmdArgumentsBuilder
	{
		private const string _argSeparator = " ";
		private static readonly Regex _rgxNoneedQuotes_Exe = new Regex(@"^((\\\S|\\$|[^""\s\\])|(""(\\.|""""|[^""\\])*""))+$");
		private static readonly Regex _rgxEscapeBackslash_Exe = new Regex(@"(\\+)(?=""|$)");
		private static readonly Regex _rgxNoneedQuotes_Bat = new Regex(@"^([^""\s&|<>()^]|\^[&|<>()^]|(""[^""]*""))+$");
		private readonly List<string> _rawArguments;
		/// <summary>
		/// Gets the raw arguments.
		/// </summary>
		public Collection<string> RawArguments { get; private set; }

		/// <summary>
		/// Initializes a new instance of the CmdArgumentsBuilder class.
		/// </summary>
		public CmdArgumentsBuilder()
		{
			_rawArguments = new List<string>();
			RawArguments = new Collection<string>(_rawArguments);
		}

		/// <summary>
		/// Adds an argument to the end of arguments.
		/// </summary>
		/// <param name="arg">The argument to be added to the end of arguments.</param>
		/// <returns>The instance itself.</returns>
		public CmdArgumentsBuilder Add(string arg)
		{
			_rawArguments.Add(arg);
			return this;
		}

		/// <summary>
		/// Adds the arguments of the specified collection to the end of this instance.
		/// </summary>
		/// <param name="args">The collection whose elements should be added to the end of this instance.</param>
		/// <returns>The instance itself.</returns>
		public CmdArgumentsBuilder Add(IEnumerable<string> args)
		{
			if (args != null)
				_rawArguments.AddRange(args);

			return this;
		}

		/// <summary>
		/// Adds a bunch of key/value pairs (named parameters) to the end of this instance.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="parameters">The named parameters (key/value pairs) to be added to the end of this instance.</param>
		/// <returns>The instance itself.</returns>
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

		/// <summary>
		/// Adds a JToken object deserialized from the HTTP request body to the end of this instance.
		/// </summary>
		/// <param name="argsFromBody">The JToken object to be added as arguments. It can be an array of string, a Json object or a Json simple value.</param>
		/// <returns>The instance itself.</returns>
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

		/// <summary>
		/// Check a value is null or Json null.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to be check.</param>
		/// <returns>true if the value parameter is null or Json null; otherwise, false.</returns>
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

		/// <summary>
		/// Adds query-string name/value pairs as arguments to the end of this instance.
		/// </summary>
		/// <param name="httpRequestMessage">The HttpRequestMessage to extract arguments from.</param>
		/// <returns>The instance itself.</returns>
		public CmdArgumentsBuilder AddFromQueryString(HttpRequestMessage httpRequestMessage)
		{
			return httpRequestMessage == null ? this : Add(httpRequestMessage.GetQueryNameValuePairs());
		}

		/// <summary>
		/// Concatenates all raw arguments of this instance, using the specified escaping function to apply to each raw argument.
		/// </summary>
		/// <param name="escapeArgument">A transform function to apply to each raw argument.</param>
		/// <returns>A string whose value is the concatenated with quoted/escaped inside if necessary.</returns>
		public string ToString(Func<string, string> escapeArgument)
		{
			if (escapeArgument == null)
				escapeArgument = (string arg) => arg ?? string.Empty;

			return (_rawArguments.Count == 0) ? string.Empty :
				string.Join(_argSeparator, _rawArguments.Select(escapeArgument));
		}

		/// <summary>
		/// Using Microsoft C/C++/C# startup code rules to enclose the raw argument in a pair of double quotation marks if necessary, and makes the correspond escaping.
		/// </summary>
		/// <remarks>
		/// <para>https://msdn.microsoft.com/en-us/library/17w5ykft.aspx</para>
		/// <para>https://msdn.microsoft.com/en-us/library/a1y7w461.aspx</para>
		/// </remarks>
		/// <param name="rawArg">The raw argument to be transformed.</param>
		/// <param name="forceQuote">true to force quoting surround the raw argument; false to detect necessity - only when a string would be interpreted as multiple broken arguments or a string contains any unclosed double-quotation mark (non-literal), then the original string will be surrounded by an extra pair of double-quotation marks at the outermost layer.</param>
		/// <returns>A transformed string. All the originally nested quotes will be escaped using the  \"  as literal double-quotation marks.</returns>
		public static string QuoteExeArgument(string rawArg, bool forceQuote)
		{
			if (string.IsNullOrWhiteSpace(rawArg))
				return "\"" + (rawArg ?? string.Empty) + "\"";

			if (forceQuote == false && _rgxNoneedQuotes_Exe.IsMatch(rawArg))
				return rawArg;

			string escArg = _rgxEscapeBackslash_Exe.Replace(rawArg, m => m.Groups[1].Value + m.Groups[1].Value);

			return "\"" + escArg.Replace("\"", "\\\"") + "\"";
		}

		/// <summary>
		/// Using Windows command-line parser (CMD.EXE) rules to enclose the raw argument in a pair of double quotation marks if necessary, and makes the correspond escaping.
		/// </summary>
		/// <param name="rawArg">The raw argument to be transformed.</param>
		/// <param name="forceQuote">true to force quoting surround the raw argument; false to detect necessity - only when a string would be interpreted as multiple broken arguments or a string contains any unclosed double-quotation mark (non-literal), then the original string will be surrounded by an extra pair of double-quotation marks at the outermost layer.</param>
		/// <returns>A transformed string. Every originally nested double-quotation mark will be escaped as double double-quotation marks.</returns>
		public static string QuoteBatArgument(string rawArg, bool forceQuote)
		{
			if (string.IsNullOrWhiteSpace(rawArg))
				return "\"" + (rawArg ?? string.Empty) + "\"";

			if (forceQuote == false && _rgxNoneedQuotes_Bat.IsMatch(rawArg))
				return rawArg;

			return "\"" + rawArg.Replace("\"", "\"\"") + "\"";
		}

		/// <summary>
		/// Simply concatenate all raw arguments, using a space separator between each argument.
		/// </summary>
		/// <remarks>
		/// If a raw argument is null, an empty string (String.Empty) is used instead.
		/// </remarks>
		/// <returns>A string that consists of all the raw arguments delimited by the space separator. If there is no argument, the method returns String.Empty.</returns>
		public override string ToString()
		{
			return ToString(null);
		}
	}
}
