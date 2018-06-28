using System;
using System.Collections.ObjectModel;

namespace Player
{
	[Serializable]
	public struct SerializableMediaCollection
	{
		public ObservableCollection<Media> Unordered;
		public ObservableCollection<Media> ByArtist;
		public ObservableCollection<Media> ByAlbum;
	}
}
