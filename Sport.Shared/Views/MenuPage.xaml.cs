using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Windows.Input;

namespace SportChallengeMatchRank.Shared
{
	public partial class MenuPage : ContentPage
	{
		public ListView ListView
		{
			get;
			private set;
		}

		Dictionary<string, ICommand> _options;

		public Dictionary<string, ICommand> Options
		{
			get
			{
				return _options;
			}
			set
			{
				_options = value;
				ListView.ItemsSource = _options;
			}
		}

		public MenuPage()
		{
			InitializeComponent();
			ListView = listView;
		}
	}
}

