using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class PlaylistsView : ContentControl
	{
		private ItemSubcontent _CurrentContent = new ItemSubcontent()
		{
			MediaLoader = key => Controller.Library.Playlists[key]
		};

		public PlaylistsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			ViewerOperator.Initialize(MainGrid, Controller.Library.Playlists.Keys, _CurrentContent);
		}
	}
}
