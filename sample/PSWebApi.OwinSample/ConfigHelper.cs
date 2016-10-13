using System.Configuration;
using System.Globalization;
using WebServer = System.Web.Hosting.HostingEnvironment;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample
{
	public static class ConfigHelper
	{
		private const string _scriptRootSettingKey = "ScriptRoot";
		private const string _corsOriginsSettingKey = "CorsOrigins";
		private const string _supportsCredentialsSettingKey = "CorsSupportsCredentials";
		private const string _preflightMaxAgeSettingKey = "CorsPreflightMaxAge";
		private static readonly ScriptPath _scriptPath;

		static ConfigHelper()
		{
			string scriptRoot = ConfigurationManager.AppSettings[_scriptRootSettingKey];

			if (scriptRoot != null)
				scriptRoot = scriptRoot.Trim();

			if (string.IsNullOrEmpty(scriptRoot))
				throw new SettingsPropertyNotFoundException(_scriptRootSettingKey);

			if (scriptRoot.StartsWith("~"))
				scriptRoot = WebServer.MapPath(scriptRoot);

			_scriptPath = new ScriptPath(scriptRoot);
		}

		public static string LocalFullPath(this string relativePath)
		{
			return _scriptPath.GetFullPath(relativePath);
		}

		private static volatile string _corsOrigins = null;
		public static string CorsOrigins
		{
			get
			{
				if (_corsOrigins == null)
				{
					_corsOrigins = ConfigurationManager.AppSettings[_corsOriginsSettingKey];

					if (_corsOrigins == null)
						_corsOrigins = string.Empty;
				}

				return _corsOrigins;
			}
			set
			{
				_corsOrigins = value;
			}
		}

		private static bool? _supportsCredentials = null;
		public static bool SupportsCredentials
		{
			get
			{
				if (_supportsCredentials == null)
				{
					string withCredentials = ConfigurationManager.AppSettings[_supportsCredentialsSettingKey];
					bool bCredentials = false;

					if (!string.IsNullOrEmpty(withCredentials))
						bool.TryParse(withCredentials, out bCredentials);

					_supportsCredentials = bCredentials;
				}

				return _supportsCredentials.Value;
			}
			set
			{
				_supportsCredentials = value;
			}
		}

		private static long? _preflightMaxAge = null;
		public static long PreflightMaxAge
		{
			get
			{
				if (_preflightMaxAge == null)
				{
					string strPreflightMaxAge = ConfigurationManager.AppSettings[_preflightMaxAgeSettingKey];
					long preflightMaxAge = 0L;

					if (!string.IsNullOrEmpty(strPreflightMaxAge))
						long.TryParse(strPreflightMaxAge, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out preflightMaxAge);

					_preflightMaxAge = preflightMaxAge;
				}

				return _preflightMaxAge.Value;
			}
			set
			{
				_preflightMaxAge = value;
			}
		}

		#region Invoking Command Line

		private const string _cmdForceArgumentQuoteSettingKey = "CmdForceArgumentQuote";
		private const string _cmdTimeoutSecondsSettingKey = "CmdTimeoutSeconds";

		private static bool? _cmdForceArgumentQuote = null;
		public static bool CmdForceArgumentQuote
		{
			get
			{
				if (_cmdForceArgumentQuote == null)
				{
					string strCmdForceArgumentQuote = ConfigurationManager.AppSettings[_cmdForceArgumentQuoteSettingKey];
					bool cmdForceArgumentQuote = false;

					if (!string.IsNullOrEmpty(strCmdForceArgumentQuote))
						bool.TryParse(strCmdForceArgumentQuote, out cmdForceArgumentQuote);

					_cmdForceArgumentQuote = cmdForceArgumentQuote;
				}

				return _cmdForceArgumentQuote.Value;
			}
			set
			{
				_cmdForceArgumentQuote = value;
			}
		}

		private static int? _cmdTimeoutSeconds = null;
		public static int CmdTimeoutSeconds
		{
			get
			{
				if (_cmdTimeoutSeconds == null)
				{
					string strCmdTimeoutSeconds = ConfigurationManager.AppSettings[_cmdTimeoutSecondsSettingKey];
					int cmdTimeoutSeconds = 3600;

					if (!string.IsNullOrEmpty(strCmdTimeoutSeconds))
						int.TryParse(strCmdTimeoutSeconds, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out cmdTimeoutSeconds);

					_cmdTimeoutSeconds = cmdTimeoutSeconds;
				}

				return _cmdTimeoutSeconds.Value;
			}
			set
			{
				_cmdTimeoutSeconds = value;
			}
		}

		#endregion
	}
}
