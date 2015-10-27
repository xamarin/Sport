using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Sport.Shared
{
	public partial class GameResultFormView : ContentView
	{
		public Challenge Challenge
		{
			get;
			set;
		}

		public GameResult GameResult
		{
			get;
			set;
		}

		public int Index
		{
			get;
			set;
		}

		public string GameIndex
		{
			get
			{
				return "Game {0}".Fmt(Index + 1);
			}
		}

		public GameResultFormView(Challenge challenge, GameResult gameResult, int index)
		{
			Index = index;
			Challenge = challenge;
			GameResult = gameResult;
			BindingContext = this;

			InitializeComponent();
		}

		void TextChanged(object sender, TextChangedEventArgs e)
		{
			if(string.IsNullOrEmpty(e.NewTextValue))
				return;

			int o;
			if(!int.TryParse(e.NewTextValue, out o))
			{
				var entry = sender as Entry;
				entry.Text = e.OldTextValue;
			}
		}
	}
}