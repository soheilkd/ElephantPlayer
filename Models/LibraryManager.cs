using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace Player
{
	public static class LibraryManager
	{
		private static string Path => App.Settings.LibraryLocation;

		public static Collection<Media> LoadedCollection;

		public static void Save(Collection<Media> medias)
		{
			var coli = new ObservableCollection<Media>(medias);
			using (var stream = new FileStream(Path, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, coli);
		}
		public static Collection<Media> Load()
		{
			if (!File.Exists(App.Settings.LibraryLocation))
				return new Collection<Media>();
			using (FileStream stream = new FileStream(Path, FileMode.Open))
				LoadedCollection = (Collection<Media>)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}

		public static void ExportAsXML(Collection<Media> medias)
		{
			using (var writer = new StreamWriter(Path))
				(new XmlSerializer(typeof(Collection<Media>))).Serialize(writer, medias);
		}
		public static Collection<Media> ImportXML()
		{
			using (XmlReader reader = XmlReader.Create(Path))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Collection<Media>));
				if ((new XmlSerializer(typeof(Collection<Media>))).CanDeserialize(reader))
					return serializer.Deserialize(reader) as Collection<Media>;
				else throw new XmlException("Determinated XML is not deserializable");
			}
		}
		
		public static void ExportAsText(Collection<Media> medias)
		{
			using (var writer = new StreamWriter(Path))
			{
				int i = 0;
				medias.For(each =>
				{
					writer.WriteLine($"$BEGIN {i} {each.Type}");
					writer.WriteLine($"Title :\t\t {each.Title}");
					writer.WriteLine($"Artist :\t\t {each.Artist}");
					writer.WriteLine($"Album :\t\t {each.Album}");
					writer.WriteLine($"Plays :\t\t {each.PlayCount}");
					writer.WriteLine($"Added :\t\t {each.AdditionDate}");
					writer.WriteLine($"Dir :\t\t {each.Directory}");
					writer.WriteLine($"Length :\t\t {each.Length.ToNewString()}");
					writer.WriteLine($"Name :\t\t {each.Name}");
					writer.WriteLine($"Path :\t\t {each.Path}");
					writer.WriteLine($"$END {i} {each.Type}");
					writer.WriteLine("__________________________");
					i++;
				});
			}
		}
	}
}
