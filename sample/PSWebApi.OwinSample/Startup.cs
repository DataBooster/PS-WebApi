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
			HttpConfiguration httpConfig = new HttpConfiguration();
			PSConfiguration psConfig = httpConfig.RegisterPsWebApi();

			EnableCors(httpConfig);
			httpConfig.MapHttpAttributeRoutes();

			httpConfig.Routes.MapHttpRoute(
				name: "PSWebApi",
				routeTemplate: "ps/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokePS_Async" },
				constraints: new { script = @".+\.ps1$" }
			);

			httpConfig.Routes.MapHttpRoute(
				name: "PSWebApi-Ext",
				routeTemplate: "ps.{ext}/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokePS_Async" },
				constraints: new { script = @".+\.ps1$", ext = psConfig.UriPathExtConstraint() }
			);

			httpConfig.Routes.MapHttpRoute(
				name: "CmdWebApi",
				routeTemplate: "cmd/{*script}",
				defaults: new { controller = "PSWebApi", action = "InvokeCMD_Async", ext = "string" },
				constraints: new { script = @".+\.(bat|exe)$" }
			);

			httpConfig.Routes.MapHttpRoute(
				name: "MiscApi",
				routeTemplate: "api/{controller}/{action}"
			);

			//#if DEBUG
			httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
			//#endif
			_httpServer = new HttpServer(httpConfig);
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
