using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using Xamarin;

namespace Sport.Mobile.Shared
{
	public class ChallengeHistoryViewModel : BaseViewModel
	{
		Membership _membership;

		public Membership Membership
		{
			get
			{
				return _membership;
			}
			set
			{
				if(value != _membership)
				{
					if(Challenges != null)
						Challenges.Clear();
				}

				SetPropertyChanged(ref _membership, value);
			}
		}

		public ObservableCollection<ChallengeViewModel> Challenges
		{
			get;
			set;
		} = new ObservableCollection<ChallengeViewModel>();

		public ICommand GetChallengeHistoryCommand
		{
			get
			{
				return new Command(async() => await GetChallengeHistory(true));
			}
		}

		async public Task GetChallengeHistory(bool forceRefresh = false)
		{
			using(new Busy(this))
			{
				ChallengeViewModel empty = null;
				if(Challenges.Count == 0)
				{
					empty = new ChallengeViewModel() {
						EmptyMessage = "Loading previous challenges"
					};

					Challenges.Add(empty);
				}

				if(forceRefresh)
				{
					await AzureService.Instance.GameResultManager.PullLatestAsync().ConfigureAwait(false);
					await AzureService.Instance.ChallengeManager.PullLatestAsync().ConfigureAwait(false);
				}

				var list = await AzureService.Instance.ChallengeManager.Table.Where(c => c.DateCompleted != null
					  && (c.ChallengerAthleteId == Membership.AthleteId || c.ChallengeeAthleteId == Membership.AthleteId)
					  && c.LeagueId == Membership.LeagueId)
                      .OrderByDescending(c => c.DateCompleted).ToListAsync();

				Challenges.Clear();
				list.ForEach(c => Challenges.Add(new ChallengeViewModel { Challenge = c }));

				if(Challenges.Count == 0)
				{
					Challenges.Add(new ChallengeViewModel { EmptyMessage = "There are no previous challenges." });
				}
			}
		}
	}
}