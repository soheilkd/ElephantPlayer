using System;
using System.Collections.Generic;
using System.IO;

namespace EPlayer.Library.Models
{
	[Serializable]
	public abstract class Media
	{
		public string Name { get; set; }
		public List<DateTime> Plays { get; set; } = new List<DateTime>();
		public DateTime AdditionDate { get; set; }
		public string Path { get; set; }
		public string Title { get; set; }
		public bool DoesExist => File.Exists(Path);
		public bool IsPlaying { get; set; }
		public TimeSpan Duration { get; set; }

		public Media() => AdditionDate = DateTime.Now;
		public Media(string path)
		{
			Path = path;
			ReadProperties();
		}

		public virtual bool MatchesQuery(string query)
		{
			throw new NotImplementedException();
		}

		protected virtual void ReadProperties()
		{
			throw new NotImplementedException();
		}
	}
}
