using Library.Extensions;
using Player.Models;
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
				ApplyNavigations(Controller.Library.Playlists.Select(each => each.Name), default, typeof(PlaylistView), PlaylistNavigation);
		}

		private void CreateButtonClick(object sender, RoutedEventArgs e)
		{
			Controller.Library.Playlists.Add(new MediaQueue() { Name = NewPlaylistBox.Text });
			NewPlaylistBox.Text = "New Playlist Name";
			ApplyNavigations(Controller.Library.Playlists.Select(each => each.Name), default, typeof(PlaylistView), PlaylistNavigation);
			PlaylistNavigation.Focus();
		}
	}
}
