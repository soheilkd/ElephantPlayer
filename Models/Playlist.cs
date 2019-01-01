using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Library.Extensions;
using Library.Serialization.Models;

namespace Player.Models
{
	[Serializable]
	public class Playlist : ObservableCollection<Media>
	{
		public string Name { get; set; }
		public SerializableBitmap Thumbnail { get; set; }

		public Playlist(string name = "New Playlist", Collection<Media> medias = default, SerializableBitmap thumbnail = default) : base(new List<Media>())
		{
			Thumbnail = thumbnail;
			if (medias != default)
				medias.For(each => Add(each));
		}
	}
}
