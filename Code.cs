using Microsoft.Win32;
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
using System.Xml;
using DColor = System.Drawing.Color;
using Draw = System.Drawing;
using K = Gma.System.MouseKeyHook;
using R = Player.Properties.Resources;
using W = System.Windows.Forms;
namespace Player.User
{
    public static class Text
    {
        public static FontFamily GetFont(int index)
        {
            switch (index)
            {
                case 0: return new FontFamily("Arial");
                case 1: return new FontFamily("Comic Sans MS");
                case 2: return new FontFamily("Courgette");
                case 3: return new FontFamily("Kalam");
                case 4: return new FontFamily("Kalam Light");
                case 5: return new FontFamily("Segoe Print");
                case 6: return new FontFamily("Segoe UI");
                case 7: return new FontFamily("Segoe UI Light");
                case 8: return new FontFamily("Segoe UI SemiLight");
                case 9: return new FontFamily("Segoe UI SemiBold");
                case 10: return new FontFamily("Segoe UI Black");
                case 11: return new FontFamily("Tahoma");
                default: return new FontFamily("Segoe UI");
            }
        }
        public static class Segoe
        {
            public static TextBlock GetTextBlock(string text, double FontSize = 16) => new TextBlock()
            {
                FontFamily = Font,
                Text = text,
                FontSize = FontSize
            };
            public static FontFamily Font { get => new FontFamily("Segoe MDL2 Assets"); }
            public static string Settings => "";//
            public static string Folder => "";//
            public static string File => "";//
            public static string Info => "";
            public static string List => "";//
            public static string Favorite => "";//
            public static string Unfavorite => "";
        }
        public static class FontAwesome
        {
            public static TextBlock GetTextBlock(string text, double FontSize = 16) => new TextBlock()
            {
                FontFamily = Font,
                Text = text,
                FontSize = FontSize
            };
            public static FontFamily Font { get => new FontFamily("FontAwesome"); }
            public static string Settings => "";
            public static string Folder => "";
            public static string File => "";
            public static string Info => "";
            public static string List => "";
            public static string Favorite => "";
            public static string Unfavorite => "";
            public static string[] Sounds => new string[] { "", "", "" };
        }
    }
    public static class Dialogs
    {
        public static (bool ok, string folder) RequestDirectory()
        {
            W::FolderBrowserDialog FBD = new W::FolderBrowserDialog()
            {
                Description = "",
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = false
            };
            return (FBD.ShowDialog() == W::DialogResult.OK, FBD.SelectedPath);
        }
        public static (bool ok, string[] files) RequestFiles(string filter = "", bool multi = true)
        {
            OpenFileDialog FD = new OpenFileDialog()
            {
                Filter = filter,
                CheckFileExists = true,
                Multiselect = multi
            };
            return (FD.ShowDialog().Value, FD.FileNames);
        }
        public static (bool? ok, string path) SaveFile(string Filter = "", string DefaultName = "", string DefaultExt = "", string WindowTitle = "Save")
        {
            SaveFileDialog SFD = new SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                Filter = Filter,
                FileName = DefaultName,
                DefaultExt = DefaultExt,
                Title = WindowTitle
            };
            return (SFD.ShowDialog(), SFD.FileName);
        }
    }
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
        public static int Width => W::Screen.PrimaryScreen.WorkingArea.Width;
        public static int FullWidth => W::Screen.PrimaryScreen.Bounds.Width;
        public static int FullHeight => W::Screen.PrimaryScreen.Bounds.Height;
        public static int Height => W::Screen.PrimaryScreen.WorkingArea.Height;
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
}

namespace Player.Events
{
    public class InstanceEventArgs : EventArgs
    {
        private InstanceEventArgs() { }
        public InstanceEventArgs(IList<string> args) { Args = args; }
        private IList<string> Args { get; set; }
        public string this[int index] => Args[index];
        public int ArgsCount => Args.Count;
    }
}

