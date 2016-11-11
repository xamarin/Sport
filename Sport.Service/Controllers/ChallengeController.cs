using Microsoft.Azure.Mobile.Server;
using Sport.Service.Models;
using Sport.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Sport.Service.Controllers
{
	[Authorize]
	public class ChallengeController : TableController<Challenge>
	{
		AuthenticationController _authController = new AuthenticationController();
		NotificationController _notificationController = new NotificationController();
		MobileServiceContext _context = new MobileServiceContext();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			DomainManager = new EntityDomainManager<Challenge>(_context, Request, true);
		}

		public static IQueryable<ChallengeDto> ConvertChallengeToDto(IQueryable<Challenge> queryable)
		{
			return queryable.Select(dto => new ChallengeDto
			{
				Id = dto.Id,
				ChallengerAthleteId = dto.ChallengerAthleteId,
				ChallengeeAthleteId = dto.ChallengeeAthleteId,
				LeagueId = dto.LeagueId,
				Deleted = dto.Deleted,
				CreatedAt = dto.CreatedAt,
				Version = dto.Version,
				BattleForRank = dto.BattleForRank,
				UpdatedAt = dto.UpdatedAt,
				ProposedTime = dto.ProposedTime,
				DateAccepted = dto.DateAccepted,
				DateCompleted = dto.DateCompleted,
				CustomMessage = dto.CustomMessage,
			});
		}

		// GET tables/Challenge
		public IQueryable<ChallengeDto> GetAllChallenges()
		{
			return ConvertChallengeToDto(Query());
		}

		// GET tables/Challenge/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<ChallengeDto> GetChallenge(string id)
		{
			return SingleResult.Create(ConvertChallengeToDto(Lookup(id).Queryable));
		}

		// POST tables/Challenge
		public async Task<IHttpActionResult> PostChallenge(ChallengeDto item)
		{
			var challenger = _context.Athletes.SingleOrDefault(a => a.Id == item.ChallengerAthleteId);
			var challengee = _context.Athletes.SingleOrDefault(a => a.Id == item.ChallengeeAthleteId);

			_authController.EnsureHasPermission(challenger, Request);

			if (challenger == null || challengee == null)
				throw "The opponent in this challenge no longer belongs to this league.".ToException(Request);

			var challengerMembership = _context.Memberships.SingleOrDefault(m => m.AbandonDate == null && m.AthleteId == challenger.Id && m.LeagueId == item.LeagueId);
			var challengeeMembership = _context.Memberships.SingleOrDefault(m => m.AbandonDate == null && m.AthleteId == challengee.Id && m.LeagueId == item.LeagueId);

			if (challengerMembership == null || challengeeMembership == null)
				throw "The opponent in this challenge no longer belongs to this league.".ToException(Request);

			//Check to see if there are any ongoing challenges between both athletes
			var challengeeOngoing = _context.Challenges.Where(c => ((c.ChallengeeAthleteId == item.ChallengeeAthleteId || c.ChallengerAthleteId == item.ChallengeeAthleteId)
                && (c.ChallengeeAthleteId == item.ChallengerAthleteId || c.ChallengerAthleteId == item.ChallengerAthleteId)
                && c.LeagueId == item.LeagueId && c.DateCompleted == null) && !c.Deleted);

			if (challengeeOngoing.Count() > 0)
				throw "{0} already has an existing challenge underway with {1}.".Fmt(challengee.Alias, challenger.Alias).ToException(Request);

			var league = _context.Leagues.SingleOrDefault(l => l.Id == item.LeagueId);

			try
			{
				var challenge = item.ToChallenge();
				var json = Newtonsoft.Json.JsonConvert.SerializeObject(challenge);

				Challenge current = await InsertAsync(challenge);
				var result = CreatedAtRoute("Tables", new { id = current.Id }, current.ToChallengeDto());

				var message = "{0}: You have been challenged to a duel by {1}!".Fmt(league.Name, challenger.Alias);
				var payload = new NotificationPayload
				{
					Action = PushActions.ChallengePosted,
					Payload = { { "challengeId", current.Id }, { "leagueId", current.LeagueId } }
				};
				_notificationController.NotifyByTag(message, current.ChallengeeAthleteId, payload);
				return result;
			}
			catch (Exception e)
			{
				return null;
			}

			//Not awaiting so the user's result is not delayed
		}


		// PATCH tables/Challenge/48D68C86-6EA6-4C25-AA33-223FC9A27959
		async public Task<Challenge> PatchChallenge(string id, Delta<Challenge> patch)
		{
			var prevChallenge = Lookup(id).Queryable.FirstOrDefault();
			var updatedChallenge = patch.GetEntity();

			var challenger = _context.Athletes.SingleOrDefault(a => a.Id == updatedChallenge.ChallengerAthleteId);
			var challengee = _context.Athletes.SingleOrDefault(a => a.Id == updatedChallenge.ChallengeeAthleteId);

			//The challengee is accepting the challenge
			if (prevChallenge.DateAccepted == null && updatedChallenge.DateAccepted != null)
			{
				_authController.EnsureHasPermission(new Athlete[] { challengee, challenger }, Request);
				return await AcceptChallenge(prevChallenge);
			}
			else
			{
				return await UpdateAsync(id, patch);
			}
		}

		// DELETE tables/Challenge/48D68C86-6EA6-4C25-AA33-223FC9A27959
		async public Task DeleteChallenge(string id)
		{
			var challenge = Lookup(id).Queryable.FirstOrDefault();

			if (challenge == null)
				return;

			var challenger = _context.Athletes.SingleOrDefault(a => a.Id == challenge.ChallengerAthleteId);
			var challengee = _context.Athletes.SingleOrDefault(a => a.Id == challenge.ChallengeeAthleteId);
			var canDelete = false;

			if (_authController.UserId == challengee.UserId)
			{
				//Challengee is declining
				var message = "Your challenge with {0} has been declined.".Fmt(challenge.ChallengeeAthlete.Alias);
				var payload = new NotificationPayload
				{
					Action = PushActions.ChallengeDeclined,
					Payload = { { "challengeId", challenge.Id }, { "leagueId", challenge.LeagueId } }
				};
				await _notificationController.NotifyByTag(message, challenge.ChallengerAthleteId, payload);
				canDelete = true;
			}
			else if(_authController.UserId == challenger.UserId)
			{
				//Challenger is revoking
				var message = "Your challenge with {0} has been revoked.".Fmt(challenge.ChallengerAthlete.Alias);
				var payload = new NotificationPayload
				{
					Action = PushActions.ChallengeRevoked,
					Payload = { { "challengeId", challenge.Id }, { "leagueId", challenge.LeagueId } }
				};
				await _notificationController.NotifyByTag(message, challenge.ChallengerAthleteId, payload);
				canDelete = true;
			}

			if(canDelete)
				await DeleteAsync(challenge.Id);
		}

		async Task<Challenge> AcceptChallenge(Challenge challenge)
		{
			if (challenge == null)
				throw "This challenge no longer exists".ToException(Request);

			_authController.EnsureHasPermission(challenge.ChallengeeAthlete, Request);
			challenge.DateAccepted = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			var league = _context.Leagues.SingleOrDefault(l => l.Id == challenge.LeagueId);
			var challengee = _context.Athletes.SingleOrDefault(a => a.Id == challenge.ChallengeeAthleteId);
			var message = "{0}: Your challenge with {1} has been accepted!".Fmt(league.Name, challengee.Alias);
			var payload = new NotificationPayload
			{
				Action = PushActions.ChallengeAccepted,
				Payload = { { "challengeId", challenge.Id }, { "leagueId", challenge.LeagueId } }
			};

			await _notificationController.NotifyByTag(message, challenge.ChallengerAthleteId, payload);
			return challenge;
		}

		[Route("api/postMatchResults")]
		public ChallengeDto PostMatchResults(List<GameResultDto> results)
		{
			if (results.Count < 1)
				throw "No game scores were submitted.".ToException(Request);

			var challengeId = results.First().ChallengeId;
			var challenge = _context.Challenges.SingleOrDefault(c => c.Id == challengeId);

			if (challenge == null)
				throw "This challenge no longer exists".ToException(Request);

			if (challenge.DateCompleted != null)
				throw "Scores for this challenge have already been submitted.".ToException(Request);

			var league = _context.Leagues.SingleOrDefault(l => l.Id == challenge.LeagueId);

			if (league == null)
				throw "This league no longer exists".ToException(Request);

			if (challenge.ChallengerAthlete == null || challenge.ChallengeeAthlete == null)
				throw "The opponent in this challenge no longer belongs to this league".ToException(Request);

			var challengerMembership = _context.Memberships.SingleOrDefault(m => m.AthleteId == challenge.ChallengerAthlete.Id && m.AbandonDate == null && m.LeagueId == challenge.LeagueId);
			var challengeeMembership = _context.Memberships.SingleOrDefault(m => m.AthleteId == challenge.ChallengeeAthlete.Id && m.AbandonDate == null && m.LeagueId == challenge.LeagueId);

			if (challengerMembership == null || challengeeMembership == null)
				throw "The opponent in this challenge no longer belongs to this league".ToException(Request);

			_authController.EnsureHasPermission(new[] { challenge.ChallengerAthlete, challenge.ChallengeeAthlete }, Request);

			var tempChallenge = new Challenge();
			tempChallenge.League = league;
			tempChallenge.MatchResult = results.Select(g => g.ToGameResult()).ToList();

			var errorMessage = tempChallenge.ValidateMatchResults();

			if (errorMessage != null)
				throw errorMessage.ToException(Request);

			tempChallenge = null;
			challenge.DateCompleted = DateTime.UtcNow;
			var dto = challenge.ToChallengeDto();

			foreach (var result in results)
			{
				result.Id = Guid.NewGuid().ToString();
				_context.GameResults.Add(result.ToGameResult());
			}

			try
			{
				var prevChallengeeRating = challengeeMembership.CurrentRating;
				var prevChallengerRating = challengerMembership.CurrentRating;
				var wasChallengeeRatedHigher = prevChallengeeRating > prevChallengerRating;
				
				var challengeeIsWinner = DetermineWinnerAndUpdateRating(challenge);
				_context.SaveChanges();

				var winner = challengeeIsWinner ? challenge.ChallengeeAthlete : challenge.ChallengerAthlete;
				var loser = challengeeIsWinner ? challenge.ChallengerAthlete : challenge.ChallengeeAthlete;
				var mems = challenge.League.Memberships.OrderByDescending(m => m.CurrentRating).ToList();
                var newRank = mems.IndexOf(mems.Single(m => m.AthleteId == winner.Id)) + 1;
				var message = $"{winner.Alias} victors over {loser.Alias} and is now in {newRank.ToOrdinal()} place in {league.Name}.";
				var payload = new NotificationPayload
				{
					Action = PushActions.ChallengeCompleted,
					Payload = { { "challengeId", challengeId },
						{ "leagueId", league.Id },
						{"winningAthleteId", winner.Id},
						{"losingAthleteId", loser.Id} }
				};

				if(!Startup.IsDemoMode)
				{
					_notificationController.NotifyByTag(message, challenge.LeagueId, payload);
				}
			}
			catch (DbEntityValidationException e)
			{
				#region Error Print

				foreach (var eve in e.EntityValidationErrors)
				{
					Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
						eve.Entry.Entity.GetType().Name, eve.Entry.State);
					foreach (var ve in eve.ValidationErrors)
					{
						Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
							ve.PropertyName, ve.ErrorMessage);
					}
				}
				throw;

				#endregion
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return dto;
		}

		bool DetermineWinnerAndUpdateRating(Challenge challenge)
		{
			var player1Membership = (from membership in challenge.ChallengeeAthlete.Memberships
									 where membership.LeagueId == challenge.LeagueId
									 select membership).FirstOrDefault();

			var player2Membership = (from membership in challenge.ChallengerAthlete.Memberships
									 where membership.LeagueId == challenge.LeagueId
									 select membership).FirstOrDefault();

			IncreaseGameCounts(challenge, player1Membership, player2Membership);

			var player1Rating = player1Membership.CurrentRating;
			var player2Rating = player2Membership.CurrentRating;

			var r1 = Math.Pow(10, player1Rating / 400.0);
			var r2 = Math.Pow(10, player2Rating / 400.0);

			var e1 = (float)(r1 / (r1 + r2));
			var e2 = (float)(r2 / (r1 + r2));

			var s1 = S(challenge, challenge.ChallengeeAthlete);
			var s2 = S(challenge, challenge.ChallengerAthlete);

			var k1 = K(player1Membership);
			var k2 = K(player2Membership);

			player1Membership.CurrentRating = (float)Math.Round(player1Rating + k1 * (s1 - e1));
			player2Membership.CurrentRating = (float)Math.Round(player2Rating + k2 * (s2 - e2));

			Console.WriteLine($"player1 rating = {player1Membership.CurrentRating}\nplayer2 rating = {player2Membership.CurrentRating}");
			return s1 > s2;
		}

		public static float K(Membership membership)
		{
			if (membership.NumberOfGamesPlayed < 30)
				return 40f;
			else if (membership.CurrentRating > 2400)
				return 10f;
			else
				return 20f;
		}

		public static float S(Challenge challenge, Athlete player)
		{
			var challengeeWins = (from game in challenge.MatchResult
								  where game.ChallengeeScore > game.ChallengerScore
								  select 1).Sum();

			var challengerWins = (from game in challenge.MatchResult
								  where game.ChallengerScore > game.ChallengeeScore
								  select 1).Sum();

			Console.WriteLine($"Challengee wins = {challengeeWins}\tChallenger wins= {challengerWins}");

			if (challengeeWins == challengerWins)
				return 0.5f;

			var winner = challengeeWins > challengerWins ? challenge.ChallengeeAthlete : challenge.ChallengerAthlete;

			if (winner.UserId == player.UserId) // is this good enough to compare?
				return 1f;

			return 0f;
		}

		public static void IncreaseGameCounts(Challenge challenge, Membership player1Membership, Membership player2Membership)
		{
			player1Membership.NumberOfGamesPlayed++;
			player2Membership.NumberOfGamesPlayed++;
		}

		[HttpGet]
		[Route("api/nudgeAthlete")]
		public async Task NudgeAthlete(string challengeId)
		{
			if (Startup.IsDemoMode)
				throw "Nudging is disabled in Demo Mode.".ToException(Request);

			var challenge = Lookup(challengeId).Queryable.FirstOrDefault();

			if (challenge == null)
				throw "This challenge no longer exists".ToException(Request);

			_authController.EnsureHasPermission(challenge.ChallengerAthlete, Request);

			if (challenge.ChallengerAthlete == null)
				throw "The challenger no longer exists".ToException(Request);

			var message = "{0} would be much obliged if you'd accept their challenge.".Fmt(challenge.ChallengerAthlete.Alias);
			var payload = new NotificationPayload
			{
				Action = PushActions.ChallengePosted,
				Payload = { { "challengeId", challengeId }, { "leagueId", challenge.LeagueId } }
			};

			await _notificationController.NotifyByTag(message, challenge.ChallengeeAthleteId, payload);
		}
	}
}