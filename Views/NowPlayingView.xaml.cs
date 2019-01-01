using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Library;
using Player.Models;

namespace Player.Views
{
    public partial class NowPlayingView : ContentControl
    {
        public NowPlayingView()
        {
            InitializeComponent();
			Controller.PlayRequest += Controller_PlayRequest;
        }

		private void Controller_PlayRequest(object sender, InfoExchangeArgs<(MediaQueue, Media)> e)
		{
			MediaDataGrid.ItemsSource = e.Parameter.Item1;
			MainTextBlock.Text = $"{e.Parameter.Item2.Artist} - {e.Parameter.Item2.Title}\r\n{e.Parameter.Item2.Album}";
            Task.Run(() =>
            {
                var art = e.Parameter.Item2.Artwork;
                Dispatcher.Invoke(() => ArtworkImage.Source = art);
            });
		}
	}
}
