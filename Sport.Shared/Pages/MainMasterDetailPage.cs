using System;

using Xamarin.Forms;
using System.Collections.Generic;

namespace SportChallengeMatchRank.Shared
{
	public class MenuPage : ContentPage
	{
		public ListView Menu
		{
			get;
			set;
		}

		public MenuPage()
		{
			Icon = "settings.png";
			Title = "menu"; // The Title property must be set.
			BackgroundColor = Color.FromHex("333333");

			Menu = new MenuListView();

			var menuLabel = new ContentView {
				Padding = new Thickness(10, 36, 0, 5),
				Content = new Label {
					TextColor = Color.FromHex("AAAAAA"),
					Text = "MENU", 
				}
			};

			var layout = new StackLayout { 
				Spacing = 0, 
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			layout.Children.Add(menuLabel);
			layout.Children.Add(Menu);

			Content = layout;
		}
	}

	public class MenuListView : ListView
	{
		public MenuListView()
		{
			List<MenuItem> data = new MenuListData();

			ItemsSource = data;
			VerticalOptions = LayoutOptions.FillAndExpand;
			BackgroundColor = Color.Transparent;

			var cell = new DataTemplate(typeof(ImageCell));
			cell.SetBinding(TextCell.TextProperty, "Title");
			cell.SetBinding(ImageCell.ImageSourceProperty, "IconSource");

			ItemTemplate = cell;
		}
	}

	public class MenuListData : List<MenuItem>
	{
		public MenuListData()
		{
			this.Add(new MenuItem() { 
					Title = "Contracts", 
					IconSource = "contracts.png", 
					TargetType = typeof(ContractsPage)
				});

			this.Add(new MenuItem() { 
					Title = "Leads", 
					IconSource = "Lead.png", 
					TargetType = typeof(LeadsPage)
				});

			this.Add(new MenuItem() { 
					Title = "Accounts", 
					IconSource = "Accounts.png", 
					TargetType = typeof(AccountsPage)
				});

			this.Add(new MenuItem() {
					Title = "Opportunities",
					IconSource = "Opportunity.png",
					TargetType = typeof(OpportunitiesPage)
				});
		}
	}

	public class MenuItem
	{
		public string Title
		{
			get;
			set;
		}

		public string IconSource
		{
			get;
			set;
		}

		public Type TargetType
		{
			get;
			set;
		}
	}
}