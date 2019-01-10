using System;
using System.Collections.Generic;

namespace Player.Models
{
	[Serializable]
	public class Playlist : MediaQueue
	{
		public string Name { get; set; }

		public Playlist(string name, IEnumerable<Media> medias = default) : base(medias ?? new Media[0])
		{
			Name = name;
		}
	}
}