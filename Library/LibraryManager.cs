using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Library;
using Library.Extensions;
using Player.Models;

namespace Player
{
	public static class LibraryManager
	{
		private static Lazy<MediaQueue> _LazyData = new Lazy<MediaQueue>(Load);
		public static MediaQueue Data { get => _LazyData.Value; }
		public static event InfoExchangeHandler<Media> MediaRequested;

		public static bool Contains(string path, out Media output)
		{
			output = Data.Where(item => item.Path == path).FirstOrDefault();
			return output != default;
		}
		public static void AddFromPath(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => AddFromPath(each));
			if (Contains(path, out var duplicate))
				MediaRequested.Invoke(duplicate);
			if (Media.TryLoadFromPath(path, out Media media))
			{
				Data.Insert(0, media);
				if (requestPlay)
					MediaRequested.Invoke(Data.First());
			}
		}

		public static void SortBy<T>(Func<Media, T> keySelector, bool asc = true)
		{
			Media[] p = (asc ? Data.OrderBy(keySelector) : Data.OrderByDescending(keySelector)).ToArray();
			for (var i = 0; i < Data.Count; i++)
				if (p[i] != Data[i])
					Data.Move(Data.IndexOf(p[i]), i);
		}

		public static MediaQueue Filter(string query = "")
		{
			return string.IsNullOrWhiteSpace(query) || Data.AsParallel().Where(each => each.Matches(query)).Count() == 0 //dedicates nothing found or empty query
				? Data : new MediaQueue(Data.Where(each => each.Matches(query)));
		}

		public static void Save()
		{
			using (var stream = new FileStream(Settings.LibraryLocation, FileMode.Create))
				new BinaryFormatter().Serialize(stream, Data);
		}

		private static MediaQueue Load()
		{
			if (!File.Exists(Settings.LibraryLocation))
				return new MediaQueue();
			using (var stream = new FileStream(Settings.LibraryLocation, FileMode.Open))
				return (MediaQueue)new BinaryFormatter().Deserialize(stream);
		}
	}
}