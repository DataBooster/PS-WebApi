// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Text;
using System.Net.Http.Formatting;

namespace DataBooster.PSWebApi
{
	internal class PSMediaTypeFormatter : MediaTypeFormatter
	{
		private readonly PSConfiguration _psConfiguration;
		public PSConfiguration Configuration { get { return _psConfiguration; } }

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

		public override bool CanReadType(Type type)
		{
			return false;
		}

		public override bool CanWriteType(Type type)
		{
			return type == typeof(string);
		}
	}
}
