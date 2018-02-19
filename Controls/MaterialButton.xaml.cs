using Player.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialIcons;
using System.Windows.Media.Animation;
using System;
#pragma warning disable 1591
namespace Player.Controls
{
    public partial class MaterialButton : System.Windows.Controls.UserControl
    {
        public event EventHandler Click;
        public const int HED = 200; //Hover Ellipse Duration
        private DoubleAnimation ButtonPressedAnimation =
            new DoubleAnimation()
            {
                AutoReverse = false,
                Duration = TimeSpan.FromMilliseconds(HED),
                From = 0,
                To = 1
            };
        Storyboard ButtonPressedStory = new Storyboard() { AutoReverse = true };
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(MaterialIconType), typeof(MaterialButton), new PropertyMetadata(MaterialIconType.ic_warning));
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register("Theme", typeof(Styling.Theme), typeof(MaterialButton), new PropertyMetadata(Styling.Theme.Get(0)));
        
        public Styling.Theme Theme
        {
            get => (Styling.Theme)GetValue(ThemeProperty);
            set
            {
                SetValue(ThemeProperty, value);
                MainIcon.Foreground = value.ButtonsBrush;
                MainEllipse.Stroke = Theme.BarsBrush;
            }
        }
        public MaterialIconType Icon { get => (MaterialIconType)GetValue(IconProperty); set { SetValue(IconProperty, value); MainIcon.Icon = value; } }
        public void UXMouseEnter(object sender, MouseEventArgs e)
        {
            ButtonPressedStory.Seek(TimeSpan.FromMilliseconds(0));
            ButtonPressedStory.AutoReverse = false;
            ButtonPressedStory.Begin();
            MainEllipse.Fill = Theme.ButtonsBrush;
            MainIcon.Foreground = Theme.BackgroundBrush;
        }
        public void UXMouseLeave(object sender, MouseEventArgs e)
        {
            //UXMouseUp(sender, null);
            ButtonPressedStory.AutoReverse = true;
            ButtonPressedStory.Begin();
            ButtonPressedStory.Seek(TimeSpan.FromMilliseconds(HED));
            MainIcon.Foreground = Theme.ButtonsBrush;
        }
        public MaterialButton() => InitializeComponent();
        public void UXMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainEllipse.StrokeThickness = 2;
        }
        public void UXMouseUp(object sender, MouseButtonEventArgs e)
        {
            MainIcon.Foreground = Theme.BackgroundBrush;
            MainEllipse.StrokeThickness = 0;
            Click?.Invoke(this, null);
        }
        async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);
            MainIcon.Icon = Icon;
            MainIcon.Foreground = Theme.ButtonsBrush;
            Storyboard.SetTarget(ButtonPressedAnimation, MainEllipse);
            Storyboard.SetTargetProperty(ButtonPressedAnimation, new PropertyPath(OpacityProperty));
            ButtonPressedStory.Children.Add(ButtonPressedAnimation);
            MainEllipse.Opacity = 0;
            MainEllipse.StrokeThickness = 0;
            int dec = Height > 25 ? 3 : 2;
            MainEllipse.Height = Height + dec;
            MainEllipse.Width = Height + dec;
            MainEllipse.Margin = new Thickness()
            {
                Right = 0,
                Bottom = -1,
                Left = -1,
                Top = -1
            };
        }
    }
}