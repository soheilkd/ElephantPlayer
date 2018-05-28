using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Player.Controls
{
    public partial class RatingCell : StackPanel
    {
        public RatingCell()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RatingValueProperty = DependencyProperty.Register(
            "RatingValue",
            typeof(Int32),
            typeof(RatingCell),
            new PropertyMetadata(0, new PropertyChangedCallback(RatingValueChanged)));

        public Int32 RatingValue
        {
            get => (int)GetValue(RatingValueProperty);
            set
            {
                if (value < 0)
                    SetValue(RatingValueProperty, 0);
                else if (value > 5)
                    SetValue(RatingValueProperty, 5);
                else
                    SetValue(RatingValueProperty, value);
            }
        }
        private Int32 tempRatingValue;
        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Int32 ratingValue = (Int32)e.NewValue;
            UIElementCollection children = sender.As<RatingCell>().Children;
            ToggleButton button = null;
            for (Int32 i = 0; i < ratingValue; i++)
            {
                button = children[i] as ToggleButton;
                button.IsChecked = true;
            }

            for (Int32 i = ratingValue; i < children.Count; i++)
            {
                button = children[i] as ToggleButton;
                button.IsChecked = true;
            }
        }
        
        private void RatingButtonMouseLeave(object sender, MouseEventArgs e)
        {
            Update(RatingValue);
        }
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
        private void RatingButtonMouseEnter(object sender, MouseEventArgs e)
        {
            tempRatingValue = int.Parse(sender.As<ToggleButton>().Tag.ToString());
            Update(tempRatingValue);
        }

        private void Parentic_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RatingValue = tempRatingValue;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) => Parentic_MouseUp(this, null);
    }
}