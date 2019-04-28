using System;
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Player.Models
{
	[DataContract]
	public class Video : Media
	{
		private VideoProperties _Properties;
		public VideoProperties Properties
		{
			get => _Properties;
			set
			{
				_Properties = value;

				//Data will be stored in properties to make the class serializable, since MusicProperties is not serializable
				Orientation = value.Orientation;
				Title = value.Title;
				Duration = value.Duration;
			}
		}
		[DataMember]
		public VideoOrientation Orientation { get; private set; }

		public Video(string path) : base(path) { }
		public Video(StorageFile file) : base(file) { }

		protected override async void ReadProperties(StorageFile file)
		{
			Properties = await file.Properties.GetVideoPropertiesAsync();
		}
		protected override async void ReadProperties(string path)
		{
			ReadProperties(await StorageFile.GetFileFromPathAsync(path));
		}
	}
}
