using System;
using System.Windows;
using System.Windows.Controls;
namespace Player.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public event EventHandler<double> ValueChanged;
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericUpDown), new PropertyMetadata(10));
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));
        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set
            {
                SetValue(MaximumProperty, value);
                MainUpDown.Maximum = value;
            }
        }
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set
            {
                SetValue(MinimumProperty, value);
                MainUpDown.Minimum = value;
            }
        }
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set
            {
                if (Value > Maximum || Value < Minimum) throw new ArgumentOutOfRangeException(nameof(Value));
                MainUpDown.Value = value;
                ValueChanged?.Invoke(this, value);
            }
        }
        public NumericUpDown()
        {
            InitializeComponent();
        }

        private void MainUpDown_ValueChanged(object sender, EventArgs e) => Value = (int)MainUpDown.Value;
    }
}
