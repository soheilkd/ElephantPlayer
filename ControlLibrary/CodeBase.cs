using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Player.Controls
{
	public static class Extensions
	{
		public static string ToNewString(this TimeSpan time) => time.ToString("c").Substring(3, 5);
		
		public static BitmapImage GetBitmap(this IconKind icon, Brush foreground = null)
		{
			var control = new MaterialButton()
			{
				Icon = icon,
				Foreground = foreground ?? Brushes.White,
				Background = Brushes.Transparent
			};
			control.UpdateLayout();
			control.Height = 100;
			control.Width = 100;
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
	}

}
