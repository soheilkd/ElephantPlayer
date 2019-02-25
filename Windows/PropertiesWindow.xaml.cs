using Library;
using Library.Controls;
using Microsoft.Win32;
using Player.Extensions;
using Player.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Player.Windows
{
	public partial class PropertiesWindow : Window
	{
		private const char Seperator = ';';

		private OpenFileDialog _OpenArtDialog = new OpenFileDialog()
		{
			CheckFileExists = true,
			Filter = "Images|*.jpg;*.png;*.jpeg",
			Title = "Open artwork"
		};
		private Media _Media;
		private TagLib.File _TagFile;

		public PropertiesWindow() => InitializeComponent();

		public static void OpenNewWindowFor(Media media)
		{
			var window = new PropertiesWindow();
			window.LoadFor(media);
			window.Show();
		}
		public void LoadFor(Media media)
		{
			if (media.Type != MediaType.Music)
			{
				MessageBox.Show("Cannot edit properties of this type of media", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				Close();
				return;
			}
			_Media = media;
			_TagFile = TagLib.File.Create(media.Path);
			_Media.Load();
			ApplyTagForUI(_TagFile.Tag);
		}
		private void ApplyTagForUI(TagLib.Tag tag)
		{
			TitleBox.Text = tag.Title;
			AlbumBox.Text = tag.Album;
			ArtistBox.Text = tag.Title;
			AlbumBox.Text = string.Join(Seperator.ToString(), tag.Performers);
			AlbumArtistBox.Text = string.Join(Seperator.ToString(), tag.AlbumArtists);
			GenreBox.Text = string.Join(Seperator.ToString(), tag.Genres);
			CommentBox.Text = tag.Comment;
			CopyrightBox.Text = tag.Copyright;
			LyricsBox.Text = tag.Lyrics;
			ArtworkImage.Source = tag.Pictures.Length >= 1 ? tag.Pictures[0].GetBitmapImage() : IconProvider.GetBitmap(IconType.Music);
		}

		private void RemoveArtworkClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Pictures = new TagLib.IPicture[0];
			ArtworkImage.Source = IconProvider.GetBitmap(IconType.Music);
		}
		private void SaveButtonClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Title = TitleBox.Text;
			_TagFile.Tag.Album = AlbumBox.Text;
			_TagFile.Tag.Performers = ArtistBox.Text.Split(Seperator);
			_TagFile.Tag.AlbumArtists = AlbumArtistBox.Text.Split(Seperator);
			_TagFile.Tag.Genres = GenreBox.Text.Split(Seperator);
			_TagFile.Tag.Comment = CommentBox.Text;
			_TagFile.Tag.Copyright = CopyrightBox.Text;
			_TagFile.Tag.Lyrics = LyricsBox.Text;

			_TagFile.Save();
			_Media.Reload();
			Close();
		}
		private void ArtworkImage_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (_OpenArtDialog.ShowDialog() ?? false)
			{
				_TagFile.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(_OpenArtDialog.FileName) };
				ArtworkImage.Source = new BitmapImage(new Uri(_OpenArtDialog.FileName));
			}
		}
	}
}
