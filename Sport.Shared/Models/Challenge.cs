using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;
using System.Text;

namespace Sport.Shared
{
	public class Challenge : BaseModel
	{
		public Challenge()
		{
			MatchResult = new List<GameResult>();
		}

		[JsonIgnore]
		public Athlete ChallengeeAthlete
		{
			get
			{
				return ChallengeeAthleteId == null ? null : DataManager.Instance.Athletes.Get(ChallengeeAthleteId);
			}
		}

		[JsonIgnore]
		public Athlete ChallengerAthlete
		{
			get
			{
				return ChallengerAthleteId == null ? null : DataManager.Instance.Athletes.Get(ChallengerAthleteId);
			}
		}

		[JsonIgnore]
		public League League
		{
			get
			{
				return LeagueId == null ? null : DataManager.Instance.Leagues.Get(LeagueId);
			}
		}

		[JsonIgnore]
		public Athlete WinningAthlete
		{
			get
			{
				int a = this.GetChallengerWinningGames().Length;
				int b = this.GetChallengeeWinningGames().Length;

				if(a > b)
					return ChallengerAthlete;

				return b > a ? ChallengeeAthlete : null;
			}
		}

		[JsonIgnore]
		public DateTimeOffset ProposedTimeLocal
		{
			get
			{
				return ProposedTime.ToLocalTime();
			}
		}

		string _leagueId;

		public string LeagueId
		{
			get
			{
				return _leagueId;
			}
			set
			{
				SetPropertyChanged(ref _leagueId, value);
				SetPropertyChanged("League");
				SetPropertyChanged("Summary");
			}
		}

		string _challengerAthleteId;

		public string ChallengerAthleteId
		{
			get
			{
				return _challengerAthleteId;
			}
			set
			{
				SetPropertyChanged(ref _challengerAthleteId, value);
				SetPropertyChanged("ChallengerAthlete");
				SetPropertyChanged("Summary");
			}
		}

		string _challengeeAthleteId;

		public string ChallengeeAthleteId
		{
			get
			{
				return _challengeeAthleteId;
			}
			set
			{
				SetPropertyChanged(ref _challengeeAthleteId, value);
				SetPropertyChanged("ChallengeeAthlete");
				SetPropertyChanged("Summary");
			}
		}

		int _height = 30;

		public int Height
		{
			get
			{
				return _height;
			}
			set
			{
				SetPropertyChanged(ref _height, value);
			}
		}

		DateTimeOffset? _dateCompleted;

		public DateTimeOffset? DateCompleted
		{
			get
			{
				return _dateCompleted;
			}
			set
			{
				SetPropertyChanged(ref _dateCompleted, value);
				SetPropertyChanged("IsCompleted");
			}
		}

		DateTimeOffset? _dateAccepted;

		public DateTimeOffset? DateAccepted
		{
			get
			{
				return _dateAccepted;
			}
			set
			{
				SetPropertyChanged(ref _dateAccepted, value);
				SetPropertyChanged("IsAccepted");
			}
		}

		DateTimeOffset _proposedTime;

		public DateTimeOffset ProposedTime
		{
			get
			{
				return _proposedTime;
			}
			set
			{
				SetPropertyChanged(ref _proposedTime, value);
				SetPropertyChanged("Summary");
			}
		}

		int? _battleForRank;

		public int? BattleForRank
		{
			get
			{
				return _battleForRank;
			}
			set
			{
				SetPropertyChanged(ref _battleForRank, value);
			}
		}

		public int? BattleForRankDisplay
		{
			get
			{
				if(BattleForRank == null)
					return null;
						
				return BattleForRank.Value + 1;
			}
		}


		[JsonIgnore]
		public bool IsAccepted
		{
			get
			{
				return DateAccepted.HasValue;
			}
		}

		[JsonIgnore]
		public bool IsCompleted
		{
			get
			{
				return DateCompleted.HasValue;
			}
		}

		string _customMessage;

		public string CustomMessage
		{
			get
			{
				return _customMessage;
			}
			set
			{
				SetPropertyChanged(ref _customMessage, value);
			}
		}

		[JsonIgnore]
		public List<GameResult> ChallengeeWinningGames
		{
			get
			{
				return this.GetChallengeeWinningGames().ToList();
			}
		}

		[JsonIgnore]
		public List<GameResult> ChallengerWinningGames
		{
			get
			{
				return this.GetChallengerWinningGames().ToList();
			}
		}

		[JsonIgnore]
		public string ProposedTimeString
		{
			get
			{
				var date = ProposedTimeLocal.LocalDateTime;
				return "{0} at {1}".Fmt(date.ToString("dddd, MMMMM dd"), date.ToString("t"), League?.Name);
			}
		}

		[JsonIgnore]
		public string BattleForPlaceBetween
		{
			get
			{
				return "{0} {1}".Fmt(BattleForPlace, BattleBetween).Trim();
			}
		}

