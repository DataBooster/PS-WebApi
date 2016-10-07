// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> class to handle PowerShell supported output formats. Such as JSON, XML, CSV, HTML, String, etc.
	/// </summary>
	public class PSMediaTypeFormatter : JsonMediaTypeFormatter
	{
		private readonly PSConfiguration _psConfiguration;
		public PSConfiguration Configuration { get { return _psConfiguration; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="PSMediaTypeFormatter"/> class.
		/// </summary>
		/// <param name="psConfiguration">PSConfiguration</param>
		public PSMediaTypeFormatter(PSConfiguration psConfiguration = null)
		{
			_psConfiguration = psConfiguration ?? new PSConfiguration();

			foreach (var converter in _psConfiguration.SupportedConverters)
			{
				foreach (var mediaType in converter.MediaTypes)
				{
					if (!SupportedMediaTypes.Any(t => t.MediaType.Equals(mediaType.MediaType)))
						SupportedMediaTypes.Add(mediaType);

					if (!string.IsNullOrWhiteSpace(converter.UriPathExtension))
						this.AddUriPathExtensionMapping(converter.UriPathExtension, mediaType);
				}
			}

			// Solving the 415 issue for GET request without Content-Type and Content-Length: 0
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
		}
	}
}
