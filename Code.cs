using Player.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using Draw = System.Drawing;
using Forms = System.Windows.Forms;

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
                InstanceManager.NativeMethods.GetCursorPos(ref w32);
                return new Draw.Point(w32.X, w32.Y);
            }
        }
    }
    public class Keyboard
    {
        private static Gma.System.MouseKeyHook.IKeyboardMouseEvents _events = Gma.System.MouseKeyHook.Hook.GlobalEvents();
        public static event Forms::KeyPressEventHandler KeyPress
        {
            add => _events.KeyPress += value;
            remove => _events.KeyPress -= value;
        }
        public static event Forms::KeyEventHandler KeyDown
        {
            add => _events.KeyDown += value;
            remove => _events.KeyDown -= value;
        }
        public static event Forms::KeyEventHandler KeyUp
        {
            add => _events.KeyDown += value;
            remove => _events.KeyDown -= value;
        }
        public Keyboard() { }
        ~Keyboard() => _events.Dispose();
        
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

    public enum InfoType
    {
        Integer, Double, Media, Object, StringArray,
        RequestNext, RequestPrev, Handling, UserInterface,
        Management, AppInterface, Internal, StartingMedia,
        EndingMedia, PlayPause, NewMedia, MediaRequested, EditingTag,
        InterfaceUpdate, MediaUpdate, Crash, PopupRequest,
        ArtworkClick, SomethingHappened, MediaRemoved, MediaMoved
    }
    
    public class InfoExchangeArgs
    {
        public InfoType Type { get; set; }
        public object Object { get; set; }
        public object[] ObjectArray { get; set; }
        public int Integer { get; set; }
        public Media Media { get; set; }
        
        public InfoExchangeArgs() { }
        public InfoExchangeArgs(InfoType type) => Type = type;
        public InfoExchangeArgs(int integer)
        {
            Type = InfoType.Integer;
            Integer = integer;
        }
    }
}

namespace Player
{
    public enum PlayPara { None, Next, Prev }
    public enum PlayMode { Shuffle, RepeatOne, RepeatAll, Queue }
    public enum MediaType { Music, Video, Online, NotMedia }

    public partial class App : Application, InstanceManager.ISingleInstanceApp
    {
        public static event EventHandler<InstanceEventArgs> NewInstanceRequested;

