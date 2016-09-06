using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MessageBar
{
	public class MessageView : UIView
	{
		UILabel _label;

		public string Message
		{
			get;
			set;
		}

		public MessageType MessageType
		{
			get;
			set;
		}

		public nfloat Margin
		{
			get;
			set;
		} = 4f;

		public nfloat Padding
		{
			get;
			set;
		} = 18f;

		public Action<bool> OnDismiss
		{
			get;
			set;
		}

		public bool Hit
		{
			get;
			set;
		}

		public nfloat Height
		{
			get;
			private set;
		}

		public nfloat Width
		{
			get;
			private set;
		}

		internal IStyleSheetProvider StylesheetProvider
		{
			get;
			set;
		}

		public MessageView()
		{
		}

		public MessageView(string title, string description, MessageType type)
		{
			BackgroundColor = UIColor.FromRGBA(0, 0, 0, 200);
			ClipsToBounds = false;
			UserInteractionEnabled = true;
			Message = description;
			MessageType = type;
			Width = GetStatusBarFrame().Width - Margin * 2;
			Height = 60f;

			Layer.CornerRadius = 2f;
			Layer.MasksToBounds = true;

			NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, OrientationChanged);
		}

		public override void LayoutSubviews()
		{
			if(_label == null)
			{
				_label = new UILabel(new CGRect(Padding, Padding / 2, Width - Padding * 2, Height - Padding));
				_label.AdjustsFontSizeToFitWidth = true;
				_label.LineBreakMode = UILineBreakMode.TailTruncation;
				_label.Font = UIFont.SystemFontOfSize(16);

				var desc = new NSString(Message);
				var size = desc.StringSize(_label.Font);
				_label.Lines = size.Width > _label.Frame.Width ? 2 : 1;

				if(MessageType == MessageType.Error)
				{
					_label.TextColor = UIColor.FromRGB(245, 109, 79);
				}
				else
				{
					_label.TextColor = UIColor.FromRGB(255, 255, 255);
				}

				Add(_label);
			}

			_label.Text = Message;

			base.LayoutSubviews();
		}

		public override bool Equals(object obj)
		{
			if(!(obj is MessageView))
				return false;

			var messageView = (MessageView)obj;

			return MessageType == messageView.MessageType && Message == messageView.Message;
		}

		CGRect GetStatusBarFrame()
		{
			var windowFrame = OrientFrame(UIApplication.SharedApplication.KeyWindow.Frame);
			var statusFrame = OrientFrame(UIApplication.SharedApplication.StatusBarFrame);

			return new CGRect(windowFrame.X, windowFrame.Y, windowFrame.Width, statusFrame.Height);
		}

		void OrientationChanged(NSNotification notification)
		{
			Frame = new CGRect(Frame.X, Frame.Y, GetStatusBarFrame().Width, Frame.Height);
			SetNeedsDisplay();
		}

		CGRect OrientFrame(CGRect frame)
		{
			//This size has already inverted in iOS8, but not on simulator, seems odd
			if(!IsRunningiOS8OrLater() && (IsDeviceLandscape(UIDevice.CurrentDevice.Orientation) || IsStatusBarLandscape(UIApplication.SharedApplication.StatusBarOrientation)))
			{
				frame = new CGRect(frame.X, frame.Y, frame.Height, frame.Width);
			}

			return frame;
		}

		bool IsDeviceLandscape(UIDeviceOrientation orientation)
		{
			return orientation == UIDeviceOrientation.LandscapeLeft || orientation == UIDeviceOrientation.LandscapeRight;
		}

		bool IsStatusBarLandscape(UIInterfaceOrientation orientation)
		{
			return orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight;
		}

		bool IsRunningiOS7OrLater()
		{
			string systemVersion = UIDevice.CurrentDevice.SystemVersion;

			return IsRunningiOS8OrLater() || systemVersion.Contains("7");
		}

		bool IsRunningiOS8OrLater()
		{
			string systemVersion = UIDevice.CurrentDevice.SystemVersion;

			return systemVersion.Contains("8");
		}
	}
}