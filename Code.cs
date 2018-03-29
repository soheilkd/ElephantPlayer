using Microsoft.Win32;
using Player.InstanceManager;
using Player.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using DColor = System.Drawing.Color;
using Draw = System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player.User
{
    public static class UI
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
    public static class Screen
    {
        public static double Width => SystemParameters.PrimaryScreenWidth;
        public static double FullWidth => SystemParameters.FullPrimaryScreenWidth;
        public static double FullHeight => SystemParameters.PrimaryScreenHeight;
        public static double Height => SystemParameters.PrimaryScreenHeight;
    }
    public static class Mouse
    {
        public static double Y => Position.Y;
        public static double X => Position.X;
        public static MouseDevice PrimaryDevice => System.Windows.Input.Mouse.PrimaryDevice;
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point { public int X; public int Y; }
        static Draw.Point Position
        {
            get
            {
                Win32Point w32 = new Win32Point();
                NativeMethods.GetCursorPos(ref w32);
                return new Draw.Point(w32.X, w32.Y);
            }
        }
    }
}

namespace Player.Events
{
    public class InstanceEventArgs : EventArgs
    {
        private InstanceEventArgs() { }
        public InstanceEventArgs(IList<string> args) { _Args = args; }
        private IList<string> _Args { get; set; }
        public string this[int index] => Args[index];
        public int ArgsCount => _Args.Count;
        public string[] Args => _Args.ToArray();
    }
}

namespace Player.Events
{
    public enum InfoExchangeType
    {
        Integer, Double,
        Media, Object,
        RequestNext, RequestPrev,
        Handling, UserInterface,
        Management, AppInterface,
        Internal, StartingMedia,
        EndingMedia, PlayPause
    }

    public class MediaEventArgs : EventArgs
    {
        public TagLib.File File { get; set; }
        public int Index { get; set; }
        public Media Media { get; set; }
        public MediaView Sender { get; set; }
    }
    public class SettingsEventArgs
    {
        public Preferences NewSettings { get; set; }
    }
    public class InfoExchangeArgs
    {
        public InfoExchangeType Type { get; set; }
        public int Integer { get; set; }
        public double Double { get; set; }
        public Media Media { get; set; }
        public object Object { get; set; }

        public InfoExchangeArgs() { }
        public InfoExchangeArgs(InfoExchangeType type) => Type = type;
    }
}

