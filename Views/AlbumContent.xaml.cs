using Library.Serialization.Models;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumContent : UserControl
	{
		public AlbumContent() => InitializeComponent();

		public void ChangeAlbum(string album)
		{
			MainList.Items = Controller.Library.Albums[album];
			Task.Run(async () =>
			{
				var image = await Web.GetAlbumImage(album);
				MainImage.Dispatcher.Invoke(() => MainImage.Source = image);
			});
		}
	}
}
