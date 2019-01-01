using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Library.Extensions;

namespace Player.Models
{
	[Serializable]
	public class MediaQueue : ObservableCollection<Media>
	{
		private int _Position;
		public Media Current => Position < Count ? this[Position] : new Media();

		public int Position
		{
			get => _Position;
			set
			{
				if (value >= Count)
					value = 0;
				_Position = value;
				this.For(each => each.IsPlaying = false);
				Current.IsPlaying = true;
			}
		}

		public MediaQueue(IEnumerable<Media> collection) : base(collection) { }
		public MediaQueue() : base() { }

		public void ClearIsPlayings(Media except = default)
		{
			if (except != null)
				Position = IndexOf(except);
			else
				this.For(each => each.IsPlaying = false);
		}

		public bool MoveNext()
		{
			if (++Position >= Count)
				Position = 0;
			return true;
		}
		public bool MovePrevious()
		{
			if (Position == 0)
				Position = Count - 1;
			else
				Position--;
			return true;
		}

		public Media Next()
		{
			MoveNext();
			return Current;
		}
		public Media Previous()
		{
			MovePrevious();
			return Current;
		}
		public Media Get(int index)
		{
			Position = index;
			return Current;
		}
		public Media Get(Media media)
		{
			Position = IndexOf(media);
			return Current;
		}

		public void Reset() => Position = 0;
		public void Dispose() => Clear();

		public void Shuffle()
		{
			var rand = new Random(DateTime.Now.Millisecond);
			Media[] t = this.ToArray();
			var c = Count;
			Clear();
			MiscExtensions.Repeat(() => Add(t[rand.Next(c)]), c);
		}

		public void SortBy<T>(Func<Media, T> keySelector, bool asc = true)
		{
			Media[] p = (asc ? this.OrderBy(keySelector) : this.OrderByDescending(keySelector)).ToArray();
			for (var i = 0; i < Count; i++)
				if (p[i] != this[i])
					Move(IndexOf(p[i]), i);
		}
		public MediaQueue Search(string query = "")
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				var col = from item in this where item.Matches(query) select item;
				if (col.Count() != 0)
					return new MediaQueue(col);
			}
			return this;
		}

		public void Add(string fromPath)
		{
			if (Directory.Exists(fromPath))
				Directory.GetFiles(fromPath, "*", SearchOption.AllDirectories).For(each => Add(each));
			if (Contains(fromPath, out var duplicate))
				Controller.Play(duplicate, this);
			if (Media.TryLoadFromPath(fromPath, out Media media))
				Insert(0, media);
		}
		public void Add(Collection<Media> collection)
		{
			var c = Count;
			collection.For(each => Add(each));
			if (c != Count) //Means something is added
				Controller.Play(this.First());
		}
		public void Add(string[] fromPaths)
		{
			fromPaths.For(each => Add(each));
		}

		public bool Contains(string path, out Media output)
		{
			output = this.Where(item => item.Path == path).FirstOrDefault();
			return output != default;
		}

		public string[] GetArtists()
		{
			return this.Select(each => each.Artist).Distinct().ToArray();
		}
		public string[] GetAlbums()
		{
			return this.Select(each => each.Album).Distinct().ToArray();
		}
	}
}