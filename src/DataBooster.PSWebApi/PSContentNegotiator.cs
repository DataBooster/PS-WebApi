// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections.Generic;

namespace DataBooster.PSWebApi
{
	internal class PSContentNegotiator
	{
		private class PsDefaultContentNegotiator : DefaultContentNegotiator
		{
			public Encoding NegotiateEncoding(HttpRequestMessage request, MediaTypeFormatter formatter)
			{
				return base.SelectResponseCharacterEncoding(request, formatter);
			}
		}

		private static readonly PsDefaultContentNegotiator _psDefaultContentNegotiator;
		private static readonly Encoding _defaultEncoding;

		static PSContentNegotiator()
		{
			_psDefaultContentNegotiator = new PsDefaultContentNegotiator();
			_defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
		}

		private readonly MediaTypeHeaderValue _negotiatedMediaType;
		public MediaTypeHeaderValue NegotiatedMediaType { get { return _negotiatedMediaType; } }

		private readonly Encoding _negotiatedEncoding;
		public Encoding NegotiatedEncoding { get { return _negotiatedEncoding ?? _defaultEncoding; } }

		private readonly PSConverterRegistry _negotiatedPsConverter;
		public PSConverterRegistry NegotiatedPsConverter { get { return _negotiatedPsConverter; } }

		public PSContentNegotiator(HttpRequestMessage request)
		{
			HttpConfiguration configuration = request.GetConfiguration();
			IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
			IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;

			ContentNegotiationResult contentNegotiationResult = contentNegotiator.Negotiate(typeof(string), request, formatters);

			_negotiatedMediaType = contentNegotiationResult.MediaType;

			MediaTypeFormatter resultformatter = contentNegotiationResult.Formatter;
			if (resultformatter != null)
				_negotiatedEncoding = _psDefaultContentNegotiator.NegotiateEncoding(request, resultformatter);

			PSMediaTypeFormatter negotiatedMediaTypeFormatter = resultformatter as PSMediaTypeFormatter;
			if (negotiatedMediaTypeFormatter != null)
				_negotiatedPsConverter = negotiatedMediaTypeFormatter.Configuration.Lookup(_negotiatedMediaType);
		}
	}
}
