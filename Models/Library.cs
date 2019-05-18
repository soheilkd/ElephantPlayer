using Library.Extensions;
using Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Player.Models
{
	[Serializable]
	public class Library : MediaQueue
	{
		public SortedAutoDictionary<string, MediaQueue, Media> Artists { get; } = new SortedAutoDictionary<string, MediaQueue, Media>(each => each.Artist);
		public SortedAutoDictionary<string, MediaQueue, Media> Albums { get; } = new SortedAutoDictionary<string, MediaQueue, Media>(each => each.Album);
		public List<MediaQueue> Playlists { get; } = new List<MediaQueue>();

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Artists.Add(this[0]);
					Albums.Add(this[0]);
					break;
				case NotifyCollectionChangedAction.Remove:
					Media m = e.OldItems[0].To<Media>();
					Artists.Remove(m);
					Albums.Remove(m);
					Playlists.For(each => each.Remove(m));
					break;
				case NotifyCollectionChangedAction.Reset:
					Artists.Clear();
					Albums.Clear();
					Playlists.For(each => each.Clear());
					break;
				default:
					break;
			}
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.PropertyName == "Artist") Artists.Reorganize();
			if (e.PropertyName == "Album") Albums.Reorganize();
		}
	}
}
