using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Player.Extensions;
using Player.Models;

namespace Player.Controllers
{
	public class LibraryController : ObservableCollection<Media>
	{
		private MediaQueue Data;
		public event EventHandler<InfoExchangeArgs<Media>> MediaRequested;

		public LibraryController()
		{
			Load().For(each => Add(each));
			Data = new MediaQueue(this);
		}

		public void AddFromPath(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => AddFromPath(each));
			if (Media.TryLoadFromPath(path, out Media media))
			{
				System.Collections.Generic.IEnumerable<Media> duplication = this.Where(item => item.Path == path);
				if (duplication.Count() != 0 && requestPlay)
				{
					MediaRequested?.Invoke(this, new InfoExchangeArgs<Media>(duplication.First()));
					return;
				}
				Insert(0, media);
				Data.Insert(0, media);
				if (requestPlay)
					MediaRequested?.Invoke(this, new InfoExchangeArgs<Media>(this.First()));
			}
		}

		public void SortBy<T>(Func<Media, T> keySelector, bool asc = true)
		{
			Media[] p = (asc ? this.OrderBy(keySelector) : this.OrderByDescending(keySelector)).ToArray();
			for (var i = 0; i < Count; i++)
				if (p[i] != this[i])
					Move(IndexOf(p[i]), i);
		}

		public void Filter(string query = "")
		{
			if (Data.AsParallel().Where(each => each.Matches(query)).Count() == 0)
			{
				Filter(string.Empty);
				return;
			}
			ClearItems();
			foreach (Media item in Data.Where(each => each.Matches(query)))
				Add(item);
		}

		public void Delete(Media media)
		{
			Remove(media);
			Data.Remove(media);
			File.Delete(media.Path);
		}

		public void CloseSeason()
		{
			if (Settings.RevalidateOnExit)
				Revalidate();
			Save(this);
		}

		public void Revalidate()
		{
			this.For(each => each.Reload());
			Media[] t = (from each in this where each.Type != MediaType.None select each).ToArray();
			Clear();
			t.For(each => Add(each));
			Save(this);
		}

		#region Static
		public static Collection<Media> LoadedCollection;

		public static Collection<Media> Load()
		{
			if (!File.Exists(Settings.LibraryLocation))
				return new Collection<Media>();
			using (var stream = new FileStream(Settings.LibraryLocation, FileMode.Open))
				LoadedCollection = (Collection<Media>)new BinaryFormatter().Deserialize(stream);
			return LoadedCollection;
		}

		public static void Save(Collection<Media> medias)
		{
			var coli = new ObservableCollection<Media>(medias);
			using (var stream = new FileStream(Settings.LibraryLocation, FileMode.Create))
				new BinaryFormatter().Serialize(stream, coli);
		}

		public static bool TryLoad(string path, out Collection<Media> output)
		{
			var last = Settings.LibraryLocation;
			Settings.LibraryLocation = path;
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
				Settings.LibraryLocation = last;
			}
		}
		#endregion

		public static implicit operator MediaQueue(LibraryController controller)
		{
			return controller.Data;
		}
	}
}