using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Sport.Shared
{
	public partial class LeagueEditPage : LeagueEditXaml
	{
		public Action OnUpdate
		{
			get;
			set;
		}

		public LeagueEditPage(League league = null)
		{
			ViewModel.League = league ?? new League();
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Edit League";

			btnStartLeague.Clicked += async(sender, e) =>
			{
				try
				{
					DateTime? startTime;
					using(new HUD("Starting league..."))
					{
						startTime = await ViewModel.StartLeague();
					}

					if(startTime != null)
					{
						btnStartLeague.IsVisible = false;
						"It's on like a prawn that yawns at dawn!".ToToast(ToastNotificationType.Success, "League Started!");
						await Navigation.PopModalAsync();
					}
				}
				catch(Exception ex)
				{
					ex.Message.ToToast(ToastNotificationType.Error, "Unable to start league ");
				}
			};
			
			btnSaveLeague.Clicked += async(sender, e) =>
			{
				await SaveLeague();
			};

			var btnCancel = new ToolbarItem {
				Text = "Cancel"		
			};

			btnCancel.Clicked += async(sender, e) =>
			{
				await Navigation.PopModalAsync();		
			};

			ToolbarItems.Add(btnCancel);

			btnDeleteLeague.Clicked += async(sender, e) =>
			{
				var accepted = await DisplayAlert("Delete League?", "Are you totes sure you want to delete this league?", "Yes", "No");

				if(accepted)
				{
					using(new HUD("Deleting league..."))
					{
						await ViewModel.DeleteLeague();
					}

					if(OnUpdate != null)
						OnUpdate();
					
					await Navigation.PopModalAsync();
					"The {0} league has been deleted.".Fmt(ViewModel.League.Name).ToToast(ToastNotificationType.Success);
				}
			};
		}

		protected override void OnAppearing()
		{
			if(ViewModel.League.Id == null)
				name.Focus();

			ViewModel.UpdateMembershipStatus();
			base.OnAppearing();
		}

		async Task SaveLeague()
		{
			if(!ViewModel.IsValid())
			{
				ViewModel.ErrorMessage.ToToast(ToastNotificationType.Warning, "Please fix these errors");
				return;
			}

			bool success;
			using(new HUD("Saving..."))
			{
				success = await ViewModel.SaveLeague();
			}

			if(!success)
				return;

			DataManager.Instance.Leagues.AddOrUpdate(ViewModel.League);

			if(OnUpdate != null)
				OnUpdate();

			await Navigation.PopModalAsync();
			"The {0} league has been saved.".Fmt(ViewModel.League.Name).ToToast(ToastNotificationType.Success);
		}

		async public void OnSelectPhotoButtonClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.League.Sport))
			{
				"Please enter a sport first.".ToToast(ToastNotificationType.Warning);
				return;				
			}

			var photoPage = new PhotoSelectionPage(ViewModel.League);
			photoPage.OnImageSelected = async() =>
			{
				ViewModel.SetPropertyChanged("League");		
				await Navigation.PopAsync();
				photoPage.OnImageSelected = null;
			};

			await Navigation.PushAsync(photoPage);
		}
	}

	public partial class LeagueEditXaml : BaseContentPage<LeagueEditViewModel>
	{
	}
}