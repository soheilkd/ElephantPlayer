using Player.Extensions;
using Player.Library;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Player.Models
{
	public class MediaManager : ObservableCollection<Media>
	{
		public event EventHandler<InfoExchangeArgs<Media>> RequestReceived;
		public MediaManager()
		{
			LibraryManager.Load().For(each => Add(each));
			QueueEnumerator.Filter(this, String.Empty);
			CollectionChanged += (_,__) => QueueEnumerator.Filter(this);
		}

		public MediaEnumerator QueueEnumerator = new MediaEnumerator();

		public Media Current { get => QueueEnumerator.Current; }

		public void AddFromPath(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => AddFromPath(each));
			if (Media.TryLoadFromPath(path, out var media))
			{
				var duplication = this.Where(item => item.Path == path);
				if (duplication.Count() != 0 && requestPlay)
				{
					RequestPlay(duplication.First());
					return;
				}
				Insert(0, media);
				if (requestPlay)
					RequestPlay();
			}
		}

		public void Delete(Media media)
		{
			bool reqNext = Current == media;
			File.Delete(media.Path);
			this.Where(each => each.Path == media.Path).ToArray().For(each => Remove(each));
			if (reqNext)
				RequestPlay(Next());
		}

		public Media Next(Media media)
		{
			this.For(each => each.IsPlaying = false);
			media.IsPlaying = true;
			return QueueEnumerator.Get(media);
		}
		public Media Next()
		{
			return Next(QueueEnumerator.GetNext());
		}
		public Media Previous()
		{
			return Next(QueueEnumerator.GetPrevious());
		}

		public void CloseSeason()
		{
			if (Settings.Current.RevalidateOnExit)
				Revalidate();
			LibraryManager.Save(this);
		}

		private void RequestPlay() => RequestPlay(this[0]);
		private void RequestPlay(Media media)
		{
			if (!QueueEnumerator.Contains(media))
				QueueEnumerator = new MediaEnumerator(this);
			RequestReceived?.Invoke(this, new InfoExchangeArgs<Media>(media));
		}

		public void Revalidate()
		{
			this.For(each => each.Reload());
			var t = (from each in this where each.Type != MediaType.None select each).ToArray();
			Clear();
			t.For(each => Add(each));
			LibraryManager.Save(this);
		}

		public void SortQueueBy<T>(Func<Media, T> keySelector, bool asc = true)
			=> QueueEnumerator.SortBy(keySelector, asc);
	}
}
