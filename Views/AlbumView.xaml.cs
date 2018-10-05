using System;
using System.Windows.Controls;
using Player.Models;

namespace Player.Views
{
	public partial class AlbumView : Grid
	{
		public AlbumView(MediaQueue queue, Action<MediaQueue, Media> onPlay)
		{
			InitializeComponent();
			MediaDataGrid.ItemsSource = queue;
			MediaDataGrid.MediaRequested += (_, e) => onPlay(e.Parameter.Item1, e.Parameter.Item2);
		}

		public AlbumView()
		{
			InitializeComponent();
		}
	}
}
