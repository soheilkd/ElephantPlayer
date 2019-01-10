using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Player.Views.ViewerOperator;

namespace Player.Views
{
	public partial class PlaylistsView : ContentControl
	{
		private int CallTime = -1; //For lazy loading

		public PlaylistsView() => InitializeComponent();

		private void Content_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ == 0)
			{
				var playlistNames = Controller.Playlists.Select(each => each.Name).ToArray();
				ApplyNavigations(playlistNames, default, typeof(PlaylistView), PlaylistNavigation);
			}
		}
	}
}