namespace Player.Management
{
    public enum SortBy { Artist, Title, AlbumArtist, Name, Path, Album }
    public enum ManagementChange
    {
        NewMedia,
        EditingTag,
        InterfaceUpdate,
        MediaUpdate,
        Crash,
        PopupRequest,
        ArtworkClick,
        SomethingHappened
    }
    public class ManagementChangeEventArgs : EventArgs
    {
        public Events.MediaEventArgs Changes { get; set; }
        public ManagementChange Change {get;set;}
        public static ManagementChangeEventArgs CreateForTagEditing(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.EditingTag, Changes = e };
        public static ManagementChangeEventArgs CreateForArtwork(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.ArtworkClick, Changes = e };
        public static ManagementChangeEventArgs CreateForArtwork(int index)
        => new ManagementChangeEventArgs() { Change = ManagementChange.ArtworkClick, Changes = new Events.MediaEventArgs() { Index = index } };
        public static ManagementChangeEventArgs CreateForInterfaceUpdate(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.InterfaceUpdate, Changes = e };
        public static ManagementChangeEventArgs CreateForMediaUpdate(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.MediaUpdate, Changes = e };
        public static ManagementChangeEventArgs CreateForPopupRequest(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.PopupRequest, Changes = e };
        public static ManagementChangeEventArgs Default(Events.MediaEventArgs optional = null)
        => new ManagementChangeEventArgs() { Change = ManagementChange.SomethingHappened, Changes = optional };
    }
    [Serializable()]
    public class MassiveLibrary : IDisposable, ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MassiveLibrary() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class MediaManager
    {
        public MediaManager()
        {
            ActiveViewMode = (ViewMode)Preferences.ViewMode;
        }
        public int Count => AllMedias.Count;
        private static string[] SupportedMusics = new string[]
        {
            "mp3",
            "wma",
            "aac",
            "m4a"
        };
        private static string[] SupportedVideos = new string[]
        {
            "mp4",
            "mpg",
            "mkv",
            "wmv",
            "mov",
            "avi",
            "m4v",
            "ts",
            "wav",
            "mpeg",
            "webm"
        };
        public static string SupportedFilesFilter
        {
            get
            {
                string filter = "Supported Musics |";
                for (int i = 0; i < SupportedMusics.Length; i++)
                    filter += $"*.{SupportedMusics[i]};";
                filter += "|Supported Videos |";
                for (int i = 0; i < SupportedVideos.Length; i++)
                    filter += $"*.{SupportedVideos[i]};";
                filter += "|All Supported Files |";
                for (int i = 0; i < SupportedMusics.Length; i++)
                    filter += $"*.{SupportedMusics[i]};";
                for (int i = 0; i < SupportedVideos.Length; i++)
                    filter += $"*.{SupportedVideos[i]};";
                return filter;
            }
        }
        public Media CurrentlyPlaying => AllMedias[CurrentlyPlayingIndex];
        public event EventHandler<ManagementChangeEventArgs> Change;
        private List<Media> AllMedias = new List<Media>();
        public Preferences Preferences { private get; set; } = Preferences.Load();
        public Media this[int index] => AllMedias[index];
        private Queue<int> PlayQueue = new Queue<int>();
        private Random Randomness = new Random(2);
        public static MediaType GetType(string FileName)
        {
            if (!File.Exists(FileName)) return MediaType.NotMedia;
            string ext = GetExtension(FileName);
            for (int i = 0; i < SupportedMusics.Length; i++)
                if (ext == SupportedMusics[i]) return MediaType.Music;
            for (int i = 0; i < SupportedVideos.Length; i++)
                if (ext == SupportedVideos[i]) return MediaType.Video;
            return MediaType.NotMedia;
        }
        public int Total => AllMedias.Count;
        public PlayMode ActivePlayMode { get; set; } = PlayMode.RepeatAll;
        private ViewMode activeViewMode;
        public ViewMode ActiveViewMode
        {
            get => activeViewMode;
            set
            {
                activeViewMode = value;
            }
        }
        private int currentlyPlayingIndex;
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
        private Random Shuffle = new Random(2);
        
        public bool Add(string path)
        {
            if (path.EndsWith(".elp"))
                return DownloadPlaylist(path);
            if (GetType(path) == MediaType.NotMedia)
                return false;
            int p = AllMedias.Count;
            AllMedias.Add(new Media(path));
           
            Change?.Invoke(this, new ManagementChangeEventArgs()
            {
                Change = ManagementChange.NewMedia,
                Changes = new Events.MediaEventArgs()
                {
                    Media = AllMedias[p],
                    Index = p
                }
            });
            return true;
        }
        public bool Add(string[] paths)
        {
            var c = AllMedias.Count;
            for (int i = 0; i < paths.Length; i++)
                Add(paths[i]);
            if (c != AllMedias.Count)
                return true;
            else
                return false;
        }

        public bool Remove(string path)
        {
            int i = -1;
            for (; i < AllMedias.Count; i++)
                if (AllMedias[i].Path == path)
                    break;
            if (i == -1) return false;
            return Remove(i);
        }
        public bool Remove(Media media) => Remove(Find(media));
        public bool Remove(int index)
        {
            AllMedias.RemoveAt(index);
            if (index < CurrentlyPlayingIndex)
                CurrentlyPlayingIndex--;
            Change?.Invoke(this, new ManagementChangeEventArgs() { Change = ManagementChange.InterfaceUpdate });

            return true;
        }

