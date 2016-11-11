using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Sport.Mobile.Shared
{
	public class Challenge : BaseModel
	{
		League _league;
		[JsonIgnore]
		public League League
		{
			get
			{
				if(_league == null)
				{
					Task.Run(async () => {
						_league = await AzureService.Instance.LeagueManager.Table.LookupAsync(LeagueId);
					}).Wait();
				}

				return _league;
			}
		}

		Athlete _challengeeAthlete;
		[JsonIgnore]
		public Athlete ChallengeeAthlete
		{
			get
			{
				if(_challengeeAthlete == null)
				{
					Task.Run(async () => {
						_challengeeAthlete = await AzureService.Instance.AthleteManager.Table.LookupAsync(ChallengeeAthleteId);
					}).Wait();
				}

				return _challengeeAthlete;
			}
		}

		Athlete _challengerAthlete;
		[JsonIgnore]
		public Athlete ChallengerAthlete
		{
			get
			{
				if(_challengerAthlete == null)
				{
					Task.Run(async () => {
						_challengerAthlete = await AzureService.Instance.AthleteManager.Table.LookupAsync(ChallengerAthleteId);
					}).Wait();
				}

				return _challengerAthlete;
			}
		}

		List<GameResult> _matchResult;

		public List<GameResult> MatchResult
		{
			get
			{
				if(_matchResult == null)
				{
					Task.Run(async () => {
						_matchResult = await AzureService.Instance.GameResultManager.Table.Where(r => r.ChallengeId == Id).ToListAsync();
					}).Wait();
				}

				return _matchResult;
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

		[JsonIgnore]
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

		[JsonIgnore]
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
				return $"{date.ToString("t")} {date.ToString("t")}";
			}
		}

		[JsonIgnore]
		public string BattleForPlaceBetween
		{
			get
			{
				return $"{BattleForPlace} {BattleBetween}";
			}
		}

		[JsonIgnore]
		public string BattleForPlace
		{
			get
			{
				if(BattleForRank == null)
					return null;

				return $"an epic battle for {BattleForRankDisplay.Value.ToOrdinal()} place";
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

				return $"between {ChallengerAthlete.Alias} and {ChallengeeAthlete.Alias}";
			}
		}

		[JsonIgnore]
		public string Summary
		{
			get
			{
				if(League == null || ChallengerAthlete == null || ChallengeeAthlete == null)
					return null;

				return $"{ChallengerAthlete.Alias} vs {ChallengeeAthlete.Alias}";
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

		[JsonIgnore]
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

		public override void LocalRefresh()
		{
			base.LocalRefresh();
			_challengerAthlete = null;
			_challengeeAthlete = null;
			_matchResult = null;
			_league = null;
		}

		public override void NotifyPropertiesChanged([CallerMemberName] string caller = "")
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

			base.NotifyPropertiesChanged(caller);
		}

		public override bool Equals(object obj)
		{
			var comp = new ChallengeComparer();
			return comp.Equals(this, obj as Challenge);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
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
			if(!x.Challenge.CreatedAt.HasValue || !y.Challenge.DateCompleted.HasValue)
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