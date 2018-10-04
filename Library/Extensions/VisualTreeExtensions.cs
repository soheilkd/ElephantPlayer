using System.Windows;
using System.Windows.Media;

namespace Player.Extensions
{
	public static class VisualTreeExtensions
	{
		public static DependencyObject GetParent(this DependencyObject obj, int layer = 1)
		{
			DependencyObject layerObject = obj;
			MiscExtensions.Repeat(() => layerObject = VisualTreeHelper.GetParent(layerObject), layer);
			return layerObject;
		}
	}
}
