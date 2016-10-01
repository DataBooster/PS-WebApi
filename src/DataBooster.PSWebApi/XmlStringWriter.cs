// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace DataBooster.PSWebApi
{
	internal class XmlStringWriter : StringWriter
	{
		private readonly Encoding _encoding;

		public XmlStringWriter(Encoding encoding)
			: base(CultureInfo.InvariantCulture)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			_encoding = encoding;
		}

		public override Encoding Encoding
		{
			get
			{
				return _encoding;
			}
		}
	}
}
