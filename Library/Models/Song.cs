using System;

namespace EPlayer.Library.Models
{
	[Serializable]
	public class Song : Media
	{
		public string Artist { get; set; }
		public string Album { get; set; }
		public string Lyrics { get; set; }

		public Song(string path) : base(path) { }
	}
}
