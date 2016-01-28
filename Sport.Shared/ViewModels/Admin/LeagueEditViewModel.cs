using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace Sport.Shared
{
	public class LeagueEditViewModel : BaseViewModel
	{
		bool _wasMember;

		public LeagueEditViewModel()
		{
			League = new League();
		}

		public LeagueEditViewModel(League league = null)
		{
			League = league ?? new League();
		}

		#region Properties

		public bool CanStartLeague
		{
			get
			{
				return League != null && League.Id != null && !League.HasStarted;
			}
		}

		string errorMessage;
		public const string ErrorMessagePropertyName = "ErrorMessage";

		public string ErrorMessage
		{
			get
			{
				return errorMessage;
			}
			set
			{
				SetPropertyChanged(ref errorMessage, value, ErrorMessagePropertyName);
			}
		}

		League _league;

		public League League
		{
			get
			{
				return _league;
			}
			set
			{
				SetPropertyChanged(ref _league, value);
				ErrorMessage = null;
				SetPropertyChanged("CanStartLeague");
				UpdateMembershipStatus();
			}
		}

		bool isMember;
		public const string IsMemberPropertyName = "IsMember";

		public bool IsMember
		{
			get
			{
				return isMember;
			}
			set
			{
				SetPropertyChanged(ref isMember, value, IsMemberPropertyName);
			}
		}

		#endregion

		public void UpdateMembershipStatus()
		{
			_wasMember = League.Id != null && App.CurrentAthlete.Memberships.Any(m => m.LeagueId == League.Id);
			IsMember = _wasMember;
		}

		async public Task<bool> SaveLeague()
		{
			League.Name = League.Name ?? League.Name.Trim();
			League.Sport = League.Sport ?? League.Sport.Trim();
			League.CreatedByAthleteId = App.CurrentAthlete.Id;

			League.Memberships.Clear();
			League.MembershipIds.Clear();
			League.OngoingChallenges.Clear();
			League.PastChallenges.Clear();

			var task = AzureService.Instance.SaveLeague(League);
			await RunSafe(task);

			if(task.IsFaulted)
				return false;

			if(!_wasMember && IsMember)
			{
				var membership = new Membership {
					AthleteId = App.CurrentAthlete.Id,
					LeagueId = League.Id,
					CurrentRank = 0,
				};

				task = AzureService.Instance.SaveMembership(membership);
				await RunSafe(task);
				return !task.IsFaulted;
			}

			return true;
		}

		async public Task DeleteLeague()
		{
			await RunSafe(AzureService.Instance.DeleteLeague(League.Id));
		}

		async public Task<DateTime?> StartLeague()
		{
			var task = AzureService.Instance.StartLeague(League.Id);
			await RunSafe(task);

			if(!task.IsCompleted)
				return null;

			var date = task.Result;
			League.HasStarted = date != null;
			SetPropertyChanged("CanStartLeague");
			return date;
		}

		public bool IsValid()
		{
			var sb = new StringBuilder();
			if(string.IsNullOrWhiteSpace(League.Name))
			{
				sb.AppendLine("enter a league name");
			}

			if(string.IsNullOrWhiteSpace(League.Sport))
			{
				sb.AppendLine("enter a sport");
			}

			ErrorMessage = sb.Length > 0 ? sb.ToString().Trim() : null;
			return ErrorMessage == null;
		}
	}
}