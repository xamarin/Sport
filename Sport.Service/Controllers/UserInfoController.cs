using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Mobile.Server.Authentication;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Net.Http;

namespace Sport.Service.Controllers
{
    [Authorize]
    [MobileAppController]
    public class UserInfoController : ApiController
    {
        // GET api/UserInfo
        public string Get()
        {
            return "Hello from custom controller!";
        }

        //public ApiServices Services { get; set; }
       // [AuthorizeLevel(AuthorizationLevel.User)]
        [HttpGet, Route("api/getUserInfo")]
        public async Task<JObject> GetUserInfo()
        {
            //Get the current logged in user
            //ServiceUser user = this.User as ServiceUser;
            var cp = User as ClaimsPrincipal;

            if (cp == null)
            {
                throw new InvalidOperationException("This can only be called by authenticated clients");
            }

            var id = cp.FindFirst(ClaimTypes.NameIdentifier);
            //var creds = await User.GetAppServiceIdentityAsync<GoogleCredentials>(Request);

            //Get Identity Information for the current logged in user
            var identities = cp.Identities;//.GetIdentitiesAsync();
            var result = new JObject();

            //Check if the user has logged in using Facebook as Identity provider
            var fb = await User.GetAppServiceIdentityAsync<FacebookCredentials>(Request);// identities.OfType<FacebookCredentials>().FirstOrDefault();
            if (fb != null)
            {
                var accessToken = fb.AccessToken;
                result.Add("facebook", await GetProviderInfo("https://graph.facebook.com/me?fields=name,email&access_token=" + accessToken));
            }

            //Check if the user has logged in using Microsoft Identity provider
            var ms = await User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(Request);//identities.OfType<MicrosoftAccountCredentials>().FirstOrDefault();
            if (ms != null)
            {
                var accessToken = ms.AccessToken;
                result.Add("microsoft", await GetProviderInfo("https://apis.live.net/v5.0/me/?method=GET&access_token=" + accessToken));
            }

            //Check if the user has logged in using Google as Identity provider
            var google = await User.GetAppServiceIdentityAsync<GoogleCredentials>(Request);//identities.OfType<GoogleCredentials>().FirstOrDefault();
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
