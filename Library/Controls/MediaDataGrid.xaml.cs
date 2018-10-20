using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Player.Extensions;
using Player.Models;

namespace Player.Controls
{
	public partial class MediaDataGrid : DataGrid
	{
		private MediaQueue _Items;
		public new MediaQueue ItemsSource
		{
			get => _Items;
			set
			{
				SetValue(ItemsSourceProperty, value);
				_Items = value;
			}
		}
		
		public event EventHandler<QueueEventArgs> MediaRequested;

		public static readonly DependencyProperty TitleColumnVisibilityProperty =
			DependencyProperty.Register(nameof(TitleColumnVisibility), typeof(Visibility), typeof(MediaDataGrid), new PropertyMetadata(Visibility.Visible));
		public static readonly DependencyProperty ArtistColumnVisibilityProperty =
			DependencyProperty.Register(nameof(ArtistColumnVisibility), typeof(Visibility), typeof(MediaDataGrid), new PropertyMetadata(Visibility.Visible));
		public static readonly DependencyProperty AlbumColumnVisibilityProperty =
			DependencyProperty.Register(nameof(AlbumColumnVisibility), typeof(Visibility), typeof(MediaDataGrid), new PropertyMetadata(Visibility.Visible));
		public static readonly DependencyProperty PlaysColumnVisibilityProperty =
			DependencyProperty.Register(nameof(PlaysColumnVisibility), typeof(Visibility), typeof(MediaDataGrid), new PropertyMetadata(Visibility.Visible));
		public static readonly DependencyProperty DateColumnVisibilityProperty =
			DependencyProperty.Register(nameof(DateColumnVisibility), typeof(Visibility), typeof(MediaDataGrid), new PropertyMetadata(Visibility.Visible));

		public Visibility TitleColumnVisibility
		{
			get => (Visibility)GetValue(TitleColumnVisibilityProperty);
			set => SetValue(TitleColumnVisibilityProperty, value);
		}
		public Visibility ArtistColumnVisibility
		{
			get => (Visibility)GetValue(ArtistColumnVisibilityProperty);
			set => SetValue(ArtistColumnVisibilityProperty, value);
		}
		public Visibility AlbumColumnVisibility
		{
			get => (Visibility)GetValue(AlbumColumnVisibilityProperty);
			set => SetValue(AlbumColumnVisibilityProperty, value);
		}
		public Visibility PlaysColumnVisibility
		{
			get => (Visibility)GetValue(PlaysColumnVisibilityProperty);
			set => SetValue(PlaysColumnVisibilityProperty, value);
		}
		public Visibility DateColumnVisibility
		{
			get => (Visibility)GetValue(DateColumnVisibilityProperty);
			set => SetValue(DateColumnVisibilityProperty, value);
		}
		private SaveFileDialog MediaTransferDialog = new SaveFileDialog()
		{
			AddExtension = false,
			CheckPathExists = true,
			CreatePrompt = false,
			DereferenceLinks = true,
			InitialDirectory = Settings.LastPath
		};

		public MediaDataGrid()
		{
			InitializeComponent();
		}

		private void Menu_TagDetergent(object sender, RoutedEventArgs e)
		{
			For(item => item.CleanTag());
		}
		private void Menu_MoveClick(object sender, RoutedEventArgs e)
		{
			switch ((sender.As<MenuItem>().Header ?? "INDIV").ToString().Substring(0, 1))
			{
				case "B":
					MediaTransferDialog.Title = "Move";
					if (MediaTransferDialog.ShowDialog().Value)
					{
						Settings.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Settings.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.MoveTo(Resources["LastPath"].ToString()));
					break;
			}
		}
		private void Menu_CopyClick(object sender, RoutedEventArgs e)
		{
			switch ((sender.As<MenuItem>().Header ?? "INDIV").ToString().Substring(0, 1))
			{
				case "B":
					MediaTransferDialog.Title = "Copy";
					if (MediaTransferDialog.ShowDialog().Value)
					{
						Settings.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Settings.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.CopyTo(Resources["LastPath"].ToString()));
					break;
			}
		}
		private void Menu_RemoveClick(object sender, RoutedEventArgs e)
		{
			For(each => ItemsSource.Remove(each));
		}
		private void Menu_DeleteClick(object sender, RoutedEventArgs e)
		{
			string msg = "Sure? These will be deleted:\r\n";
			For(item => msg += $"{item.Path}\r\n");
			if (MessageBox.Show(msg, "Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
				return;
			For(item =>
			{
				File.Delete(item.Path);
				ItemsSource.Remove(item);
			});
		}
		private void Menu_LocationClick(object sender, RoutedEventArgs e)
		{
			For(item => Process.Start("explorer.exe", "/select," + item.Path));
		}
		private void Menu_PropertiesClick(object sender, RoutedEventArgs e)
		{
			For(each =>
			{
				var pro = new PropertiesUI();
				pro.SaveRequested += (_, f) =>
				{
						f.Parameter.Save();
						each.Reload();
				};
				pro.LoadFor(each);
			});
		}

		private void For(Action<Media> action) =>
			SelectedItems.Cast<Media>().ToArray().For(each => action(each));

		private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (SelectedItem == null)
				return;
			MediaRequested?.Invoke(this, new QueueEventArgs(ItemsSource, SelectedItem as Media));
		}

		private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
		{
			//SelectedItem = null;
		}
	}
}
