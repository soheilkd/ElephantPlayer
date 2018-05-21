using Player.Events;
using System;
using System.Collections.Generic;
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
    public class Media
    {
        public Media() { }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public TimeSpan Length { get; set; }
        public int PlayCount;
        public bool IsOffline;
        public MediaType MediaType;
        [NonSerialized] public bool IsPlaying;
        [NonSerialized] public string Lyrics;
        [NonSerialized] public bool IsLoaded;
        [NonSerialized] public long Duration;
        [NonSerialized] public System.Windows.Media.Imaging.BitmapSource Artwork;
        public bool IsMedia => MediaType != MediaType.None;
        public bool IsVideo => MediaType == MediaType.Video || MediaType == MediaType.OnlineVideo;
        public bool IsValid
        {
            get
            {
                switch (MediaType)
                {
                    case MediaType.Music:
                    case MediaType.Video:
                    case MediaType.File: return File.Exists(Path);
                    case MediaType.OnlineMusic:
                    case MediaType.OnlineVideo:
                    case MediaType.OnlineFile:
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
                    default: return false;
                }
            }
        }

        public Media(string path)
        {
            if (path.StartsWith("https://"))
                path = path.Replace("https://", "http://");
            Uri uri = new Uri(path);
            if (uri.IsFile)
            {
                switch (MediaManager.GetType(path))
                {
                    case MediaType.Music:
                        using (var t = TagLib.File.Create(path))
                        {
                            Name = path.Substring(path.LastIndexOf("\\") + 1);
                            Path = path;
                            Artist = t.Tag.FirstPerformer ?? path.Substring(0, path.LastIndexOf("\\"));
                            Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
                            Album = t.Tag.Album ?? String.Empty;
                            Artwork = t.Tag.Pictures.Length >= 1 ? Imaging.Get.BitmapSource(t.Tag.Pictures[0]) : Imaging.Images.MusicArt;
                            MediaType = MediaType.Music;
                            Lyrics = t.Tag.Lyrics ?? String.Empty;
                            IsLoaded = true;
                        }
                        break;
                    case MediaType.Video:
                        Name = path.Substring(path.LastIndexOf("\\") + 1);
                        Title = Name;
                        Path = path;
                        Artist = path.Substring(0, path.LastIndexOf("\\"));
                        Artist = Artist.Substring(Artist.LastIndexOf("\\") + 1);
                        Album = "Video";
                        Artwork = Imaging.Images.VideoArt;
                        MediaType = MediaType.Video;
                        IsLoaded = true;
                        Length = TimeSpan.Zero;
                        break;
                    case MediaType.None:
                        Name = null;
                        Title = null;
                        Path = "NULL";
                        Artist = null;
                        Album = null;
                        Artwork = null;
                        Length = TimeSpan.Zero;
                        IsLoaded = false;
                        MediaType = MediaType.None;
                        break;
                    default: break;
                }
            }
            else
            {
                Name = Uri.UnescapeDataString(uri.Segments[uri.Segments.Count() - 1]);
                Title = Name;
                Path = uri.AbsoluteUri;
                Artist = uri.Host;
                Album = "Cloud";
                Duration = 1;
                Artwork = Imaging.Images.NetArt;
                MediaType = MediaManager.GetType(path, true);
                IsLoaded = true;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is Media e) return Path.Equals(e.Path ?? "NULL", StringComparison.CurrentCultureIgnoreCase);
            else return false;
        }
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => Path;
    }

    public enum PlayMode { Shuffle, RepeatOne, RepeatAll, Queue }
    public class MediaManager
    {
        public MediaManager() { }
        public MediaView this[int index] => MediaViews[index];
        private static string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
        private static string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
        private static string[] SupportedFiles = "zip;rar;bin;dat".Split(';');
        private List<MediaView> MediaViews = new List<MediaView>();
        private int CurrentQueuePosition = -1;
        private List<int> PlayQueue = new List<int>();
        private Random Shuffle = new Random(2);
        public int Count => MediaViews.Count;
        public MediaView CurrentlyPlaying => MediaViews[CurrentlyPlayingIndex];
        public event EventHandler<InfoExchangeArgs> Change;
        public Preferences Preferences { private get; set; } = Preferences.Load();
        public PlayMode ActivePlayMode { get; set; } = PlayMode.RepeatAll;
        public int CurrentlyPlayingIndex { get; private set; }

        public static MediaType GetType(string FileName, bool checkOnline = false)
        {
            if (checkOnline && FileName.StartsWith("http"))
            {
                switch (GetType(FileName))
                {
                    case MediaType.Music: return MediaType.OnlineMusic;
                    case MediaType.Video: return MediaType.OnlineVideo;
                    default: return MediaType.OnlineFile;
                }
            }
            string ext = GetExtension(FileName);
            if (SupportedMusics.Contains(ext)) return MediaType.Music;
            else if (SupportedVideos.Contains(ext)) return MediaType.Video;
            else if (SupportedFiles.Contains(ext)) return MediaType.File;
            return MediaType.None;
        }

        public void Add(Uri uri, bool requestPlay = false)
        {
            var media = new Media(uri.AbsoluteUri);
            if (!media.IsValid)
                return;
            Add(media);
            if (requestPlay)
                RequestPlay(Count - 1);
        }
        public void Add(string path, bool requestPlay = false)
        {
            var media = new Media(path);
            if (Contains(media, out var view))
            {
                if (requestPlay)
                    RequestPlay(view);
                return;
            }
            Add(media, requestPlay);
            if (requestPlay)
                RequestPlay(Count - 1);
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
        public void Add(Media media, bool requestPlay = false)
        {
            if (!media.IsValid)
                return;
            MediaView view = new MediaView(media);
            view.DoubleClicked += (sender, e) => 
            RequestPlay(sender as MediaView);
            view.PlayClicked += (sender, e) => RequestPlay(sender as MediaView);
            view.RemoveRequested += (sender, e) => Remove(sender as MediaView);
            view.TagSaveRequested += (sender, e) => Change?.Invoke(sender, new InfoExchangeArgs(InfoType.EditingTag));
            view.RepeatRequested += (sender, e) =>
            {
                var sender2 = sender as MediaView;
                for (int i = 0; i < e.Integer; i++)
                {
                    MediaViews.Add(sender2.MemberwiseClone());
                    InvokeNewMedia(MediaViews[Count - 1]);
                }
            };
            view.PlayAfterRequested += (sender, e) =>
            {
                var s = sender as MediaView;
                MediaViews.Remove(s);
                MediaViews.Insert(CurrentlyPlayingIndex + 1, s);
                Change?.Invoke(this, new InfoExchangeArgs(InfoType.MediaUpdate));
            };
            MediaViews.Add(view);
            InvokeNewMedia(MediaViews[Count - 1]);
            if (requestPlay)
                RequestPlay(item => item.Media == media);
        }

        public void Remove(string path)
        {
            MediaViews.AsParallel().Where(item => item.Media.Path == path).ForAll(item => Remove(item));
        }
        public void Remove(MediaView media)
        {
            MediaViews.Remove(media);
            Change?.Invoke(this, new InfoExchangeArgs() { Type = InfoType.MediaRemoved, Object = media });
        }
        public void Remove(int index) => Remove(MediaViews[index]);

        public static string GetExtension(string full) => full.Substring((full?? " . ").LastIndexOf(".") + 1).ToLower();
        public static string GetExtension(Media media) => GetExtension(media.Path);
        public static string GetFilter(string ext = ".mp3") => $"{GetType(ext)} | *{ext}";
        public static string GetFilter(Media media) => $"{media.MediaType} | *{GetExtension(media.Path)}";

        public MediaView Next(object sender)
        {
            CurrentlyPlayingIndex = MediaViews.FindIndex(item => item.Equals(sender));
            return CurrentlyPlaying;
        }
        public MediaView Next(int y = -1)
        {
            if (y != -1)
                CurrentlyPlayingIndex = y;
            else
                switch (ActivePlayMode)
                {
                    case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, Count); break;
                    case PlayMode.Queue:
                        if (CurrentQueuePosition == PlayQueue.Count - 1)
                        {
                            CurrentlyPlayingIndex = PlayQueue[0];
                            CurrentQueuePosition = 0;
                        }
                        else
                            CurrentlyPlayingIndex = PlayQueue[CurrentQueuePosition++];
                        break;
                    case PlayMode.RepeatAll:
                        if (CurrentlyPlayingIndex != Count - 1) CurrentlyPlayingIndex++;
                        else CurrentlyPlayingIndex = 0;
                        break;
                    default: break;
                }
            return CurrentlyPlaying;
        }
        public MediaView Previous()
        {
            switch (ActivePlayMode)
            {
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, Count); break;
                case PlayMode.Queue:
                    if (CurrentQueuePosition == 0)
                    {
                        CurrentQueuePosition = PlayQueue.Count - 1;
                        CurrentlyPlayingIndex = PlayQueue[CurrentQueuePosition];
                    }
                    else
                        CurrentlyPlayingIndex = PlayQueue[CurrentQueuePosition--];
                    break;
                case PlayMode.RepeatAll:
                    if (CurrentlyPlayingIndex != 0) CurrentlyPlayingIndex--;
                    else CurrentlyPlayingIndex = Count - 1;
                    break;
                default: break;
            }
            return CurrentlyPlaying;
        }

        public void Repeat(int index, int times = 1) => Parallel.For(0, times, (i) => PlayNext(index));

        public void PlayNext(int index) => PlayQueue.Insert(CurrentQueuePosition + 1, index);

        public void ShowProperties(int index)
        {

        }

        public void Close()
        {
            MassiveLibrary.Save(MediaViews.Select(item => item.Media).ToArray());
        }

        public void AddCount() => MediaViews[CurrentlyPlayingIndex].Media.PlayCount++;

        public bool IsPlaying(int index) => index == CurrentlyPlayingIndex;

        private void RequestPlay(int index) => RequestPlay(MediaViews[index]);
        private void RequestPlay(MediaView view)
        {
            Change?.Invoke(view, new InfoExchangeArgs()
            {
                Type = InfoType.MediaRequested
            });
        }
        private void RequestPlay(Predicate<MediaView> match) => RequestPlay(MediaViews.Find(match));

        private void InvokeNewMedia(MediaView view) => Change?.Invoke(this, new InfoExchangeArgs(InfoType.NewMedia) { Object = view });
        public bool Contains(Media media, out MediaView view)
        {
            int temp = MediaViews.FindIndex(item => item.Media == media);
            if (temp == -1)
            {
                view = null;
                return false;
            }
            else
            {
                view = MediaViews[temp];
                return true;
            }
        }
        public ParallelQuery<MediaView> AsParallel() => MediaViews.AsParallel();

        public void ResetIsPlayings() => MediaViews.ForEach(item => item.IsPlaying = false);
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
