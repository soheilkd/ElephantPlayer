using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Library.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class PlaylistView : ContentControl
	{
		public PlaylistView(string playlistName): base()
		{
			var matchingPlaylists = from each in Controller.Playlists where each.Name == playlistName select each;
			if (matchingPlaylists.Count() == 1)
				MediaDataGrid.ItemsSource = new MediaQueue(matchingPlaylists.First());
		}

		public PlaylistView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			var totalLength = new TimeSpan(0);
			MediaDataGrid.ItemsSource.For(each => totalLength += each.Length);
			BarTextBlock.Text = $"Showing {MediaDataGrid.ItemsSource.Count} media[s], totally {(int)totalLength.TotalMinutes} minutes";
		}
	}
}
