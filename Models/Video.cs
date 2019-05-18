using System;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Player.Models
{
	public class Video : Media
	{
		public VideoProperties Properties = default;
		public VideoOrientation Orientation => Properties.Orientation;
		public string Title => Properties.Title;
		public TimeSpan Duration => Properties.Duration;

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