		[JsonIgnore]
		public string BattleForPlace
		{
			get
			{
				if(BattleForRank == null)
					return null;
				
				return "an epic battle for {0} place".Fmt(BattleForRankDisplay.Value.ToOrdinal());
			}
		}

		[JsonIgnore]
		public string BattleBetween
		{
			get
			{
				if(League == null)
					return null;

				if(ChallengerAthlete == null || ChallengeeAthlete == null)
					return null;

				return "between {0} and {1}".Fmt(ChallengerAthlete.Alias, ChallengeeAthlete.Alias);
			}
		}

		[JsonIgnore]
		public string Summary
		{
			get
			{
				if(League == null || ChallengerAthlete == null || ChallengeeAthlete == null)
					return null;

				return "{0} vs {1}".Fmt(ChallengerAthlete.Alias, ChallengeeAthlete.Alias);
			}
		}

		List<GameResult> _matchResult;

		public List<GameResult> MatchResult
		{
			get
			{
				return _matchResult;
			}
			set
			{
				SetPropertyChanged(ref _matchResult, value);
			}
		}

		[JsonIgnore]
		public bool IsChallengerWinningAthlete
		{
			get
			{
				return WinningAthlete != null && WinningAthlete.Id == ChallengerAthleteId;
			}
		}

		[JsonIgnore]
		public bool IsChallengeeWinningAthlete
		{
			get
			{
				return WinningAthlete != null && WinningAthlete.Id == ChallengeeAthleteId;
			}
		}


		public string MatchResultSummary
		{
			get
			{
				if(MatchResult == null)
					return null;

				var sb = new StringBuilder();
				MatchResult.ForEach(g => sb.AppendFormat("{0} - {1}  :  ", g.ChallengerScore, g.ChallengeeScore));
				return sb.ToString().TrimEnd(" ,:".ToCharArray());
			}
		}

		public void NotifyPropertiesChanged()
		{
			SetPropertyChanged("League");
			SetPropertyChanged("Summary");
			SetPropertyChanged("ChallengeeAthlete");
			SetPropertyChanged("ChallengerAthlete");
			SetPropertyChanged("IsCompleted");
			SetPropertyChanged("ProposedTimeString");
			SetPropertyChanged("ProposedTimeLocal");
			SetPropertyChanged("IsAccepted");
			SetPropertyChanged("BattleForPlace");
			SetPropertyChanged("BattleForRankDisplay");
			SetPropertyChanged("BattleBetween");
			SetPropertyChanged("BattleForPlaceBetween");
			SetPropertyChanged("MatchResult");
			SetPropertyChanged("MatchResultSummary");
			SetPropertyChanged("ChallengerWinningGames");
			SetPropertyChanged("ChallengeeWinningGames");
			SetPropertyChanged("IsChallengeeWinningAthlete");
			SetPropertyChanged("IsChallengerWinningAthlete");
		}

		public override bool Equals(object obj)
		{
			var comp = new ChallengeComparer();
			return comp.Equals(this, obj as Challenge);
		}
	}

	#region Comparers

	public class ChallengeComparer : IEqualityComparer<Challenge>
	{
		public bool Equals(Challenge x, Challenge y)
		{
			if(x == null || y == null)
				return false;

			if(x.Id == y.Id
			   && x.UpdatedAt == y.UpdatedAt
			   && x.DateAccepted == y.DateAccepted
			   && x.DateCompleted == y.DateCompleted
			   && x.BattleForRank == y.BattleForRank
			   && x.ProposedTime == y.ProposedTime
			   && x.WinningAthlete == y.WinningAthlete
			   && x.MatchResult.Count == y.MatchResult.Count
			   && x.ChallengeeAthleteId == y.ChallengeeAthleteId
			   && x.ChallengerAthleteId == y.ChallengerAthleteId)
			{
				for(int i = 0; i < x.MatchResult.Count; i++)
				{
					var xGame = x.MatchResult[i];
					var yGame = y.MatchResult[i];

					if(xGame.ChallengeeScore != yGame.ChallengeeScore ||
					   xGame.ChallengerScore != yGame.ChallengerScore)
						return false;
				}

				return true;
			}

			return false;
		}

		public int GetHashCode(Challenge obj)
		{
			return obj.Id != null ? obj.Id.GetHashCode() : base.GetHashCode();
		}
	}

	public class ChallengeSortComparer : IComparer<ChallengeViewModel>
	{
		public int Compare(ChallengeViewModel x, ChallengeViewModel y)
		{
			if(!x.Challenge.DateCreated.HasValue || !y.Challenge.DateCompleted.HasValue)
				return 0;

			if(x.Challenge.DateCompleted.Value > y.Challenge.DateCompleted.Value)
				return -1;

			if(x.Challenge.DateCompleted.Value < y.Challenge.DateCompleted.Value)
				return 1;

			return 0;
		}
	}

	#endregion
}