// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace DataBooster.PSWebApi
{
	public class PSConfiguration
	{
		private readonly Collection<PSConverterRegistry> _supportedConverters;
		public Collection<PSConverterRegistry> SupportedConverters { get { return _supportedConverters; } }

		private readonly PSConverterRegistry _jsonConverter;
		public PSConverterRegistry JsonConverter { get { return _jsonConverter; } }

		private readonly PSConverterRegistry _xmlConverter;
		public PSConverterRegistry XmlConverter { get { return _xmlConverter; } }

		private readonly PSConverterRegistry _csvConverter;
		public PSConverterRegistry CsvConverter { get { return _csvConverter; } }

		private readonly PSConverterRegistry _htmlConverter;
		public PSConverterRegistry HtmlConverter { get { return _htmlConverter; } }

		private readonly PSConverterRegistry _textConverter;
		public PSConverterRegistry TextConverter { get { return _textConverter; } }

		private readonly PSConverterRegistry _stringConverter;
		public PSConverterRegistry StringConverter { get { return _stringConverter; } }

		private readonly PSConverterRegistry _nullConverter;
		public PSConverterRegistry NullConverter { get { return _nullConverter; } }

		public PSConfiguration(bool supportJson = true, bool supportXml = true, bool supportCsv = true, bool supportHtml = true, bool supportText = true, bool supportString = true)
		{
			_supportedConverters = new Collection<PSConverterRegistry>();

			if (supportJson)
			{
				_jsonConverter = new PSConverterRegistry(new string[] { "application/json", "text/json" }, "json", "ConvertTo-Json");
				_supportedConverters.Add(_jsonConverter);
			}

			if (supportXml)
			{
				_xmlConverter = new PSConverterRegistry(new string[] { "application/xml", "text/xml" }, "xml", "ConvertTo-Xml");
				_supportedConverters.Add(_xmlConverter);
			}

			if (supportCsv)
			{
				_csvConverter = new PSConverterRegistry(new string[] { "text/csv", "application/csv" }, "csv", "ConvertTo-Csv");
				_supportedConverters.Add(_csvConverter);
			}

			if (supportHtml)
			{
				_htmlConverter = new PSConverterRegistry(new string[] { "text/html", "application/xhtml" }, "html", "ConvertTo-Html");
				_supportedConverters.Add(_htmlConverter);
			}

			if (supportText)
			{
				_textConverter = new PSConverterRegistry(new string[] { "text/plain" }, "text", "Out-String");
				_supportedConverters.Add(_textConverter);
			}

			if (supportString)
			{
				_stringConverter = new PSConverterRegistry(new string[] { "application/string" }, "string", string.Empty);
				_supportedConverters.Add(_stringConverter);
			}

			_nullConverter = new PSConverterRegistry(new string[] { "application/null" }, "null", "Out-Null");
			_supportedConverters.Add(_nullConverter);
		}

		public PSConverterRegistry Lookup(string media_type)
		{
			return string.IsNullOrEmpty(media_type) ? null :
				_supportedConverters.Where(c => c.MediaTypes.Any(m => m.MediaType.Equals(media_type, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
		}

		public PSConverterRegistry Lookup(MediaTypeHeaderValue mediaType)
		{
			return mediaType == null ? null : Lookup(mediaType.MediaType);
		}

		public string UriPathExtConstraint()
		{
			return string.Join("|", _supportedConverters.Select(c => c.UriPathExtension).Where(e => !string.IsNullOrEmpty(e)));
		}
	}
}
