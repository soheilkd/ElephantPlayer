using Library;
using Library.Serialization.Models;
using Player.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumTile : UserControl
	{
		public event InfoExchangeHandler<string> Expanded;
		public event EventHandler Collapsed;
		private bool _IsStatusChangingByCode = false;

		public AlbumTile() => InitializeComponent();

		public AlbumTile(string album)
		{
			InitializeComponent();

			MainTextBlock.Text = album;

			MainToggle.Checked += delegate { if (!_IsStatusChangingByCode) Expanded.Invoke(this, album); };
			MainToggle.Unchecked += delegate { if (!_IsStatusChangingByCode) Collapsed.Invoke(this, null); };

			Task.Run(async () =>
			{
				var image = await Web.GetAlbumImage(album);
				MainImage.Dispatcher.Invoke(() => MainImage.Source = image);
			});
		}

		public void ChangeStatus(bool? isChecked)
		{
			_IsStatusChangingByCode = true;
			MainToggle.IsChecked = isChecked;
			_IsStatusChangingByCode = false;
		}
	}
}
