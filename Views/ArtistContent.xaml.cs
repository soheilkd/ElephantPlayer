using Library.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ArtistContent : UserControl
	{
		private static readonly GridLength CompactGridLength = new GridLength(0);
		private string _Artist = default;

		public ArtistContent() => InitializeComponent();

		public void ChangeArtist(string artist)
		{
			_Artist = artist;
			MainList.Items = Controller.Library.Artists[artist];
			MainImage.Source = null;
			MainGrid.ColumnDefinitions[0].Width = CompactGridLength;
			MainTextBlock.Text = null;
			MainGrid.RowDefinitions[0].Height = CompactGridLength;
			Task.Run(() =>
			{
				LoadArtistImage();
				LoadArtistBio();
			});
		}

		private void LoadArtistImage()
		{
			var image = Web.GetArtistImage(_Artist);
			Dispatcher.Invoke(() =>
			{
				MainImage.Source = image.ToBitmap();
				MainGrid.ColumnDefinitions[0].Width = image == null ? CompactGridLength : GridLength.Auto;
			});
		}

		private void LoadArtistBio()
		{
			var bio = Web.TryGetArtist(_Artist, out var artistInfo) ? artistInfo.Bio.getContent() : default;
			Dispatcher.Invoke(() =>
			{
				MainTextBlock.Text = bio;
				MainGrid.RowDefinitions[0].Height = string.IsNullOrWhiteSpace(MainTextBlock.Text) ? CompactGridLength : GridLength.Auto;
			});
		}
	}
}
