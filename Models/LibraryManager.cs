using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player
{
	public static class LibraryManager
	{
		private static string LibraryPath => $"{App.Path}\\Library.bin";
		public static Collection<Media> LoadedCollection;

		public static void Save(Collection<Media> medias)
		{
			var coli = new ObservableCollection<Media>(medias);
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, coli);
		}
		public static Collection<Media> Load()
		{
			if (!File.Exists(LibraryPath))
				return new Collection<Media>();
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Open))
				LoadedCollection = (Collection<Media>)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}
	}
}
