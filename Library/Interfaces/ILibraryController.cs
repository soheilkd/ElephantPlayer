using EPlayer.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPlayer.Library.Interfaces
{
	public interface ILibraryController
	{
		Task<MediaLibrary> Load();
	}
}
