// Decompiled 
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Player.Controls
{
    public enum IconType
    {
        None, MusicNote, Video, OndemandVideo, Pause, Play,
        Warning, FullScreen, Subtitle, Settings, Text, Folder, Next, Previous, Copy,
        DownArrow, UpArrow, HorizMore, Equalizer, ExpandLess, ExpandMore, Repeat, RepeatOne, Shuffle,
        Volume0, Volume1, Volume2, Volume3, MusicQueue, Cloud, Add, CloudLoad, CloudDone, Cancel
    }
    public partial class MaterialIcon : UserControl, IComponentConnector
    {
        public static readonly DependencyProperty IconProperty = 
            DependencyProperty.Register(nameof(Icon), typeof(IconType), typeof(MaterialIcon), new FrameworkPropertyMetadata(IconType.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIconChanged)));
        public static readonly DependencyProperty IconResourceProperty =
            DependencyProperty.Register(nameof(IconResource), typeof(object), typeof(MaterialIcon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        
        public MaterialIcon()
        {
            InitializeComponent();
            ((FrameworkElement)FindName(nameof(PART_ContentControl))).DataContext = this;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconType newValue = (IconType)e.NewValue;
            if (newValue == IconType.None)
                return;
            object resource = Application.Current.Resources[newValue.ToString()];
            d.SetValue(IconResourceProperty, resource);
        }

        public IconType Icon
        {
            get => (IconType)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public object IconResource
        {
            get => GetValue(IconResourceProperty);
            set => SetValue(IconResourceProperty, value);
        }
        
    }
}
