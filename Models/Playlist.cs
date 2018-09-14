using System;
using System.Collections.ObjectModel;

namespace Player.Models
{
	[Serializable]
	public class Playlist : ObservableCollection<Media>
	{
		private readonly Collection<int> _UserOrder = new Collection<int>();

		public SerializableBitmap Thumbnail { get; set; }

		public Playlist(Collection<Media> medias, SerializableBitmap thumbnail = default) : base(medias)
		{
			Thumbnail = thumbnail;
			for (int i = 0; i < Count; i++)
				_UserOrder.Add(i);
		}
	}
}
