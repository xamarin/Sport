using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Sport.Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sport.Service.Controllers
{
	[AuthorizeLevel(AuthorizationLevel.User)]
	public class AuthenticationController : TableController<Athlete>
	{
		AppContext _context = new AppContext();

		[Route("api/getUserIdentity")]
		public async Task<GoogleCredentials> GetUserIdentity()
		{
			ServiceUser serviceUser = User as ServiceUser;
			if(serviceUser != null)
			{
				var identity = await serviceUser.GetIdentitiesAsync();
				var credentials = identity.OfType<GoogleCredentials>().FirstOrDefault();
				return credentials;
			}

			return null;
		}

		string _userId;
		public string UserId
		{
			get
			{
				if(_userId == null) { }
				{
					var identity = GetUserIdentity().Result;
					if(identity != null)
						_userId = identity.UserId;
				}

				return _userId;
			}
		}

		public bool IsCurrentUser(Athlete athlete)
		{
			if(athlete == null)
				return false;

			var identity = GetUserIdentity().Result;
			if(identity == null)
				return false;

			return athlete.UserId == identity.UserId;
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