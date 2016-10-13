using Microsoft.Azure.Mobile.Server;
using Sport.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	//[Authorize]
	public class AthleteController : TableController<Athlete>
	{
		MobileServiceContext _context = new MobileServiceContext();
		AuthenticationController _authController = new AuthenticationController();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			DomainManager = new EntityDomainManager<Athlete>(_context, Request);
		}

		IQueryable<AthleteDto> ConvertAthleteToDto(IQueryable<Athlete> queryable)
		{
			return queryable.Select(dto => new AthleteDto
			{
				Name = dto.Name,
				Id = dto.Id,
				Email = dto.Email,
				Alias = dto.Alias,
				Deleted = dto.Deleted,
				CreatedAt = dto.CreatedAt,
				Version = dto.Version,
				IsAdmin = dto.IsAdmin,
				UserId = dto.UserId,
				UpdatedAt = dto.UpdatedAt,
				DeviceToken = dto.DeviceToken,
				DevicePlatform = dto.DevicePlatform,
				NotificationRegistrationId = dto.NotificationRegistrationId,
				ProfileImageUrl = dto.ProfileImageUrl,
				AuthenticationId = dto.AuthenticationId,
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

			//TODO - TEMP
			var c = _authController.IsCurrentUser(athlete);
			var b = _authController.IsCurrentUser(saved);

			if (!c || (saved.UserId != null && !b))
				throw "Invalid permission".ToException(Request);

			var exists = _context.Athletes.Any(l => l.Alias != null && l.Alias.Equals(athlete.Alias, StringComparison.InvariantCultureIgnoreCase)
				&& l.Id != athlete.Id);

			if (exists)
			{
				throw "The alias '{0}' is already in use.".Fmt(athlete.Alias).ToException(Request);
			}

			//Disacard any attempt to modify these properties by the client
			athlete.IsAdmin = saved.IsAdmin;
			athlete.UserId = saved.UserId;

			return await UpdateAsync(id, patch);
		}

		// POST tables/Athlete
		public async Task<IHttpActionResult> PostAthlete(AthleteDto item)
		{
			bool first = _context.Athletes.Count() == 0;

			var exists = _context.Athletes.Any(l => l.Email.Equals(item.Email, StringComparison.InvariantCultureIgnoreCase)
				|| (l.Alias != null && l.Alias.Equals(item.Alias, StringComparison.InvariantCultureIgnoreCase)));

			if (exists)
				return Conflict();

			if ((item.Alias == null || item.Alias.Trim() == string.Empty) && item.Name != null)
				item.Alias = item.Name.Split(' ')[0];

			item.UserId = _authController.UserId;
			Athlete athlete = await InsertAsync(item.ToAthlete());

			if (first)
			{
				//Seed some leagues for the user to join
				Seed(athlete);
			}

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

		protected void Seed(Athlete athlete)
		{
			var leagues = new List<League>
			{
				new League
				{
				  Id = Guid.NewGuid().ToString(),
					Name = "Table Tennis",
					Description = "It's like tennis for giants",
					ImageUrl = "https://c5.staticflickr.com/6/5151/14307225316_24c814660a_k.jpg",
					MatchGameCount = 3,
					MaxChallengeRange = 2,
					IsEnabled = true,
					HasStarted = true,
					MinHoursBetweenChallenge = 24,
					StartDate = DateTime.Now,
					EndDate = DateTime.Now.AddMonths(3),
					IsAcceptingMembers = true,
					Season = 1,
					RulesUrl = "http://www.ittf.com/ittf_handbook/2014/2014_EN_HBK_CHPT_2.pdf",
					CreatedByAthlete = athlete,
					Memberships = new List<Membership>(),
					Challenges = new List<Challenge>()
				},
				new League
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Billiards",
					Description = "Otherwise known as Pocketball and Stick",
					ImageUrl = "https://c8.staticflickr.com/2/1690/24814543375_58e16dbfa3_b.jpg",
					MatchGameCount = 1,
					MaxChallengeRange = 2,
					IsEnabled = true,
					HasStarted = true,
					MinHoursBetweenChallenge = 24,
					StartDate = DateTime.Now,
					EndDate = DateTime.Now.AddMonths(3),
					IsAcceptingMembers = true,
					Season = 1,
					CreatedByAthlete = athlete,
				}
			};

			leagues.ForEach(l => _context.Leagues.Add(l));
			_context.SaveChanges();
		}
	}
}