using Library.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumContent : UserControl
	{
		private static readonly GridLength CompactGridLength = new GridLength(0);
		private string _Album = default;

		public AlbumContent() => InitializeComponent();

		public void ChangeAlbum(string album)
		{
			_Album = album;
			MainList.Items = Controller.Library.Albums[album];
			MainImage.Source = null;
			MainGrid.ColumnDefinitions[0].Width = CompactGridLength;
			Task.Run(() => LoadAlbumImage());
		}

		private void LoadAlbumImage()
		{
			var image = Web.GetAlbumImage(_Album);
			Dispatcher.Invoke(() =>
			{
				MainImage.Source = image.ToBitmap();
				MainGrid.ColumnDefinitions[0].Width = image == null ? CompactGridLength : GridLength.Auto;
			});
		}
	}
}
