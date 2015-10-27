using Newtonsoft.Json;
using Sport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Sport.Service
{
	public static class Extensions
	{
		public static HttpResponseException ToException(this string s, HttpRequestMessage msg)
		{
			return new HttpResponseException(msg.CreateBadRequestResponse(s));
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			Random rng = new Random();
			int n = list.Count;
			while(n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static GameResult ToGameResult(this GameResultDto dto)
		{
			return new GameResult
			{
				Id = dto.Id,
				ChallengeId = dto.ChallengeId,
				ChallengeeScore = dto.ChallengeeScore,
				ChallengerScore = dto.ChallengerScore,
				Index = dto.Index
			};
		}

		public static ChallengeDto ToChallengeDto(this Challenge dto)
		{
			return new ChallengeDto
			{
				Id = dto.Id,
				ChallengerAthleteId = dto.ChallengerAthleteId,
				ChallengeeAthleteId = dto.ChallengeeAthleteId,
				LeagueId = dto.LeagueId,
				UpdatedAt = dto.UpdatedAt,
				DateCreated = dto.CreatedAt,
				BattleForRank = dto.BattleForRank,
				ProposedTime = dto.ProposedTime,
				DateAccepted = dto.DateAccepted,
				DateCompleted = dto.DateCompleted,
				CustomMessage = dto.CustomMessage,
				MatchResult = dto.MatchResult.Select(r => new GameResultDto
				{
					Id = r.Id,
					DateCreated = r.CreatedAt,
					ChallengeId = r.ChallengeId,
					ChallengeeScore = r.ChallengeeScore,
					ChallengerScore = r.ChallengerScore,
					Index = r.Index
				}).ToList()
			};
		}

		public static Challenge ToChallenge(this ChallengeDto dto)
		{
			return new Challenge
			{
				Id = dto.Id,
				UpdatedAt = dto.UpdatedAt,
				ChallengerAthleteId = dto.ChallengerAthleteId,
				ChallengeeAthleteId = dto.ChallengeeAthleteId,
				LeagueId = dto.LeagueId,
				BattleForRank = dto.BattleForRank,
				ProposedTime = dto.ProposedTime,
				DateAccepted = dto.DateAccepted,
				DateCompleted = dto.DateCompleted,
				CustomMessage= dto.CustomMessage
			};
		}

		public static Athlete ToAthlete(this AthleteDto dto)
		{
			return new Athlete
			{
				Name = dto.Name,
				Id = dto.Id,
				UpdatedAt = dto.UpdatedAt,
				Email = dto.Email,
				IsAdmin = dto.IsAdmin,
				UserId = dto.UserId,
				DeviceToken = dto.DeviceToken,
				Alias = dto.Alias,
				DevicePlatform = dto.DevicePlatform,
				NotificationRegistrationId = dto.NotificationRegistrationId,
				ProfileImageUrl = dto.ProfileImageUrl,
				AuthenticationId = dto.AuthenticationId,
			};
		}

		public static League ToLeague(this LeagueDto dto)
		{
			return new League
			{
				Id = dto.Id,
				Name = dto.Name,
				Description = dto.Description,
				Sport = dto.Sport,
				IsEnabled = dto.IsEnabled,
				StartDate = dto.StartDate,
				EndDate = dto.EndDate,
				UpdatedAt = dto.UpdatedAt,
				Season = dto.Season,
				RulesUrl = dto.RulesUrl,
				MaxChallengeRange = dto.MaxChallengeRange,
				MinHoursBetweenChallenge = dto.MinHoursBetweenChallenge,
				MatchGameCount = dto.MatchGameCount,
				HasStarted = dto.HasStarted,
				ImageUrl = dto.ImageUrl,
				CreatedByAthleteId = dto.CreatedByAthleteId,
				IsAcceptingMembers = dto.IsAcceptingMembers
			};
		}

		public static Membership ToMembership(this MembershipDto dto)
		{
			return new Membership
			{
				Id = dto.Id,
				UpdatedAt = dto.UpdatedAt,
				CurrentRank = dto.CurrentRank,
				AthleteId = dto.AthleteId,
				IsAdmin = dto.IsAdmin,
				CreatedAt = dto.DateCreated,
				LastRankChange = dto.LastRankChange,
				LeagueId = dto.LeagueId,
				AbandonDate = dto.AbandonDate,
			};
		}

		public static AthleteDto ToAthleteDto(this Athlete dto)
		{
			return new AthleteDto
			{
				Name = dto.Name,
				Id = dto.Id,
				UpdatedAt = dto.UpdatedAt,
				DateCreated = dto.CreatedAt,
				Email = dto.Email,
				UserId = dto.UserId,
				IsAdmin = dto.IsAdmin,
				DeviceToken = dto.DeviceToken,
				DevicePlatform = dto.DevicePlatform,
				AuthenticationId = dto.AuthenticationId,
				MembershipIds = dto.Memberships.Where(m => m.AbandonDate == null).Select(m => m.Id).ToList(),
			};
		}

		public static string Fmt(this string s, params object[] args)
		{
			return string.Format(s, args);
		}
	}
}