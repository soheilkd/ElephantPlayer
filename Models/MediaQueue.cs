using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player.Models
{
    [Serializable]
    public class MediaQueue<T> : ObservableCollection<T> where T : Media
    {
        private int _Position;
        public T Current => Position < Count ? this[Position] : default;
        public new T this[int index]
        {
            get
            {
                Position = index;
                return Current;
            }
            set => base[index] = value;
        }
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

        public MediaQueue(IEnumerable<T> collection) : base(collection) { }
        public MediaQueue() : base() { }

        public void ClearIsPlayings(T except = default)
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

        public T Next()
        {
            MoveNext();
            return Current;
        }
        public T Previous()
        {
            MovePrevious();
            return Current;
        }

        public void Reset() => Position = 0;
        public void Dispose() => Clear();

        public MediaQueue<T> Search(string query = default)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                IEnumerable<T> col = from item in this where item.MatchesQuery(query) select item;
                if (col.Count() != 0)
                    return new MediaQueue<T>(col);
            }
            return this;
        }

        public bool Contains(string path, out T output)
        {
            output = this.Where(item => item.Path == path).FirstOrDefault();
            return output != default;
        }
        public bool Contains(string path) => this.Where(item => item.Path == path).Count() != 0;
    }

    //Without <T>, it would be MediaQueue<Media> as default
    public class MediaQueue : MediaQueue<Media> { }
}