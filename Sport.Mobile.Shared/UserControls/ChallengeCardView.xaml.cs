using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public partial class ChallengeCardView : ContentView
	{
		public ChallengeCardView()
		{
			InitializeComponent();
			root.BindingContext = this;

			root.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command((arg) => { ((ChallengeCardView)root.Parent).OnClicked?.Execute(ViewModel); })
			});
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

		public Command OnClicked
		{
			get;
			set;
		}

		public Command OnPostResults
		{
			get;
			set;
		}

		public Command OnNudge
		{
			get;
			set;
		}

		public Command OnAccepted
		{
			get;
			set;
		}

		public Command OnDeclined
		{
			get;
			set;
		}

		void HandlePostResults(object sender, EventArgs e)
		{
			OnPostResults?.Execute(ViewModel);
		}

		void HandleDeclined(object sender, EventArgs e)
		{
			OnDeclined?.Execute(ViewModel);
		}

		void HandleAccepted(object sender, EventArgs e)
		{
			OnAccepted?.Execute(ViewModel);
		}

		void HandleNudgeAthlete(object sender, EventArgs e)
		{
			OnNudge?.Execute(ViewModel);
		}
	}
}