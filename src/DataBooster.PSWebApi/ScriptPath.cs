// Copyright (c) 2016 Abel Cheng <abelcys@gmail.com>. Licensed under the MIT license.
// Repository: https://pswebapi.codeplex.com/, https://github.com/DataBooster/PS-WebApi

using System;
using System.IO;

namespace DataBooster.PSWebApi
{
	/// <summary>
	/// Performs operations on String instances that contain script directory or file path information.
	/// </summary>
	public class ScriptPath
	{
		private readonly Uri _scriptRoot;

		/// <summary>
		/// Initializes a new instance of the ScriptPath class with the specified root directory as the base path.
		/// </summary>
		/// <param name="scriptRoot">The root directory accepts an absolute path of Windows directory path (like D:\directory\, \\computer\directory\) or URI path (like file:///D:/directory/, file://computer/directory/). </param>
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

		/// <summary>
		/// Returns the absolute path for the relative path.
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns>An absolute path string constructed from the base path (initialized by scriptRoot in constructor) and the relativePath.</returns>
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

		/// <summary>
		/// Return the local operating-system representation of the base path.
		/// </summary>
		/// <returns>A string that contains the local operating-system representation of the base path.</returns>
		public override string ToString()
		{
			return _scriptRoot.LocalPath;
		}
	}
}
