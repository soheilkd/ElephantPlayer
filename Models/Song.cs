using System;
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Player.Models
{
	[DataContract]
	public class Song : Media
	{
		[IgnoreDataMember]
		private MusicProperties _Properties;
		public MusicProperties Properties
		{
			get => _Properties;
			private set
			{
				_Properties = value;

				//Data will be stored in properties to make the class serializable, since MusicProperties is not serializable
				Artist = value.Artist;
				Album = value.Album;
				AlbumArtist = value.AlbumArtist;
				Title = value.Title;
				Duration = value.Duration;
			}
		}

		[DataMember]
		public string Artist { get; private set; }
		[DataMember]
		public string Album { get; private set; }
		[DataMember]
		public string AlbumArtist { get; private set; }

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
