using System.Web.Http;
using DataBooster.PSWebApi;

namespace PSWebApi.OwinSample.Controllers
{
	public class MiscController : ApiController
	{
		[AcceptVerbs("GET", "POST")]
		public string WhoAmI()
		{
			string userName = this.GetUserName();
			return string.IsNullOrEmpty(userName) ? "?" : userName;
		}
	}
}
