using Lastfm.Services;
using Library.Extensions;
using System;
using System.Net;
using System.Windows.Media.Imaging;

namespace Player.Web
{
    public static class API
    {
        //Session is required for API connection to LastFM 
        private static readonly Session _Session = new Session("cab344dc5414176234071148bc813382", "ef529dce9081c695dc32f31b800c7b9a");

        public static Album GetAlbum(string album, string artist = default)
        {
            try
            {
                if (artist != default)
                    return new Album(artist, album, _Session);
                return Album.Search(album, _Session).GetFirstMatch();
            }
            catch (Exception)
            {
                return default;
            }
        }
        public static Artist GetArtist(string name)
        {
            try
            {
                return Artist.Search(name, _Session).GetFirstMatch();
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static void DownloadImage(string url, Action<BitmapImage> onDone)
        {
            var client = new WebClient();
            client.DownloadDataCompleted += (_, d) => onDone(d.Result.ToBitmap());
            client.DownloadDataAsync(new Uri(url));
        }
    }
}
