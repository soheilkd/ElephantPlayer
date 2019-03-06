using Library;
using Library.Serialization.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Player.Views
{
	public partial class ArtistTile : UserControl
	{
		public event InfoExchangeHandler<string> Expanded;
		public event EventHandler Collapsed;
		private bool _IsStatusChangingByCode = false;

		public ArtistTile() => InitializeComponent();

		public ArtistTile(string artist)
		{
			InitializeComponent();

			MainTextBlock.Text = artist;

			MainToggle.Checked += delegate { if (!_IsStatusChangingByCode) Expanded.Invoke(this, artist); };
			MainToggle.Unchecked += delegate { if (!_IsStatusChangingByCode) Collapsed.Invoke(this, null); };

			Task.Run(async () =>
			{
				var image = await Web.GetArtistImage(artist);
				Dispatcher.Invoke(() => MainImage.Source = image);
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
