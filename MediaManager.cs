using Player.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Player
{
    public enum MediaType { Music, Video, File, OnlineMusic, OnlineVideo, OnlineFile, None }
    [Serializable]
    public class Media : INotifyPropertyChanged
    {
        public Media() { }
        public string _Name;
        public string Name { get => _Name; set { _Name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }
        public string _Artist;
        public string Artist { get => _Artist; set { _Artist = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Artist))); } }
        public string _Title;
        public string Title { get => _Title; set { _Title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); } }
        public string _Album;
        public string Album { get => _Album; set { _Album = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Album))); } }
        public Uri Url { get; set; }
        public int _Rate;
        public int Rate { get => _Rate; set { _Rate = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rate))); } }
        public TimeSpan _Len;
        public TimeSpan Length { get => _Len; set { _Len = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length))); } }
        public int _PlayCount;
        public int PlayCount { get => _PlayCount; set { _PlayCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayCount))); } }
        public bool IsOffline => (int)Type <= 2;
        public MediaType Type;
        public string Path => Url.IsFile ? Url.LocalPath : Url.AbsoluteUri;
        [NonSerialized] public string Lyrics;
        [NonSerialized] public bool IsLoaded = false;
        [NonSerialized] public bool IsPlaying = false;
        [NonSerialized] public System.Windows.Media.Imaging.BitmapSource Artwork;
        public bool IsVideo => Type == MediaType.Video || Type == MediaType.OnlineVideo;
        public string Ext
        {
            get
            {
                if (IsOffline)
                    return Path.Substring((Path ?? " . ").LastIndexOf('.') + 1).ToLower();
                else
                    return Url.Segments.Last().Substring(Url.Segments.Last().LastIndexOf('.') + 1).ToLower();
            }
        }

        public bool IsValid
        {
            get
            {
                if (IsOffline)
                    return File.Exists(Path);
                else
                {
                    if (!IsLoaded)
                        return true;
                    var request = (HttpWebRequest)WebRequest.Create(Path);
                    request.AddRange(0, 10);
                    try
                    {
                        request.Timeout = 5000;
                        var response = request.GetResponse();
                        Thread.Sleep(1);
                        if (!response.ContentType.EndsWith("octet-stream") && !response.ContentType.StartsWith("video") && !response.ContentType.StartsWith("app"))
                        {
                            MessageBox.Show("Requested Uri is not a valid octet-stream", ".NET", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                        response.Dispose();
                        response = null;
                    }
                    catch (WebException e)
                    {
                        MessageBox.Show(e.Message);
                        return false;
                    }
                    return true;
                }
            }
        }

        private static string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
        private static string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
        private static string[] SupportedFiles = "zip;rar;bin;dat".Split(';');
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public Media(string path) : this(new Uri(path)) { }
        public Media(Uri url)
        {
            Url = url;
            Load();
        }
        public override string ToString() => $"{Artist} - {Title}";

        public void MoveTo(string dir)
        {
            dir += Name;
            File.Move(Path, dir);
            Url = new Uri(dir);
        }
        public void CopyTo(string dir)
        {
            dir += Name;
            File.Copy(Path, dir, true);
        }
        public void Load()
        {
            if (IsLoaded)
                return;
            if (Url.IsFile)
            {
                MediaType type;
                if (SupportedMusics.Contains(Ext))
                    type = MediaType.Music;
                else if (SupportedVideos.Contains(Ext))
                    type = MediaType.Video;
                else if (SupportedFiles.Contains(Ext))
                    type = MediaType.File;
                else
                    type = MediaType.None;

                switch (type)
                {
                    case MediaType.Music:
                        using (var t = TagLib.File.Create(Path))
                        {
                            Name = Path.Substring(Path.LastIndexOf("\\") + 1);
                            Artist = t.Tag.FirstPerformer ?? Path.Substring(0, Path.LastIndexOf("\\"));
                            Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
                            Album = t.Tag.Album ?? String.Empty;
                            Artwork = t.Tag.Pictures.Length >= 1 ? Imaging.Get.BitmapSource(t.Tag.Pictures[0]) : Imaging.Images.MusicArt;
                            Type = MediaType.Music;
                            Lyrics = t.Tag.Lyrics ?? String.Empty;
                            IsLoaded = true;
                        }
                        break;
                    case MediaType.Video:
                        Name = Path.Substring(Path.LastIndexOf("\\") + 1);
                        Title = Name;
                        Artist = Path.Substring(0, Path.LastIndexOf("\\"));
                        Artist = Artist.Substring(Artist.LastIndexOf("\\") + 1);
                        Album = "Video";
                        Artwork = Imaging.Images.VideoArt;
                        Type = MediaType.Video;
                        IsLoaded = true;
                        Length = TimeSpan.Zero;
                        break;
                    case MediaType.None:
                        Name = null;
                        Title = null;
                        Artist = null;
                        Album = null;
                        Artwork = null;
                        Length = TimeSpan.Zero;
                        IsLoaded = false;
                        Type = MediaType.None;
                        break;
                    default: break;
                }
            }
            else
            {
                Type = MediaType.OnlineFile;
                if (Url.AbsoluteUri.StartsWith("https://"))
                    Url = new Uri(Url.AbsoluteUri.Replace("https://", "http://"));
                Name = Uri.UnescapeDataString(Url.Segments.Last());
                Title = Name;
                Url = Url;
                Artist = Url.Host;
                Album = "Cloud";
                Artwork = Imaging.Images.NetArt;
                if (SupportedMusics.Contains(Ext))
                    Type = MediaType.OnlineMusic;
                else if (SupportedVideos.Contains(Ext))
                    Type = MediaType.OnlineVideo;
                else if (SupportedFiles.Contains(Ext))
                    Type = MediaType.OnlineFile;
                else
                    Type = MediaType.None;
                IsLoaded = true;
            }
        }
        public void Reload()
        {
            IsLoaded = false;
            Load();
        }
        public static bool IsMedia(string path) => new Media(path).IsValid;
        public Media Shallow => MemberwiseClone() as Media;
    }

    public enum PlayMode { Shuffle, RepeatOne, RepeatAll }
    public class MediaManager: ObservableCollection<Media>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
           
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < VariousSources.Length; i++)
                            VariousSources[i].Insert(e.NewStartingIndex, e.NewItems[0] as Media);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < VariousSources.Length; i++)
                            VariousSources[i].Remove(e.OldItems[0] as Media);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < VariousSources.Length; i++)
                        VariousSources[i][e.OldStartingIndex] = e.NewItems[0] as Media;
                        break;
                case NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < VariousSources.Length; i++)
                        VariousSources[i].Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    for (int i = 0; i < VariousSources.Length; i++)
                        VariousSources[i].Clear();
                    break;
                default:
                    break;
            }
        }
        public MediaManager() { }
        
        public ObservableCollection<Media> ActiveQueue { get; set; } 
        private Random Shuffle = new Random(DateTime.Now.Millisecond);
        public Media CurrentlyPlaying => 
            ActiveQueue[CurrentlyPlayingIndex];
        public event EventHandler<InfoExchangeArgs> Change;
        private int _CurrentlyPlayingIndex = 0;
        public int CurrentlyPlayingIndex
        {
            get => _CurrentlyPlayingIndex;
            private set
            {
                _CurrentlyPlayingIndex = value;
                this.AsParallel().ForAll(item => item.IsPlaying = false);
                ActiveQueue[value].IsPlaying = true;
            }
        }
        public bool IsFiltered => VariousSources[0].Count == Count;
        public ObservableCollection<Media>[] VariousSources = new ObservableCollection<Media>[]
        {
            new ObservableCollection<Media>(),
            new ObservableCollection<Media>(),
            new ObservableCollection<Media>(),
            new ObservableCollection<Media>()
        };

        public void Add(string path, bool requestPlay = false)
        {
            var media = new Media(path);
            var duplication = this.Where(item => item.Path == path);
            if (duplication.Count() != 0)
            {
                if (requestPlay)
                    RequestPlay(duplication.First());
                return;
            }
            Insert(0, media);
            if (requestPlay)
                RequestPlay();
        }
        public void Add(string[] paths, bool requestPlay = false)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (Directory.Exists(paths[i]))
                    Add(Directory.GetFiles(paths[i], "*", SearchOption.AllDirectories), requestPlay);
                else
                    Add(paths[i], requestPlay);
            }
        }

        public void Remove(string path)
        {
            foreach (var item in this.Where(item => item.Path == path))
                Remove(item);
        }
        public void Delete(Media media)
        {
            bool reqNext = CurrentlyPlaying == media;
            File.Delete(media.Path);
            var sames = this.Where(item => item.Path == media.Path).ToArray();
            foreach (var item in sames)
                Remove(item);
            if (reqNext)
                RequestPlay(Next());
        }

        public Media Play(int index)
        {
            CurrentlyPlayingIndex = index;
            return CurrentlyPlaying;
        }
        public Media Next()
        {
            switch (App.Settings.PlayMode)
            {
                case PlayMode.Shuffle: return Play(Shuffle.Next(0, Count));
                case PlayMode.RepeatAll: return Play(CurrentlyPlayingIndex == Count - 1 ? 0 : ++CurrentlyPlayingIndex);
                default: break;
            }
            return Play(CurrentlyPlayingIndex);
        }
        public Media Previous()
        {
            switch (App.Settings.PlayMode)
            {
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, Count); break;
                case PlayMode.RepeatAll:
                    if (CurrentlyPlayingIndex != 0) return Play(--CurrentlyPlayingIndex);
                    else return Play(Count - 1);
                default: break;
            }
            return Play(CurrentlyPlayingIndex);
        }
        
        public void Repeat(int index, int times = 1) => Parallel.For(0, times, (i) => Insert(index, Items[index]));

        public void DeployLibrary()
        {
            MassiveLibrary.Save(this.ToArray());
        }

        public void AddCount() => this[CurrentlyPlayingIndex].PlayCount++;

        private void RequestPlay() => RequestPlay(this[0]);
        private void RequestPlay(Media media)
        {
            Change?.Invoke(media, new InfoExchangeArgs()
            {
                Type = InfoType.MediaRequested
            });
        }

        public void FilterVariousSources(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                for (int i = 0; i < VariousSources.Length; i++)
                    VariousSources[i] = new ObservableCollection<Media>(this.Select(item => item));
                return;
            }
            VariousSources[0] = new ObservableCollection<Media>(this.Where(item => (item.Title ?? "INDIVIDABLE").ToLower().Contains(query.ToLower())));
            VariousSources[1] = new ObservableCollection<Media>(this.Where(item => (item.Artist ?? "INDIVIDABLE").ToLower().Contains(query.ToLower())));
            VariousSources[2] = new ObservableCollection<Media>(this.Where(item => (item.Album ?? "INDIVIDABLE").ToLower().Contains(query.ToLower())));
        }
        
        public void UpdateOnPath(Media source)
        {
            this.Where(item => item.Path == source.Path).AsParallel().ForAll(item => item.Reload());
        }
        public void Revalidate()
        {
            foreach (var item in this)
                item.Reload();
            var invalids = this.Where(item => !item.IsValid).ToArray();
            foreach (var item in invalids)
                Remove(item);
        }
        public void Detergent(Media media, bool prompt = true)
        {
            if (media.Type != MediaType.Music)
            {
                MessageBox.Show("Not supported on this type of media", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string deleteWord(string target, string word)
            {
                string output = "";
                int lit1 = target.ToLower().IndexOf(word);
                if (target.StartsWith(word)) lit1 = 0;
                if (lit1 == -1)
                    return target;
                string temp1 = target.Substring(0, lit1);
                if (temp1 == String.Empty) temp1 = " ";
                if (temp1.LastIndexOf(' ') == -1) return " ";
                string temp2 = temp1.Substring(0, temp1.LastIndexOf(' '));
                output += temp2;
                int lit2 = target.ToLower().LastIndexOf(word);
                temp1 = target.Substring(lit2);
                if (!temp1.EndsWith(temp1))
                    temp2 = temp1.Substring(temp1.IndexOf(' '));
                else temp2 = "";
                output += temp2;
                return output;
            }
            string c(string target)
            {
                if (String.IsNullOrWhiteSpace(target))
                    return target;
                string[] litretures = new string[] { ".com", ".ir", ".org", "www.", "@", ".me", ".biz", ".net" };
                var lit = new List<string>();
                foreach (var item in litretures)
                    if (target.ToLower().Contains(item))
                        lit.Add(item);
                foreach (var item in lit)
                    target = deleteWord(target, item);
                return target;
            }
            
            using (var file = TagLib.File.Create(media.Path))
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
                    if (res == MessageBoxResult.No)
                        return;
                }
                if (CurrentlyPlaying != media)
                {
                    try
                    {
                        file.Save();
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("I/O Exception occured, try again");
                        return;
                    }
                    media.Reload();
                }
                else
                {
                    Change?.Invoke(media, new InfoExchangeArgs(InfoType.EditingTag) { Object = file });
                }
            }
        }

        public void Download(Media media)
        {
            var SavePath = $"{App.Path}Downloads\\{media.Title}";
            var Client = new WebClient();
            Client.DownloadProgressChanged += (_, e) => media.Title = $"Downloading... {e.ProgressPercentage}%";
            Client.DownloadFileCompleted += (_, e) =>
            {
                if (media.Type == MediaType.OnlineFile)
                {
                    var path = $"{App.Path}Downloads\\{media.Name.Substring(0, media.Name.LastIndexOf(".z"))}";
                    using (var zip = new Ionic.Zip.ZipFile(SavePath))
                        zip.ExtractAll(path, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    File.Delete(SavePath);
                    var d = from item in Directory.GetFiles(path, "*", SearchOption.AllDirectories) where Media.IsMedia(item) select new Media(item);
                    int index = IndexOf(media);
                    foreach (var item in d)
                    {
                        Detergent(item, false);
                        Insert(index, item);
                    }
                    Remove(media);
                }
                else
                {
                    int index = IndexOf(media);
                    Remove(media);
                    Insert(index, new Media(SavePath));
                    if (CurrentlyPlayingIndex == index)
                        RequestPlay(this[index]);
                }
            };
            Client.DownloadFileAsync(media.Url, SavePath);
        }

    }

    [Serializable]
    public class MassiveLibrary
    {
        private static readonly string LibraryPath = $"{App.Path}Library.dll";
        public Media[] Medias { get; set; } = new Media[0];
        public MassiveLibrary(Media[] medias) => Medias = medias;
        public MassiveLibrary() { }
        public void Save()
        {
            using (FileStream stream = new FileStream(LibraryPath, FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, this);
        }
        public static void Save(Media[] medias)
        {
            using (FileStream stream = new FileStream(LibraryPath, FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, new MassiveLibrary(medias));
        }
        public static MassiveLibrary Load()
        {
            if (!File.Exists(LibraryPath))
                return new MassiveLibrary();
            using (FileStream stream = new FileStream(LibraryPath, FileMode.Open))
                return (new BinaryFormatter()).Deserialize(stream) as MassiveLibrary;
        }
    }

}
