using EPlayer.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPlayer.Library.Abstracts
{
	[Serializable]
	public abstract class LibraryControllerBase
	{
		public event InfoExchangeHandler<(MediaQueue, Media)> PlayRequest;
		public MediaQueue Queue { get; set; } = new MediaQueue();

		public MediaLibrary LoadedLibrary { get; private set; }

		protected virtual MediaLibrary Load()
		{
			throw new NotImplementedException();
		}
		protected virtual void Unload()
		{
			throw new NotImplementedException();
		}

		public virtual void Play(Media media, MediaQueue queue = default)
		{
			if (queue != null)
				Queue = queue;
			else if (!Queue.Contains(media))
				Queue.Add(media);
			PlayRequest?.Invoke(this, (Queue, media));
		}
	}
}
