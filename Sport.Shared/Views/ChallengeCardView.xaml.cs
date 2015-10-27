using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;

namespace Sport.Shared
{
	public partial class ChallengeCardView : ContentView
	{
		public ChallengeCardView()
		{
			InitializeComponent();
			root.BindingContext = this;

			root.GestureRecognizers.Add(new TapGestureRecognizer((view) =>
			{
				((ChallengeCardView)root.Parent).OnClicked?.Invoke();
			}));
		}

		public static readonly BindableProperty ViewModelProperty =
			BindableProperty.Create("ViewModel", typeof(ChallengeDetailsViewModel), typeof(ChallengeCardView), null);

		public ChallengeDetailsViewModel ViewModel
		{
			get
			{
				return (ChallengeDetailsViewModel)GetValue(ViewModelProperty);
			}
			set
			{
				SetValue(ViewModelProperty, value);
			}
		}

		public Action OnClicked
		{
			get;
			set;
		}

		public Action OnPostResults
		{
			get;
			set;
		}

		public Action OnNudge
		{
			get;
			set;
		}

		public Action OnAccepted
		{
			get;
			set;
		}

		public Action OnDeclined
		{
			get;
			set;
		}

		void HandlePostResults(object sender, EventArgs e)
		{
			OnPostResults?.Invoke();
		}

		void HandleDeclined(object sender, EventArgs e)
		{
			OnDeclined?.Invoke();
		}

		void HandleAccepted(object sender, EventArgs e)
		{
			OnAccepted?.Invoke();
		}

		void HandleNudgeAthlete(object sender, EventArgs e)
		{
			OnNudge?.Invoke();
		}
	}
}