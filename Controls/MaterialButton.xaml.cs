using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialIcons;
using System.Windows.Media.Animation;
using System;
using System.Windows.Controls;
#pragma warning disable 1591
namespace Player.Controls
{
    public partial class MaterialButton : UserControl
    {
        public event EventHandler Click;
        public const int HED = 100; //Hover Ellipse Duration
        private ThicknessAnimation ButtonPressedAnimation =
            new ThicknessAnimation()
            {
                AutoReverse = false,
                Duration = TimeSpan.FromMilliseconds(HED),
                From = new Thickness(-2),
                To = new Thickness(-5)
            };
        Storyboard ButtonPressedStory = new Storyboard() { AutoReverse = true };
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(MaterialIconType), typeof(MaterialButton), new PropertyMetadata(MaterialIconType.ic_warning));
        public MaterialIconType Icon { get => (MaterialIconType)GetValue(IconProperty); set { SetValue(IconProperty, value); MainIcon.Icon = value; } }

        public void UXMouseEnter(object sender, MouseEventArgs e)
        {
            ButtonPressedStory.AutoReverse = true;
            ButtonPressedStory.Begin();
            ButtonPressedStory.Seek(TimeSpan.FromMilliseconds(HED));
        }
        public void UXMouseLeave(object sender, MouseEventArgs e)
        {
            ButtonPressedStory.AutoReverse = false;
            ButtonPressedStory.Begin();
            ButtonPressedStory.Seek(TimeSpan.FromMilliseconds(0));
            MainEllipse.Fill = System.Windows.Media.Brushes.Transparent;
        }
        bool GoingToUX_Leave = false;
        bool GoingToUX_Enter = false;
        private void ButtonPressedStory_Completed(object sender, EventArgs e)
        {

        }

        public MaterialButton()
        {
            InitializeComponent();
            ButtonPressedStory.Completed += ButtonPressedStory_Completed;
        }
         public void UXMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainEllipse.Fill = MainIcon.Foreground;
        }
        public void UXMouseUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, null);
            MainEllipse.Fill = System.Windows.Media.Brushes.Transparent;
        }
        async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);
            MainIcon.Icon = Icon;
            Storyboard.SetTarget(ButtonPressedAnimation, MainEllipse);
            Storyboard.SetTargetProperty(ButtonPressedAnimation, new PropertyPath(MarginProperty));
            ButtonPressedStory.Children.Add(ButtonPressedAnimation);
        }
    }
}