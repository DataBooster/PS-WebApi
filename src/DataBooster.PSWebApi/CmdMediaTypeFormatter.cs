// Copyright (c) 2017 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> class to handle RedirectStandardInput from the request body.
	/// </summary>
	public class CmdMediaTypeFormatter : MediaTypeFormatter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CmdMediaTypeFormatter"/> class.
		/// </summary>
		/// <param name="mediaTypes">A list of supported request Content-Types for redirecting StandardInput.</param>
		public CmdMediaTypeFormatter(IEnumerable<MediaTypeHeaderValue> mediaTypes = null)
		{
			if (mediaTypes == null)
				mediaTypes = new MediaTypeHeaderValue[] {
					new MediaTypeHeaderValue("application/stdin"),
					new MediaTypeHeaderValue("text/stdin"),
					new MediaTypeHeaderValue("application/standardinput"),
					new MediaTypeHeaderValue("text/standardinput"),
					new MediaTypeHeaderValue("application/redirectstandardinput"),
					new MediaTypeHeaderValue("text/redirectstandardinput")
				};

			foreach (MediaTypeHeaderValue m in mediaTypes)
				SupportedMediaTypes.Add(m);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CmdMediaTypeFormatter"/> class.
		/// </summary>
		/// <param name="mediaTypes">A list supported MediaTypes.</param>
		public CmdMediaTypeFormatter(IEnumerable<string> mediaTypes) :
			this((mediaTypes == null) ? null : mediaTypes.Select(s => new MediaTypeHeaderValue(s)))
		{
		}

		/// <inheritdoc />
		public override bool CanReadType(Type type)
		{
			if (type == typeof(string) || type == typeof(JToken) || typeof(JValue).IsAssignableFrom(type) || typeof(IDictionary<string, object>).IsAssignableFrom(type))
				return true;
			else
				return false;
		}

		/// <inheritdoc />
		public override bool CanWriteType(Type type)
		{
			return false;
		}

		/// <inheritdoc />
		public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (readStream == null)
			{
				throw new ArgumentNullException("readStream");
			}

			if (content == null)
			{
				throw new ArgumentNullException("content");
			}

			string bodyText = await content.ReadAsStringAsync().ConfigureAwait(false);

			if (type == typeof(string))
				return bodyText;

			if (type == typeof(JToken) || typeof(JValue).IsAssignableFrom(type))
				return new JRaw(bodyText);

			if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
				return JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyText);

			return null;
		}
	}
}
