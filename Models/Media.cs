using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Player
{
	[Flags]
	public enum MediaType
	{
		None = 0B0,
		File = 0B10,
		Music = File | 0B100,
		Video = File | 0B1000,
		OnlineFile = File | 0B10000,
		OnlineMusic = Music | OnlineFile,
		OnlineVideo = Video | OnlineFile
	}

	[Serializable]
	public class Media : INotifyPropertyChanged
	{
		protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _Name;
		private string _Artist;
		private string _Title;
		private string _Album;
		private string _Dir;
		private int _PlayCount;
		private bool _IsPlaying;
		private TimeSpan _Len;
		public string Name { get => _Name; set => Set(ref _Name, value); }
		public string Artist { get => _Artist; set => Set(ref _Artist, value); }
		public string Title { get => _Title; set => Set(ref _Title, value); }
		public string Album { get => _Album; set => Set(ref _Album, value); }
		public string Directory { get => _Dir; set => Set(ref _Dir, value); }
		public int PlayCount { get => _PlayCount; set => Set(ref _PlayCount, value); }
		public bool IsPlaying { get => _IsPlaying; set => Set(ref _IsPlaying, value); }
		public TimeSpan Length { get => _Len; set => Set(ref _Len, value); }
		public DateTime AdditionDate { get; private set; }
		public Uri Url { get; set; }
		public bool IsOffline => Url.IsFile;
		public MediaType Type;
		public string Path => Url.IsFile ? Url.LocalPath : Url.AbsoluteUri;
		public bool IsVideo => Type == MediaType.Video;
		[field: NonSerialized] public string Lyrics = "";
		[field: NonSerialized] public bool IsLoaded = false;
		[field: NonSerialized] public BitmapSource Artwork;
		[field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

		public Media() { }
		public Media(Uri url)
		{
			//Just because defualt .Net webClient doesn't support https protocol
			if (!url.IsFile && url.AbsoluteUri[4] == 's')
				Url = new Uri(url.AbsoluteUri.Remove(4, 1));
			else Url = url;
			Url = url;
			AdditionDate = DateTime.Now;
		}
		public Media(string path) : this(new Uri(path, true)) { }

		public override string ToString() => $"{Artist} - {Title}";
	}
}
