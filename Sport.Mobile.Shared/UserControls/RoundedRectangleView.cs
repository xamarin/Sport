using System;
using System.Collections.ObjectModel;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public class RoundedRectangleView : ContentView
	{
		public event EventHandler Clicked;
			
		#region Properties

		public ContentView View
		{
			get;
			set;
		}

		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(CircleView), 16.0);

		public double CornerRadius
		{
			get
			{
				return (double)GetValue(CornerRadiusProperty);
			}
			set
			{
				SetValue(CornerRadiusProperty, value);
			}
		}

		public static readonly BindableProperty FillColorProperty =
			BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(CircleView), Color.Transparent);

		public Color FillColor
		{
			get
			{
				return (Color)GetValue(FillColorProperty);
			}
			set
			{
				SetValue(FillColorProperty, value);
			}
		}

		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CircleView), null);

		public object CommandParameter
		{
			get
			{
				return GetValue(CommandParameterProperty);
			}
			set
			{
				SetValue(CommandParameterProperty, value);
			}
		}

		View _innerContent;
		
		public View InnerContent
		{
			get
			{
				return _innerContent;
			}
			set
			{
				if(value == null && _innerContent != null && _root.Children.Contains(_innerContent))
					_root.Children.Remove(_innerContent);

				if(_innerContent != value)
				{
					_innerContent = value;

					if(_innerContent != null)
					{
						_innerContent.HorizontalOptions = LayoutOptions.Center;
						_innerContent.VerticalOptions = LayoutOptions.Center;
						_root.Children.Add(_innerContent);
					}
				}
			}
		}

		#endregion

		SKCanvasView _canvasView;
		Grid _root;

		public RoundedRectangleView()
		{
			_canvasView = new SKCanvasView();
			_canvasView.PaintSurface += PaintContents;
			_root = new Grid
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			_root.Children.Add(_canvasView);
			Content = _root;

			GestureRecognizers.Add(new TapGestureRecognizer((obj) => Clicked?.Invoke(this, new EventArgs())));
		}

		void PaintContents(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(SKColors.Transparent);
			var b = canvas.ClipBounds;

			using(var paint = new SKPaint
			{
				IsAntialias = true,
				IsStroke = false,
				Color = FillColor.ToSKColor(),
			})
			{
				canvas.DrawRoundRect(b, (float)CornerRadius, (float)CornerRadius, paint);
			}
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			_canvasView.HeightRequest = height;
			_canvasView.WidthRequest = width;
		}
	}
}

