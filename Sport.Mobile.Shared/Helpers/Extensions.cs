using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public static partial class Extensions
	{
		public static void Track(this Exception ex)
		{
			try
			{
				//Log exception to Mobile Center
				var baseException = ex.GetBaseException();
				var stack = baseException.StackTrace;

				if(stack.Length > 101)
					stack = stack.Substring(0, 100);

				Microsoft.Azure.Mobile.Analytics.Analytics.TrackEvent("Exception Occurred", new Dictionary<string, string>
				{
					{ "Message", baseException.Message },
					{ "StackTrace", stack}
				});
			}
			catch
			{
			}
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach(T item in source)
				action(item);
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
			var list = items.Where(m => m.Id == itemId).ToList();
			foreach(var item in list)
				list.Remove(item);
		}

		public static void RemoveModel<T>(this ObservableCollection<T> items, T item) where T : BaseModel
		{
			items.RemoveModel(item.Id);
		}

		public static T Get<T>(this Dictionary<string, T> dict, string id) where T : BaseModel
		{
			if(id == null)
				return null;

			T v = null;
			dict.TryGetValue(id, out v);
			return v;
		}

		static object _sync = new object();
		public static void AddOrUpdate<T>(this Dictionary<string, T> dict, T model) where T : BaseModel
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
				league.LocalRefresh();

				//Retain the previous theme when replacing a league
				//				if(dict.ContainsKey(league.Id))
				//				{
				//					var oldLeague = dict[league.Id] as League;
				//					league.Theme = oldLeague.Theme;
				//				}
			}

			lock(_sync)
			{
				if(dict.ContainsKey(model.Id))
				{
					if(!model.Equals(dict[model.Id]))
						dict[model.Id] = model;
				}
				else
				{
					dict.Add(model.Id, model);
				}
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

			if(otherMembership == null)
				return "{0} is not a member of the {1} league".Fmt(membership.Athlete.Alias, membership.League.Name);

			var challenge = membership.GetOngoingChallenge(athlete);
			if(challenge != null)
			{
				return "{0} already has an ongoing challenge with {1}".Fmt(membership.Athlete.Alias, athlete.Alias);
			}

			return null;
		}

		public static bool CanChallengeAthlete(this Membership membership, Athlete athlete)
		{
			return membership.GetChallengeConflictReason(athlete) == null;
		}

		public static void ToToast(this string message, ToastNotificationType type = ToastNotificationType.Info, string title = null)
		{
			Device.BeginInvokeOnMainThread(() => {
				var toaster = DependencyService.Get<IToastNotifier>();
				toaster?.Notify(type, title ?? type.ToString().ToUpper(), message, TimeSpan.FromSeconds(2.5f));
			});
		}
	}
}