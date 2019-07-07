using System;

namespace EPlayer.Library.Models
{
	public class Video : Media
	{
		public Video(string path) : base(path) { }

		protected override void ReadProperties()
		{
			Title = Name;
		}
	}
}
