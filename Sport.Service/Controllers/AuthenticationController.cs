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
		
	[AuthorizeLevel(AuthorizationLevel.User)]
    public async Task<JObject> GetUserInfo()
    {
        //Get the current logged in user
        ServiceUser user = this.User as ServiceUser;
        if (user == null)
        {
            throw new InvalidOperationException("This can only be called by authenticated clients");
        }

        //Get Identity Information for the current logged in user
        var identities = await user.GetIdentitiesAsync();
        var result = new JObject();

        //Check if the user has logged in using Facebook as Identity provider
        var fb = identities.OfType<FacebookCredentials>().FirstOrDefault();
        if (fb != null)
        {
            var accessToken = fb.AccessToken;
            result.Add("facebook", await GetProviderInfo("https://graph.facebook.com/me?access_token=" + accessToken));
        }

        //Check if the user has logged in using Microsoft Identity provider
        var ms = identities.OfType<MicrosoftAccountCredentials>().FirstOrDefault();
        if (ms != null)
        {
            var accessToken = ms.AccessToken;
            result.Add("microsoft", await GetProviderInfo("https://apis.live.net/v5.0/me/?method=GET&access_token=" + accessToken));
        }

        //Check if the user has logged in using Google as Identity provider
        var google = identities.OfType<GoogleCredentials>().FirstOrDefault();
        if (google != null)
        {
            var accessToken = google.AccessToken;
            result.Add("google", await GetProviderInfo("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + accessToken));
        }

        return result;
    }

    private async Task<JToken> GetProviderInfo(string url)
    {
        var c = new HttpClient();
        var resp = await c.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return JToken.Parse(await resp.Content.ReadAsStringAsync());
    }
}
}