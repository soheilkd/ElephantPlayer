using System.Linq;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class PlaylistContent : UserControl
	{
		public PlaylistContent() => InitializeComponent();

		public void ChangePlaylist(string playlist)
		{
			MainList.Items = Controller.Library.Playlists.Where(item => item.Name == playlist).First();
		}
	}
}