        public bool Delete(string path) => Delete(Find(path));
        public bool Delete(Media media) => Delete(Find(media));
        public bool Delete(int index)
        {
            try
            {
                File.Delete(AllMedias[index].Path);
            }
            catch (Exception)
            {
                return false;
            }
            Remove(index);
            return true;
        }

        public bool Move(Media media, string to)
        {
            for (int i = 0; i < AllMedias.Count; i++)
                if (AllMedias[i] == media)
                    return Move(i, to);
            return false;
        }
        public bool Move(int index, string to)
        {
            try
            {
                File.Move(AllMedias[index].Path, to);
                AllMedias[index].Path = to;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        public bool Copy(string from, string to)
        {
            try
            {
                File.Copy(from, to);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Copy(Media media, string to) => Copy(media.Path, to);
        public bool Copy(int index, string to) => Copy(AllMedias[index], to);
        
        public void Reload(int index)
        {
            if (index == -1)
                index = CurrentlyPlayingIndex;
            AllMedias[index] = new Media(AllMedias[index].Path);
        }

        public bool Sort(SortBy by)
        {
            return true;
        }

        public bool DownloadPlaylist(string path)
        {
            try
            {
                if (!path.ToLower().EndsWith(".elp")) return false;
                string[] lines = File.ReadAllLines(path);
                List<Media> list = new List<Media>();
                for (int i = 0; i < lines.Length; i++)
                    Add(lines[i]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool DownloadPlaylist(string path, out Media[] medias)
        {
            try
            {
                if (!path.ToLower().EndsWith(".elp")) { medias = new Media[0]; return false; }
                string[] lines = File.ReadAllLines(path);
                List<Media> list = new List<Media>();
                for (int i = 0; i < lines.Length; i++)
                    if (GetType(lines[i]) != MediaType.NotMedia)
                        list.Add(new Media(lines[i]));
                medias = list.ToArray();
                return true;
            }
            catch (Exception)
            {
                medias = new Media[0];
                return false;
            }
        }

        public bool UploadPlaylist(string path)
        {
            string[] array = new string[AllMedias.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = AllMedias[i].Path;
            try
            {
                File.WriteAllLines(path, array);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool UploadPlaylist(string path, Media[] content)
        {
            string[] array = new string[content.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = content[i].Path;
            try
            {
                File.WriteAllLines(path, array);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        public static string GetExtension(string full) => full.Substring(full.LastIndexOf(".") + 1).ToLower();
        public static string GetExtension(Media media) => GetExtension(media.Path);
        public static string GetFilter(string ext = ".mp3") => $"{GetType(ext)} | *{ext}";
        public static string GetFilter(Media media) => $"{media.MediaType} | *{GetExtension(media.Path)}";
        
        public void Clear()
        {
            AllMedias.Clear();
        }

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
                case PlayMode.Queue: CurrentlyPlayingIndex = PlayQueue.Dequeue(); break;
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
                case PlayMode.Queue: CurrentlyPlayingIndex = PlayQueue.Dequeue(); break;
                case PlayMode.RepeatAll:
                    if (CurrentlyPlayingIndex != 0) CurrentlyPlayingIndex--;
                    else CurrentlyPlayingIndex = AllMedias.Count - 1;
                    break;
                default: break;
            }
            return CurrentlyPlaying;
        }
        
        public void Play(Media media) => Next(Find(media));
        public void ChangeSettings(Preferences newSettings)
        {
            Preferences = newSettings;
        }
    }
}

namespace Player
{
    public partial class App : Application, ISingleInstanceApp
    {
        public static event EventHandler<Events.InstanceEventArgs> NewInstanceRequested;
        public const string LauncherIdentifier = "ElephantPlayerBySoheilKD_CERTID8585";
        public static string ExeFullPath = Environment.GetCommandLineArgs()[0];
        public static string ExePath = ExeFullPath.Substring(0, ExeFullPath.LastIndexOf("\\") + 1);
        public static string PrefPath = $"{ExePath}SettingsProvider.dll";
        public static string LyricsPath = $"{ExePath}LyricsProvider.dll";
        public static string AppName = $"ELPWMP";
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Environment.MachineName.Equals("Soheil-PC", StringComparison.CurrentCultureIgnoreCase) && !File.Exists($"{ExePath}\\Bakhshesh.LazemNistEdamKonid"))
            {
                MessageBox.Show("Jizzzze", "LOL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Instance<App>.InitializeAsFirstInstance(LauncherIdentifier))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();
                Instance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            NewInstanceRequested?.Invoke(this, new Events.InstanceEventArgs(args));
            return true;
        }
    }
    [Serializable]
    public class Preferences
    {
        public int PlayMode { get; set; } = 0;
        public int MainKey { get; set; } = 0;
        public int TileFontIndex { get; set; } = 0;
        public int ViewMode { get; set; } = 0;
        public Size LastSize { get; set; } = new Size(700, 400);
        public Point LastLoc { get; set; } = new Point(20, 20);

        public bool VisionOrientation { get; set; } = true;
        public bool MetaInit { get; set; } = true;
        public bool LyricsProvider { get; set; } = true;
        public bool SideBarColl { get; set; } = true;
        public bool BackgroundOptimization { get; set; } = false;
        public bool Stream { get; set; } = false;
        public bool Restrict { get; set; } = false;
        public bool VolumeSetter { get; set; } = false;
        public bool MassiveLibrary { get; set; } = false;
        public bool LibraryValidation { get; set; } = false;
        public bool LightWeight { get; set; } = false;
        public bool HighLatency { get; set; } = false;
        public bool WMDebug { get; set; } = false;
        public bool IPC { get; set; } = true;
        public bool ManualGarbageCollector { get; set; } = false;
        public string DefaultPath { get; set; }
        
        public static Preferences Load()
        {
            using (FileStream stream = File.Open(App.PrefPath, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                return (Preferences)bin.Deserialize(stream);
            }
        }
        public void Save()
        {
            using (FileStream stream = File.Open(App.PrefPath, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, this);
            }
        }
        
    }
    
    public static class Debug
    {
        public static void Print<T>(T obj) => Console.WriteLine(obj.ToString());
        public static void ThrowFakeException(string Message = "") => throw new Exception(Message);
        public static void Display<T>(T message, string caption = "Debug") => MessageBox.Show(message.ToString(), caption);
    }
}

namespace Player.Taskbar
{
    public class Thumb
    {
        public static class Commands
        {
#pragma warning disable CS0067
            public class Play : ICommand
            {
                public event EventHandler CanExecuteChanged;
                public event EventHandler Raised;
                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) => Raised?.Invoke(this, null);
            }
            public class Pause : ICommand
            {
                public event EventHandler CanExecuteChanged;
                public event EventHandler Raised;
                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) => Raised?.Invoke(this, null);
            }
            public class Previous : ICommand
            {
                public event EventHandler CanExecuteChanged;
                public event EventHandler Raised;
                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) => Raised?.Invoke(this, null);
            }
            public class Next : ICommand
            {
                public event EventHandler CanExecuteChanged;
                public event EventHandler Raised;
                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) => Raised?.Invoke(this, null);
            }
#pragma warning restore CS0067
        }
        public event EventHandler PlayPressed;
        public event EventHandler PausePressed;
        public event EventHandler PrevPressed;
        public event EventHandler NextPressed;
        public ThumbButtonInfo PlayThumb = new ThumbButtonInfo()
        {
            IsInteractive = true,
            IsEnabled = true,
            IsBackgroundVisible = true,
            DismissWhenClicked = false,
            CommandParameter = "",
            Visibility = Visibility.Visible,
            Description = "Play",
            ImageSource = Resource.Image.ToBitmapSource( Properties.Resources.Play)
        };
        public ThumbButtonInfo PauseThumb = new ThumbButtonInfo()
        {
            IsInteractive = true,
            IsEnabled = true,
            IsBackgroundVisible = true,
            DismissWhenClicked = false,
            CommandParameter = "",
            Visibility = Visibility.Visible,
            Description = "Pause",
            ImageSource = Resource.Image.ToBitmapSource(Properties.Resources.Pause)
        };
        public ThumbButtonInfo PrevThumb = new ThumbButtonInfo()
        {
            IsInteractive = true,
            IsEnabled = true,
            IsBackgroundVisible = true,
            DismissWhenClicked = true,
            CommandParameter = "",
            Visibility = Visibility.Visible,
            Description = "Previous",
            ImageSource = Resource.Image.ToBitmapSource(Properties.Resources.Previous)
        };
        public ThumbButtonInfo NextThumb = new ThumbButtonInfo()
        {
            IsInteractive = true,
            IsEnabled = true,
            IsBackgroundVisible = true,
            DismissWhenClicked = true,
            CommandParameter = "",
            Visibility = Visibility.Visible,
            Description = "Next",
            ImageSource = Resource.Image.ToBitmapSource(Properties.Resources.Next)
        };
        private TaskbarItemInfo TaskbarItem = new TaskbarItemInfo();
        private Commands.Play PlayHandler = new Commands.Play();
        private Commands.Pause PauseHandler = new Commands.Pause();
        private Commands.Previous PrevHandler = new Commands.Previous();
        private Commands.Next NextHandler = new Commands.Next();
        public TaskbarItemInfo Info => TaskbarItem;
        public Thumb()
        {
            PrevThumb.Command = PrevHandler;
            NextThumb.Command = NextHandler;
            PlayThumb.Command = PlayHandler;
            PauseThumb.Command = PauseHandler;
            PlayHandler.Raised += (sender, e) => PlayPressed?.Invoke(sender, e);
            PauseHandler.Raised += (sender, e) => PausePressed?.Invoke(sender, e);
            PrevHandler.Raised += (sender, e) => PrevPressed?.Invoke(sender, e);
            NextHandler.Raised += (sender, e) => NextPressed?.Invoke(sender, e);
            TaskbarItem.ThumbButtonInfos.Clear();
            TaskbarItem.ThumbButtonInfos.Add(PrevThumb);
            TaskbarItem.ThumbButtonInfos.Add(PauseThumb);
            TaskbarItem.ThumbButtonInfos.Add(NextThumb);
        }
        public void Refresh(bool IsPlaying = false) => TaskbarItem.ThumbButtonInfos[1] = IsPlaying ? PauseThumb : PlayThumb;
        public void SetProgressState(TaskbarItemProgressState state) => TaskbarItem.ProgressState = state;
        public void SetProgressValue(double value) => TaskbarItem.ProgressValue = value;
    }
}

namespace Player.Types
{
    public enum ViewMode { Singular, GroupByArtist, GroupByDir, GroupByAlbum }
    public enum ContextTool { Border, MainDisplay }
    public enum Orientation { Portrait, Landscape }
    public enum PlayPara { None, Next, Prev }
   
    public enum PlayMode { Single, Shuffle, RepeatOne, RepeatAll, Queue }
    public enum MediaViewMode { Default, Compact, Expanded }
    public enum MediaComparsion { Title, Name, Path, Type, Artist, Album }
    public enum MediaType { Music, Video, NotMedia }
    public class Media : IDisposable
    {
        public Media() { }
        public bool IsPlaying { get; set; }
        public bool IsMedia => MediaType != MediaType.NotMedia;
        public bool IsVideo => MediaType == MediaType.Video;
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public string Lyrics { get; set; }
        public long Duration { get; set; }
        public ImageSource Artwork { get; set; }
        public MediaType MediaType;
        public bool Contains(string para)
        {
            return Name.ToLower().Contains(para.ToLower())
                || Title.ToLower().Contains(para.ToLower())
                || Album.ToLower().Contains(para.ToLower())
                || Artist.ToLower().Contains(para.ToLower());
        }
        public override string ToString() => Path;
        public static readonly Media Empty = new Media()
        {
            Artist = "",
            Album = "",
            Artwork = null,
            disposedValue = false,
            MediaType = MediaType.NotMedia,
            Name = "",
            Path = "",
            Title = ""
        };
        public Media(string path)
        {
            switch (Management.MediaManager.GetType(path))
            {
                case MediaType.Music:
                    using (var t = TagLib.File.Create(path))
                    {
                        Name = path.Substring(path.LastIndexOf("\\") + 1);
                        Path = path;
                        Artist = t.Tag.FirstPerformer;
                        Title = t.Tag.Title;
                        Album = t.Tag.Album;
                        Duration = t.Length;
                        Artwork = t.Tag.Pictures.Length >= 1 ? Resource.Image.ToBitmapSource(t.Tag.Pictures[0]) : Resource.Image.ToBitmapSource(Properties.Resources.MusicArtwork);
                        MediaType = MediaType.Music;
                        Lyrics = t.Tag.Lyrics ?? " ";
                    }
                    break;
                case MediaType.Video:
                    Name = path.Substring(path.LastIndexOf("\\") + 1);
                    Title = Name;
                    Path = path;
                    Artist = path.Substring(0, path.LastIndexOf("\\"));
                    Album = "Video";
                    Duration = 1;
                    Artwork = Resource.Image.ToBitmapSource(Properties.Resources.VideoArtwork);
                    MediaType = MediaType.Video;
                    break;
                case MediaType.NotMedia:
                    throw new IOException($"Given path is not valid media\r\nPath:{path}");
                default: break;
            }
        }
        public override bool Equals(object obj) => Path == (obj as Media).Path;
        public bool Equals(Media obj) => Path == obj.Path;
        public override int GetHashCode() => base.GetHashCode();
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Album = null;
                    Artist = null;
                    Artwork = null;
                    MediaType = 0;
                    Name = null;
                    Path = null;
                    Title = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Media() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        
        #endregion
    }
}

namespace Player.Resource
{
    public static class Theming
    {
        public static Brush ToBrush(DColor e) => ToBrush(ToColor(e));
        public static Brush ToBrush(Color e) => new SolidColorBrush(e);
        public static Brush ToBrush(Color? e) => new SolidColorBrush(e.Value);
        public static DColor ToDrawingColor(Color e) => DColor.FromArgb(e.A, e.R, e.G, e.B);
        public static DColor ToDrawingColor(Color? e) => ToDrawingColor(e.Value);
        public static Color ToColor(DColor e) => Color.FromArgb(e.A, e.R, e.G, e.B);
    }
    public static class Image
    {
        public static Draw.Image ToImage(TagLib.IPicture picture) => Draw.Image.FromStream(new MemoryStream(picture.Data.Data));
        public static BitmapImage ToImage(BitmapSource source)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }
        private static BitmapSource ToBitmapSource(Draw.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        public static BitmapSource ToBitmapSource(Draw::Image myImage)
        {
            var bitmap = new Draw.Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            return bitmapSource;
        }
        public static BitmapSource ToBitmapSource(TagLib.IPicture picture) => ToBitmapSource(ToImage(picture));

        public static byte[] ToByte(BitmapImage image)
        {
            byte[] data;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }
    }
    public static class String
    {
        public static string Dumps => @"C:\Users\Soheil\AppData\Local\CrashDumps";
    }
}

namespace Player.Extensions
{
    public static class Ext
    {
        public static bool IsDigitsOnly(this string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] < '0' || str[i] > '9') return false;
            return true;
        }
        public static int ToInt(this double e) => Convert.ToInt32(e);
    }
}