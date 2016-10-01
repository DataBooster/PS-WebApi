// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;

namespace DataBooster.PSWebApi
{
	public class ScriptPath
	{
		private readonly Uri _scriptRoot;

		public ScriptPath(string scriptRoot)
		{
			if (scriptRoot == null)
				throw new ArgumentNullException("scriptRoot");

			string rootPath = scriptRoot.Trim();
			if (rootPath.Length == 0)
				throw new ArgumentNullException("scriptRoot");

			Uri baseUri = new Uri(rootPath);
			_scriptRoot = baseUri.LocalPath.EndsWith(@"\") ? baseUri : new Uri(baseUri.LocalPath + @"\");

			if (!Directory.Exists(_scriptRoot.LocalPath))
				throw new DirectoryNotFoundException(_scriptRoot.LocalPath);
		}

		public string GetFullPath(string relativePath)
		{
			if (relativePath == null)
				throw new ArgumentNullException(relativePath);

			string scriptPath = relativePath.Trim();

			if (scriptPath.Length == 0)
				throw new ArgumentNullException(relativePath);

			Uri scriptUri = new Uri(_scriptRoot, relativePath);
			string fullPath = scriptUri.LocalPath;

			if (!fullPath.StartsWith(_scriptRoot.LocalPath))
				throw new ArgumentOutOfRangeException(relativePath);

			return fullPath;
		}

		public override string ToString()
		{
			return _scriptRoot.LocalPath;
		}
	}
}
