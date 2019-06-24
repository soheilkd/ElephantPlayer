using System;
using System.Linq;

namespace Player.Models
{
	[Serializable]
	public class Song : Media
	{
		public string Lyrics = "";

		public string Artist { get; set; }
		public string Album { get; set; }

		public Song(string path) : base(path) { }

		protected override void ReadProperties()
		{
			Name = Path.Substring(Path.LastIndexOf("\\") + 1);
			var properties = ShellAPI.GetProperty(Path);
			Artist = properties.Music.Artist.Value.First();
			Title = properties.Title.Value;
			Album = properties.Music.AlbumTitle.Value;
			Lyrics = properties.Music.Lyrics.Value;
			Duration = TimeSpan.FromTicks((long)properties.Media.Duration.Value);
		}
	}
}
