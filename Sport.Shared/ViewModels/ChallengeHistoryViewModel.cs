using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using Xamarin;

namespace Sport.Shared
{
	public class ChallengeHistoryViewModel : BaseViewModel
	{
		bool _hasLoadedBefore;

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
					_hasLoadedBefore = false;

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
		}

		public ChallengeHistoryViewModel()
		{
			Challenges = new ObservableCollection<ChallengeViewModel>();
		}

		public ICommand GetChallengeHistoryCommand
		{
			get
			{
				return new Command(async() => await GetChallengeHistory(true));
			}
		}

		async public Task GetChallengeHistory(bool forceRefresh = false)
		{
			if(!forceRefresh && _hasLoadedBefore)
				return;

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

				var task = AzureService.Instance.GetChallengesForMembership(Membership);
				await RunSafe(task);

				if(task.IsFaulted)
					return;

				if(empty != null && Challenges.Contains(empty))
					Challenges.Remove(empty);
				
				LocalRefresh(task.Result);
			}
		}

		public void LocalRefresh(List<Challenge> challenges)
		{
			try
			{
				var current = Challenges.Select(vm => vm.Challenge).ToList();

				var comparer = new ChallengeComparer();
				var toRemove = current.Except(challenges, comparer).ToList();
				var toAdd = challenges.Except(current, comparer).ToList();
				toRemove.ForEach(c => Challenges.Remove(Challenges.Single(vm => vm.Challenge == c)));

				var preSort = new List<ChallengeViewModel>();
				toAdd.ForEach(c => preSort.Add(new ChallengeViewModel {
					Challenge = c
				}));

				preSort.Sort(new ChallengeSortComparer());
				preSort.ForEach(Challenges.Add);

				if(Challenges.Count == 0)
				{
					Challenges.Add(new ChallengeViewModel() {
						EmptyMessage = "{0} no challenges for this league".Fmt(Membership.AthleteId == App.CurrentAthlete.Id
							? "You have" : "{0} has".Fmt(Membership.Athlete.Alias))
					});
				}
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
			}
		}
	}
}