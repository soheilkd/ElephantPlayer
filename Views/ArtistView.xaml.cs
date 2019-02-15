using Lastfm.Services;
using Library.Serialization.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ArtistView : Grid
	{
		public ArtistView() => InitializeComponent();
		public ArtistView(string artistName) : this()
		{
			MediaDataGrid.ItemsSource = Controller.Library.Artists[artistName];
			LoadArtistInfo(artistName);
		}

		public void LoadArtistInfo(string artistName)
		{
			if (Controller.Resource.ContainsKey(artistName))
			{
				ArtistImage.Source = new SerializableBitmap(Controller.Resource[artistName]);
				MediaDataGrid.Margin = new Thickness(0, 100, 0, 20);
				Task.Run(() =>
				{
					Artist artist = Web.GetArtist(artistName);
					if (artist == null) return;
					var summary = artist.Bio.GetSummary();
					if (summary.StartsWith("<a"))
						summary = "No Content";
					Dispatcher.Invoke(() => ArtistDescription.Text = summary);
				});
			}
		}
	}
}
