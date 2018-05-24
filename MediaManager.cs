using Player.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Uri Url { get; set; }
        public string Path { get => Url.IsFile ? Url.LocalPath : Url.AbsoluteUri; }
        public TimeSpan Length { get; set; }
        public int PlayCount;
        public bool IsOffline => (int)Type <= 2;
        public MediaType Type;
        [NonSerialized] public bool IsPlaying;
        [NonSerialized] public string Lyrics;
        [NonSerialized] public bool IsLoaded;
        [NonSerialized] public long Duration;
        [NonSerialized] public System.Windows.Media.Imaging.BitmapSource Artwork;
        public bool IsMedia => Type != MediaType.None;
        public bool IsVideo => Type == MediaType.Video || Type == MediaType.OnlineVideo;
        public string Ext => Path.Substring((Path ?? " . ").LastIndexOf(".") + 1).ToLower();
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

        public static Media FromString(string path) => new Media(new Uri(path));
        public Media(Uri url)
        {
            Url = url;
            if (url.IsFile)
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
                if (url.AbsoluteUri.StartsWith("https://"))
                    url = new Uri(url.AbsoluteUri.Replace("https://", "http://"));
                Name = Uri.UnescapeDataString(url.Segments[url.Segments.Count() - 1]);
                Title = Name;
                Url = url;
                Artist = url.Host;
                Album = "Cloud";
                Duration = 1;
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
        public override bool Equals(object obj)
        {
            if (obj is Media e) return Path.Equals(e.Path ?? "NULL", StringComparison.CurrentCultureIgnoreCase);
            else return false;
        }
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => Path;

        public void MoveTo(string dir)
        {
            dir += Name;
            File.Move(Path, dir);
            Url = new Uri(dir);
        }
        public void CopyTo(string dir)
        {
            dir += Name;
            File.Copy(Path, dir);
        }
    }

    public enum PlayMode { Shuffle, RepeatOne, RepeatAll }
    public class MediaManager: ObservableCollection<MediaView>
    {
        public MediaManager() { }
        private Random Shuffle = new Random(2);
        public MediaView CurrentlyPlaying => Items[CurrentlyPlayingIndex];
        public event EventHandler<InfoExchangeArgs> Change;
        public int CurrentlyPlayingIndex { get; private set; }
        public event System.Windows.Controls.ContextMenuEventHandler ContextMenuOpening;
        public event System.Windows.Controls.ContextMenuEventHandler ContextMenuClosing;
        
        public void Add(string path, bool requestPlay = false)
        {
            var media = Media.FromString(path);
            if (Contains(media, out var view))
            {
                if (requestPlay)
                    RequestPlay(view);
                return;
            }
            Add(media, requestPlay);
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
                    Add(sender2.MemberwiseClone());
                    InvokeNewMedia(Items[Count - 1]);
                }
            };
            view.PlayAfterRequested += (sender, e) =>
            {
                var s = sender as MediaView;
                Remove(s);
                Insert(CurrentlyPlayingIndex + 1, s);
                Change?.Invoke(this, new InfoExchangeArgs(InfoType.MediaUpdate));
            };
            view.ContextMenuOpening += ContextMenuOpening;
            view.ContextMenuClosing += ContextMenuClosing;
            Add(view);
            InvokeNewMedia(Items[Count - 1]);
            if (requestPlay)
                RequestPlay();
        }

        public void Remove(string path)
        {
            foreach (var item in this.Where(item => item.Media.Path == path))
                Remove(item);
        }

        public Media Play(object view) => Play(IndexOf(view as MediaView));
        public Media Play(int index)
        {
            CurrentlyPlayingIndex = index;
            foreach (var item in Items)
                item.IsPlaying = false;
            this[CurrentlyPlayingIndex].IsPlaying = true;
            return CurrentlyPlaying.Media;
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

        public void Close()
        {
            MassiveLibrary.Save(this.Select(item => item.Media).ToArray());
        }

        public void AddCount() => Items[CurrentlyPlayingIndex].Media.PlayCount++;

        private void RequestPlay() => RequestPlay(this[Count - 1]);
        private void RequestPlay(MediaView view)
        {
            Change?.Invoke(view, new InfoExchangeArgs()
            {
                Type = InfoType.MediaRequested
            });
        }

        private void InvokeNewMedia(MediaView view) => Change?.Invoke(this, new InfoExchangeArgs(InfoType.NewMedia) { Object = view });
        public bool Contains(Media media, out MediaView view)
        {
            var temp = this.Where(item => item.Media.Path == media.Path); 
            if (temp.Count() == 0)
            {
                view = null;
                return false;
            }
            else
            {
                view = temp.First();
                return true;
            }
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
