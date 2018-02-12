using Microsoft.Win32;
using Player.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
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

namespace Player.InstanceManager
{
    internal enum WM
    {
        NULL = 0x0000,
        CREATE = 0x0001,
        DESTROY = 0x0002,
        MOVE = 0x0003,
        SIZE = 0x0005,
        ACTIVATE = 0x0006,
        SETFOCUS = 0x0007,
        KILLFOCUS = 0x0008,
        ENABLE = 0x000A,
        SETREDRAW = 0x000B,
        SETTEXT = 0x000C,
        GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E,
        PAINT = 0x000F,
        CLOSE = 0x0010,
        QUERYENDSESSION = 0x0011,
        QUIT = 0x0012,
        QUERYOPEN = 0x0013,
        ERASEBKGND = 0x0014,
        SYSCOLORCHANGE = 0x0015,
        SHOWWINDOW = 0x0018,
        ACTIVATEAPP = 0x001C,
        SETCURSOR = 0x0020,
        MOUSEACTIVATE = 0x0021,
        CHILDACTIVATE = 0x0022,
        QUEUESYNC = 0x0023,
        GETMINMAXINFO = 0x0024,

        WINDOWPOSCHANGING = 0x0046,
        WINDOWPOSCHANGED = 0x0047,

        CONTEXTMENU = 0x007B,
        STYLECHANGING = 0x007C,
        STYLECHANGED = 0x007D,
        DISPLAYCHANGE = 0x007E,
        GETICON = 0x007F,
        SETICON = 0x0080,
        NCCREATE = 0x0081,
        NCDESTROY = 0x0082,
        NCCALCSIZE = 0x0083,
        NCHITTEST = 0x0084,
        NCPAINT = 0x0085,
        NCACTIVATE = 0x0086,
        GETDLGCODE = 0x0087,
        SYNCPAINT = 0x0088,
        NCMOUSEMOVE = 0x00A0,
        NCLBUTTONDOWN = 0x00A1,
        NCLBUTTONUP = 0x00A2,
        NCLBUTTONDBLCLK = 0x00A3,
        NCRBUTTONDOWN = 0x00A4,
        NCRBUTTONUP = 0x00A5,
        NCRBUTTONDBLCLK = 0x00A6,
        NCMBUTTONDOWN = 0x00A7,
        NCMBUTTONUP = 0x00A8,
        NCMBUTTONDBLCLK = 0x00A9,

        SYSKEYDOWN = 0x0104,
        SYSKEYUP = 0x0105,
        SYSCHAR = 0x0106,
        SYSDEADCHAR = 0x0107,
        COMMAND = 0x0111,
        SYSCOMMAND = 0x0112,

        MOUSEMOVE = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        LBUTTONDBLCLK = 0x0203,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MBUTTONDBLCLK = 0x0209,
        MOUSEWHEEL = 0x020A,
        XBUTTONDOWN = 0x020B,
        XBUTTONUP = 0x020C,
        XBUTTONDBLCLK = 0x020D,
        MOUSEHWHEEL = 0x020E,


        CAPTURECHANGED = 0x0215,

        ENTERSIZEMOVE = 0x0231,
        EXITSIZEMOVE = 0x0232,

        IME_SETCONTEXT = 0x0281,
        IME_NOTIFY = 0x0282,
        IME_CONTROL = 0x0283,
        IME_COMPOSITIONFULL = 0x0284,
        IME_SELECT = 0x0285,
        IME_CHAR = 0x0286,
        IME_REQUEST = 0x0288,
        IME_KEYDOWN = 0x0290,
        IME_KEYUP = 0x0291,

        NCMOUSELEAVE = 0x02A2,

        DWMCOMPOSITIONCHANGED = 0x031E,
        DWMNCRENDERINGCHANGED = 0x031F,
        DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

        #region Windows 7
        DWMSENDICONICTHUMBNAIL = 0x0323,
        DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
        #endregion

        USER = 0x0400,

