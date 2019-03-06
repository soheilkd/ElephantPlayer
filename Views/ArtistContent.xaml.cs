using System.Threading.Tasks;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ArtistContent : UserControl
	{
		public ArtistContent()
		{
			InitializeComponent();
		}

		public void ChangeArtist(string artist)
		{
			MainList.Items = Controller.Library.Artists[artist];
			Task.Run(async () =>
			{
				System.Windows.Media.Imaging.BitmapImage image = await Web.GetArtistImage(artist);
				MainImage.Dispatcher.Invoke(() => MainImage.Source = image);
			});
		}
	}
}
