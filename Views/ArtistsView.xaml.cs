using System;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ArtistsView : ContentControl
	{
		private ItemSubcontent _CurrentContent = new ItemSubcontent()
		{
			MediaLoader = key => Controller.Library.Artists[key],
			ImageLoader = key => Web.GetArtistImage(key),
			BioLoader = key => Web.TryGetArtist(key, out var artistInfo) ? artistInfo.Bio.getContent() : default
		};

		public ArtistsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			ViewerOperator.Initialize(MainGrid, Controller.Library.Artists.Keys, _CurrentContent);
		}
	}
}
