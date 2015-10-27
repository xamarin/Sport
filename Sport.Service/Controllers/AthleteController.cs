using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using Sport.Service.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	[AuthorizeLevel(AuthorizationLevel.User)]
	public class AthleteController : TableController<Athlete>
	{
		AppContext _context = new AppContext();
		AuthenticationController _authController = new AuthenticationController();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			DomainManager = new EntityDomainManager<Athlete>(_context, Request, Services);
		}

		IQueryable<AthleteDto> ConvertAthleteToDto(IQueryable<Athlete> queryable)
		{
			return queryable.Select(a => new AthleteDto
			{
				Name = a.Name,
				Id = a.Id,
				Email = a.Email,
				Alias = a.Alias,
				DateCreated = a.CreatedAt,
				IsAdmin = a.IsAdmin,
				UserId = a.UserId,
				UpdatedAt = a.UpdatedAt,
				DeviceToken = a.DeviceToken,
				DevicePlatform = a.DevicePlatform,
				NotificationRegistrationId = a.NotificationRegistrationId,
				ProfileImageUrl = a.ProfileImageUrl,
				AuthenticationId = a.AuthenticationId,
				MembershipIds = a.Memberships.Where(m => m.AbandonDate == null).Select(m => m.Id).ToList(),
			});
		}


		// GET tables/Athlete
		public IQueryable<AthleteDto> GetAllAthletes()
		{
			return ConvertAthleteToDto(Query());
		}

		// GET tables/Athlete/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<AthleteDto> GetAthlete(string id)
		{
			return SingleResult.Create(ConvertAthleteToDto(Lookup(id).Queryable));
		}

		// PATCH tables/Athlete/48D68C86-6EA6-4C25-AA33-223FC9A27959
		async public Task<Athlete> PatchAthlete(string id, Delta<Athlete> patch)
		{
			var athlete = patch.GetEntity();
			var saved = _context.Athletes.SingleOrDefault(a => a.Id == athlete.Id);

			var c = _authController.IsCurrentUser(athlete);
			var b = _authController.IsCurrentUser(saved);

			if (!c || (saved.UserId != null && !b))
				throw "Invalid permission".ToException(Request);

			var exists = _context.Athletes.Any(l => l.Alias != null && l.Alias.Equals(athlete.Alias, StringComparison.InvariantCultureIgnoreCase)
				&& l.Id != athlete.Id);

			if(exists)
			{
				throw "The alias '{0}' is already in use.".Fmt(athlete.Alias).ToException(Request);
			}

			athlete.IsAdmin = saved.IsAdmin;
			return await UpdateAsync(id, patch);
		}

		// POST tables/Athlete
		public async Task<IHttpActionResult> PostAthlete(AthleteDto item)
		{
			var exists = _context.Athletes.Any(l => l.Email.Equals(item.Email, StringComparison.InvariantCultureIgnoreCase)
				|| (l.Alias != null && l.Alias.Equals(item.Alias, StringComparison.InvariantCultureIgnoreCase)));

			if(exists)
				return Conflict();

			if((item.Alias == null || item.Alias.Trim() == string.Empty) && item.Name != null)
				item.Alias = item.Name.Split(' ')[0];

			Athlete athlete = await InsertAsync(item.ToAthlete());
			return CreatedAtRoute("Tables", new
			{
				id = athlete.Id
			}, athlete);
		}

		// DELETE tables/Athlete/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task DeleteAthlete(string id)
		{
			_authController.EnsureAdmin(Request);
            return DeleteAsync(id);
		}

		[Route("api/getAthletesForLeague")]
		public IQueryable<AthleteDto> GetAthletesForLeague(string leagueId)
		{
			var query = from m in _context.Memberships
						from a in _context.Athletes
						where m.AthleteId == a.Id
						&& m.LeagueId == leagueId
						&& m.AbandonDate == null
						select a;

			return ConvertAthleteToDto(query);
		}
	}
}