using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public class RoundedButton : ContentView
	{
		public event EventHandler Clicked;
			
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
		#endregion

		SKCanvasView _canvasView;

		public RoundedButton()
		{
			_canvasView = new SKCanvasView();
			_canvasView.PaintSurface += PaintContents;
			Content = _canvasView;

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
				canvas.DrawRoundRect(b, 14, 14, paint);
			}

			if(!string.IsNullOrWhiteSpace(Text))
			{
				using(var paint = new SKPaint
				{
					IsAntialias = true,
					Color = TextColor.ToSKColor(),
					TextSize = Device.OS == TargetPlatform.Android ? 52 : 36,
				})
				{
					var tw = paint.MeasureText(Text);
					var w = b.MidX - (tw / 2);
					var h = b.MidY + (paint.TextSize / 2) - 6;
					canvas.DrawText(Text, w, h, paint);
				}
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

