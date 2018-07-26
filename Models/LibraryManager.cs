using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player
{
	public static class LibraryManager
	{
		private static string Path => App.Settings.LibraryLocation;

		public static Collection<Media> LoadedCollection;

		public static Collection<Media> Load()
		{
			if (!File.Exists(App.Settings.LibraryLocation))
				return new Collection<Media>();
			using (FileStream stream = new FileStream(Path, FileMode.Open))
				LoadedCollection = (Collection<Media>)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}
		
		public static void Save(Collection<Media> medias)
		{
			var coli = new ObservableCollection<Media>(medias);
			using (var stream = new FileStream(Path, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, coli);
		}

		public static bool TryLoad(string path, out Collection<Media> output)
		{
			var last = App.Settings.LibraryLocation;
			App.Settings.LibraryLocation = path;
			try
			{
				output = Load();
				return true;
			}
			catch
			{
				output = null;
				return false;
			}
			finally
			{
				App.Settings.LibraryLocation = last;
			}
		}
	}
}