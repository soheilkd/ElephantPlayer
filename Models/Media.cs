using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Player
{
	public enum MediaType
	{
		None = 0,
		Music = 1,
		Video = 2
	}

	[Serializable]
	public class Media : INotifyPropertyChanged
	{
		private string _Name;
		private string _Artist;
		private string _Title;
		private string _Album;
		private string _Dir;
		private bool _IsSelected;
		private int _PlayCount;
		private bool _IsPlaying;
		private TimeSpan _Len;
		public MediaType Type;
		public string Lyrics = "";
		[field: NonSerialized] public bool IsLoaded = false;
		[field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;
		
		public string Name { get => _Name; set => Set(ref _Name, value); }
		public string Artist { get => _Artist; set => Set(ref _Artist, value); }
		public string Title { get => _Title; set => Set(ref _Title, value); }
		public string Album { get => _Album; set => Set(ref _Album, value); }
		public string Directory { get => _Dir; set => Set(ref _Dir, value); }
		public int PlayCount { get => _PlayCount; set => Set(ref _PlayCount, value); }
		public bool IsPlaying { get => _IsPlaying; set => Set(ref _IsPlaying, value); }
		public bool IsSelected { get => _IsSelected; set => Set(ref _IsSelected, value); }
		public TimeSpan Length { get => _Len; set => Set(ref _Len, value); }
		public DateTime AdditionDate { get; private set; }
		public string Path { get; set; }
		public bool IsVideo => Type == MediaType.Video;
		public bool DoesExist => File.Exists(Path);

		public Media() { }
		public Media(string path)
		{
			Path = path;
			AdditionDate = DateTime.Now;
		}
		
		protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public bool Matches(string query)
		{
			if (String.IsNullOrWhiteSpace(query)) return true;
			return
				Title.IncaseContains(query) ||
				Name.IncaseContains(query) ||
				Artist.IncaseContains(query) ||
				Album.IncaseContains(query) ||
				Path.IncaseContains(query);
		}

		public void MoveTo(string dir)
		{
			dir += Name;
			File.Move(Path, dir);
			Path = dir;
		}
		public void CopyTo(string dir)
		{
			dir += Name;
			File.Copy(Path, dir, true);
		}

		#region Load
		private static readonly string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
		private static readonly string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');

		private static MediaType GetMediaType(string path)
		{
			var ext = path.Substring(path.LastIndexOf('.') + 1).ToLower();
			if (SupportedMusics.Contains(ext)) return MediaType.Music;
			else if (SupportedVideos.Contains(ext)) return MediaType.Video;
			else return MediaType.None;
		}

		public void Load()
		{
			if (IsLoaded)
				return;
			if (!DoesExist)
			{
				Type = MediaType.None;
				return;
			}
			Name = Path.Substring(Path.LastIndexOf("\\") + 1);
			Directory = Path.Substring(0, Path.LastIndexOf("\\"));
			switch (Type = GetMediaType(Path))
			{
				case MediaType.Music:
					using (var t = TagLib.File.Create(Path))
					{
						Artist = t.Tag.FirstPerformer ?? Path.Substring(0, Path.LastIndexOf("\\"));
						Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
						Album = t.Tag.Album ?? String.Empty;
						Lyrics = t.Tag.Lyrics ?? String.Empty;
					}
					break;
				case MediaType.Video:
					Title = Name;
					Artist = Path.Substring(0, Path.LastIndexOf("\\"));
					Artist = Artist.Substring(Artist.LastIndexOf("\\") + 1);
					Album = "Video";
					break;
				default: break;
			}
			IsLoaded = true;
		}
		public void Reload()
		{
			IsLoaded = false;
			Load();
		}
		#endregion
		public void CleanTag(bool prompt = true)
		{
			string deleteWord(string target, string word)
			{
				string output = "";
				int lit1 = target.ToLower().IndexOf(word);
				if (target.StartsWith(word)) lit1 = 0;
				if (lit1 == -1) return target;
				string temp1 = target.Substring(0, lit1);
				if (temp1 == String.Empty) temp1 = " ";
				if (temp1.LastIndexOf(' ') == -1) return " ";
				string temp2 = temp1.Substring(0, temp1.LastIndexOf(' '));
				output += temp2;
				int lit2 = target.ToLower().LastIndexOf(word);
				temp1 = target.Substring(lit2);
				if (!temp1.EndsWith(temp1)) temp2 = temp1.Substring(temp1.IndexOf(' '));
				else temp2 = "";
				output += temp2;
				return output;
			}
			string c(string target)
			{
				if (String.IsNullOrWhiteSpace(target)) return target;
				string[] litretures = ".com;.ir;.org;www.;@;.me;.biz;.net".Split(';');
				var lit = new List<string>();
				litretures.For(each => lit.Add(each), each => target.IncaseContains(each));
				lit.ToArray().For(each => target = deleteWord(target, each));
				return target;
			}

			using (var file = TagLib.File.Create(Path))
			{
				string[] form(TagLib.Tag tag2)
				{
					return new string[]
					{
						tag2.Album,
						tag2.Comment,
						tag2.FirstComposer,
						tag2.Conductor,
						tag2.Copyright,
						tag2.FirstGenre,
						tag2.FirstPerformer,
						tag2.Title
					};
				}
				var tag = file.Tag;
				var manip1 = form(tag);
				tag.Album = c(tag.Album ?? " ");
				tag.Comment = "";
				tag.Composers = new string[] { c(tag.FirstComposer ?? " ") };
				tag.Conductor = c(tag.Conductor ?? " ");
				tag.Copyright = "";
				tag.Genres = new string[] { c(tag.FirstGenre ?? " ") };
				tag.Performers = new string[] { c(tag.FirstPerformer ?? " ") };
				tag.Title = c(tag.Title ?? " ");
				var manip2 = form(tag);
				var manip = "Detergent will change these values: \r\n";
				if (manip1[0] != manip2[0]) manip += "Album: " + manip1[0] + " => " + manip2[0] + "\r\n";
				if (manip1[1] != manip2[1]) manip += "Comment: " + manip1[1] + " => " + manip2[1] + "\r\n";
				if (manip1[2] != manip2[2]) manip += "Composer: " + manip1[2] + " => " + manip2[2] + "\r\n";
				if (manip1[3] != manip2[3]) manip += "Conductor: " + manip1[3] + " => " + manip2[3] + "\r\n";
				if (manip1[4] != manip2[4]) manip += "Copyright: " + manip1[4] + " => " + manip2[4] + "\r\n";
				if (manip1[5] != manip2[5]) manip += "Genre: " + manip1[5] + " => " + manip2[5] + "\r\n";
				if (manip1[6] != manip2[6]) manip += "Artist: " + manip1[6] + " => " + manip2[6] + "\r\n";
				if (manip1[7] != manip2[7]) manip += "Title: " + manip1[7] + " => " + manip2[7] + "\r\n";
				if (manip.Length <= 42)
				{
					MessageBox.Show("Couldn't find any changable thing", "JIZZZ", MessageBoxButton.OK, MessageBoxImage.Asterisk);
					return;
				}
				manip += "\r\nContinue?";
				if (prompt)
				{
					var res = MessageBox.Show(manip, "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
					if (res == MessageBoxResult.No) return;
				}
				file.Save();
				Reload();
			}
		}

		public static bool TryLoadFromPath(string path, out Media media)
		{
			media = new Media(path);
			if (!media.DoesExist)
				return false;
			media.Load();
			return media.Type != MediaType.None;
		}

		public static implicit operator Uri(Media media) => new Uri(media.Path);
		public static implicit operator MediaType(Media media) => media.Type;
	}
}
