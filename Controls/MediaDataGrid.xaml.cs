using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Library.Extensions;
using Microsoft.Win32;
using Player.Models;
using Player.Windows;

namespace Player.Controls
{
	public partial class MediaDataGrid : ListBox
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

		private SaveFileDialog MediaTransferDialog = new SaveFileDialog()
		{
			AddExtension = false,
			CheckPathExists = true,
			CreatePrompt = false,
			DereferenceLinks = true,
			InitialDirectory = Controller.Settings.LastPath
		};

		public MediaDataGrid()
		{
			InitializeComponent();
			ContextMenu.Items.Add(AddToPlaylistMenu);
			ContextMenu.Items.Add(RemoveFromPlaylistMenu);
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
						Controller.Settings.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Controller.Settings.LastPath;
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
						Controller.Settings.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Controller.Settings.LastPath;
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
			var msg = "Sure? These will be deleted:\r\n";
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
				var pro = new Windows.PropertiesWindow();
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

		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (SelectedItem == null)
				return;
			Controller.Play(SelectedItem as Media, ItemsSource);
		}

		private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
		{

		}

		private void DGR_Border_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Controller.Play((Media)((ContentControl)sender).Content, ItemsSource);
		}

		public MenuItem AddToPlaylistMenu = new MenuItem()
		{
			Header = "Add To Playlist"
		};
		public MenuItem RemoveFromPlaylistMenu = new MenuItem()
		{
			Header = "Remove From Playlist"
		};
		private void ListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			AddToPlaylistMenu.Items.Clear();
			var notIncluded = Controller.Playlists.Where(each => !each.Contains(SelectedItem as Media));
			notIncluded.ForEach(each =>
			{
				var item = new MenuItem()
				{
					Header = each.Name
				};
				item.Click += (_, __) => each.Add(SelectedItem as Media);
				AddToPlaylistMenu.Items.Add(item);
			});
			var addNew = new MenuItem()
			{
				Header = "New Playlist..."
			};
			addNew.Click += (_, __) =>
			{
				var name = NewPlaylistWindow.RequestName();
				Controller.Playlists.Add(new Playlist(name));
			};
			AddToPlaylistMenu.Items.Add(addNew);

			RemoveFromPlaylistMenu.Items.Clear();
			var included = Controller.Playlists.Where(each => each.Contains(SelectedItem as Media));
			included.ForEach(each =>
			{
				var item = new MenuItem()
				{
					Header = each.Name
				};
				item.Click += (_, __) => each.Remove(SelectedItem as Media);
			});
			RemoveFromPlaylistMenu.Visibility = RemoveFromPlaylistMenu.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
		}

	}
}
