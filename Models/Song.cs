using System;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Player.Models
{
	public class Song : Media
	{
		public MusicProperties Properties { get; private set; } = default;
		public string Artist => Properties.Artist;
		public string Album => Properties.Album;
		public string AlbumArtist => Properties.AlbumArtist;
		public string Title => Properties.Title;
		public TimeSpan Duration => Properties.Duration;

		public Song(string path) : base(path) { }
		public Song(StorageFile file) : base(file) { }

		protected override async void ReadProperties(StorageFile file)
		{
			Properties = await file.Properties.GetMusicPropertiesAsync();
			NotifyPropertyChange(Properties);
		}
		protected override async void ReadProperties(string path)
		{
			ReadProperties(await StorageFile.GetFileFromPathAsync(path));
		}
	}
}
