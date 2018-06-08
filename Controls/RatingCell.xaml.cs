using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Player.Controls
{
    public partial class RatingCell : StackPanel
    {
        public int RatingValue
        {
            get => (int)GetValue(RatingValueProperty);
            set => SetValue(RatingValueProperty, value);
        }
        private int tempRatingValue;

        public RatingCell()
        {
            InitializeComponent();
            Loaded += (_, __) => Update(RatingValue);
        }

        public static readonly DependencyProperty RatingValueProperty = DependencyProperty.Register(
            "RatingValue",
            typeof(Int32),
            typeof(RatingCell),
            new PropertyMetadata(0, new PropertyChangedCallback(RatingValueChanged)));

        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            int ratingValue = (int)e.NewValue;
            UIElementCollection children = sender.As<RatingCell>().Children;
            ToggleButton button = null;
            for (int i = 0; i < ratingValue; i++)
            {
                button = children[i] as ToggleButton;
                button.IsChecked = true;
            }

            for (int i = ratingValue; i < children.Count; i++)
            {
                button = children[i] as ToggleButton;
                button.IsChecked = true;
            }
        }
        private void RatingButtonMouseLeave(object sender, MouseEventArgs e) => Update(RatingValue);
        private void RatingButtonMouseEnter(object sender, MouseEventArgs e)
        {
            tempRatingValue = int.Parse(sender.As<ToggleButton>().Tag.ToString());
            Update(tempRatingValue);
        }
        private void Parentic_MouseUp(object sender, MouseButtonEventArgs e) => RatingValue = tempRatingValue;
        private void ToggleButton_Click(object sender, RoutedEventArgs e) => Parentic_MouseUp(this, null);

        private void Update(int withValue)
        {
            ToggleButton button = null;
            for (Int32 i = 0; i < withValue; i++)
            {
                button = Children[i] as ToggleButton;
                button.IsChecked = true;
            }

            for (Int32 i = withValue; i < Children.Count; i++)
            {
                button = Children[i] as ToggleButton;
                button.IsChecked = false;
            }
        }
    }
}