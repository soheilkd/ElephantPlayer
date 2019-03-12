using Library.Extensions;
using Player.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ItemSubcontent : UserControl
	{
		private static readonly GridLength CompactGridLength = new GridLength(0);
		public Func<string, byte[]> ImageLoader { get; set; }
		public Func<string, string> BioLoader{ get; set; }
		public Func<string, MediaQueue> MediaLoader { get; set; }

		public ItemSubcontent() => InitializeComponent();

		public void DefineLoaders(Func<string, MediaQueue> mediaLoader, Func<string, byte[]> imageLoader = default, Func<string, string> bioLoader = default)
		{
			MediaLoader = mediaLoader;
			ImageLoader = imageLoader;
			BioLoader = bioLoader;
		}

		public void ChangeContent(string lookup)
		{
			MainList.Items = MediaLoader?.Invoke(lookup);
			MainImage.Source = null;
			MainGrid.ColumnDefinitions[0].Width = CompactGridLength;
			MainTextBlock.Text = null;
			MainGrid.RowDefinitions[0].Height = CompactGridLength;
			Task.Run(() =>
			{
				LoadImage(lookup);
				LoadBio(lookup);
			});
		}

		private void LoadImage(string lookup)
		{
			var image = ImageLoader?.Invoke(lookup);
			Dispatcher.Invoke(() =>
			{
				MainImage.Source = image.ToBitmap();
				MainGrid.ColumnDefinitions[0].Width = image == null ? CompactGridLength : GridLength.Auto;
			});
		}

		private void LoadBio(string lookup)
		{
			var bio = BioLoader?.Invoke(lookup);
			Dispatcher.Invoke(() =>
			{
				MainTextBlock.Text = bio;
				MainGrid.RowDefinitions[0].Height = string.IsNullOrWhiteSpace(MainTextBlock.Text) ? CompactGridLength : GridLength.Auto;
			});
		}
	}
}
