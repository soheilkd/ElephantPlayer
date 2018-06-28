using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player
{
	public static class LibraryManager
	{
		private static string LibraryPath => $"{App.Path}\\Library.dll";
		public static SerializableMediaCollection LoadedCollection;
		public static void Save(Collection<Media> medias)
		{
			SerializableMediaCollection collection;
			collection.Unordered = new ObservableCollection<Media>(medias);
			collection.ByArtist = new ObservableCollection<Media>(medias.OrderBy(each => each.Artist));
			collection.ByAlbum = new ObservableCollection<Media>(medias.OrderBy(each => each.Album));
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, collection);
		}
		public static SerializableMediaCollection Load()
		{
			if (!File.Exists(LibraryPath))
				return new SerializableMediaCollection()
				{
					Unordered = new ObservableCollection<Media>(),
					ByAlbum = new ObservableCollection<Media>(),
					ByArtist = new ObservableCollection<Media>(),
				};
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Open))
				LoadedCollection = (SerializableMediaCollection)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}
	}
}
