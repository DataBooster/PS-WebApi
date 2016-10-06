// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataBooster.PSWebApi
{
	public class PSConverterRegistry
	{
		private readonly List<MediaTypeHeaderValue> _mediaTypes;
		public Collection<MediaTypeHeaderValue> MediaTypes { get; private set; }

		private readonly string _uriPathExtension;
		public string UriPathExtension { get { return _uriPathExtension; } }

		private readonly string _conversionCmdlet;
		public string ConversionCmdlet { get { return _conversionCmdlet; } }

		private IEnumerable<KeyValuePair<string, object>> _cmdletParameters;
		public IEnumerable<KeyValuePair<string, object>> CmdletParameters
		{
			get { return _cmdletParameters; }
			set { _cmdletParameters = value; }
		}

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

		public PSConverterRegistry(IEnumerable<string> mediaTypes, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null) :
			this(mediaTypes.Select(s => new MediaTypeHeaderValue(s)), uriPathExtension, conversionCmdlet, cmdletParameters)
		{
		}

		public PSConverterRegistry(MediaTypeHeaderValue mediaType, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null)
			: this(new MediaTypeHeaderValue[] { mediaType }, uriPathExtension, conversionCmdlet, cmdletParameters)
		{
			if (mediaType == null)
				throw new ArgumentNullException("mediaType");
		}

		public PSConverterRegistry(string mediaType, string uriPathExtension, string conversionCmdlet, IEnumerable<KeyValuePair<string, object>> cmdletParameters = null)
			: this(new MediaTypeHeaderValue(mediaType), uriPathExtension, conversionCmdlet, cmdletParameters)
		{
		}
	}
}
