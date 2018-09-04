using Player.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player.Library
{
	public static class LibraryManager
	{
		private static string Path => Settings.Current.LibraryLocation;

		public static Collection<Media> LoadedCollection;

		public static Collection<Media> Load()
		{
			if (!File.Exists(Settings.Current.LibraryLocation))
				return new Collection<Media>();
			using (FileStream stream = new FileStream(Path, FileMode.Open))
				LoadedCollection = (Collection<Media>)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}

		public static void Save(Collection<Media> medias)
		{
			ObservableCollection<Media> coli = new ObservableCollection<Media>(medias);
			using (FileStream stream = new FileStream(Path, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, coli);
		}

		public static bool TryLoad(string path, out Collection<Media> output)
		{
			string last = Settings.Current.LibraryLocation;
			Settings.Current.LibraryLocation = path;
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
				Settings.Current.LibraryLocation = last;
			}
		}
	}
}