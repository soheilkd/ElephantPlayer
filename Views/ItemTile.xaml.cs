using Library;
using Library.Controls;
using Library.Extensions;
using Library.Serialization.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Player.Views
{
	public partial class ItemTile : UserControl
	{
		private static readonly byte[] un = IconProvider.GetBitmap(IconType.Person).ToData();
		public event InfoExchangeHandler<string> Expanded;
		public event EventHandler Collapsed;
		private bool _IsStatusChangingByCode = false;

		public ItemTile() => InitializeComponent();

		public ItemTile(string title, Func<string, byte[]> imageLoader)
		{
			InitializeComponent();

			MainTextBlock.Text = title;

			MainToggle.Checked += delegate { if (!_IsStatusChangingByCode) Expanded.Invoke(this, title); };
			MainToggle.Unchecked += delegate { if (!_IsStatusChangingByCode) Collapsed.Invoke(this, null); };
			//MainImage.Source = Library.Controls.IconProvider.GetBitmap(Library.Controls.IconType.Person);
			Task.Run(() =>
			{
				var image = imageLoader(title) ?? un;
				Dispatcher.Invoke(() => MainImage.Source = image.ToBitmap());
			});
		}

		public void ChangeStatus(bool? isChecked, bool raiseEvent = false)
		{
			_IsStatusChangingByCode = !raiseEvent;
			MainToggle.IsChecked = isChecked;
			_IsStatusChangingByCode = false;
		}
	}
}
