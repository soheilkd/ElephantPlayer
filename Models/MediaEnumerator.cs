using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player.Models
{
	class MediaEnumerator : ObservableCollection<Media>, IEnumerator<Media>
	{
		private int _current;
		public Media Current => this[_current];
		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			_current++;
			if (_current == Count)
				_current = 0;
			return true;
		}
		public bool MovePrevious()
		{
			_current--;
			if (_current <= -1)
				_current = Count - 1;
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
			_current = index;
			return Current;
		}
		public Media Get(Media media)
		{
			_current = IndexOf(media);
			return Current;
		}

		public void Reset() => _current = 0;
		public void Dispose() => Clear();
		
		public void RemoveThose(Func<Media, bool> filter)
		{
			this.Where(filter).AsParallel().ForAll(each => Remove(each));
		}
		public void KeepThose(Func<Media, bool> filter)
		{
			var t = this.Where(filter);
			Clear();
			foreach (var item in t)
				Add(item);
			t = null;
		}

		public void Shuffle()
		{
			Random rand = new Random(DateTime.Now.Millisecond);
			var t = this.ToArray();
			var c = Count;
			Clear();
			Extensions.Do(() => Add(t[rand.Next(c)]), c);
		}
	}
}
