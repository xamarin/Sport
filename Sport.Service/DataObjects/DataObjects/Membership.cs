
namespace Sport
{
	public class Membership : MembershipBase
	{
		public virtual League League
		{
			get;
			set;
		}

		public virtual Athlete Athlete
		{
			get;
			set;
		}
	}
}