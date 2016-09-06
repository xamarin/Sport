using System.Linq;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sport.Shared
{
	public static partial class Extensions
	{
		public static Dictionary<string, string> ToKeyValuePair(this string querystring)
		{
			return Regex.Matches(querystring, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);
		}

		public static Athlete Opponent(this Challenge c, string athleteId)
		{
			return c.ChallengeeAthleteId == athleteId ? c.ChallengerAthlete : c.ChallengerAthleteId == athleteId ? c.ChallengeeAthlete : null;
		}

		public static bool InvolvesAthlete(this Challenge c, string athleteId)
		{
			return c.ChallengerAthleteId == athleteId || c.ChallengeeAthleteId == athleteId;
		}

		public static Challenge InvolvingAthlete(this List<Challenge> list, string athleteId)
		{
			return list.FirstOrDefault(c => c.InvolvesAthlete(athleteId));
		}

		public static bool InvolvesAthletes(this Challenge c, string athleteA, string athleteB)
		{
			return (c.ChallengerAthleteId == athleteA && c.ChallengeeAthleteId == athleteB)
			|| (c.ChallengerAthleteId == athleteB && c.ChallengeeAthleteId == athleteA);
		}

		public static string ToOrdinal(this DateTime date)
		{
			return "{0}, {1} {2}".Fmt(date.ToString("dddd"), date.ToString("MMM"), date.Day.ToOrdinal());
		}

		public static bool ContainsNoCase(this string s, string contains)
		{
			if(s == null || contains == null)
				return false;

			return s.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static string TrimStart(this string s, string toTrim)
		{
			if(s.StartsWith(toTrim, true, Thread.CurrentThread.CurrentCulture))
				return s.Substring(toTrim.Length);

			return s;
		}

		public static string TrimEnd(this string s, string toTrim)
		{
			if(s.EndsWith(toTrim, true, Thread.CurrentThread.CurrentCulture))
				return s.Substring(0, s.Length - toTrim.Length);

			return s;
		}

		public static string Fmt(this string s, params object[] args)
		{
			return string.Format(s, args);
		}

		public static GameResult[] GetChallengerWinningGames(this Challenge challenge)
		{
			return challenge.MatchResult.Where(gr => gr.ChallengerScore > gr.ChallengeeScore).ToArray();
		}

		public static GameResult[] GetChallengeeWinningGames(this Challenge challenge)
		{
			return challenge.MatchResult.Where(gr => gr.ChallengeeScore > gr.ChallengerScore).ToArray();
		}

		public static string ValidateMatchResults(this Challenge challenge)
		{
			var challengeeWins = 0;
			var challengerWins = 0;
			foreach(var g in challenge.MatchResult)
			{
				if(!g.ChallengeeScore.HasValue && !g.ChallengerScore.HasValue)
					continue;

				if(g.ChallengeeScore.Value == 0 && g.ChallengerScore.Value == 0)
					continue;

				if((g.ChallengeeScore.HasValue && !g.ChallengerScore.HasValue) || (!g.ChallengeeScore.HasValue && g.ChallengerScore.HasValue))
					return "Please ensure both players have valid scores.";

				if(g.ChallengeeScore > g.ChallengerScore)
				{
					challengeeWins++;
				}
				else if(g.ChallengerScore > g.ChallengeeScore)
				{
					challengerWins++;
				}
				else
				{
					return "Please ensure there are no tie scores.";
				}
			}

			var minWins = Math.Ceiling(challenge.League.MatchGameCount / 2f);

			if(challengeeWins == challengerWins || (challengeeWins < minWins && challengerWins < minWins))
				return "Please ensure there is a clear victor.";

			return null;
		}

		public static bool IsEmpty(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
		}

		public static string ToOrdinal(this int num)
		{
			if(num <= 0)
				return num.ToString();

			switch(num % 100)
			{
				case 11:
				case 12:
				case 13:
					return num + "th";
			}

			switch(num % 10)
			{
				case 1:
					return num + "st";
				case 2:
					return num + "nd";
				case 3:
					return num + "rd";
				default:
					return num + "th";
			}

		}
	}

	public class DeviceRegistration
	{
		public string Platform
		{
			get;
			set;
		}

		public string Handle
		{
			get;
			set;
		}

		public string[] Tags
		{
			get;
			set;
		}
	}

	public class NotificationPayload
	{
		public NotificationPayload()
		{
			Payload = new Dictionary<string, string>();
		}

		public string Action
		{
			get;
			set;
		}

		public Dictionary<string, string> Payload
		{
			get;
			set;
		}
	}

	public struct PushActions
	{
		public static string ChallengePosted = "ChallengePosted";
		public static string ChallengeRevoked = "ChallengeRevoked";
		public static string ChallengeAccepted = "ChallengeAccepted";
		public static string ChallengeDeclined = "ChallengeDeclined";
		public static string ChallengeCompleted = "ChallengeCompleted";
		public static string LeagueStarted = "LeagueStarted";
		public static string LeagueEnded = "LeagueEnded";
		public static string LeagueEnrollmentStarted = "LeagueEnrollmentStarted";
	}
}