        public const string LauncherIdentifier = "ElephantPlayerBySoheilKD_CERTID8585";
        public static string ExePath = Environment.GetCommandLineArgs()[0];
        public static string Path = ExePath.Substring(0, ExePath.LastIndexOf("\\") + 1);
        public static string LibraryPath = $"{Path}Library.dll";
        
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Unhandled {e.ExceptionObject}\r\n" +
                    $"Terminating: {e.IsTerminating}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                if (e.IsTerminating)
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
            };
            if (!Environment.MachineName.Equals("Soheil-PC", StringComparison.CurrentCultureIgnoreCase) && !File.Exists($"{Path}\\Bakhshesh.LazemNistEdamKonid"))
            {
                MessageBox.Show("Jizzzze", "LOL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (InstanceManager.Instance<App>.InitializeAsFirstInstance(LauncherIdentifier))
            {
                var application = new App();

                application.InitializeComponent();

                application.Run();
                InstanceManager.Instance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            NewInstanceRequested?.Invoke(this, new InstanceEventArgs(args));
            return true;
        }
    }
    
    public class MediaManager
    {
        public MediaManager() { }
        public int Count => AllMedias.Count;
        private static string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
        private static string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
        public Media CurrentlyPlaying => AllMedias[CurrentlyPlayingIndex];
        public event EventHandler<InfoExchangeArgs> Change;
        private List<Media> AllMedias = new List<Media>();
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
                    Media = value
                });
            }
        }
        private List<int> PlayQueue = new List<int>();
        private Random Randomness = new Random(2);
        public static MediaType GetType(string FileName)
        {
            if (FileName.StartsWith("http")) return MediaType.Online;
            string ext = GetExtension(FileName);
            if (SupportedMusics.Contains(ext)) return MediaType.Music;
            else if (SupportedVideos.Contains(ext)) return MediaType.Video;
            return MediaType.NotMedia;
        }
        public PlayMode ActivePlayMode { get; set; } = PlayMode.RepeatAll;
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
        private int CurrentQueuePosition = 0;

        public void Add(Uri uri, bool requestPlay = false)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AddRange(0, 10);
            try
            {
                request.Timeout = 5000;
                var response = request.GetResponse();

                if (!response.ContentType.EndsWith("octet-stream") && !response.ContentType.StartsWith("video") && !response.ContentType.StartsWith("app"))
                {
                    MessageBox.Show("Requested Uri is not a valid octet-stream", "NET", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                response.Dispose();
                response = null;
            }
            catch (WebException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            Add(new Media(uri.AbsoluteUri));
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
            if (!Check(path))
                return;
            if (Find(path) != -1)
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

            AllMedias.Add(new Media(path));
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
                if (Check(paths[i]))
                    Add(paths[i]);
        }
        public void Add(Media media)
        {
            int p = AllMedias.Count;
            if (!media.IsLoaded)
                media = new Media(media.Path);
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
            File.Delete(AllMedias[index].Path);
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


        public void DownloadPlaylist(string path)
        {
            if (!path.ToLower().EndsWith(".elp")) return;
            string[] lines = File.ReadAllLines(path);
            List<Media> list = new List<Media>();
            for (int i = 0; i < lines.Length; i++)
                Add(lines[i]);
        }
        public static void DownloadPlaylist(string path, out Media[] medias)
        {
            if (!path.ToLower().EndsWith(".elp")) { medias = new Media[0]; return; }
            string[] lines = File.ReadAllLines(path);
            List<Media> list = new List<Media>();
            for (int i = 0; i < lines.Length; i++)
                if (Check(lines[i]))
                    list.Add(new Media(lines[i]));
            medias = list.ToArray();
        }

        public void UploadPlaylist(string path) =>
            File.WriteAllLines(path, from item in AllMedias select item.Path);
        public static void UploadPlaylist(string path, Media[] content) =>
            File.WriteAllLines(path, from item in content select item.Path);

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

        public void Repeat(int index, int times)
        {
            for (int i = 0; i < times; i++)
                PlayQueue.Add(index);
        }
        public void Repeat(int index) =>
            PlayQueue.Add(index);

        public void PlayNext(int index)
        {

        }

        public void ShowProperties(int index)
        {

        }

        public void Close()
        {
            MassiveLibrary.Save(AllMedias.ToArray());
        }

        public void Play(Media media) => Next(Find(media));
        public void ChangeSettings(Preferences newSettings)
        {
            Preferences = newSettings;
        }
        public void AddCount() => AllMedias[CurrentlyPlayingIndex].PlayCount++;

        public void Set(Media oldMedia, Media newMedia) => this[Find(oldMedia)] = newMedia;

        private static bool Check(string path) => File.Exists(path) && GetType(path) != MediaType.NotMedia;
        private static bool Check(Media media) => File.Exists(media.Path) && media.MediaType != MediaType.NotMedia;
    }


    [Serializable]
    public class Preferences
    {
        public int PlayMode { get; set; } = 0;
        public int MainKey { get; set; } = 0;

        public Size LastSize { get; set; } = new Size(400, 400);
        public Point LastLoc { get; set; } = new Point(20, 20);

        public bool VisionOrientation { get; set; } = true;
        public bool VolumeSetter { get; set; } = false;
        public bool LibraryValidation { get; set; } = false;
        public bool ManualGarbageCollector { get; set; } = false;
        public int MouseOverTimeout { get; set; } = 5000;
        public static Preferences Load()
        {
            using (FileStream stream = File.Open($"{App.Path}SettingsProvider.dll", FileMode.Open))
                return (Preferences)(new BinaryFormatter()).Deserialize(stream);
        }
        public void Save()
        {
            using (FileStream stream = File.Open($"{App.Path}SettingsProvider.dll", FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, this);
        }
    }
    
    [Serializable]
    public class Media : IDisposable
    {
        public Media() { }
        public bool IsMedia => MediaType != MediaType.NotMedia;
        public bool IsVideo => MediaType == MediaType.Video;
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public int PlayCount;
        public bool IsOffline;
        [NonSerialized] public bool IsPlaying;
        [NonSerialized] public string Lyrics;
        [NonSerialized] public bool IsLoaded;
        [NonSerialized] public long Duration;
        [NonSerialized] public BitmapSource Artwork;
        [NonSerialized] public bool IsRemoved;
        
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
                            Artist = t.Tag.FirstPerformer;
                            Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
                            Album = t.Tag.Album;
                            Duration = t.Length;
                            Artwork = t.Tag.Pictures.Length >= 1 ? ConvertTo.BitmapSource(t.Tag.Pictures[0]) : ConvertTo.BitmapSource(Properties.Resources.MusicArt);
                            MediaType = MediaType.Music;
                            Lyrics = t.Tag.Lyrics ?? " ";
                            IsLoaded = true;
                        }
                        break;
                    case MediaType.Video:
                        Name = path.Substring(path.LastIndexOf("\\") + 1);
                        Title = Name;
                        Path = path;
                        Artist = path.Substring(0, path.LastIndexOf("\\"));
                        Album = "Video";
                        Duration = 1;
                        Artwork = ConvertTo.BitmapSource(Properties.Resources.VideoArt);
                        MediaType = MediaType.Video;
                        IsLoaded = true;
                        break;
                    case MediaType.NotMedia:
                        throw new IOException($"Given path is not valid media\r\nPath:{path}");
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
                Artwork = ConvertTo.BitmapSource(Properties.Resources.NetArt);
                MediaType = MediaType.Online;
                IsLoaded = true;
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
                    Album = String.Empty;
                    Artist = String.Empty;
                    Name = String.Empty;
                    Path = String.Empty;
                    Title = String.Empty;
                    Lyrics = String.Empty;
                    Artwork = null;
                    MediaType = 0;
                }
                disposedValue = true;
            }
        }
        
        ~Media() => Dispose(false);
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion 
    }

    [Serializable]
    public class MassiveLibrary
    {
        public Media[] Medias { get; set; } = new Media[0];
        public MassiveLibrary(Media[] medias) => Medias = medias;
        public MassiveLibrary() { }
        public void Save()
        {
            using (FileStream stream = new FileStream(App.LibraryPath, FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, this);
        }
        public static void Save(Media[] medias)
        {
            var newMedias =
                from item in medias
                where !item.IsRemoved
                select item;
            int c = newMedias.Count();
            medias = new Media[c];
            int i = 0;
            foreach (var item in newMedias)
            {
                medias[i++] = item;
            }
            using (FileStream stream = new FileStream(App.LibraryPath, FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, new MassiveLibrary(medias));
        }
        public static MassiveLibrary Load()
        {
            if (!File.Exists(App.LibraryPath))
                return new MassiveLibrary();
            using (FileStream stream = new FileStream(App.LibraryPath, FileMode.Open))
                return (new BinaryFormatter()).Deserialize(stream) as MassiveLibrary;
        }
    }

    public static class Debug
    {
        public static void Print<T>(T obj) => Console.WriteLine(obj.ToString());
        public static void ThrowFakeException(string Message = "") => throw new Exception(Message);
        public static void Display<T>(T message, string caption = "Debug") => MessageBox.Show(message.ToString(), caption);
    }
    public static class ConvertTo
    {
        public static BitmapImage Bitmap<T>(T element) where T : System.Windows.Controls.Control
        {
            element.BeginInit();
            element.UpdateLayout();

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Clear();
            Transform transform = element.LayoutTransform;
            element.LayoutTransform = null;
            Size size = new Size(element.Width, element.Height);
            element.Measure(size);
            element.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(element);

            MemoryStream memStream = new MemoryStream();

            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(memStream);
            memStream.Flush();
            var output = new BitmapImage();
            output.BeginInit();
            output.StreamSource = memStream;
            output.EndInit();

            return output;
        }
        public static Draw.Image Image(TagLib.IPicture picture) => Draw.Image.FromStream(new MemoryStream(picture.Data.Data));

        public static BitmapSource BitmapSource(Draw.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        public static BitmapSource BitmapSource(TagLib.IPicture picture)
        {
            var bitmap = new Draw.Bitmap(Image(picture));
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
    }
    public static class Extensions
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
            ImageSource = ConvertTo.Bitmap(new Controls.MaterialIcon() { Icon = Controls.IconType.play_arrow, Foreground = Brushes.White })
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
            ImageSource = ConvertTo.Bitmap(new Controls.MaterialIcon() { Icon = Controls.IconType.pause, Foreground = Brushes.White })
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
            ImageSource = ConvertTo.Bitmap(new Controls.MaterialIcon() { Icon = Controls.IconType.skip_previous, Foreground = Brushes.White })
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
            ImageSource = ConvertTo.Bitmap(new Controls.MaterialIcon() { Icon = Controls.IconType.skip_next, Foreground = Brushes.White })
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


namespace Player.InstanceManager
{
    internal enum WM
    {
        NULL = 0x0000, CREATE = 0x0001, DESTROY = 0x0002, MOVE = 0x0003, SIZE = 0x0005, ACTIVATE = 0x0006,
        SETFOCUS = 0x0007, KILLFOCUS = 0x0008, ENABLE = 0x000A, SETREDRAW = 0x000B, SETTEXT = 0x000C, GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E, PAINT = 0x000F, CLOSE = 0x0010, QUERYENDSESSION = 0x0011, QUIT = 0x0012, QUERYOPEN = 0x0013,
        ERASEBKGND = 0x0014, SYSCOLORCHANGE = 0x0015, SHOWWINDOW = 0x0018, ACTIVATEAPP = 0x001C, SETCURSOR = 0x0020, MOUSEACTIVATE = 0x0021,
        CHILDACTIVATE = 0x0022, QUEUESYNC = 0x0023, GETMINMAXINFO = 0x0024, WINDOWPOSCHANGING = 0x0046, WINDOWPOSCHANGED = 0x0047,
        CONTEXTMENU = 0x007B, STYLECHANGING = 0x007C, STYLECHANGED = 0x007D, DISPLAYCHANGE = 0x007E, GETICON = 0x007F, SETICON = 0x0080,
        NCCREATE = 0x0081, NCDESTROY = 0x0082, NCCALCSIZE = 0x0083, NCHITTEST = 0x0084, NCPAINT = 0x0085, NCACTIVATE = 0x0086,
        GETDLGCODE = 0x0087, SYNCPAINT = 0x0088, NCMOUSEMOVE = 0x00A0, NCLBUTTONDOWN = 0x00A1, NCLBUTTONUP = 0x00A2, NCLBUTTONDBLCLK = 0x00A3,
        NCRBUTTONDOWN = 0x00A4, NCRBUTTONUP = 0x00A5, NCRBUTTONDBLCLK = 0x00A6, NCMBUTTONDOWN = 0x00A7, NCMBUTTONUP = 0x00A8, NCMBUTTONDBLCLK = 0x00A9,
        SYSKEYDOWN = 0x0104, SYSKEYUP = 0x0105, SYSCHAR = 0x0106, SYSDEADCHAR = 0x0107, COMMAND = 0x0111, SYSCOMMAND = 0x0112,
        LBUTTONDOWN = 0x0201, LBUTTONUP = 0x0202, LBUTTONDBLCLK = 0x0203, RBUTTONDOWN = 0x0204, RBUTTONUP = 0x0205, RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207, MBUTTONUP = 0x0208, MBUTTONDBLCLK = 0x0209, MOUSEWHEEL = 0x020A, MOUSEHWHEEL = 0x020E, MOUSEMOVE = 0x0200,
        XBUTTONDOWN = 0x020B, XBUTTONUP = 0x020C, XBUTTONDBLCLK = 0x020D, CAPTURECHANGED = 0x0215, ENTERSIZEMOVE = 0x0231, EXITSIZEMOVE = 0x0232,
        IME_SETCONTEXT = 0x0281, IME_NOTIFY = 0x0282, IME_CONTROL = 0x0283, IME_COMPOSITIONFULL = 0x0284, IME_SELECT = 0x0285, IME_CHAR = 0x0286,
        IME_REQUEST = 0x0288, IME_KEYDOWN = 0x0290, IME_KEYUP = 0x0291, NCMOUSELEAVE = 0x02A2, USER = 0x0400, TRAYMOUSEMESSAGE = 0x800, APP = 0x8000,
        DWMCOMPOSITIONCHANGED = 0x031E, DWMNCRENDERINGCHANGED = 0x031F, DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        DWMWINDOWMAXIMIZEDCHANGE = 0x0321, DWMSENDICONICTHUMBNAIL = 0x0323, DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref User.Mouse.Win32Point pt);
        public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);
        [DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
        private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);
        [DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
        private static extern IntPtr _LocalFree(IntPtr hMem);
        public static string[] CommandLineToArgvW(string cmdLine)
        {
            IntPtr argv = IntPtr.Zero;
            try
            {
                argv = _CommandLineToArgvW(cmdLine, out int numArgs);
                if (argv == IntPtr.Zero)
                    throw new Win32Exception();
                var result = new string[numArgs];
                for (int i = 0; i < numArgs; i++)
                {
                    IntPtr currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(typeof(IntPtr)));
                    result[i] = Marshal.PtrToStringUni(currArg);
                }
                return result;
            }
            finally
            {
                IntPtr p = _LocalFree(argv);
            }
        }
    }

    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }

    public static class Instance<TApplication>
                where TApplication : Application, ISingleInstanceApp
    {
        private const string Delimiter = ":";
        private const string ChannelNameSuffix = "SingeInstanceIPCChannel";
        private const string RemoteServiceName = "SingleInstanceApplicationService";
        private const string IpcProtocol = "ipc://";
        private static Mutex singleInstanceMutex;
        private static IpcServerChannel channel;
        private static IList<string> commandLineArgs;
        public static IList<string> CommandLineArgs => commandLineArgs;

        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            commandLineArgs = GetCommandLineArgs(uniqueName);
            string applicationIdentifier = uniqueName + Environment.UserName;
            string channelName = String.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out bool firstInstance);
            if (firstInstance)
                CreateRemoteService(channelName);
            else
                SignalFirstInstance(channelName, commandLineArgs);
            return firstInstance;
        }
        public static void Cleanup()
        {
            if (singleInstanceMutex != null)
            {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }
            if (channel != null)
            {
                ChannelServices.UnregisterChannel(channel);
                channel = null;
            }
        }
        private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
        {
            string[] args = null;
            if (AppDomain.CurrentDomain.ActivationContext == null)
                args = Environment.GetCommandLineArgs();
            else
            {
                string appFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
                string cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if (File.Exists(cmdLinePath))
                {
                    try
                    {
                        using (TextReader reader = new StreamReader(cmdLinePath, System.Text.Encoding.Unicode))
                            args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
                        File.Delete(cmdLinePath);
                    }
                    catch (IOException) { }
                }
            }
            if (args == null)
                args = new string[] { };

            return new List<string>(args);
        }
        private static void CreateRemoteService(string channelName)
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = TypeFilterLevel.Full
            };
            IDictionary props = new Dictionary<string, string>
            {
                ["name"] = channelName,
                ["portName"] = channelName,
                ["exclusiveAddressUse"] = "false"
            };
            channel = new IpcServerChannel(props, serverProvider);
            ChannelServices.RegisterChannel(channel, true);
            IPCRemoteService remoteService = new IPCRemoteService();
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }
        private static void SignalFirstInstance(string channelName, IList<string> args)
        {
            IpcClientChannel secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);
            string remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;
            IPCRemoteService firstInstanceRemoteServiceReference = (IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);
            firstInstanceRemoteServiceReference?.InvokeFirstInstance(args);
        }
        private static object ActivateFirstInstanceCallback(object arg)
        {
            IList<string> args = arg as IList<string>;
            ActivateFirstInstance(args);
            return null;
        }
        private static void ActivateFirstInstance(IList<string> args)
        {
            if (Application.Current == null)
                return;
            ((TApplication)Application.Current).SignalExternalCommandLineArgs(args);
        }
        private class IPCRemoteService : MarshalByRefObject
        {
            public void InvokeFirstInstance(IList<string> args)
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal, new DispatcherOperationCallback(Instance<TApplication>.ActivateFirstInstanceCallback), args);
            }
            public override object InitializeLifetimeService() => null;
        }
    }
}