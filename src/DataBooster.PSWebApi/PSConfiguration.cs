// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// Represents a configuration of PSMediaTypeFormatter instances, which contains all currently supported PSConverterRegistry add-ins.
	/// </summary>
	public class PSConfiguration
	{
		private readonly List<PSConverterRegistry> _supportedConverters;
		/// <summary>
		/// Gets the PSConverterRegistry collection which contains all currently supported media-type response converters.
		/// </summary>
		public Collection<PSConverterRegistry> SupportedConverters { get; private set; }

		private readonly PSConverterRegistry _jsonConverter;
		/// <summary>
		/// Gets the PowerShell converter for JSON response media-type.
		/// </summary>
		public PSConverterRegistry JsonConverter { get { return _jsonConverter; } }

		private readonly PSConverterRegistry _xmlConverter;
		/// <summary>
		/// Gets the PowerShell converter for XML response media-type.
		/// </summary>
		public PSConverterRegistry XmlConverter { get { return _xmlConverter; } }

		private readonly PSConverterRegistry _csvConverter;
		/// <summary>
		/// Gets the PowerShell converter for CSV response media-type.
		/// </summary>
		public PSConverterRegistry CsvConverter { get { return _csvConverter; } }

		private readonly PSConverterRegistry _htmlConverter;
		/// <summary>
		/// Gets the PowerShell converter for HTML response media-type.
		/// </summary>
		public PSConverterRegistry HtmlConverter { get { return _htmlConverter; } }

		private readonly PSConverterRegistry _textConverter;
		/// <summary>
		/// Gets the PowerShell converter for text response media-type.
		/// </summary>
		public PSConverterRegistry TextConverter { get { return _textConverter; } }

		private readonly PSConverterRegistry _stringConverter;
		/// <summary>
		/// Gets the PowerShell converter for string response media-type.
		/// </summary>
		public PSConverterRegistry StringConverter { get { return _stringConverter; } }

		private readonly PSConverterRegistry _nullConverter;
		/// <summary>
		/// Gets the PowerShell converter for null (deleting output instead of sending it down the pipeline) response media-type.
		/// </summary>
		public PSConverterRegistry NullConverter { get { return _nullConverter; } }

		/// <summary>
		/// Initializes a new instance of the PSConfiguration class.
		/// </summary>
		/// <param name="supportJson">true to support JSON response; otherwise, false.</param>
		/// <param name="supportXml">true to support XML response; otherwise, false.</param>
		/// <param name="supportCsv">true to support CSV response; otherwise, false.</param>
		/// <param name="supportHtml">true to support HTML response; otherwise, false.</param>
		/// <param name="supportText">true to support text response; otherwise, false.</param>
		/// <param name="supportString">true to support string response; otherwise, false.</param>
		public PSConfiguration(bool supportJson = true, bool supportXml = true, bool supportCsv = true, bool supportHtml = true, bool supportText = true, bool supportString = true)
		{
			_supportedConverters = new List<PSConverterRegistry>();
			SupportedConverters = new Collection<PSConverterRegistry>(_supportedConverters);

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
				_textConverter = new PSConverterRegistry(new string[] { "text/plain" }, "string", "Out-String");
				_supportedConverters.Add(_textConverter);
			}

			if (supportString)
			{
				_stringConverter = new PSConverterRegistry(new string[] { "application/string" }, "str", string.Empty);		// .ToString()
				_supportedConverters.Add(_stringConverter);
			}

			_nullConverter = new PSConverterRegistry(new string[] { "application/null" }, "null", "Out-Null");
			_supportedConverters.Add(_nullConverter);
		}

		/// <summary>
		/// Searches for the first PSConverterRegistry (PowerShell converter) which supports the specified media_type.
		/// </summary>
		/// <param name="media_type">The response media type to support.</param>
		/// <returns>The first PSConverterRegistry that support the specified media type, if found; otherwise, return null.</returns>
		public PSConverterRegistry Lookup(string media_type)
		{
			return string.IsNullOrEmpty(media_type) ? null :
				_supportedConverters.Where(c => c.MediaTypes.Any(m => m.MediaType.Equals(media_type, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
		}

		/// <summary>
		/// Searches for the first PSConverterRegistry (PowerShell converter) which supports the specified mediaType.
		/// </summary>
		/// <param name="mediaType">The response media type to support.</param>
		/// <returns>The first PSConverterRegistry that support the specified media type, if found; otherwise, return null.</returns>
		public PSConverterRegistry Lookup(MediaTypeHeaderValue mediaType)
		{
			return mediaType == null ? null : Lookup(mediaType.MediaType);
		}

		/// <summary>
		/// Returns a constrainted expression that specify supported Uri path extensions for routeTemplate.
		/// </summary>
		/// <returns>A regular expression string to indicate supported Uri path extensions for routeTemplate</returns>
		public string UriPathExtConstraint()
		{
			return string.Join("|", _supportedConverters.Select(c => c.UriPathExtension).Where(e => !string.IsNullOrEmpty(e)));
		}
	}
}
