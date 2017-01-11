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
	/// Represents a PowerShell converter associated with one or more media types for a HTTP response.
	/// </summary>
	public class PSConverterRegistry
	{
		private readonly List<MediaTypeHeaderValue> _mediaTypes;
		/// <summary>
		/// Gets the collection of media types supported by this PowerShell converter.
		/// </summary>
		public Collection<MediaTypeHeaderValue> MediaTypes { get; private set; }

		private readonly string _uriPathExtension;
		/// <summary>
		/// Gets the extension of Uri path (Uris ending with the given uriPathExtension) associated with media types supported by this PowerShell converter.
		/// </summary>
		public string UriPathExtension { get { return _uriPathExtension; } }

		private readonly string _conversionCmdlet;
		/// <summary>
		/// Gets the PowerShell Cmdlet that can be used for converting a PowerShell object to the associated media types formatted string.
		/// </summary>
		public string ConversionCmdlet { get { return _conversionCmdlet; } }

		private IEnumerable<KeyValuePair<string, object>> _cmdletParameters;
		/// <summary>
		/// Gets or sets the predetermined parameters for the PowerShell Cmdlet associated with this converter.
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> CmdletParameters
		{
			get { return _cmdletParameters; }
			set { _cmdletParameters = value; }
		}

		/// <summary>
		/// Initializes a new instance of the PSConverterRegistry class with supported media types, associated uriPathExtension, PowerShell Cmdlet and predetermined parameters.
		/// </summary>
		/// <param name="mediaTypes">The media types supported by this PowerShell converter.</param>
		/// <param name="uriPathExtension">An extension of Uri path (Uris ending with the given uriPathExtension) associated with media types supported by this PowerShell converter.</param>
		/// <param name="conversionCmdlet">The PowerShell Cmdlet that can be used for converting a PowerShell object to the associated media types formatted string.</param>
		/// <param name="cmdletParameters">The predetermined parameters for the PowerShell Cmdlet associated with this converter.(optional)</param>
		public PSConverterRegistry(IEnumerable<MediaTypeHeaderValue> mediaTypes, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null)
		{
			if (mediaTypes == null)
				throw new ArgumentNullException("mediaTypes");
			else
			{
				_mediaTypes = mediaTypes.ToList();
				if (_mediaTypes.Count == 0)
					throw new ArgumentNullException("mediaTypes");
				MediaTypes = new Collection<MediaTypeHeaderValue>(_mediaTypes);
			}

			_uriPathExtension = (uriPathExtension == null) ? string.Empty : uriPathExtension.Trim();
			_conversionCmdlet = conversionCmdlet ?? string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the PSConverterRegistry class with supported media types, associated uriPathExtension, PowerShell Cmdlet and predetermined parameters.
		/// </summary>
		/// <param name="mediaTypes">The media types supported by this PowerShell converter.</param>
		/// <param name="uriPathExtension">An extension of Uri path (Uris ending with the given uriPathExtension) associated with media types supported by this PowerShell converter.</param>
		/// <param name="conversionCmdlet">The PowerShell Cmdlet that can be used for converting a PowerShell object to the associated media types formatted string.</param>
		/// <param name="cmdletParameters">The predetermined parameters for the PowerShell Cmdlet associated with this converter.(optional)</param>
		public PSConverterRegistry(IEnumerable<string> mediaTypes, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null) :
			this(mediaTypes.Select(s => new MediaTypeHeaderValue(s)), uriPathExtension, conversionCmdlet, cmdletParameters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the PSConverterRegistry class with a supported media type, associated uriPathExtension, PowerShell Cmdlet and predetermined parameters.
		/// </summary>
		/// <param name="mediaType">A media type supported by this PowerShell converter.</param>
		/// <param name="uriPathExtension">An extension of Uri path (Uris ending with the given uriPathExtension) associated with media types supported by this PowerShell converter.</param>
		/// <param name="conversionCmdlet">The PowerShell Cmdlet that can be used for converting a PowerShell object to the associated media types formatted string.</param>
		/// <param name="cmdletParameters">The predetermined parameters for the PowerShell Cmdlet associated with this converter.(optional)</param>
		public PSConverterRegistry(MediaTypeHeaderValue mediaType, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null)
			: this(new MediaTypeHeaderValue[] { mediaType }, uriPathExtension, conversionCmdlet, cmdletParameters)
		{
			if (mediaType == null)
				throw new ArgumentNullException("mediaType");
		}

		/// <summary>
		/// Initializes a new instance of the PSConverterRegistry class with a supported media type, associated uriPathExtension, PowerShell Cmdlet and predetermined parameters.
		/// </summary>
		/// <param name="mediaType">A media type supported by this PowerShell converter.</param>
		/// <param name="uriPathExtension">An extension of Uri path (Uris ending with the given uriPathExtension) associated with media types supported by this PowerShell converter.</param>
		/// <param name="conversionCmdlet">The PowerShell Cmdlet that can be used for converting a PowerShell object to the associated media types formatted string.</param>
		/// <param name="cmdletParameters">The predetermined parameters for the PowerShell Cmdlet associated with this converter.(optional)</param>
		public PSConverterRegistry(string mediaType, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null)
			: this(new MediaTypeHeaderValue(mediaType), uriPathExtension, conversionCmdlet, cmdletParameters)
		{
		}
	}
}
