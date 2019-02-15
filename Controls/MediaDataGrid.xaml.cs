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

		private void OrganizeAddToPlaylistMenu()
		{
			var medias = SelectedItems.Cast<Media>();
			AddToPlaylistMenu.Items.Clear();
			foreach (var item in Controller.Library.Playlists)
			{
				if (!medias.All(each => item.Contains(each)))
				{
					var menuItem = new MenuItem()
					{
						Header = item.Name
					};
					menuItem.Click += (_, __) => medias.ForEach(each => item.Add(each));
					AddToPlaylistMenu.Items.Add(menuItem);
				}
			}
			AddToPlaylistMenu.Height = AddToPlaylistMenu.Items.Count != 0 ? double.NaN : 0;
		}
		private void OrganizeRemoveFromPlaylistMenu()
		{
			var medias = SelectedItems.Cast<Media>();
			RemoveFromPlaylistMenu.Items.Clear();
			foreach (var item in Controller.Library.Playlists)
			{
				if (medias.All(each => item.Contains(each)))
				{
					var menuItem = new MenuItem()
					{
						Header = item.Name
					};
					menuItem.Click += (_, __) => medias.ForEach(each => item.Remove(each));
					RemoveFromPlaylistMenu.Items.Add(menuItem);
				}
			}
			RemoveFromPlaylistMenu.Height = RemoveFromPlaylistMenu.Items.Count != 0 ? double.NaN : 0;
		}

		private void ListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			OrganizeAddToPlaylistMenu();
			OrganizeRemoveFromPlaylistMenu();
		}

		private void SearchTextChanged(object sender, TextChangedEventArgs e)
		{
			SetValue(ItemsSourceProperty, ItemsSource.Search(sender.As<TextBox>().Text));
		}
	}
}
