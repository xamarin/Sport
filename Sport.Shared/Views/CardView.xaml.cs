using System;
using Xamarin.Forms;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Sport.Shared
{
	[ContentProperty("Content")]
	public partial class CardView : Layout
	{
		#region Properties

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create("Title", typeof(string), typeof(CardView), null);

		public string Title
		{
			get
			{
				return (string)GetValue(TitleProperty);
			}
			set
			{
				SetValue(TitleProperty, value);
				title.Text = value;
			}
		}

		public static readonly BindableProperty SubtitleProperty =
			BindableProperty.Create("Subtitle", typeof(string), typeof(CardView), null);

		public string Subtitle
		{
			get
			{
				return (string)GetValue(TitleProperty);
			}
			set
			{
				SetValue(TitleProperty, value);
				subtitle.Text = value;
			}
		}

		//		public static readonly BindableProperty IconProperty =
		//			BindableProperty.Create("Icon", typeof(Image), typeof(CardView), null);
		//
		//		public Image Icon
		//		{
		//			get
		//			{
		//				return (Image)GetValue(IconProperty);
		//			}
		//			set
		//			{
		//				SetValue(IconProperty, value);
		//				//icon = value;
		//			}
		//		}

		View _content;

		public View Content
		{
			get
			{
				return _content;
			}
			set
			{
				_content = value;
				view.Content = _content;
			}
		}

		View _chrome;
		ObservableCollection<Element> _children;

		public View Chrome
		{
			get
			{
				return _chrome;
			}
			set
			{
				_chrome = value;
				if(_children == null)
				{
					var obj = this.GetType().InvokeMember("InternalChildren", BindingFlags.GetProperty | BindingFlags.NonPublic, Type.DefaultBinder, this, null);
					if(obj != null)
					{
						var elements = obj as ObservableCollection<Element>;

						if(elements != null)
							_children = elements;
					}
				}

				_children.Add(_chrome);
			}
		}

		public Action OnClicked
		{
			get;
			set;
		}

		#endregion

		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			return root.GetSizeRequest(widthConstraint, heightConstraint);
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			Chrome.Layout(new Rectangle(x, y, width, height));
		}

		public CardView()
		{
			InitializeComponent();
			root.GestureRecognizers.Add(new TapGestureRecognizer((view) =>
			{
				OnClicked?.Invoke();
			}));
		}
	}
}