using Player.Pages;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Player
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			contentFrame.Navigate(typeof(Page));
			//TODO: Have to optimize the library, it doesnt load from disk now
		}

		private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			int index = int.Parse((args.InvokedItemContainer.Tag ?? -1).ToString());
			if (args.IsSettingsInvoked) contentFrame.Navigate(typeof(SettingsPage));
			else if (index == 0) contentFrame.Navigate(typeof(SongsPage));
			else if (index == 1) contentFrame.Navigate(typeof(VideosPage));
			else if (index == 2) contentFrame.Navigate(typeof(PlaylistsPage));
			else if (index == 3) contentFrame.Navigate(typeof(StreamPage));
		}

		private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Controller.Library.ReadLibrary();
			
			Controller.Library.Songs.CollectionChanged += (_, __) => Controller.SaveAll();
			Controller.Library.Videos.CollectionChanged += (_, __) => Controller.SaveAll();
		}
	}
}
