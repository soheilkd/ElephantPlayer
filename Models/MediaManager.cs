using Player.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Player
{
	public enum PlayMode { Shuffle, RepeatOne, Repeat }

	public class MediaManager : ObservableCollection<Media>
	{
		public event EventHandler<InfoExchangeArgs<Media>> RequestReceived;
		public MediaManager()
		{
			LibraryManager.Load().For(each => Add(each));
		}

		MediaEnumerator QueueEnumerator = new MediaEnumerator();

		public Media Current { get => QueueEnumerator.Current; }

		public void AddFromPath(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => AddFromPath(each));
			if (MediaOperator.TryLoadFromPath(path, out var media))
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
			File.Delete(media);
			this.Where(each => each.Path == media.Path).ToArray().For(each => Remove(each));
			if (reqNext)
				RequestPlay(Next());
		}
		
		public Media Play(Media media, bool isFromQueue = false)
		{
			if (!MediaOperator.DoesExists(media))
			{
				Remove(media);
				media = Next();
			}
			this.For(each => each.IsPlaying = false);
			media.IsPlaying = true;
			if (!isFromQueue)
				Requeue();
			return QueueEnumerator.Get(media);
		}
		public Media Next()
		{
			return Play(QueueEnumerator.GetNext(), true);
		}
		public Media Previous()
		{
			return Play(QueueEnumerator.GetPrevious(), true);
		}

		public void CloseSeason()
		{
			if (App.Settings.RevalidateOnExit)
				Revalidate();
			LibraryManager.Save(this);
		}

		private void RequestPlay() => RequestPlay(this[0]);
		private void RequestPlay(Media media)
		{
			if (!QueueEnumerator.Contains(media))
				Requeue();
			RequestReceived?.Invoke(this, new InfoExchangeArgs<Media>(media));
		}

		public void Revalidate()
		{
			var t = (from each in this where MediaOperator.Load(each).Type != MediaType.None select each).ToArray();
			Clear();
			t.For(each => Add(each));
			LibraryManager.Save(this);
		}

		public void Requeue(PlayMode playMode = PlayMode.Repeat)
		{
			QueueEnumerator.Clear();
			switch (playMode)
			{
				case PlayMode.Shuffle:
					Requeue(PlayMode.Repeat);
					QueueEnumerator.Shuffle();
					break;
				case PlayMode.RepeatOne:
					Extensions.Do(() => QueueEnumerator.Add(Current), 10);
					break;
				case PlayMode.Repeat:
					QueueEnumerator.Reset();
					this.For(each => QueueEnumerator.Add(each));
					break;
				default: break;
			}
		}
	}
}
