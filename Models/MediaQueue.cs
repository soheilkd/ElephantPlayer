using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player.Models
{
	[Serializable]
	public class MediaQueue : ObservableCollection<Media>
	{
		private int _Position;
		public Media Current => Position < Count ? this[Position] : default;
		public int Position
		{
			get => _Position;
			set
			{
				if (value >= Count)
					value = 0;
				_Position = value;
				RefreshIsPlayings();
			}
		}

		public MediaQueue(IEnumerable<Media> collection) : base(collection) { }
		public MediaQueue() : base() { }

		public void RefreshIsPlayings()
		{
			for (int i = 0; i < Count; i++)
				this[i].IsPlaying = false;
			Current.IsPlaying = true;
		}

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

		public void Reset() => Position = 0;
		public void Dispose() => Clear();

		public MediaQueue Search(string query = default)
		{
			if (!string.IsNullOrWhiteSpace(query))
			{
				IEnumerable<Media> col = from item in this where item.MatchesQuery(query) select item;
				if (col.Count() != 0)
					return new MediaQueue(col);
			}
			return this;
		}

		public bool Contains(string path, out Media output)
		{
			output = this.Where(item => item.Path == path).FirstOrDefault();
			return output != default;
		}
		public bool Contains(string path) => this.Where(item => item.Path == path).Count() != 0;
	}
}