namespace Player.User
{
    public class Keyboard
    {
        public enum KeyboardKey
        {
            Space = 32,
            Esc = 27,
            Left = 37,
            Up = 38,
            Right = 39,
            Down = 40,
            MediaNext = 176,
            MediaPrevious = 177,
            MediaStop = 178,
            MediaPlayPause = 179,
            Letter_S = 83
        }
        public class KeyPressEventArgs : EventArgs
        {
            public bool Alt { get; set; }
            public bool Ctrl { get; set; }
            public bool Shift { get; set; }
            public KeyboardKey Key { get; set; }
        }
        private KeyPressEventArgs CreateArgs(bool alt, bool ctrl, bool shift, KeyboardKey key) =>
            new KeyPressEventArgs() { Alt = alt, Ctrl = ctrl, Shift = shift, Key = key };
        private K::IKeyboardMouseEvents OnlineKeyboardMouseEvents;
        public delegate void KeyUpEvent(object sender, KeyPressEventArgs e);
        public delegate void KeyDownEvent(object sender, KeyPressEventArgs e);
        public event KeyDownEvent KeyDown;
        public event KeyUpEvent KeyUp;
        private void SubscribeApplication()
        {
            Unsubscribe();
            Subscribe(K::Hook.AppEvents());
        }
        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(K::Hook.GlobalEvents());
        }
        private void Subscribe(K::IKeyboardMouseEvents events)
        {
            OnlineKeyboardMouseEvents = events;
            OnlineKeyboardMouseEvents.KeyDown += (sender, e) => KeyDown?.Invoke(e.KeyCode, CreateArgs(e.Alt, e.Control, e.Shift, (KeyboardKey)(int)e.KeyCode));
            OnlineKeyboardMouseEvents.KeyUp += (sender, e) => KeyUp?.Invoke(e.KeyCode, CreateArgs(e.Alt, e.Control, e.Shift, (KeyboardKey)(int)e.KeyCode));
        }
        private void Unsubscribe()
        {
            if (OnlineKeyboardMouseEvents == null) return;
            OnlineKeyboardMouseEvents.KeyUp -= (sender, e) => KeyDown?.Invoke(e.KeyCode, CreateArgs(e.Alt, e.Control, e.Shift, (KeyboardKey)(int)e.KeyCode));
            OnlineKeyboardMouseEvents.KeyDown -= (sender, e) => KeyDown?.Invoke(e.KeyCode, CreateArgs(e.Alt, e.Control, e.Shift, (KeyboardKey)(int)e.KeyCode));
            OnlineKeyboardMouseEvents.Dispose();
            OnlineKeyboardMouseEvents = null;
        }
        public Keyboard() => SubscribeGlobal();
        public void Dispose() => Unsubscribe();
    }
}

namespace Player.Events
{
    public class PlaybackEventArgs : EventArgs
    {
        public Stretch Stretch { get; set; }
        public StretchDirection StretchDirection { get; set; }
        public double SpeedRatio { get; set; }
        public double SpeakerBalance { get; set; }
    }
    public class MediaEventArgs : EventArgs
    {
        public TagLib.File File { get; set; }
        public Media Media { get; set; }
        public MediaView Sender { get; set; }
        public int Index { get; set; }
    }
    public class SettingsEventArgs
    {
        public Preferences NewSettings { get; set; }
    }
}

namespace Player.Management
{
    public enum SortBy { Artist, Title, AlbumArtist, Name, Path, Album }
    public enum ManagementChange
    {
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
        public Events.MediaEventArgs MediaChanges { get; set; }
        public ManagementChange Change {get;set;}
        public static ManagementChangeEventArgs CreateForTagEditing(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.EditingTag, MediaChanges = e };
        public static ManagementChangeEventArgs CreateForArtwork(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.ArtworkClick, MediaChanges = e };
        public static ManagementChangeEventArgs CreateForArtwork(int index)
        => new ManagementChangeEventArgs() { Change = ManagementChange.ArtworkClick, MediaChanges = new Events.MediaEventArgs() { Index = index } };
        public static ManagementChangeEventArgs CreateForInterfaceUpdate(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.InterfaceUpdate, MediaChanges = e };
        public static ManagementChangeEventArgs CreateForMediaUpdate(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.MediaUpdate, MediaChanges = e };
        public static ManagementChangeEventArgs CreateForPopupRequest(Events.MediaEventArgs e)
        => new ManagementChangeEventArgs() { Change = ManagementChange.PopupRequest, MediaChanges = e };
        public static ManagementChangeEventArgs Default(Events.MediaEventArgs optional = null)
        => new ManagementChangeEventArgs() { Change = ManagementChange.SomethingHappened, MediaChanges = optional };
    }
    public class MediaManager
    {
        public MediaManager()
        {
            UniversalMetaEditor.SaveRequested += UniversalMetaEditor_SaveRequested;
            ActiveViewMode = (ViewMode)Preferences.ViewMode;
        }

