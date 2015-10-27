using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace Sport.Shared
{
	public static partial class Extensions
	{
		public static void EnsureLeaguesThemed(this IList<League> leagues)
		{
			foreach(var l in leagues)
			{
				l.Theme = App.Current.GetTheme(l);
			}
		}

		public static void Sort<T>(this ObservableCollection<T> collection, IComparer<T> comparer)
		{
			List<T> sorted = collection.ToList();
			sorted.Sort(comparer);

			for(int i = 0; i < sorted.Count(); i++)
				collection.Move(collection.IndexOf(sorted[i]), i);
		}

		public static void RemoveModel<T>(this ObservableCollection<T> items, string itemId) where T : BaseModel
		{
			items.Where(m => m.Id == itemId).ToList().ForEach(m => items.Remove(m));
		}

		public static void RemoveModel<T>(this ObservableCollection<T> items, T item) where T : BaseModel
		{
			items.RemoveModel(item.Id);
		}

		public static T Get<T>(this ConcurrentDictionary<string, T> dict, string id) where T : BaseModel
		{
			if(id == null)
				return null;

			T v = null;
			dict.TryGetValue(id, out v);
			return v;
		}

		public static void AddOrUpdate<T>(this ConcurrentDictionary<string, T> dict, T model) where T : BaseModel
		{
			if(model == null)
				return;

			//TODO move to an IRefreshable interface
			var athlete = model as Athlete;
			if(athlete != null)
			{
				athlete.LocalRefresh();
			}

			var league = model as League;
			if(league != null)
			{
				league.RefreshMemberships();
				league.RefreshChallenges();

				//Retain the previous theme when replacing a league
//				if(dict.ContainsKey(league.Id))
//				{
//					var oldLeague = dict[league.Id] as League;
//					league.Theme = oldLeague.Theme;
//				}
			}

			if(dict.ContainsKey(model.Id))
			{
				if(!model.Equals(dict[model.Id]))
					dict[model.Id] = model;
			}
			else
			{
				dict.TryAdd(model.Id, model);
			}
		}

		public static string GetChallengeConflictReason(this Membership membership, Athlete athlete)
		{
			if(!membership.League.HasStarted)
				return "The league hasn't started yet";

			if(membership.Athlete == null || athlete.Id == membership.Athlete.Id)
				return "You cannot challenge yourself";

			//Check to see if they are part of the same league
			var otherMembership = athlete.Memberships.SingleOrDefault(m => m.LeagueId == membership.LeagueId);

			if(otherMembership != null)
			{
				//Ensure they are within range and lower in rank than the challengee
				var diff = otherMembership.CurrentRank - membership.CurrentRank;
				if(diff <= 0 || diff > membership.League.MaxChallengeRange)
				{
					return "{0} is not within a valid range of being challenged".Fmt(membership.Athlete.Alias);
				}
			}
			else
			{
				return "{0} is not a member of the {1} league".Fmt(membership.Athlete.Alias, membership.League.Name);
			}

			var challenge = membership.GetOngoingChallenge(membership.Athlete);
			if(challenge != null)
			{
				return "{0} already has an ongoing challenge with {1}".Fmt(membership.Athlete.Alias, challenge.Opponent(membership.Athlete.Id).Alias);
			}

			//Athlete is within range but let's make sure there aren't already challenges out there 
			challenge = membership.GetOngoingChallenge(athlete);
			if(challenge != null)
			{
				var player = athlete.Id == App.CurrentAthlete.Id ? "You already have" : athlete.Alias + " already has";
				return "{0} an ongoing challenge with {1}".Fmt(player, challenge.Opponent(athlete.Id).Alias);
			}

			return null;
		}

		public static bool CanChallengeAthlete(this Membership membership, Athlete athlete)
		{
			return membership.GetChallengeConflictReason(athlete) == null;
		}

		public static void ToToast(this string message, ToastNotificationType type = ToastNotificationType.Info, string title = null)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				var toaster = DependencyService.Get<IToastNotifier>();
				toaster.Notify(type, title ?? type.ToString().ToUpper(), message, TimeSpan.FromSeconds(2.5f));
			});
		}
	}
}