using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Lastfm.Services;
using Library.Extensions;
using Library.Serialization.Models;
using Player.Models;

namespace Player.Views
{
	public partial class ArtistView : Grid
	{
		private string ArtistName;

        public ArtistView(string artistName) : this()
        {
            MediaDataGrid.ItemsSource = new MediaQueue(Controller.Library.Where(each => each.Artist == artistName));
            ArtistName = artistName;
            LoadArtist();
		}

		public ArtistView()
		{
			InitializeComponent();
		}

        public void LoadArtist()
        {
            if (Controller.Resource.ContainsKey(ArtistName))
            {
                ArtistImage.Source = new SerializableBitmap(Controller.Resource[ArtistName]);
                MediaDataGrid.Margin = new Thickness(0, 100, 0, 20);
                Task.Run(() =>
                {
                    var artist = Web.API.GetArtist(ArtistName);
                    var summary = artist.Bio.GetSummary();
                    if (summary.StartsWith("<a"))
                        summary = "No Content";
                    Dispatcher.Invoke(() => ArtistDescription.Text = summary);
                });
            }
        }

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			var totalLength = new TimeSpan(0);
			MediaDataGrid.ItemsSource.For(each => totalLength += each.Length);
			Controller.Library.CollectionChanged += Library_CollectionChanged;
		}

		private void Library_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			MediaDataGrid.ItemsSource = new MediaQueue(Controller.Library.Where(each => each.Artist == ArtistName));
		}
	}
}
