using Microsoft.Azure.Mobile.Server;
using Sport.Service.Models;
using Sport.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	[Authorize]
	public class MembershipController : TableController<Membership>
    {
		object _syncObject = new object();
		AuthenticationController _authController = new AuthenticationController();
		NotificationController _notificationController = new NotificationController();
        MobileServiceContext _context = new MobileServiceContext();

		protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            DomainManager = new EntityDomainManager<Membership>(_context, Request, true);
        }

        // GET tables/Member
		public IQueryable<MembershipDto> GetAllMemberships()
        {
			return ConvertMembershipToDto(Query());
        }

        // GET tables/Member/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<MembershipDto> GetMembership(string id)
        {
			return SingleResult.Create(ConvertMembershipToDto(Lookup(id).Queryable));
		}

		IQueryable<MembershipDto> ConvertMembershipToDto(IQueryable<Membership> queryable)
		{
			return queryable.Select(dto => new MembershipDto
			{
				Id = dto.Id,
				UpdatedAt = dto.UpdatedAt,
				AthleteId = dto.Athlete.Id,
				LeagueId = dto.League.Id,
				IsAdmin = dto.IsAdmin,
				AbandonDate = dto.AbandonDate,
                Deleted = dto.Deleted,
                CreatedAt = dto.CreatedAt,
                Version = dto.Version,
                CurrentRating = dto.CurrentRating,
				NumberOfGamesPlayed = dto.NumberOfGamesPlayed,
				LastRankChange = dto.LastRankChange,
			});
		}

        // PATCH tables/Member/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task<Membership> PatchMembership(string id, Delta<Membership> patch)
        {
			_authController.EnsureHasPermission(patch.GetEntity().Athlete, Request);
            return UpdateAsync(id, patch);
        }

        // POST tables/Member
		public async Task<IHttpActionResult> PostMembership(MembershipDto item)
        {
			Membership current = null;
			var exists = _context.Memberships.FirstOrDefault(m => m.AthleteId == item.AthleteId && m.LeagueId == item.LeagueId && m.AbandonDate == null);
			if(exists != null)
			{
				//Athlete is already a member of this league
				current = exists;
			}
			else
			{
				try
				{
					var membership = item.ToMembership();
					current = await InsertAsync(membership);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			var result = CreatedAtRoute("Tables", new { id = current.Id }, current);

			if(Startup.IsDemoMode)
			{
				var tasks = new List<Task>();
				//Keep the lists to a reasonable amount for the public-facing service
				lock (_syncObject)
				{
					var query = _context.Memberships.Where(m => m.LeagueId == item.LeagueId && m.AbandonDate == null);
					var list = ConvertMembershipToDto(query).ToList();

					if (list.Count > Startup.MaxLeagueMembershipCount)
					{
						var diff = list.Count - Startup.MaxLeagueMembershipCount;
						var oldest = list.OrderBy(m => m.CreatedAt).Take(diff).Select(m => m.Id).ToList();

						foreach (var mId in oldest)
						{
							if (mId == current.Id)
							{
								continue;
							}

							tasks.Add(DeleteMembershipInternal(mId));
						}
					}
				}

				try
				{
					await Task.WhenAll(tasks);
				}
				catch (Exception ex)
				{
					//TODO log to Insights
					Console.WriteLine(ex);
				}
			}

			try
			{
				var leagueName = _context.Leagues.Where(l => l.Id == item.LeagueId).Select(l => l.Name).ToList().First();
				var athlete = _context.Athletes.Where(a => a.Id == item.AthleteId).First();
				var message = "{0} joined the {1} league".Fmt(athlete.Alias, leagueName);

				if(!Startup.IsDemoMode)
				{
					await _notificationController.NotifyByTag(message, item.LeagueId);
				}
			}
			catch(Exception e)
			{
				//TODO log to Insights
				Console.WriteLine(e);
			}
            
			return result;
		}

		// DELETE tables/Member/48D68C86-6EA6-4C25-AA33-223FC9A27959
		async public Task DeleteMembership(string id)
		{
			var membership = _context.Memberships.SingleOrDefault(m => m.Id == id);
			_authController.EnsureHasPermission(membership.Athlete, Request);
			await DeleteMembershipInternal(id);
		}

		object _deleteSync = new object();
		async Task DeleteMembershipInternal(string id)
        {
			lock(_deleteSync)
			{
				var membership = _context.Memberships.SingleOrDefault(m => m.Id == id);

				//Need to remove all the ongoing challenges (not past challenges since those should be locked and sealed in time for eternity for all to see)
				var challenges = _context.Challenges.Where(c => c.LeagueId == membership.LeagueId && c.DateCompleted == null && !c.Deleted
					&& (c.ChallengerAthleteId == membership.AthleteId || c.ChallengeeAthleteId == membership.AthleteId)).ToList();

				if(!Startup.IsDemoMode)
				{
					foreach (var c in challenges)
					{
						try
						{
							var league = _context.Leagues.SingleOrDefault(l => l.Id == c.LeagueId);
							var payload = new NotificationPayload
							{
								Action = PushActions.ChallengeDeclined,
								Payload = { { "leagueId", c.LeagueId }, { "challengeId", c.Id } }
							};
							var message = "You challenge with {0} has ben removed because they abandoned the {1} league".Fmt(membership.Athlete.Alias, league.Name);
							_notificationController.NotifyByTag(message, c.Opponent(membership.AthleteId).Id, payload);
						}
						catch (Exception e)
						{
							//TODO log to Insights
							Console.WriteLine(e);
						}
					}
				}

				membership.AbandonDate = DateTime.UtcNow;
				challenges.ForEach(c => c.Deleted = true);
				_context.SaveChanges();
			}
		}
	}
}