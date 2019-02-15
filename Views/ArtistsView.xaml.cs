using Lastfm.Services;
using System.Windows;
using System.Windows.Controls;
using static Player.Views.ViewerOperator;

namespace Player.Views
{
	public partial class ArtistsView : ContentControl
	{
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters ArtistsView tab on MainWindow
		public ArtistsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ == 0)
				ApplyNavigations(Controller.Library.Artists.Keys, typeof(Artist), typeof(ArtistView), ArtistNavigation);
		}
	}
}
