using System;
using Player.Models;

namespace Player
{
	public class QueueEventArgs : EventArgs
	{
		public MediaQueue Queue { get; set; }
		public Media Media { get; set; }

		public QueueEventArgs(MediaQueue queue, Media media = default)
		{
			Queue = queue;
			Media = media;
		}
	}
}
