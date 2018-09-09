using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Draw = System.Drawing;

namespace Player.Controls
{
	internal static class IconProvider
	{
		private static readonly ResourceDictionary Icons = new ResourceDictionary()
		{
			Source = new Uri("/PlayerLibrary;component/Controls/Icons.xaml", UriKind.RelativeOrAbsolute)
		};

		internal static StreamGeometry GetPath(IconType type)
		{
			return (StreamGeometry)Icons[type.ToString()];
		}

		internal static BitmapSource GetBitmap(IconType type, SolidColorBrush brush = null)
		{
			Icon control = new Icon()
			{
				Type = type,
				Foreground = brush ?? Brushes.White,
				OpacityMask = Brushes.White,
				Height = 50,
				Width = 50
			};
			control.UpdateLayout();

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			control.LayoutTransform = null;
			Size size = new Size(50, 50);
			control.Measure(size);
			control.Arrange(new Rect(size));

			RenderTargetBitmap render = new RenderTargetBitmap(23, 23, 96d, 96d, PixelFormats.Pbgra32);
			render.Render(control);
			encoder.Frames.Add(BitmapFrame.Create(render));

			MemoryStream stream = new MemoryStream();
			encoder.Save(stream);
			stream.Flush();
			var output = new BitmapImage();
			output.BeginInit();
			output.StreamSource = stream;
			output.EndInit();
			return output;
		}
	}
}