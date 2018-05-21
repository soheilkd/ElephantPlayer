﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Player.Events;
using System.Media;
using System.Windows.Media.Imaging;

namespace Player
{
    public partial class PropertiesUI : Window
    {
        private OpenFileDialog ArtworkDialog = new OpenFileDialog()
        {
            CheckFileExists = true,
            Filter = "Images|*.jpg;*.png;*.jpeg",
            Title = "Open artwork"
        };
        private Media Media;
        private TagLib.File File;
        public event EventHandler<InfoExchangeArgs> ChangeRequested;

        public PropertiesUI() => InitializeComponent();

        public static void OpenFor(Media media, EventHandler<InfoExchangeArgs> onSave)
        {
            var ui = new PropertiesUI
            {
                Media = media,
                File = TagLib.File.Create(media.Path)
            };
            var tag = ui.File.Tag;
            ui.TitleBox.Text = tag.Title ?? String.Empty;
            ui.ArtistBox.Text = tag.FirstPerformer ?? String.Empty;
            ui.AlbumArtistBox.Text = tag.FirstAlbumArtist ?? String.Empty;
            ui.ComposerBox.Text = tag.FirstComposer ?? String.Empty;
            ui.ConductorBox.Text = tag.Conductor ?? String.Empty;
            ui.GenreBox.Text = tag.FirstGenre ?? String.Empty;
            ui.TrackBox.Text = tag.Track.ToString() ?? String.Empty;
            ui.CommentBox.Text = tag.Comment ?? String.Empty;
            ui.YearBox.Text = tag.Year.ToString() ?? String.Empty;
            ui.CopyrightBox.Text = tag.Copyright ?? String.Empty;
            ui.LyricsBox.Text = tag.Lyrics ?? String.Empty;
            ui.ArtworkImage.Source = media.Artwork;
            ui.ChangeRequested += onSave;
            ui.Show();
        }

        private void BrowseArtworkClick(object sender, RoutedEventArgs e)
        {
            if (ArtworkDialog.ShowDialog().Value)
            {
                File.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(ArtworkDialog.FileName) };
                ArtworkImage.Source = new BitmapImage(new Uri(ArtworkDialog.FileName));
            }
        }

        private void RemoveArtworkClick(object sender, RoutedEventArgs e)
        {
            File.Tag.Pictures = new TagLib.IPicture[0];
            ArtworkImage.Source = Media.IsVideo ? Imaging.Images.VideoArt : Imaging.Images.MusicArt;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            File.Tag.Title = TitleBox.Text ?? String.Empty;
            File.Tag.Performers = new string[] { ArtistBox.Text ?? String.Empty };
            File.Tag.AlbumArtists = new string[] { AlbumArtistBox.Text ?? String.Empty };
            File.Tag.Composers = new string[] { ComposerBox.Text ?? String.Empty };
            File.Tag.Conductor = ConductorBox.Text ?? String.Empty;
            File.Tag.Genres = new string[] { GenreBox.Text ?? String.Empty };
            File.Tag.Track = UInt32.TryParse(TrackBox.Text ?? "0", out uint num) ? num : 0;
            File.Tag.Comment = CommentBox.Text ?? String.Empty;
            File.Tag.Year = UInt32.TryParse(YearBox.Text ?? "0", out uint num2) ? num2 : 0;
            File.Tag.Copyright = CopyrightBox.Text ?? String.Empty;
            File.Tag.Lyrics = LyricsBox.Text ?? String.Empty;
            ChangeRequested?.Invoke(this, new InfoExchangeArgs() { Object = File });
            Close();
        }
    }
}