        private void UniversalMetaEditor_SaveRequested(object sender, Events.MediaEventArgs e)
        {
            if (CurrentlyPlaying.Path == e.File.Name)
                Change?.Invoke(this, ManagementChangeEventArgs.CreateForTagEditing(e));
            else
                e.File.Save();
        }

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
            "mpeg"
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
        private LyricsView UniversalLyricsView = new LyricsView();
        private MetaEditor UniversalMetaEditor = new MetaEditor();
        public Media CurrentlyPlaying => AllMedias[CurrentlyPlayingIndex];
        public MediaView CurrentlyPlayingView => MediaViews[CurrentlyPlayingIndex];
        public event EventHandler<ManagementChangeEventArgs> Change;
        private List<Media> AllMedias = new List<Media>();
        public List<Controls.GroupView> GroupViews { get; set; } = new List<Controls.GroupView>();
        public List<MediaView> MediaViews { get; set; } = new List<MediaView>();
        public Preferences Preferences { private get; set; } = new Preferences(true);
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
        public static Media GetMedia(string path)
        {
            try
            {
                var file = TagLib.File.Create(path);
                var media = new Media()
                {
                    Album = file.Tag.Album,
                    AlbumArtist = file.Tag.FirstAlbumArtist,
                    Artist = file.Tag.FirstPerformer,
                    Artwork = file.Tag.Pictures.Length >=1?Getters.Image.ToBitmapSource(file.Tag.Pictures[0]):Getters.Image.ToBitmapSource(GetType(path)==MediaType.Music?R.Music:R.Video),
                    MediaType = GetType(path),
                    Date = File.GetLastWriteTime(path),
                    Name = path.Substring(path.LastIndexOf("\\") + 1),
                    Path = path,
                    Title = file.Tag.Title ?? path.Substring(path.LastIndexOf("\\") + 1).Substring(0, path.Substring(path.LastIndexOf("\\") + 1).LastIndexOf("."))
                };
                file.Dispose();
                file = null;
                return media;
            }
            catch (Exception)
            {
                return null;
            }
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
        private int CurrentlyPlayingIndex
        {
            get => currentlyPlayingIndex;
            set
            {
                for (int i = 0; i < MediaViews.Count; i++)
                    MediaViews[i].IsPlaying = false;
                MediaViews[value].IsPlaying = true;
                currentlyPlayingIndex = value;
            }
        }
        private Random Shuffle = new Random(2);

        private void MediaManager_PopupRequested(object sender, Events.MediaEventArgs e)
        {
            Change?.Invoke(this, new ManagementChangeEventArgs() { Change = ManagementChange.PopupRequest, MediaChanges = e });
        }
        private void MediaManager_ArtworkClicked(object sender, Events.MediaEventArgs e)
        {
            Change?.Invoke(this, new ManagementChangeEventArgs()
            {
                Change = ManagementChange.ArtworkClick,
                MediaChanges = new Events.MediaEventArgs()
                {
                    Sender = (MediaView)sender,
                    Index = Find((MediaView)sender)
                }
            });
        }

        public bool Add(string path)
        {
            if (GetType(path) == MediaType.NotMedia)
                return false;
            try
            {
                AllMedias.Add(GetMedia(path));
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                int p = MediaViews.Count;
                MediaViews.Add(new MediaView(Preferences));
                MediaViews[p].ArtworkClicked += MediaManager_ArtworkClicked;
                MediaViews[p].PopupRequested += MediaManager_PopupRequested;
                MediaViews[p].Artwork.Source = AllMedias[p].Artwork;
                MediaViews[p].TitleLabel.Content = AllMedias[p].Title;
                Change?.Invoke(this, new ManagementChangeEventArgs()
                {
                    Change = ManagementChange.InterfaceUpdate,
                    MediaChanges = new Events.MediaEventArgs()
                    {
                        Media = AllMedias[p]
                    }
                });
                switch (ActiveViewMode)
                {
                    case ViewMode.Singular: break;
                    case ViewMode.GroupByArtist:
                        if (FindMatch(MediaViews[p]) == -1)
                            GroupViews.Add(new Controls.GroupView(MediaViews[p], Preferences.TileTheme, AllMedias[p].Artist));
                        else
                            GroupViews[FindMatch(MediaViews[p])].Add(MediaViews[p]);
                        break;
                    case ViewMode.GroupByDir:
                        if (FindMatch(MediaViews[p]) == -1)
                            GroupViews.Add(new Controls.GroupView(MediaViews[p], Preferences.TileTheme, AllMedias[p].Path));
                        else
                            GroupViews[FindMatch(MediaViews[p])].Add(MediaViews[p]);
                        break;
                    case ViewMode.GroupByAlbum:
                        if (FindMatch(MediaViews[p]) == -1)
                            GroupViews.Add(new Controls.GroupView(MediaViews[p], Preferences.TileTheme, AllMedias[p].Album));
                        else
                            GroupViews[FindMatch(MediaViews[p])].Add(MediaViews[p]);
                        break;
                    default:
                        break;
                }
            }
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
        public bool Remove(MediaView view) => Remove(Find(view));
        public bool Remove(int index)
        {
            AllMedias.RemoveAt(index);
            MediaViews.RemoveAt(index);
            Change?.Invoke(this, new ManagementChangeEventArgs() { Change = ManagementChange.InterfaceUpdate });
            return true;
        }

        public bool Delete(string path)
        {
            try
            {
                File.Delete(path);
                Remove(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Delete(Media media) => Delete(media.Path);
        public bool Delete(int index) => Delete(AllMedias[index]);

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
        
        private int Find(MediaView view)
        {
            int i = 0;
            for (; i < MediaViews.Count; i++)
                if (MediaViews[i] == view)
                    break;
            return i;
        }
        private int Find(Media media)
        {
            int i = 0;
            for (; i < AllMedias.Count; i++)
                if (AllMedias[i] == media)
                    break;
            return i;
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
            AllMedias[index] = GetMedia(AllMedias[index].Path);
            MediaViews[index].Artwork.Source = AllMedias[index].Artwork;
            MediaViews[index].UserControl_Loaded(this, null);
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
                    if (GetType(lines[i]) != MediaType.NotMedia)
                        AllMedias.Add(GetMedia(lines[i]));
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
                        list.Add(GetMedia(lines[i]));
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
            var res = W.MessageBox.Show($"Sure? this file will be deleted:\r\n{AllMedias[index]}", " ", W.MessageBoxButtons.OKCancel, W.MessageBoxIcon.Warning);
            if (res == W.DialogResult.OK)
                Delete(index);
        }
        public void RequestDelete(MediaView view) => RequestDelete(Find(view));

        public void RequestMove(int index)
        {
            var (ok, path) = User.Dialogs.SaveFile(MediaManager.GetFilter(AllMedias[index]), AllMedias[index].Name, GetExtension(AllMedias[index]), "Move To...");
            if (ok.Value)
                Move(index, path);
        }
        public void RequestMove(MediaView view) => RequestMove(Find(view));

        public void RequestCopy(int index)
        {
            var (ok, path) = User.Dialogs.SaveFile(GetFilter(AllMedias[index]), AllMedias[index].Name, GetExtension(AllMedias[index]), "Copy To...");
            if (ok.Value)
                Copy(AllMedias[index], path);
        }
        public void RequestCopy(MediaView view) => RequestCopy(Find(view));

        public void RequestLocation(int index) => System.Diagnostics.Process.Start("explorer.exe", "/select," + AllMedias[index].Path);
        public void RequestLocation(MediaView view) => RequestLocation(Find(view));

        public void RequestMetaEdit(MediaView view) => UniversalMetaEditor.Load(AllMedias[Find(view)]);
        public void RequestMetaEdit(int index) => RequestMetaEdit(MediaViews[index]);

        public void ChangeTimespan(TimeSpan timeSpan) => CurrentlyPlayingView.Max = timeSpan.TotalMilliseconds;

        public static string GetExtension(string full) => full.Substring(full.LastIndexOf(".") + 1).ToLower();
        public static string GetExtension(Media media) => GetExtension(media.Path);
        public static string GetFilter(string ext = ".mp3") => $"{GetType(ext)} | *{ext}";
        public static string GetFilter(Media media) => $"{media.MediaType} | *{GetExtension(media.Path)}";

        public void RequestLyrics(MediaView view) => UniversalLyricsView.Load(AllMedias[Find(view)]);
        public void RequestLyrics(int index) => RequestLyrics(MediaViews[index]);

        public int FindMatch(MediaView view)
        {
            for (int i = 0; i < GroupViews.Count; i++)
                if (GroupViews[i].DoesMatch(AllMedias[Find(view)], ActiveViewMode))
                    return i;
            return -1;
        }
        public void SendTagSaveRequest(MediaView view)
        {
            if (Find(view) == CurrentlyPlayingIndex)
            {
                Change?.Invoke(this, new ManagementChangeEventArgs()
                {
                    Change = ManagementChange.EditingTag,
                    MediaChanges = new Events.MediaEventArgs()
                    {
                        File = UniversalMetaEditor.MainFile,
                        Sender = view,
                        Media = AllMedias[Find(view)]
                    }
                });
            }
            else
                UniversalMetaEditor.MainFile.Save();
        }

        public void Clear()
        {
            AllMedias.Clear();
            MediaViews.Clear();
            GroupViews.Clear();
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
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, MediaViews.Count); break;
                case PlayMode.Queue: CurrentlyPlayingIndex = PlayQueue.Dequeue(); break;
                case PlayMode.RepeatAll:
                    if (CurrentlyPlayingIndex != MediaViews.Count - 1) CurrentlyPlayingIndex++;
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
                case PlayMode.Shuffle: CurrentlyPlayingIndex = Shuffle.Next(0, MediaViews.Count); break;
                case PlayMode.Queue: CurrentlyPlayingIndex = PlayQueue.Dequeue(); break;
                case PlayMode.RepeatAll:
                    if (CurrentlyPlayingIndex != 0) CurrentlyPlayingIndex--;
                    else CurrentlyPlayingIndex = AllMedias.Count - 1;
                    break;
                default: break;
            }
            return CurrentlyPlaying;
        }

        public Media Play(MediaView parent) => Next(Find(parent));
        public void Play(Media media) => Next(Find(media));
        public void ChangeSettings(Preferences newSettings)
        {
            Preferences = newSettings;
        }
    }
}

namespace Player
{
    public static class Debug
    {
        public static void Print<T>(T obj) => Console.WriteLine(obj.ToString());
        public static void ThrowFakeException(string Message = "") => throw new Exception(Message);
        public static void Display<T>(T message, string caption = "Debug") => MessageBox.Show(message.ToString(), caption);
    }
    public static class Lyrics
    {
        private static string XMLPath = $@"{App.ExePath}Lyrics.dll";
        public static void CleanUp()
        {
            var newOp = new Dictionary<string, string>();
            foreach (var item in from item in Get() where item.Value != "" select item)
                newOp.Add(item.Key, item.Value);
            Set(newOp);
        }
        public static void Set(string UniqueKey, string Lyrics)
        {
            var op = Get();
            bool found = false;
            foreach (var item in Get())
                if (item.Key == UniqueKey)
                {
                    op[item.Key] = Lyrics;
                    found = true;
                }
            if (found) { Set(op); return; }
            op.Add(UniqueKey, Lyrics);
            Set(op);
        }
        public static string Get(string UniqueKey)
        {
            var test = Get();
            var op = from item in Get() where item.Key == UniqueKey select item.Value;
            return op.ToArray().Length == 0 ? "" : $"{op.First()}";
        }
        public static Dictionary<string, string> Get()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            using (XmlReader reader = XmlReader.Create(XMLPath))
            {
                while (reader.Read())
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Lyrics") dict.Add(reader.GetAttribute("Key"), reader.ReadInnerXml());
            }
            return dict;
        }
        public static void Set(Dictionary<string, string> dictionary)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            XmlWriter writer = XmlWriter.Create(XMLPath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("ELP_Provider");
            foreach (var item in dictionary)
            {
                writer.WriteStartElement("Lyrics");
                writer.WriteStartAttribute("Key");
                writer.WriteString(item.Key);
                writer.WriteEndAttribute();
                writer.WriteString(item.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Dispose();
            writer = null;
        }
    }

}

namespace Player.Taskbar
{
    public class Thumb
    {
#pragma warning disable CS0067
        public static class Commands
        {
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
        public ThumbButtonInfo PlayThumb = new ThumbButtonInfo()
        {
            IsInteractive = true,
            IsEnabled = true,
            IsBackgroundVisible = true,
            DismissWhenClicked = false,
            CommandParameter = "",
            Visibility = Visibility.Visible,
            Description = "Play",
            ImageSource = Getters.Image.ToBitmapSource(R.Play)
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
            ImageSource = Getters.Image.ToBitmapSource(R.Pause)
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
            ImageSource = Getters.Image.ToBitmapSource(R.Prev)
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
            ImageSource = Getters.Image.ToBitmapSource(R.Next)
        };
        public event EventHandler PlayPressed;
        public event EventHandler PausePressed;
        public event EventHandler PrevPressed;
        public event EventHandler NextPressed;
        TaskbarItemInfo TaskbarItem = new TaskbarItemInfo();
        Commands.Play PlayHandler = new Commands.Play();
        Commands.Pause PauseHandler = new Commands.Pause();
        Commands.Previous PrevHandler = new Commands.Previous();
        Commands.Next NextHandler = new Commands.Next();
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
    public enum WindowMode { Default, VideoWithControls, VideoWithoutControls, Auto }
    public enum PlayMode { Single, Shuffle, RepeatOne, RepeatAll, Queue }
    public enum MediaViewMode { Default, Compact, Expanded }
    public enum MediaComparsion { Title, Name, Path, Type, Artist, Album }
    public enum MediaType { Music, Video, NotMedia }
    public class Media : IDisposable
    {
        public bool IsMedia => MediaType != MediaType.NotMedia;
        public Media() { }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public string AlbumArtist { get; set; }
        public long Length { get; set; }
        public BitmapSource Artwork { get; set; }
        public MediaType MediaType;
        public DateTime Date;
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
            AlbumArtist = "",
            Artwork = null,
            Date = new DateTime(0),
            disposedValue = false,
            MediaType = MediaType.NotMedia,
            Name = "",
            Path = "",
            Title = ""
        };
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
                    AlbumArtist = null;
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

namespace Player.Getters
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
        public static Draw::Icon MainIcon => R.PlayerIcon;
        public static Draw::Icon TaskbarIcon => R.TaskbarIcon;
        public static BitmapSource MainImage => ToBitmapSource(R.PlayerImage);
        public static BitmapSource TaskbarImage => ToBitmapSource(R.PlayerImage);
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
        public static Draw.Image ToImage(TagLib.IPicture picture) => Draw.Image.FromStream(new MemoryStream(picture.Data.Data));
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

namespace Player.Styling
{
    public class Theme
    {
        public string Name { get; set; }
        private DColor _Background;
        private DColor _Text;
        private DColor _Bars;
        private DColor _Buttons;
        
        public DColor Background { get => _Background; set { _Background = value; BackgroundBrush = Getters.Theming.ToBrush(value); } }
        public DColor Context { get => _Text; set { _Text = value; ContextBrush = Getters.Theming.ToBrush(value); } }
        public DColor Bars { get => _Bars; set { _Bars = value; BarsBrush = Getters.Theming.ToBrush(value); } }
        public DColor Buttons { get => _Buttons; set { _Buttons = value; ButtonsBrush = Getters.Theming.ToBrush(value); } }
        public Brush BackgroundBrush { get; private set; }
        public Brush ContextBrush { get; private set; }
        public Brush BarsBrush { get; private set; }
        public Brush ButtonsBrush { get; private set; }
        public static Theme Get(int id = 0) => DefaultThemes[id];
        public static Theme Create(
            (byte A, byte R, byte G, byte B) back,
            (byte A, byte R, byte G, byte B) context,
            (byte A, byte R, byte G, byte B) bars,
            (byte A, byte R, byte G, byte B) buttons,
            string name = "NilTheme")
        {
            return new Theme()
            {
                Name = name,
                Background = DColor.FromArgb(back.A, back.R, back.G, back.B),
                Context = DColor.FromArgb(context.A, context.R, context.G, context.B),
                Bars = DColor.FromArgb(bars.A, bars.R, bars.G, bars.B),
                Buttons = DColor.FromArgb(buttons.A, buttons.R, buttons.G, buttons.B)
            };
        }
        public static Theme Create(string name, DColor back, DColor context, DColor bars, DColor buttons)
        {
            return new Theme() { Background = back, Bars = bars, Buttons = buttons, Context = context, Name = name };
        }
        private Theme() { }
        private static Theme[] DefaultThemes = new Theme[]
            {
                Create(back:(255, 255, 255, 255), context:(255, 20, 20, 20), bars:(20, 25, 25, 25), buttons:(255, 125, 125, 125), name: "Light"),
                Create(back:(255, 125, 125, 125), context:(255, 20, 20, 20), bars:(255, 200, 200, 200), buttons:(255, 255, 255, 255), name: "Gray"),
                Create(back:(255, 25, 25, 25), context:(255, 250, 250, 250),bars: (255, 75, 125, 155), buttons:(255, 175, 175, 175), name: "Dark"),
                Create(back:(255, 0, 127, 255), context:(255, 20, 20, 20), bars:(200, 255, 255, 255), buttons:(255, 255, 255, 255), name: "Azure"),
                Create(back:(245, 175, 0, 0), context:(255, 20, 20, 20), bars:(20, 255, 255, 255), buttons:(255, 255, 255, 255), name: "Cherry")
            };

    }

    namespace XAML
    {
        namespace Margin
        {
            public static class MediaView
            {
                public static Thickness Artwork(MediaViewMode mode, bool isPlaying = true)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return new Thickness(1, isPlaying ? 2 : 1, 0, 0);
                        case MediaViewMode.Compact: return new Thickness(0);
                        case MediaViewMode.Expanded: return new Thickness(2, 23, 0, 0);
                        default: return new Thickness(0, 0, 0, 0);
                    }
                }
                public static Thickness TitleLabel(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return new Thickness(0, 110, 0, 0);
                        case MediaViewMode.Compact: return new Thickness(0);
                        case MediaViewMode.Expanded: return new Thickness(0, -3, 0, 0);
                        default: return new Thickness(0, 0, 0, 0);
                    }
                }
            }
        }
        namespace Size
        {
            public static class GroupView
            {
                public static System.Windows.Size Self => new System.Windows.Size(250, 125);
            }
            public static class MediaView
            {
                public static (int h, int w) Artwork(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return (110, 110);
                        case MediaViewMode.Compact: return (0, 0);
                        case MediaViewMode.Expanded: return (199, 199);
                        default: return (135, 135);
                    }
                }
                public static double TitleLabel(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return 110;
                        case MediaViewMode.Compact: return double.NaN;
                        case MediaViewMode.Expanded: return double.NaN;
                        default: return double.NaN;
                    }
                }
                public static System.Windows.Size Self(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return new System.Windows.Size(110, 140);
                        case MediaViewMode.Compact: return new System.Windows.Size(170, 40);
                        case MediaViewMode.Expanded: return new System.Windows.Size(200, 233);
                        default: return new System.Windows.Size();
                    }
                }
            }
        }
    }
}