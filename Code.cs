using Player.Events;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Draw = System.Drawing;

namespace Player
{
    [Serializable]
    public class Preferences
    {
        [field: NonSerialized]
        public event EventHandler Changed;

        private int _MOT;

        public PlayMode PlayMode { get; set; }
        public int MainKey { get; set; } = 0;
        public double Volume { get; set; } = 1;
        public Size LastSize { get; set; }
        public bool VisionOrientation { get; set; }
        public Point LastLoc { get; set; } = new Point(20, 20);
        public string LastPath { get; set; }
        public int MouseOverTimeoutIndex { get => _MOT; set { _MOT = value; Changed?.Invoke(this, null); } }
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
            using (FileStream stream = File.Open($"{App.Path}SettingsProvider.dll", FileMode.Open))
                return (Preferences)(new BinaryFormatter()).Deserialize(stream);
        }
        public void Save()
        {
            using (FileStream stream = File.Open($"{App.Path}SettingsProvider.dll", FileMode.Create))
                (new BinaryFormatter()).Serialize(stream, this);
        }
    }
    
    public static class Extensions
    {
        public static int ToInt(this double e) => Convert.ToInt32(e);
        public static T CastTo<T>(this object obj) => (T)obj;
        public static T As<T>(this object obj) where T : class => obj as T;
        public static string ToCustomString(this TimeSpan time) => time.ToString("c").Substring(3, 5);
    }

    public static class Global
    {
        public static string CastTime(int ms) => (new TimeSpan(0, 0, 0, 0, ms)).ToString($"");
        public static MenuItem GetMenu(string header, RoutedEventHandler onClick)
        {
            var menu = new MenuItem() { Header = header };
            menu.Click += onClick;
            return menu;
        }
        public static MenuItem GetMenu(string header, (string subItem, RoutedEventHandler onClick)[] subItems)
        {
            var output = new MenuItem() { Header = header };
            for (int i = 0; i < subItems.Length; i++)
                output.Items.Add(GetMenu(subItems[i].subItem, subItems[i].onClick));
            return output;
        }
    }
}

namespace Player.Imaging
{
    public static class Images
    {
        public static BitmapImage MusicArt;
        public static BitmapImage VideoArt;
        public static BitmapImage NetArt;
        public static void Initialize()
        {
            Get.Bitmap(new Controls.SegoeIcon() { Width = 1, Height = 1 });
            MusicArt = Get.Bitmap(new Controls.SegoeIcon() { Glyph = Controls.Glyph.MusicInfo, Foreground = Brushes.White, Width = 60, Height = 60 });
            VideoArt = Get.Bitmap(new Controls.SegoeIcon() { Glyph = Controls.Glyph.Video, Foreground = Brushes.White, Width = 60, Height = 60 });
            NetArt = Get.Bitmap(new Controls.SegoeIcon() { Glyph = Controls.Glyph.Cloud, Foreground = Brushes.White, Width = 60, Height = 60 });
        }
    }
    public static class Get
    {
        public static BitmapImage Bitmap<T>(T element) where T : System.Windows.Controls.Control
        {
            //element.BeginInit();
            element.UpdateLayout();
            if (Double.IsNaN(element.Height))
                element.Height = 50d;
            if (Double.IsNaN(element.Width))
                element.Width = 50d;
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
        public static Draw.Image Image(TagLib.IPicture picture) => Draw.Image.FromStream(new MemoryStream(picture?.Data.Data));

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
}