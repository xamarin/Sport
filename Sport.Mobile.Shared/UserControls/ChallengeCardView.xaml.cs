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

		public static readonly BindableProperty OnAcceptedProperty =
			BindableProperty.Create("OnAccepted", typeof(Command), typeof(ChallengeCardView), null);

		public Command OnAccepted
		{
			get
			{
				return (Command)GetValue(OnAcceptedProperty);
			}
			set
			{
				SetValue(OnAcceptedProperty, value);
			}
		}

		public static readonly BindableProperty OnDeclinedProperty =
			BindableProperty.Create("OnDeclined", typeof(Command), typeof(ChallengeCardView), null);

		public Command OnDeclined
		{
			get
			{
				return (Command)GetValue(OnDeclinedProperty);
			}
			set
			{
				SetValue(OnDeclinedProperty, value);
			}
		}

		public static readonly BindableProperty OnNudgeProperty =
			BindableProperty.Create("OnNudge", typeof(Command), typeof(ChallengeCardView), null);

		public Command OnNudge
		{
			get
			{
				return (Command)GetValue(OnNudgeProperty);
			}
			set
			{
				SetValue(OnNudgeProperty, value);
			}
		}

		public static readonly BindableProperty OnPostResultsProperty =
			BindableProperty.Create("OnPostResults", typeof(Command), typeof(ChallengeCardView), null);

		public Command OnPostResults
		{
			get
			{
				return (Command)GetValue(OnPostResultsProperty);
			}
			set
			{
				SetValue(OnPostResultsProperty, value);
			}
		}

		public static readonly BindableProperty OnClickedProperty =
			BindableProperty.Create("OnClicked", typeof(Command), typeof(ChallengeCardView), null);

		public Command OnClicked
		{
			get
			{
				return (Command)GetValue(OnClickedProperty);
			}
			set
			{
				SetValue(OnClickedProperty, value);
			}
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