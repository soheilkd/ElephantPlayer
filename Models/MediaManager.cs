using Player.Events;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Player
{
	public enum PlayMode { Shuffle, RepeatOne, Repeat }

	public class MediaManager : ObservableCollection<Media>
	{ 
		public MediaManager()
		{
			LibraryManager.Load().Unordered.For(each => Add(each));
		}

		public Collection<Media> Queue;
		private Random Shuffle = new Random(DateTime.Now.Millisecond);
		public Media CurrentlyPlaying;
		public event EventHandler<InfoExchangeArgs<Media>> RequestReceived;

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
			bool reqNext = CurrentlyPlaying == media;
			File.Delete(media);
			this.Where(each => each.Path == media.Path).ToArray().For(each => Remove(each));
			if (reqNext)
				RequestPlay(Next());
		}

		public Media Play(int index = 0, Collection<Media> collection = null)
		{
			return Play((collection ?? this)[index]);
		}
		public Media Play(Media media)
		{
			this.For(each => each.IsPlaying = false);
			media.IsPlaying = true;
			CurrentlyPlaying = media;
			return media;
		}
		public Media Next(Collection<Media> coll = null, int currentlyPlayingIndex = -1)
		{
			if (currentlyPlayingIndex == -1)
				currentlyPlayingIndex = (coll ?? this).IndexOf(CurrentlyPlaying);
			switch (App.Settings.PlayMode)
			{
				case PlayMode.Shuffle: return Play(Shuffle.Next(0, Count));
				case PlayMode.Repeat: return Play(currentlyPlayingIndex == Count - 1 ? 0 : ++currentlyPlayingIndex);
				case PlayMode.RepeatOne: return Play(currentlyPlayingIndex);
				default: return null;
			}
		}
		public Media Previous(Collection<Media> coll = null, int currentlyPlayingIndex = -1)
		{
			if (currentlyPlayingIndex == -1)
				currentlyPlayingIndex = (coll ?? this).IndexOf(CurrentlyPlaying);
			switch (App.Settings.PlayMode)
			{
				case PlayMode.Shuffle: return Play(Shuffle.Next(0, Count));
				case PlayMode.Repeat: return Play(currentlyPlayingIndex == 0 ? Count - 1 : --currentlyPlayingIndex);
				case PlayMode.RepeatOne: return Play(currentlyPlayingIndex);
				default: return null;
			}
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
			CurrentlyPlaying = media;
			RequestReceived?.Invoke(this, new InfoExchangeArgs<Media>(media));
			if (App.Settings.ExplicitContent)
			{
				if (media.Title.ToLower().StartsWith("spankbang"))
					Remove(media);
			}
		}

		public void Revalidate()
		{
			this.For(each => MediaOperator.Reload(each));
			this.For(each => Remove(each), each => !MediaOperator.DoesExists(each));
		}

	}
}
