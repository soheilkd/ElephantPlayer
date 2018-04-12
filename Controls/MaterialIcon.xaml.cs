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
        ic_none,
        ic_music_note,
        ic_music_video,
        ic_ondemand_video,
        ic_pause,
        ic_play_arrow,
        ic_warning,
        ic_fullscreen,
        ic_subtitle,
        ic_settings,
        ic_short_text,
        ic_folder_open,
        ic_skip_next,
        ic_skip_previous,
        ic_content_copy,
        ic_arrow_downward,
        ic_arrow_upward,
        ic_more_horiz,
        ic_equalizer,
        ic_expand_less,
        ic_expand_more
    }
    public partial class MaterialIcon : UserControl, IComponentConnector
    {
        public static readonly DependencyProperty IconProperty = 
            DependencyProperty.Register(nameof(Icon), typeof(IconType), typeof(MaterialIcon), new FrameworkPropertyMetadata(IconType.ic_none, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIconChanged)));
        public static readonly DependencyProperty IconResourceProperty =
            DependencyProperty.Register(nameof(IconResource), typeof(object), typeof(MaterialIcon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        //internal ContentControl PART_ContentControl;
        //private bool _contentLoaded;

        public MaterialIcon()
        {
            InitializeComponent();
            ((FrameworkElement)FindName(nameof(PART_ContentControl))).DataContext = this;
        }

        public static ResourceDictionary res = new ResourceDictionary()
        {
            Source = new Uri(@"pack://application:,,,/Controls/Icons.xaml")
        };
        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconType newValue = (IconType)e.NewValue;
            if (newValue == IconType.ic_none)
                return;
            string str = newValue.ToString();
            object resource = res[(object)str];
            d.SetValue(IconResourceProperty, resource);
        }

        public IconType Icon
        {
            get => (IconType)GetValue(IconProperty);
            set => SetValue(IconProperty, (object)value);
        }

        public object IconResource
        {
            get => GetValue(IconResourceProperty);
            set => SetValue(IconResourceProperty, value);
        }
        
    }
}
