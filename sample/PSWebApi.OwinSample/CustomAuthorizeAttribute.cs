using System.Web.Http;
using System.Web.Http.Controllers;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample
{
	public partial class CustomAuthorizeAttribute : AuthorizeAttribute
	{
		/// <param name="actionContext">The context.</param>
		/// <returns>true if the control is authorized; otherwise, false.</returns>
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			string user = actionContext.GetUserName();
			string script = actionContext.GetRouteData("script") as string;

			return CheckPrivilege(script, user);
		}

		private bool CheckPrivilege(string script, string user)
		{
			// TO DO, please implementate your own authorization logic.

			return true;	// If allow permission
			return false;	// If deny permission
		}
	}
}
