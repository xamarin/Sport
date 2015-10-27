using System;
using System.ComponentModel;
using Sport.iOS;
using Sport.Shared;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SportEntry), typeof(SportEntryRenderer))]
namespace Sport.iOS
{
	public class SportEntryRenderer : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			var view = (SportEntry)Element;

			if(view != null)
			{
				SetBorder(view);
				SetFont(view);
				SetFontFamily(view);
				SetMaxLength(view);
				SetTextAlignment(view);
			}
		}

		void SetFont(SportEntry view)
		{
			UIFont uiFont;
			if(view.Font != Font.Default && (uiFont = view.Font.ToUIFont()) != null)
				Control.Font = uiFont;
			else if(view.Font == Font.Default)
				Control.Font = UIFont.SystemFontOfSize(17f);
		}

		void SetFontFamily(SportEntry view)
		{
			UIFont uiFont;

			if(!string.IsNullOrWhiteSpace(view.FontFamily) && (uiFont = view.Font.ToUIFont()) != null)
			{
				var ui = UIFont.FromName(view.FontFamily, (nfloat)(view.Font != null ? view.Font.FontSize : 17f));
				Control.Font = uiFont;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var view = (SportEntry)Element;

			if(e.PropertyName == SportEntry.FontProperty.PropertyName)
				SetFont(view);

			if(e.PropertyName == SportEntry.FontFamilyProperty.PropertyName)
				SetFontFamily(view);
			
			if(e.PropertyName == SportEntry.HasBorderProperty.PropertyName)
				SetBorder(view);

			if(e.PropertyName == SportEntry.MaxLengthProperty.PropertyName)
				SetMaxLength(view);

			if(e.PropertyName == SportEntry.XAlignProperty.PropertyName)
				SetTextAlignment(view);
		}

		void SetMaxLength(SportEntry view)
		{
			Control.ShouldChangeCharacters = (textField, range, replacementString) =>
			{
				var newLength = textField.Text.Length + replacementString.Length - range.Length;
				return newLength <= view.MaxLength;
			};
		}

		void SetBorder(SportEntry view)
		{
			Control.BorderStyle = view.HasBorder ? UITextBorderStyle.RoundedRect : UITextBorderStyle.None;
		}

		void SetTextAlignment(SportEntry view)
		{
			switch(view.XAlign)
			{
				case TextAlignment.Center:
					Control.TextAlignment = UITextAlignment.Center;
					break;
				case TextAlignment.End:
					Control.TextAlignment = UITextAlignment.Right;
					break;
				case TextAlignment.Start:
					Control.TextAlignment = UITextAlignment.Left;
					break;
			}
		}
	}
}