﻿using Player.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player.Models
{
	public class MediaEnumerator : ObservableCollection<Media>, IEnumerator<Media>
	{
		private int _position;
		public Media Current => _position < Count ? this[_position] : new Media();
		object IEnumerator.Current => Current;

		public MediaEnumerator(IEnumerable<Media> collection) : base(collection) { }
		public MediaEnumerator() : base() { }

		public bool MoveNext()
		{
			_position++;
			if (_position == Count)
				_position = 0;
			return true;
		}
		public bool MovePrevious()
		{
			_position--;
			if (_position <= -1)
				_position = Count - 1;
			return true;
		}

		public Media GetNext()
		{
			MoveNext();
			return Current;
		}
		public Media GetPrevious()
		{
			MovePrevious();
			return Current;
		}
		public Media Get(int index)
		{
			_position = index;
			return Current;
		}
		public Media Get(Media media)
		{
			_position = IndexOf(media);
			return Current;
		}

		public void Reset() => _position = 0;
		public void Dispose() => Clear();

		public void Shuffle()
		{
			Random rand = new Random(DateTime.Now.Millisecond);
			var t = this.ToArray();
			var c = Count;
			Clear();
			MiscExtensions.Repeat(() => Add(t[rand.Next(c)]), c);
		}

		private string _LastQuery;
		public void Filter(IEnumerable<Media> originalCollection, string query = "")
		{
			_LastQuery = query;
			if (originalCollection.AsParallel().Where(each => each.Matches(query)).Count() == 0)
			{
				Filter(originalCollection, String.Empty);
				return;
			}
			var c = Current;
			ClearItems(); 
			foreach (var item in originalCollection.Where(each => each.Matches(query)))
				 Add(item);
			_position = IndexOf(c) != -1 ? IndexOf(c) : 0;
		}

		public void SortBy<T>(Func<Media, T> keySelector, bool asc = true)
		{
			var p = this.ToArray();
			Clear();
			if (asc) p.OrderBy(keySelector).ForEach(each => Add(each));
			else p.OrderByDescending(keySelector).ForEach(each => Add(each));
			p = null;
		}
	}
}