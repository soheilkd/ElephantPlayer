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
	public partial class ArtistTile : UserControl
	{
		private static readonly byte[] un = IconProvider.GetBitmap(IconType.Person).ToData();
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
			//MainImage.Source = Library.Controls.IconProvider.GetBitmap(Library.Controls.IconType.Person);
			Task.Run(() =>
			{
				var image = Web.GetArtistImage(artist) ?? un;
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
