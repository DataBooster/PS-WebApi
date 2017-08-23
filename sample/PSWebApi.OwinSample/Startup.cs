using Owin;
using Microsoft.Owin;
using System.Web.Http;
using System.Web.Http.Cors;
using DataBooster.PSWebApi;

[assembly: OwinStartup(typeof(PSWebApi.OwinSample.Startup))]

namespace PSWebApi.OwinSample
{
	public class Startup
	{
		private static HttpServer _httpServer;

		static Startup()
		{
			HttpConfiguration config = new HttpConfiguration();

			PSMediaTypeFormatter psMediaTypeFormatter = new PSMediaTypeFormatter();
			config.Formatters.Insert(0, psMediaTypeFormatter);

			EnableCors(config);
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "PSWebApi",
				routeTemplate: "ps/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokePS_Async" },
				constraints: new { script = @".+\.ps1$" }
			);

			config.Routes.MapHttpRoute(
				name: "PSWebApi-Ext",
				routeTemplate: "ps.{ext}/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokePS_Async" },
				constraints: new { script = @".+\.ps1$", ext = psMediaTypeFormatter.Configuration.UriPathExtConstraint() }
			);

			config.Routes.MapHttpRoute(
				name: "CmdWebApi",
				routeTemplate: "cmd/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokeCMD_Async", ext = "string" },
				constraints: new { script = @".+\.(bat|exe)$" }
			);

			config.Routes.MapHttpRoute(
				name: "MiscApi",
				routeTemplate: "api/{controller}/{action}"
			);

//#if DEBUG
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
//#endif
			_httpServer = new HttpServer(config);
		}

		public void Configuration(IAppBuilder app)
		{
			app.UseWebApi(_httpServer);
		}

		private static void EnableCors(HttpConfiguration config)
		{
			if (!string.IsNullOrEmpty(ConfigHelper.CorsOrigins))
			{
				var cors = new EnableCorsAttribute(ConfigHelper.CorsOrigins, "*", "*");

				cors.SupportsCredentials = ConfigHelper.SupportsCredentials;

				if (ConfigHelper.PreflightMaxAge > 0L)
					cors.PreflightMaxAge = ConfigHelper.PreflightMaxAge;

				config.EnableCors(cors);
			}
		}
	}
}
