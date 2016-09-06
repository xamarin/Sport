using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Sport.Service.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sport.Service.Controllers
{
    [Authorize]
	public class AuthenticationController : TableController<Athlete>
	{
		MobileServiceContext _context = new MobileServiceContext();

		[HttpGet, Route("api/getUserIdentity")]
		public async Task<GoogleCredentials> GetUserIdentity(HttpRequestMessage request = null)
		{
            try
            {
				var cp = User as ClaimsPrincipal;
				var id = cp.FindFirst(ClaimTypes.NameIdentifier);
                var creds = await User.GetAppServiceIdentityAsync<GoogleCredentials>(request ?? Request);
                return creds;
            }
            catch (Exception e)
            {
                throw;
            }
        }

		string _userId;
		public string UserId
		{
			get
			{
				if (_userId == null) { }
				{
					var claimsUser = (ClaimsPrincipal)User;
					var id = claimsUser?.FindFirst(ClaimTypes.NameIdentifier);
					if(id != null)
						_userId = id.Value;
				}

				return _userId;
			}
		}

		public bool IsCurrentUser(Athlete athlete)
		{
			if(athlete == null)
				return false;

			return athlete.UserId == UserId;
		}
		public void EnsureHasPermission(Athlete athlete, HttpRequestMessage request)
		{
			EnsureHasPermission(new Athlete[] { athlete }, request);
		}

		public void EnsureHasPermission(Athlete[] athletes, HttpRequestMessage request)
		{
			foreach(var a in athletes)
			{
				if(a != null && a.UserId == UserId)
					return;
			}

			throw "Invalid permission".ToException(request);
		}

		public void EnsureAdmin(HttpRequestMessage request)
		{
			if(UserId != null)
			{
				var isAdmin = _context.Athletes.SingleOrDefault(a => a.UserId == UserId && a.IsAdmin) != null;

				if(isAdmin)
					return;
			}

			throw "Invalid permission".ToException(request);
		}
	}
}