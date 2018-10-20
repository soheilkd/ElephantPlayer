using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Player.Extensions;
using Player.Models;

namespace Player.Controllers
{
	public static class Library
	{
		private static Lazy<MediaQueue> _LazyData = new Lazy<MediaQueue>(Load);
		public static MediaQueue Data { get => _LazyData.Value; }
		public static event EventHandler<InfoExchangeArgs<Media>> MediaRequested;

		public static void AddFromPath(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => AddFromPath(each));
			if (Media.TryLoadFromPath(path, out Media media))
			{
				System.Collections.Generic.IEnumerable<Media> duplication = Data.Where(item => item.Path == path);
				if (duplication.Count() != 0 && requestPlay)
				{
					MediaRequested?.Invoke(default, new InfoExchangeArgs<Media>(duplication.First()));
					return;
				}
				Data.Insert(0, media);
				if (requestPlay)
					MediaRequested?.Invoke(default, new InfoExchangeArgs<Media>(Data.First()));
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