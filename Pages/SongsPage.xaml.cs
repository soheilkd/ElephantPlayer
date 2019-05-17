using Player.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Player.Pages
{
	public sealed partial class SongsPage : Page
	{
		public SongsPage()
		{
			this.InitializeComponent();
			TracksListView.ItemsSource = Controller.Library.Songs;
		}

		private void TracksListBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			Controller.Play(TracksListView.SelectedItem as Media);
		}
	}
}
