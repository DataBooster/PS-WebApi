// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Text;
using System.Net.Http.Formatting;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> class to handle PowerShell supported output formats. Such as JSON, XML, CSV, HTML, String, etc.
	/// </summary>
	public class PSMediaTypeFormatter : MediaTypeFormatter
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
					SupportedMediaTypes.Add(mediaType);

					if (!string.IsNullOrWhiteSpace(converter.UriPathExtension))
						this.AddUriPathExtensionMapping(converter.UriPathExtension, mediaType);
				}
			}

			// Set default supported character encodings
			SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
			SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
		}

		/// <summary>
		/// Determines whether this <see cref="PSMediaTypeFormatter"/> can read objects of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of object that will be read.</param>
		/// <returns><c>true</c> if objects of this <paramref name="type"/> can be read, otherwise <c>false</c>.</returns>
		public override bool CanReadType(Type type)
		{
			return false;
		}

		/// <summary>
		/// Determines whether this <see cref="PSMediaTypeFormatter"/> can write objects of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of object that will be written.</param>
		/// <returns><c>true</c> if objects of this <paramref name="type"/> can be written, otherwise <c>false</c>. Only string type can be written.</returns>
		public override bool CanWriteType(Type type)
		{
			return type == typeof(string);
		}
	}
}
