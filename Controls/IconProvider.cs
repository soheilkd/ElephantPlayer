using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Player.Controls
{
	internal static class IconProvider
	{
		internal static Dictionary<IconType, Geometry> Geometries = new Dictionary<IconType, Geometry>
		{
			{ IconType.Previous, Geometry.Parse("M6,18V6H8V18H6M9.5,12L18,6V18L9.5,12Z") },
			{ IconType.Play, Geometry.Parse("M8,5.14V19.14L19,12.14L8,5.14Z") },
			{ IconType.Pause,Geometry.Parse("M14,19H18V5H14M6,19H10V5H6V19Z") },
			{ IconType.Next, Geometry.Parse("M16,18H18V6H16M6,18L14.5,12L6,6V18Z") },
			{ IconType.Search, Geometry.Parse("M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5, 16A6.5, 6.5 0 0, 1 3, 9.5A6.5, 6.5 0 0, 1 9.5, 3M9.5, 5C7, 5 5, 7 5, 9.5C5, 12 7, 14 9.5, 14C12, 14 14, 12 14, 9.5C14, 7 12, 5 9.5, 5Z") },
			{ IconType.Volume0, Geometry.Parse("M7,9V15H11L16,20V4L11,9H7Z") },
			{ IconType.Volume1, Geometry.Parse("M5,9V15H9L14,20V4L9,9M18.5,12C18.5,10.23 17.5,8.71 16,7.97V16C17.5,15.29 18.5,13.76 18.5,12Z") },
			{ IconType.Volume2, Geometry.Parse("M14,3.23V5.29C16.89,6.15 19,8.83 19,12C19,15.17 16.89,17.84 14,18.7V20.77C18,19.86 21,16.28 21,12C21,7.72 18,4.14 14,3.23M16.5,12C16.5, 10.23 15.5, 8.71 14, 7.97V16C15.5, 15.29 16.5, 13.76 16.5, 12M3, 9V15H7L12, 20V4L7, 9H3Z") },
			{ IconType.Menu, Geometry.Parse("M3,6H21V8H3V6M3,11H21V13H3V11M3,16H21V18H3V16Z") },
			{ IconType.Settings, Geometry.Parse("M19.43,12.97L21.54,14.63C21.73,14.78 21.78,15.05 21.66,15.27L19.66,18.73C19.54,18.95 19.27,19.03 19.05,18.95L16.56,17.94C16.04,18.34 15.5,18.67 14.87,18.93L14.5,21.58C14.46,21.82 14.25,22 14,22H10C9.75,22 9.54,21.82 9.5,21.58L9.13,18.93C8.5,18.68 7.96,18.34 7.44,17.94L4.95,18.95C4.73,19.03 4.46,18.95 4.34,18.73L2.34,15.27C2.21,15.05 2.27,14.78 2.46,14.63L4.57,12.97L4.5,12L4.57,11L2.46,9.37C2.27,9.22 2.21,8.95 2.34,8.73L4.34,5.27C4.46,5.05 4.73,4.96 4.95,5.05L7.44,6.05C7.96,5.66 8.5,5.32 9.13,5.07L9.5,2.42C9.54,2.18 9.75,2 10,2H14C14.25,2 14.46,2.18 14.5,2.42L14.87,5.07C15.5,5.32 16.04,5.66 16.56,6.05L19.05,5.05C19.27,4.96 19.54,5.05 19.66,5.27L21.66,8.73C21.78,8.95 21.73,9.22 21.54,9.37L19.43,11L19.5,12L19.43,12.97M6.5,12C6.5,12.58 6.59,13.13 6.75,13.66L4.68,15.36L5.43,16.66L7.95,15.72C8.69,16.53 9.68,17.12 10.8,17.37L11.24,20H12.74L13.18,17.37C14.3,17.13 15.3,16.54 16.05,15.73L18.56,16.67L19.31,15.37L17.24,13.67C17.41,13.14 17.5,12.58 17.5,12C17.5,11.43 17.41,10.87 17.25,10.35L19.31,8.66L18.56,7.36L16.06,8.29C15.31,7.47 14.31,6.88 13.19,6.63L12.75,4H11.25L10.81,6.63C9.69,6.88 8.69,7.47 7.94,8.29L5.44,7.35L4.69,8.65L6.75,10.35C6.59,10.87 6.5,11.43 6.5,12M12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5M12,10.5A1.5,1.5 0 0,0 10.5,12A1.5,1.5 0 0,0 12,13.5A1.5,1.5 0 0,0 13.5,12A1.5,1.5 0 0,0 12,10.5Z") },
			{ IconType.FullScreen, Geometry.Parse("M5,5H10V7H7V10H5V5M14,5H19V10H17V7H14V5M17,14H19V19H14V17H17V14M10,17V19H5V14H7V17H10Z") },
			{ IconType.FullScreenExit, Geometry.Parse("M14,14H19V16H16V19H14V14M5,14H10V19H8V16H5V14M8,5H10V10H5V8H8V5M19,8V10H14V5H16V8H19Z") },
			{ IconType.ExpandPane, Geometry.Parse("M4,2H2V22H4V13H18.17L12.67,18.5L14.08,19.92L22,12L14.08,4.08L12.67,5.5L18.17,11H4V2Z") },
			{ IconType.CollapsePane, Geometry.Parse("M20,22H22V2H20V11H5.83L11.33,5.5L9.92,4.08L2,12L9.92,19.92L11.33,18.5L5.83,13H20V22Z") },
			{ IconType.VisionOn, Geometry.Parse("M21,17H3V5H21M21,3H3A2,2 0 0,0 1,5V17A2,2 0 0,0 3,19H8V21H16V19H21A2,2 0 0,0 23,17V5A2,2 0 0,0 21,3Z") },
			{ IconType.VisionOff, Geometry.Parse("M0.5,2.77L1.78,1.5L21,20.72L19.73,22L16.73,19H16V21H8V19H3A2,2 0 0,1 1,17V5C1,4.5 1.17,4.07 1.46,3.73L0.5,2.77M21,17V5H7.82L5.82,3H21A2,2 0 0,1 23, 5V17C23, 17.85 22.45, 18.59 21.7, 18.87L19.82, 17H21M3, 17H14.73L3, 5.27V17Z") },
			{ IconType.Back, Geometry.Parse("M20,11V13H8L13.5,18.5L12.08,19.92L4.16,12L12.08,4.08L13.5,5.5L8,11H20Z") }
		};

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
			BitmapImage output = new BitmapImage();
			output.BeginInit();
			output.StreamSource = stream;
			output.EndInit();
			return output;
		}
	}
}