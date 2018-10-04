using Player.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player.Models
{
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
			Random rand = new Random(DateTime.Now.Millisecond);
			Media[] t = this.ToArray();
			int c = Count;
			Clear();
			MiscExtensions.Repeat(() => Add(t[rand.Next(c)]), c);
		}

		public void SortBy<T>(Func<Media, T> keySelector, bool asc = true)
		{
			Media[] p = (asc ? this.OrderBy(keySelector) : this.OrderByDescending(keySelector)).ToArray();
			for (int i = 0; i < Count; i++)
				if (p[i] != this[i])
					Move(IndexOf(p[i]), i);
		}
	}
}