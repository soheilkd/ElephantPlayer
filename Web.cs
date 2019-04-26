using Lastfm.Services;
using System;
using System.Net;

namespace Player
{
	public static class Web
	{
		//Session is required for API connection to LastFM 
		private static readonly Session _Session = new Session("cab344dc5414176234071148bc813382", "ef529dce9081c695dc32f31b800c7b9a");

		public static Artist GetArtist(string name) => new Artist(name, _Session);
		public static bool TryGetArtist(string name, out Artist artist)
		{
			try
			{
				artist = new Artist(name, _Session);
				return artist != null;
			}
			catch (Exception)
			{
				artist = null;
				return false;
			}
		}
		public static byte[] GetArtistImage(string name)
		{
			return null;
		}

		public static Album GetAlbum(string album, string artist = default)
		{
			if (artist != default)
				return new Album(artist, album, _Session);
			return Album.Search(album, _Session).GetFirstMatch();
		}
		public static bool TryGetAlbum(string name, out Album album)
		{
			try
			{
				album = GetAlbum(name);
				return album != null;
			}
			catch (Exception)
			{
				album = null;
				return false;
			}
		}
		public static byte[] GetAlbumImage(string name)
		{
			return null;
		}

		public static byte[] Download(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;
			var client = new WebClient();
			try { return client.DownloadData(url); }
			catch (Exception) { return new byte[0]; }
		}
	}
}