        // This is the hard-coded message value used by WinForms for Shell_NotifyIcon.
        // It's relatively safe to reuse.
        TRAYMOUSEMESSAGE = 0x800, //WM_USER + 1024
        APP = 0x8000,
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref User.Mouse.Win32Point pt);
        /// <summary>
        /// Delegate declaration that matches WndProc signatures.
        /// </summary>
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
                {
                    throw new Win32Exception();
                }
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
                // Otherwise LocalFree failed.
                // Assert.AreEqual(IntPtr.Zero, p);
            }
        }

    }

    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }

    /// <summary>
    /// This class checks to make sure that only one instance of 
    /// this application is running at a time.
    /// </summary>
    /// <remarks>
    /// Note: this class should be used with some caution, because it does no
    /// security checking. For example, if one instance of an app that uses this class
    /// is running as Administrator, any other instance, even if it is not
    /// running as Administrator, can activate it with command line arguments.
    /// For most apps, this will not be much of an issue.
    /// </remarks>
    public static class Instance<TApplication>
                where TApplication : Application, ISingleInstanceApp

    {
        #region Private Fields

        /// <summary>
        /// String delimiter used in channel names.
        /// </summary>
        private const string Delimiter = ":";

        /// <summary>
        /// Suffix to the channel name.
        /// </summary>
        private const string ChannelNameSuffix = "SingeInstanceIPCChannel";

        /// <summary>
        /// Remote service name.
        /// </summary>
        private const string RemoteServiceName = "SingleInstanceApplicationService";

        /// <summary>
        /// IPC protocol used (string).
        /// </summary>
        private const string IpcProtocol = "ipc://";

        /// <summary>
        /// Application mutex.
        /// </summary>
        private static Mutex singleInstanceMutex;

        /// <summary>
        /// IPC channel for communications.
        /// </summary>
        private static IpcServerChannel channel;

        /// <summary>
        /// List of command line arguments for the application.
        /// </summary>
        private static IList<string> commandLineArgs;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets list of command line arguments for the application.
        /// </summary>
        public static IList<string> CommandLineArgs
        {
            get { return commandLineArgs; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the instance of the application attempting to start is the first instance. 
        /// If not, activates the first instance.
        /// </summary>
        /// <returns>True if this is the first instance of the application.</returns>
        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            commandLineArgs = GetCommandLineArgs(uniqueName);

            // Build unique application Id and the IPC channel name.
            string applicationIdentifier = uniqueName + Environment.UserName;

            string channelName = String.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);

            // Create mutex based on unique application Id to check if this is the first instance of the application. 
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out bool firstInstance);
            if (firstInstance)
            {
                CreateRemoteService(channelName);
            }
            else
            {
                SignalFirstInstance(channelName, commandLineArgs);
            }

            return firstInstance;
        }

        /// <summary>
        /// Cleans up single-instance code, clearing shared resources, mutexes, etc.
        /// </summary>
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets command line args - for ClickOnce deployed applications, command line args may not be passed directly, they have to be retrieved.
        /// </summary>
        /// <returns>List of command line arg strings.</returns>
        private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
        {
            string[] args = null;
            if (AppDomain.CurrentDomain.ActivationContext == null)
            {
                // The application was not clickonce deployed, get args from standard API's
                args = Environment.GetCommandLineArgs();
            }
            else
            {
                // The application was clickonce deployed
                // Clickonce deployed apps cannot recieve traditional commandline arguments
                // As a workaround commandline arguments can be written to a shared location before 
                // the app is launched and the app can obtain its commandline arguments from the 
                // shared location               
                string appFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);

                string cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if (File.Exists(cmdLinePath))
                {
                    try
                    {
                        using (TextReader reader = new StreamReader(cmdLinePath, System.Text.Encoding.Unicode))
                        {
                            args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
                        }

                        File.Delete(cmdLinePath);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            if (args == null)
            {
                args = new string[] { };
            }

            return new List<string>(args);
        }

        /// <summary>
        /// Creates a remote service for communication.
        /// </summary>
        /// <param name="channelName">Application's IPC channel name.</param>
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

            // Create the IPC Server channel with the channel properties
            channel = new IpcServerChannel(props, serverProvider);

            // Register the channel with the channel services
            ChannelServices.RegisterChannel(channel, true);

            // Expose the remote service with the REMOTE_SERVICE_NAME
            IPCRemoteService remoteService = new IPCRemoteService();
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }

        /// <summary>
        /// Creates a client channel and obtains a reference to the remoting service exposed by the server - 
        /// in this case, the remoting service exposed by the first instance. Calls a function of the remoting service 
        /// class to pass on command line arguments from the second instance to the first and cause it to activate itself.
        /// </summary>
        /// <param name="channelName">Application's IPC channel name.</param>
        /// <param name="args">
        /// Command line arguments for the second instance, passed to the first instance to take appropriate action.
        /// </param>
        private static void SignalFirstInstance(string channelName, IList<string> args)
        {
            IpcClientChannel secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);

            string remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;

            // Obtain a reference to the remoting service exposed by the server i.e the first instance of the application
            IPCRemoteService firstInstanceRemoteServiceReference = (IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);

            // Check that the remote service exists, in some cases the first instance may not yet have created one, in which case
            // the second instance should just exit
            if (firstInstanceRemoteServiceReference != null)
            {
                // Invoke a method of the remote service exposed by the first instance passing on the command line
                // arguments and causing the first instance to activate itself
                firstInstanceRemoteServiceReference.InvokeFirstInstance(args);
            }
        }

        /// <summary>
        /// Callback for activating first instance of the application.
        /// </summary>
        /// <param name="arg">Callback argument.</param>
        /// <returns>Always null.</returns>
        private static object ActivateFirstInstanceCallback(object arg)
        {
            // Get command line args to be passed to first instance
            IList<string> args = arg as IList<string>;
            ActivateFirstInstance(args);
            return null;
        }

        /// <summary>
        /// Activates the first instance of the application with arguments from a second instance.
        /// </summary>
        /// <param name="args">List of arguments to supply the first instance of the application.</param>
        private static void ActivateFirstInstance(IList<string> args)
        {
            // Set main window state and process command line args
            if (Application.Current == null)
            {
                return;
            }

            ((TApplication)Application.Current).SignalExternalCommandLineArgs(args);
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Remoting service class which is exposed by the server i.e the first instance and called by the second instance
        /// to pass on the command line arguments to the first instance and cause it to activate itself.
        /// </summary>
        private class IPCRemoteService : MarshalByRefObject
        {
            /// <summary>
            /// Activates the first instance of the application.
            /// </summary>
            /// <param name="args">List of arguments to pass to the first instance.</param>
            public void InvokeFirstInstance(IList<string> args)
            {
                if (Application.Current != null)
                {
                    // Do an asynchronous call to ActivateFirstInstance function
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal, new DispatcherOperationCallback(Instance<TApplication>.ActivateFirstInstanceCallback), args);
                }
            }

            /// <summary>
            /// Remoting Object's ease expires after every 5 minutes by default. We need to override the InitializeLifetimeService class
            /// to ensure that lease never expires.
            /// </summary>
            /// <returns>Always null.</returns>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }

        #endregion
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
        public Types.Media Media {
            get
            {
                if (_Media != null) return _Media;
                if (Sender != null) return Sender.Media;
                return Types.Media.Empty;
            }
            set => _Media = value;
        }
        private Types.Media _Media;
        public MediaView Sender { get; set; }
        public TagLib.File File { get; set; }
    }
    public class SettingsEventArgs
    {
        public Preferences NewSettings { get; set; }
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

namespace Player.Enums
{
    public enum ViewMode { Singular, GroupByArtist, GroupByDir, GroupByAlbum }
    public enum ContextTool { Border, MainDisplay }
    public enum Orientation { Portrait, Landscape }
    public enum PlayPara { None, Next, Prev }
    public enum WindowMode { Default, VideoWithControls, VideoWithoutControls, Auto }
    public enum PlayMode { Single, Shuffle, RepeatOne, RepeatAll }
    public enum MediaViewMode { Default, Compact, Expanded }
    public enum MediaComparsion { Title, Name, Path, Type, Artist, Album }
}

namespace Player.Types
{
    public class Media : IDisposable
    {
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
        private Media() { }
        public Media(string FilePath)
        {
            try
            {
                var file = TagLib.File.Create(FilePath);
                Path = FilePath;
                Name = Path.Substring(Path.LastIndexOf("\\") + 1, Path.LastIndexOf(".") - Path.LastIndexOf("\\") - 1);
                Title = file.Tag.Title ?? "";
                Artist = file.Tag.FirstPerformer;
                Album = file.Tag.Album ?? "Unknown";
                AlbumArtist = file.Tag.FirstAlbumArtist ?? "Unknown";
                Date = File.GetLastWriteTime(Path);
                MediaType = GetType(Path);
                Length = file.Length;
                try { Artwork = Getters.Image.ToBitmapSource(file.Tag.Pictures[0]); }
                catch (ArgumentOutOfRangeException)
                {
                    Artwork = MediaType == Type.Music ? ConvertBitmap(R.Music) : ConvertBitmap(R.Video);
                }
                file.Dispose();
            }
            catch (Exception) { CorruptFile(FilePath); }
        }
        public string Filter() => $"{MediaType} | *{GetExt()}";
        public enum Type { Music, Video, NotMedia }
        public static string[] SupportedMusics = new string[]
        {
            "mp3",
            "wma",
            "aac",
            "m4a"
        };
        public static string[] SupportedVideos = new string[]
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
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Path;
        public string AlbumArtist;
        public long Length;
        public BitmapSource Artwork { get; set; }
        public Type MediaType;
        public DateTime Date;
        public static BitmapSource ConvertBitmap(Draw.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        public static IEnumerable<Media> GetEnum(string[] files)
        {
            var r = from item in files where GetType(item) != Type.NotMedia select item;
            foreach (var item in r) { yield return new Media(item); }
        }
        public bool Contains(String para)
        {
            return Name.ToLower().Contains(para.ToLower())
                || Title.ToLower().Contains(para.ToLower())
                || Album.ToLower().Contains(para.ToLower())
                || Artist.ToLower().Contains(para.ToLower());
        }
        public static Type GetType(string FileName)
        {
            if (!File.Exists(FileName)) return Type.NotMedia;
            string ext = FileName.Substring(FileName.LastIndexOf(".") + 1).ToLower();
            for (int i = 0; i < SupportedMusics.Length; i++)
                if (ext == SupportedMusics[i]) return Type.Music;
            for (int i = 0; i < SupportedVideos.Length; i++)
                if (ext == SupportedVideos[i]) return Type.Video;
            return Type.NotMedia;
        }
        public string GetExt() => Path.Substring(Path.LastIndexOf("."));
        public override string ToString() => Path;
        private void CorruptFile(string path)
        {
            Path = path;
            Name = Path.Substring(Path.LastIndexOf("\\") + 1, Path.LastIndexOf(".") - Path.LastIndexOf("\\") - 1);
            Title = Name;
            Artist = "Unknown";
            Album = "Unknown";
            AlbumArtist = "Unknown";
            Date = File.GetLastAccessTime(Path);
            MediaType = GetType(Path);
            Artwork = MediaType == Type.Music ? ConvertBitmap(Properties.Resources.Music) : ConvertBitmap(Properties.Resources.Video);
        }
        public int GetIndex(Media[] array)
        {
            for (int i = 0; i < array.Length; i++)
                if (Equals(array[i])) return i;
            return -1;
        }
        public static readonly Media Empty = new Media()
        {
            Artist = "",
            Album = "",
            AlbumArtist = "",
            Artwork = null,
            Date = new DateTime(0),
            disposedValue = false,
            Length = 0,
            MediaType = Type.NotMedia,
            Name = "",
            Path = "",
            Title = ""
        };
        public static IEnumerable<Media> Sort(Media[] array, MediaComparsion sortBy  = MediaComparsion.Path)
        {
            switch (sortBy)
            {
                case MediaComparsion.Title: return from item in array orderby item.Title select item;
                case MediaComparsion.Name: return from item in array orderby item.Name select item;
                case MediaComparsion.Path: return from item in array orderby item.Path select item;
                case MediaComparsion.Type: return from item in array orderby item.MediaType select item;
                case MediaComparsion.Artist: return from item in array orderby item.Artist select item;
                case MediaComparsion.Album: return from item in array orderby item.Album select item;
                default: return from item in array orderby item select item;
            }
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
                Album = null;
                AlbumArtist = null;
                Artist = null;
                Artwork = null;
                Length = 0;
                MediaType = 0;
                Name = null;
                Path = null;
                Title = null;

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

    public class Playlist
    {
        public static bool IsPlaylist(string path, out Playlist output)
        {
            if (!path.EndsWith(".elp"))
            {
                output = null;
                return false;
            }
            else
            {
                output = new Playlist(path);
                return true;
            }
        }
        private List<string> _Tracks = new List<string>();
        public int Count { get => _Tracks.Count; }
        public string this[int index] { get => Tracks[index]; }
        public string this[string str]
        {
            get
            {
                for (int i = 0; i < _Tracks.Count; i++)
                    if (_Tracks[i] == str) return Tracks[i];
                throw new ArgumentException("Failed to find requested media");
            }
        }
        public Playlist(Media[] medias)
        {
            for (int i = 0; i < medias.Length; i++)
                 _Tracks.Add(medias[i].Path);
        }
        public string[] Tracks => _Tracks.ToArray();
        public Media[] TracksMedia
        {
            get
            {
                Media[] s = new Media[_Tracks.Count];
                for (int i = 0; i < _Tracks.Count; i++)
                    s[i] = new Media(Tracks[i]);
                return s;
            }
        }
        
        public Playlist() { }
        public Playlist(FileInfo file) : this(file.FullName) { }
        public Playlist(string path) => LoadPlaylist(File.ReadAllLines(path));
        private void LoadPlaylist(string Text) => LoadPlaylist(Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        private void LoadPlaylist(string[] lines, bool HandleCorruptFiles = false)
        {
            if (HandleCorruptFiles)
                _Tracks = new List<string>(
                    from item
                    in lines
                    where (new Media(item)).MediaType != Media.Type.NotMedia
                    select item);
            else
                _Tracks = new List<string>(lines);
        }
        public static void SavePlaylist(Playlist playlist, string path) => File.WriteAllLines(path, playlist.Tracks);
        public static void SavePlaylist(Media[] medias, string path)
        {
            Playlist export = new Playlist();
            for (int i = 0; i < medias.Length; i++)
                export.Add(medias[i].Path);
            SavePlaylist(export, path);
        }
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < _Tracks.Count; i++)
                output.AppendLine(Tracks[i]);
            return output.ToString();
        }
        public static bool operator ==(Playlist para1, Playlist para2)
        {
            if (para1._Tracks.Count != para2._Tracks.Count) return false;
            bool CurrentSame = false;
            for (int i = 0; i < para1.Count; i++)
            {
                CurrentSame = false;
                for (int j = 0; j < para2.Count; j++)
                    if (para1.Tracks[i] == para2.Tracks[j])
                    {
                        CurrentSame = true;
                        break;
                    }
                if (!CurrentSame) return false;
            }
            return true;
        }
        public static bool operator !=(Playlist para1, Playlist para2) => !(para1 == para2);
        public void Add(params string[] files)
        {
            for (int i = 0; i < files.Length; i++)
                _Tracks.Add(files[i]);
        }
        public void Remove(params string[] files)
        {
            for (int i = 0; i < files.Length; i++)
                for (int j = 0; j < _Tracks.Count; j++)
                    if (_Tracks[j] == files[i])
                    {
                        _Tracks.RemoveAt(j);
                        break;
                    }
        }
        public override bool Equals(object obj) => this == obj as Playlist;
        public override int GetHashCode() => base.GetHashCode();
        private int EnumCounter = 0;
        public bool MoveNext()
        {
            return ++EnumCounter >= Count;
        }
        public void Reset()
        {
            EnumCounter = 0;
        }
        public void Dispose()
        {
            for (int i = 0; i < Count; i++)
                _Tracks[i] = String.Empty;
            _Tracks = null;
        }
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
                        case MediaViewMode.Default: return new Thickness(0, 135, 0, 0);
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
                public static System.Windows.Size Self => new System.Windows.Size(355, 170);
            }
            public static class MediaView
            {
                public static (int h, int w) Artwork(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return (138, 138);
                        case MediaViewMode.Compact: return (0, 0);
                        case MediaViewMode.Expanded: return (199, 199);
                        default: return (135, 135);
                    }
                }
                public static double TitleLabel(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return 150;
                        case MediaViewMode.Compact: return double.NaN;
                        case MediaViewMode.Expanded: return double.NaN;
                        default: return double.NaN;
                    }
                }
                public static (int h, int w) Self(MediaViewMode mode)
                {
                    switch (mode)
                    {
                        case MediaViewMode.Default: return (170, 140);
                        case MediaViewMode.Compact: return (40, 185);
                        case MediaViewMode.Expanded: return (233, 200);
                        default: return (100, 100);
                    }
                }
            }
        }
    }
}