using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public class CircleView : ContentView
	{
		#region Properties

		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(CircleView), null);

		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
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

		public static readonly BindableProperty TextColorProperty =
		BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CircleView), Color.White);

		public Color TextColor
		{
			get
			{
				return (Color)GetValue(TextColorProperty);
			}
			set
			{
				SetValue(TextColorProperty, value);
			}
		}

		public static readonly BindableProperty TextSizeProperty =
		BindableProperty.Create(nameof(TextSize), typeof(double), typeof(CircleView), 20.0);

		public double TextSize
		{
			get
			{
				return (double)GetValue(TextSizeProperty);
			}
			set
			{
				SetValue(TextSizeProperty, value);
			}
		}

		#endregion

		SKCanvasView _canvasView;
		Label _textLabel;
		Grid _root;

		public CircleView()
		{
			_canvasView = new SKCanvasView();
			_canvasView.PaintSurface += PaintContents;

			_root = new Grid();
			_root.Children.Add(_canvasView);

			Content = _root;
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
				canvas.DrawCircle(b.MidX, b.MidX, b.MidX - 2, paint);
			}

			if(!string.IsNullOrWhiteSpace(Text))
			{
				if(_textLabel == null)
				{
					//Using a label here instead of drawing text because Skia text size is non-DPI pixels
					_textLabel = new Label
					{
						FontSize = TextSize,
						TextColor = TextColor,
						Margin = new Thickness(4),
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
					};

					_root.Children.Add(_textLabel);
				}

				_textLabel.Text = Text;

				//using(var paint = new SKPaint
				//{
				//	IsAntialias = true,
				//	Color = TextColor.ToSKColor(),
				//	TextSize = (float)TextSize,
				//})
				//{
				//	var tw = paint.MeasureText(Text);
				//	var w = b.MidX - (tw / 2);
				//	var h = b.MidY + (paint.TextSize / 2) - 6;
				//	canvas.DrawText(Text, w, h, paint);
				//}
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

