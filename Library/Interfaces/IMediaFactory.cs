using EPlayer.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPlayer.Library.Interfaces
{
	public interface IMediaFactory
	{
		Song SongFromPath(string filePath);
		Video VideoFromPath(string filePath);
		Media MediaFromPath(string filePath);
	}
}
