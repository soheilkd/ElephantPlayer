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
        public int Length { get; set; }
        public int PlayCount;
        public bool IsOffline;
        public MediaType MediaType;
        [NonSerialized] public bool IsPlaying;
        [NonSerialized] public string Lyrics;
        [NonSerialized] public bool IsLoaded;
        [NonSerialized] public long Duration;
        [NonSerialized] public System.Windows.Media.Imaging.BitmapSource Artwork;
        [NonSerialized] public bool IsRemoved;
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
                            Album = t.Tag.Album ?? " ";
                            Artwork = t.Tag.Pictures.Length >= 1 ? Imaging.Get.BitmapSource(t.Tag.Pictures[0]) : Imaging.Images.MusicArt;
                            MediaType = MediaType.Music;
                            Lyrics = t.Tag.Lyrics ?? " ";
                            Length = unchecked((int)t.Length);
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
                        Length = 1;
                        break;
                    case MediaType.None:
                        Name = null;
                        Title = null;
                        Path = "NULL";
                        Artist = null;
                        Album = null;
                        Artwork = null;
                        Length = -1;
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
    }

    public enum PlayMode { Shuffle, RepeatOne, RepeatAll, Queue }
    public class MediaManager
    {
        public MediaManager() { }
        private int currentlyPlayingIndex;
        private int CurrentQueuePosition = -1;
        private static string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
        private static string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
        private static string[] SupportedFiles = "zip;rar;bin;dat".Split(';');
        private List<Media> AllMedias = new List<Media>();
        private List<int> PlayQueue = new List<int>();
        private Random Shuffle = new Random(2);
        public int Count => AllMedias.Count;
        public Media CurrentlyPlaying => AllMedias[CurrentlyPlayingIndex];
        public event EventHandler<InfoExchangeArgs> Change;
        public Preferences Preferences { private get; set; } = Preferences.Load();
        public Media this[int index]
        {
            get => AllMedias[index];
            set
            {
                AllMedias[index] = value;
                Change?.Invoke(this, new InfoExchangeArgs()
                {
                    Type = InfoType.MediaUpdate,
                    Integer = index,
                    Object = value
                });
            }
        }
        public PlayMode ActivePlayMode { get; set; } = PlayMode.RepeatAll;
        public int CurrentlyPlayingIndex
        {
            get => currentlyPlayingIndex;
            set
            {
                for (int i = 0; i < AllMedias.Count; i++)
                    AllMedias[i].IsPlaying = false;
                AllMedias[value].IsPlaying = true;
                currentlyPlayingIndex = value;
            }
        }

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
            {
                Change?.Invoke(this, new InfoExchangeArgs()
                {
                    Type = InfoType.MediaRequested,
                    Integer = AllMedias.Count - 1
                });
            }
        }
        public void Add(string path, bool requestPlay = false)
        {
            var media = new Media(path);
            if (Find(media) != -1)
            {
                if (!requestPlay)
                    return;
                Change?.Invoke(this, new InfoExchangeArgs()
                {
                    Type = InfoType.MediaRequested,
                    Integer = Find(path)
                });
                return;
            }
            AllMedias.Add(media);
            Change?.Invoke(this, new InfoExchangeArgs()
            {
                Type = InfoType.NewMedia,
                Integer = AllMedias.Count - 1
            });
            if (requestPlay)
                Change?.Invoke(this, new InfoExchangeArgs()
                {
                    Type = InfoType.MediaRequested,
                    Integer = AllMedias.Count - 1
                });
        }
        public void Add(string[] paths, bool requestPlay = false)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (Directory.Exists(paths[i]))
                    Add(Directory.GetFiles(paths[i], "*", SearchOption.AllDirectories), requestPlay);
                else
                    Add(paths[i], true);
            }
        }
        public void Add(Media media)
        {
            if (!media.IsValid)
                return;
            int p = AllMedias.Count;
            if (!media.IsLoaded)
                media = new Media(media.Path) { Length = media.Length };
            AllMedias.Add(media);
            Change?.Invoke(this, new InfoExchangeArgs()
            {
                Type = InfoType.NewMedia,
                Integer = p
            });
        }

        public void Remove(string path)
        {
            for (int i = 0; i < AllMedias.Count; i++)
                if (AllMedias[i].Path == path)
                    Remove(i--); // i-- vas inke age elemente badi ham hamoon path ro dasht i--, i++ ro khonsa kone vo loop ro hamoon ejra she baz
        }
        public void Remove(Media media) => Remove(Find(media));
        public void Remove(int index)
        {
            AllMedias[index].IsRemoved = true;
            Change?.Invoke(this, new InfoExchangeArgs() { Type = InfoType.MediaRemoved, Integer = index });
        }

        public void Delete(string path) => Delete(Find(path));
        public void Delete(Media media) => Delete(Find(media));
        public void Delete(int index)
        {
            (new Thread(() => File.Delete(AllMedias[index].Path))).Start();
            Remove(index);
        }

        public void Move(Media media, string to) => Move(Find(media), to);
        public void Move(int index, string to)
        {
            File.Move(AllMedias[index].Path, to);
            AllMedias[index].Path = to;
        }

        public int Find(Media media)
        {
            for (int i = 0; i < AllMedias.Count; i++)
                if (AllMedias[i].Equals(media))
                    return i;
            return -1;
        }
        public int Find(string path)
        {
            for (int i = 0; i < AllMedias.Count; i++)
                if (AllMedias[i].Path.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                    return i;
            return -1;
        }

        public void Copy(string from, string to) => File.Copy(from, to, true);
        public void Copy(Media media, string to) => Copy(media.Path, to);
        public void Copy(int index, string to) => Copy(AllMedias[index], to);

        public void Reload(int index)
        {
            if (index == -1)
                index = CurrentlyPlayingIndex;
            AllMedias[index] = new Media(AllMedias[index].Path);
        }

        public void RequestDelete(int index)
        {
            var res = MessageBox.Show($"Sure? this file will be deleted:\r\n{AllMedias[index]}", " ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (res == MessageBoxResult.OK)
                Delete(index);
        }
        public void RequestDelete(Media view) => RequestDelete(Find(view));
        public void RequestDelete() => RequestDelete(CurrentlyPlayingIndex);

        public void RequestLocation(int index) => System.Diagnostics.Process.Start("explorer.exe", "/select," + AllMedias[index].Path);
        public void RequestLocation(Media view) => RequestLocation(Find(view));
        public void RequestLocation() => RequestLocation(CurrentlyPlayingIndex);

        public static string GetExtension(string full) => full.Substring((full?? " . ").LastIndexOf(".") + 1).ToLower();
        public static string GetExtension(Media media) => GetExtension(media.Path);
        public static string GetFilter(string ext = ".mp3") => $"{GetType(ext)} | *{ext}";
        public static string GetFilter(Media media) => $"{media.MediaType} | *{GetExtension(media.Path)}";

        public Media Next(int y = -1)
        {
            if (y != -1)
            {
                CurrentlyPlayingIndex = y;
                return CurrentlyPlaying;
            }
            switch (ActivePlayMode)
            {
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, AllMedias.Count); break;
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
                    if (CurrentlyPlayingIndex != AllMedias.Count - 1) CurrentlyPlayingIndex++;
                    else CurrentlyPlayingIndex = 0;
                    break;
                default: break;
            }
            return CurrentlyPlaying;
        }
        public Media Previous()
        {
            switch (ActivePlayMode)
            {
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, AllMedias.Count); break;
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
                    else CurrentlyPlayingIndex = AllMedias.Count - 1;
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
            MassiveLibrary.Save(AllMedias.ToArray());
        }

        public void Play(Media media) => Next(Find(media));

        public void AddCount() => AllMedias[CurrentlyPlayingIndex].PlayCount++;

    }

    [Serializable]
    public class MassiveLibrary
    {
        private static string LibraryPath = $"{App.Path}Library.dll";
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
                (new BinaryFormatter()).Serialize(stream, new MassiveLibrary((from item in medias where !item.IsRemoved select item).ToArray()));
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
