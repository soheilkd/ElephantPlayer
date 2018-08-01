using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Player
{
	public partial class PropertiesUI : MetroWindow
	{
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
			_TagFile = TagLib.File.Create(media);
			var tag = _TagFile.Tag;
			MediaOperator.Load(_Media);
			TitleBox.Text = tag.Title ?? String.Empty;
			AlbumBox.Text = tag.Album ?? String.Empty;
			ArtistBox.Text = tag.FirstPerformer ?? String.Empty;
			AlbumArtistBox.Text = tag.FirstAlbumArtist ?? String.Empty;
			ComposerBox.Text = tag.FirstComposer ?? String.Empty;
			ConductorBox.Text = tag.Conductor ?? String.Empty;
			GenreBox.Text = tag.FirstGenre ?? String.Empty;
			TrackBox.Text = tag.Track.ToString() ?? String.Empty;
			CommentBox.Text = tag.Comment ?? String.Empty;
			YearBox.Text = tag.Year.ToString() ?? String.Empty;
			CopyrightBox.Text = tag.Copyright ?? String.Empty;
			LyricsBox.Text = tag.Lyrics ?? String.Empty;
			ArtworkImage.Source = media.Artwork;
			Show();
		}

		private void RemoveArtworkClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Pictures = new TagLib.IPicture[0];
			ArtworkImage.Source = _Media.IsVideo ? Images.VideoArt : Images.MusicArt;
		}
		private void SaveButtonClick(object sender, MouseButtonEventArgs e)
		{
			_TagFile.Tag.Title = TitleBox.Text ?? String.Empty;
			_TagFile.Tag.Album = AlbumBox.Text ?? String.Empty;
			_TagFile.Tag.Performers = new string[] { ArtistBox.Text ?? String.Empty };
			_TagFile.Tag.AlbumArtists = new string[] { AlbumArtistBox.Text ?? String.Empty };
			_TagFile.Tag.Composers = new string[] { ComposerBox.Text ?? String.Empty };
			_TagFile.Tag.Conductor = ConductorBox.Text ?? String.Empty;
			_TagFile.Tag.Genres = new string[] { GenreBox.Text ?? String.Empty };
			_TagFile.Tag.Track = UInt32.TryParse(TrackBox.Text ?? "0", out uint num) ? num : 0;
			_TagFile.Tag.Comment = CommentBox.Text ?? String.Empty;
			_TagFile.Tag.Year = UInt32.TryParse(YearBox.Text ?? "0", out uint num2) ? num2 : 0;
			_TagFile.Tag.Copyright = CopyrightBox.Text ?? String.Empty;
			_TagFile.Tag.Lyrics = LyricsBox.Text ?? String.Empty;

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
