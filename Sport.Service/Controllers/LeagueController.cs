using Microsoft.Azure.Mobile.Server;
using Sport.Service.Models;
using Sport.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	//[Authorize]
	public class LeagueController : TableController<League>
	{
		AuthenticationController _authController = new AuthenticationController();
		NotificationController _notificationController = new NotificationController();
		MobileServiceContext _context = new MobileServiceContext();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			DomainManager = new EntityDomainManager<League>(_context, Request);
		}

		IQueryable<LeagueDto> ConvertLeagueToDto(IQueryable<League> queryable)
		{
			return queryable.Select(dto => new LeagueDto
			{
				Id = dto.Id,
				Name = dto.Name,
				Description = dto.Description,
				Sport = dto.Sport,
				IsEnabled = dto.IsEnabled,
				Deleted = dto.Deleted,
				CreatedAt = dto.CreatedAt,
				Version = dto.Version,
				UpdatedAt = dto.UpdatedAt,
				RulesUrl = dto.RulesUrl,
				CreatedByAthleteId = dto.CreatedByAthlete.Id,
				ImageUrl = dto.ImageUrl,
				Season = dto.Season,
				MaxChallengeRange = dto.MaxChallengeRange,
				MinHoursBetweenChallenge = dto.MinHoursBetweenChallenge,
				MatchGameCount = dto.MatchGameCount,
				HasStarted = dto.HasStarted,
				StartDate = dto.StartDate,
				EndDate = dto.EndDate,
				IsAcceptingMembers = dto.IsAcceptingMembers,
				//Memberships = dto.Memberships.Where(m => m.AbandonDate == null).OrderBy(m => m.CurrentRank).Select(m => new MembershipDto
				//{
				//	Id = m.Id,
				//	UpdatedAt = m.UpdatedAt,
				//	AthleteId = m.Athlete.Id,
				//	LeagueId = m.League.Id,
				//	IsAdmin = m.IsAdmin,
				//	CurrentRank = m.CurrentRank,
				//	LastRankChange = m.LastRankChange,
				//	DateCreated = m.CreatedAt,
				//}).ToList(),
				//OngoingChallenges = dto.Challenges.ToList().Where(c => c.DateCompleted == null).OrderBy(c => c.ProposedTime).Select(c => new ChallengeDto
				//{
				//	Id = c.Id,
				//	ChallengerAthleteId = c.ChallengerAthleteId,
				//	ChallengeeAthleteId = c.ChallengeeAthleteId,
				//	LeagueId = c.LeagueId,
				//	BattleForRank = c.BattleForRank,
				//	DateCreated = c.CreatedAt,
				//	ProposedTime = c.ProposedTime,
				//	UpdatedAt = c.UpdatedAt,
				//	DateAccepted = c.DateAccepted,
				//}).ToList()
			});
		}

		// GET tables/League
		async public Task<IQueryable<LeagueDto>> GetAllLeagues()
		{
			return ConvertLeagueToDto(Query());
		}

		// GET tables/League/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<LeagueDto> GetLeague(string id)
		{
			return SingleResult.Create(ConvertLeagueToDto(Lookup(id).Queryable));
		}

		[Route("api/startLeague")]
		public DateTime StartLeague(string id)
		{
			_authController.EnsureAdmin(Request);
			var league = _context.Leagues.SingleOrDefault(l => l.Id == id);
			league.HasStarted = true;
			league.StartDate = DateTime.Now.ToUniversalTime();

			_context.SaveChanges();

			var message = "The {0} league has officially started!".Fmt(league.Name);
			var payload = new NotificationPayload
			{
				Action = PushActions.LeagueStarted,
				Payload = { { "leagueId", id } }
			};

			_notificationController.NotifyByTag(message, league.Id, payload);
			return league.StartDate.Value.UtcDateTime;
		}

		// PATCH tables/League/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task<League> PatchLeague(string id, Delta<League> patch)
		{
			_authController.EnsureAdmin(Request);
			var league = _context.Leagues.SingleOrDefault(l => l.Id == id);

			var updated = patch.GetEntity();
			if (!league.IsAcceptingMembers && updated.IsAcceptingMembers)
			{
				//NotifyAboutNewLeagueOpenEnrollment(updated);
			}

			return UpdateAsync(id, patch);
		}

		void NotifyAboutNewLeagueOpenEnrollment(League league)
		{
			var date = league.StartDate.Value.DateTime.ToOrdinal();
			var message = "Open enrollment for the {0} league has started. The league will begin on {1}".Fmt(league.Name, date);
			var payload = new NotificationPayload
			{
				Action = PushActions.LeagueStarted,
				Payload = { { "leagueId", league.Id } }
			};

			if(!Startup.IsDemoMode)
			{
				_notificationController.NotifyByTag(message, "All", payload);
			}
		}

		// POST tables/League
		public async Task<IHttpActionResult> PostLeague(LeagueDto item)
		{
			_authController.EnsureAdmin(Request);
			var exists = _context.Leagues.Any(l => l.Name.Equals(item.Name, System.StringComparison.InvariantCultureIgnoreCase));

			if (exists)
				return BadRequest("The name of that league is already in use.");

			League league = await InsertAsync(item.ToLeague());

			if (league.IsAcceptingMembers)
			{
				NotifyAboutNewLeagueOpenEnrollment(league);
			}

			return CreatedAtRoute("Tables", new { id = league.Id }, league);
		}

		// DELETE tables/League/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task DeleteLeague(string id)
		{
			_authController.EnsureAdmin(Request);
			var league = _context.Leagues.SingleOrDefault(l => l.Id == id);
			var message = "The {0} league has been removed.".Fmt(league.Name);
			var payload = new NotificationPayload
			{
				Action = PushActions.LeagueEnded,
				Payload = { { "leagueId", id } }
			};

			if(!Startup.IsDemoMode)
			{
				_notificationController.NotifyByTag(message, league.Id, payload);
			}

			return DeleteAsync(id);
		}
	}
}