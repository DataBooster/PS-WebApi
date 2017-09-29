// Copyright (c) 2017 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DataBooster.PSWebApi
{
	public static partial class PSControllerExtensions
	{
		private const string _RegisteredPropertyKey = "[DataBooster.PSWebApi]:Registered";

		/// <summary>
		/// Registers necessary MediaTypeFormatters to HttpConfiguration for running a PS-WebAPI endpoint.
		/// </summary>
		/// <param name="config">The <see cref="HttpConfiguration"/>.  This is an extension method to HttpConfiguration, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="psConfiguration">The configuration for initializing a instance PSMediaTypeFormatter, which contains all currently supported PSConverterRegistry add-ins.</param>
		/// <param name="mediaTypes">A list of supported request Content-Types for redirecting StandardInput.</param>
		public static PSConfiguration RegisterPsWebApi(this HttpConfiguration config, PSConfiguration psConfiguration = null, IEnumerable<MediaTypeHeaderValue> mediaTypes = null)
		{
			if (config.Properties.ContainsKey(_RegisteredPropertyKey))
				throw new InvalidOperationException("Registered PSWebApi Repeatedly");

			PSMediaTypeFormatter psMediaTypeFormatter = new PSMediaTypeFormatter(psConfiguration);
			config.Formatters.Insert(0, psMediaTypeFormatter);
			config.Formatters.Insert(1, new CmdMediaTypeFormatter(mediaTypes));

			config.Properties.TryAdd(_RegisteredPropertyKey, true);

			return psMediaTypeFormatter.Configuration;
		}

		/// <summary>
		/// Distinguishes RedirectStandardInput text from the request body.
		/// </summary>
		/// <param name="rawBody">Raw JToken of the request body.</param>
		/// <returns>The text of request body if the Content-Type indicate RedirectStandardInput; Otherwise null - means that the body carried command line arguments.</returns>
		public static string DistinguishStandardInput(this JToken rawBody)
		{
			JRaw stdin = rawBody as JRaw;

			return (stdin == null) ? null : stdin.Value as string;
		}
	}
}
