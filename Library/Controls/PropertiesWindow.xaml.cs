using MahApps.Metro.Controls;
using Microsoft.Win32;
using Player.Extensions;
using Player.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Player.Controls
{
	public partial class PropertiesUI : MetroWindow
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
		public event EventHandler<InfoExchangeArgs<TagLib.File>> SaveRequested;

		public PropertiesUI() => InitializeComponent();

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
			TagLib.Tag tag = _TagFile.Tag;
			_Media.Load();
			TitleBox.Text = tag.Title ?? string.Empty;
			AlbumBox.Text = tag.Album ?? string.Empty;
			ArtistBox.Text = string.Join(Seperator.ToString(), tag.Performers) ?? string.Empty;
			AlbumArtistBox.Text = string.Join(Seperator.ToString(), tag.AlbumArtists) ?? string.Empty;
			ComposerBox.Text = string.Join(Seperator.ToString(), tag.Composers) ?? string.Empty;
			ConductorBox.Text = tag.Conductor ?? string.Empty;
			GenreBox.Text = string.Join(Seperator.ToString(), tag.Genres) ?? string.Empty;
			TrackBox.Text = tag.Track.ToString() ?? string.Empty;
			CommentBox.Text = tag.Comment ?? string.Empty;
			YearBox.Text = tag.Year.ToString() ?? string.Empty;
			CopyrightBox.Text = tag.Copyright ?? string.Empty;
			LyricsBox.Text = tag.Lyrics ?? string.Empty;
			ArtworkImage.Source = tag.Pictures.Length >= 1 ? tag.Pictures[0].ToBitmapImage() : Properties.Resources.MusicLogo.ToImageSource();
			Show();
		}

		private void RemoveArtworkClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Pictures = new TagLib.IPicture[0];
			ArtworkImage.Source = Properties.Resources.MusicLogo.ToImageSource();
		}
		private void SaveButtonClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Title = TitleBox.Text ?? string.Empty;
			_TagFile.Tag.Album = AlbumBox.Text ?? string.Empty;
			_TagFile.Tag.Performers = ArtistBox.Text.Split(Seperator) ?? new string[0];
			_TagFile.Tag.AlbumArtists = AlbumArtistBox.Text.Split(Seperator) ?? new string[0];
			_TagFile.Tag.Composers = ComposerBox.Text.Split(Seperator) ?? new string[0];
			_TagFile.Tag.Conductor = ConductorBox.Text ?? string.Empty;
			_TagFile.Tag.Genres = GenreBox.Text.Split(Seperator) ?? new string[0];
			_TagFile.Tag.Track = uint.TryParse(TrackBox.Text ?? "0", out uint num) ? num : 0;
			_TagFile.Tag.Comment = CommentBox.Text ?? string.Empty;
			_TagFile.Tag.Year = uint.TryParse(YearBox.Text ?? "0", out uint num2) ? num2 : 0;
			_TagFile.Tag.Copyright = CopyrightBox.Text ?? string.Empty;
			_TagFile.Tag.Lyrics = LyricsBox.Text ?? string.Empty;

			SaveRequested?.Invoke(this, new InfoExchangeArgs<TagLib.File>(_TagFile));
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
