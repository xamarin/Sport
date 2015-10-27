using System.Collections.Concurrent;

namespace Sport.Shared
{
	public class DataManager
	{
		/// <summary>
		/// Data is cached locally in memory due the nature of ever-evolving leaderboards and athlete details
		/// Models are stored in the DataManager's hashtables and typically referenced using the Model.ID (key)
		/// This allows any ViewModel to update a model and all other pages will reflect the updated properties
		/// This could easily be converted to SQLite or Akavache
		/// </summary>
		public DataManager()
		{
			Leagues = new ConcurrentDictionary<string, League>();
			Athletes = new ConcurrentDictionary<string, Athlete>();
			Memberships = new ConcurrentDictionary<string, Membership>();
			Challenges = new ConcurrentDictionary<string, Challenge>();
		}

		static DataManager _instance;

		public static DataManager Instance
		{
			get
			{
				if(_instance == null)
					_instance = new DataManager();

				return _instance;
			}			
		}

		#region Properties

		public ConcurrentDictionary<string, League> Leagues
		{
			get;
			set;
		}

		public ConcurrentDictionary<string, Athlete> Athletes
		{
			get;
			set;
		}

		public ConcurrentDictionary<string, Membership> Memberships
		{
			get;
			set;
		}

		public ConcurrentDictionary<string, Challenge> Challenges
		{
			get;
			set;
		}

		#endregion
	}
}