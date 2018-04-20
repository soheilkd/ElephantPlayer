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
        none, musnote, musvideo, ondemand_video, pause, play_arrow,
        warning, fullscreen, subtitle, settings, short_text, folder_open, skip_next, skip_previous, content_copy,
        arrow_downward, arrow_upward, more_horiz, equalizer, expand_less, expand_more, repeat, repeat_one, shuffle,
        volume_0, volume_1, volume_2, volume_3, queue_music, cloud, add, cloud_download, cloud_done
    }
    public partial class MaterialIcon : UserControl, IComponentConnector
    {
        public static readonly DependencyProperty IconProperty = 
            DependencyProperty.Register(nameof(Icon), typeof(IconType), typeof(MaterialIcon), new FrameworkPropertyMetadata(IconType.none, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIconChanged)));
        public static readonly DependencyProperty IconResourceProperty =
            DependencyProperty.Register(nameof(IconResource), typeof(object), typeof(MaterialIcon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        //internal ContentControl PART_ContentControl;
        //private bool _contentLoaded;

        public MaterialIcon()
        {
            InitializeComponent();
            ((FrameworkElement)FindName(nameof(PART_ContentControl))).DataContext = this;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconType newValue = (IconType)e.NewValue;
            if (newValue == IconType.none)
                return;
            object resource = App.IconDictionary[newValue.ToString()];
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
