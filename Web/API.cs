using System;
using Lastfm.Services;

namespace Player.Web
{
	public static class API
	{
		private static readonly Session _Session = new Session("cab344dc5414176234071148bc813382", "ef529dce9081c695dc32f31b800c7b9a");

		public static string GetAlbumArtworkUrl(string album)
		{
			try
			{
				return Album.Search(album, _Session).GetFirstMatch().GetImageURL();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		public static string GetArtistImageUrl(string name)
		{
			try
			{
				return Artist.Search(name, _Session).GetFirstMatch().GetImageURL();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
