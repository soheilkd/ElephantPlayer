using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Player
{
    [Serializable]
    public class Preferences
    {
        [field: NonSerialized]
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();
        [field: NonSerialized]
        private static readonly string DllFilePath = $"{App.Path}SettingsProvider.dll";

        [field: NonSerialized]
        public event EventHandler Changed;

        public PlayMode PlayMode { get; set; }
        public int MainKey { get; set; }
        public double Volume { get; set; }
        public Size LastSize { get; set; }
        public bool VisionOrientation { get; set; }
        public Point LastLoc { get; set; }
        public string LastPath { get; set; }
        public bool LiveLibrary { get; set; }
        public bool ExplicitContent { get; set; }
        public string LibraryLocation { get; set; }
        public string DownloadLocation { get; set; }

        private int _MouseOverTimeOutIndex;
        public int MouseOverTimeoutIndex { get => _MouseOverTimeOutIndex; set { _MouseOverTimeOutIndex = value; Changed?.Invoke(this, null); } }
        public int MouseOverTimeout
        {
            get
            {
                switch (MouseOverTimeoutIndex)
                {
                    case 0: return 500;
                    case 1: return 1000;
                    case 2: return 2000;
                    case 3: return 3000; 
                    case 4: return 4000; 
                    case 5: return 5000; 
                    case 6: return 10000; 
                    case 7: return 60000; 
                    default: return 2000;
                }
            }
        }

        public static Preferences Load()
        {
            using (FileStream stream = File.Open(DllFilePath, FileMode.Open))
                return Formatter.Deserialize(stream) as Preferences;
        }
        public void Save()
        {
            using (FileStream stream = File.Open(DllFilePath, FileMode.Create))
                Formatter.Serialize(stream, this);
        }
    }
    
    public static class Extensions
    {
        /// <summary>
        /// Casts object to specified type T
        /// </summary>
        /// <typeparam name="T">Destinition</typeparam>
        /// <param name="obj">Source</param>
        /// <returns></returns>
        public static T To<T>(this object obj) => (T)obj;
        public static T As<T>(this object obj) where T : class => obj as T;
        public static string ToNewString(this TimeSpan time) => time.ToString("c").Substring(3, 5);
    }

    public static class Images
    {
        public static readonly BitmapImage MusicArt = GetBitmap(Controls.Glyph.Music);
        public static readonly BitmapImage VideoArt = GetBitmap(Controls.Glyph.Video);
        public static readonly BitmapImage NetArt = GetBitmap(Controls.Glyph.Cloud);


        public static BitmapImage GetBitmap(Controls.Glyph glyph, Brush foreground = null, Brush border = null)
        {
            var control = new Controls.MaterialIcon()
            {
                Glyph = glyph,
                Foreground = foreground ?? Brushes.White,
                BorderBrush = border ?? Brushes.White
            };
            control.UpdateLayout();
            if (Double.IsNaN(control.Height))
                control.Height = 50d;
            if (Double.IsNaN(control.Width))
                control.Width = 50d;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Clear();
            Transform transform = control.LayoutTransform;
            control.LayoutTransform = null;
            Size size = new Size(control.Width, control.Height);
            control.Measure(size);
            control.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (Int32)size.Width,
                (Int32)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(control);

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

        public static BitmapImage GetBitmap(TagLib.IPicture picture)
        {
            byte[] pixels = new byte[picture.Data.Count];
            picture.Data.CopyTo(pixels, 0);
            var image = new BitmapImage();
            using (var ms = new MemoryStream(pixels))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }
            picture.Data.Clear();
            pixels = new byte[0];
            return image;
        }
    }
}