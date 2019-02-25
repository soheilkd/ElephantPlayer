using System.Linq;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class PlaylistView : ContentControl
	{
		public PlaylistView(string playlistName)
		{
			InitializeComponent();
			MediaDataGrid.Items = Controller.Library.Playlists.Where(each => each.Name == playlistName).First();
		}
	}
}
