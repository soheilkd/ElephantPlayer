using System;
using Lastfm.Services;

namespace Player.Web
{
	public static class API
	{
		private static readonly Session _Session = new Session("cab344dc5414176234071148bc813382", "ef529dce9081c695dc32f31b800c7b9a");

		public static string GetAlbumArtworkUrl(string artist, string album)
		{
			try
			{
				return new Album(artist, album, _Session).GetImageURL();
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
				return new Artist(name, _Session).GetImageURL();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
