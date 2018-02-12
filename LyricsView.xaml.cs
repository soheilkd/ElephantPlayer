using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Player.Types;
using TagLib;
namespace Player
{
    public partial class LyricsView : Window
    {
        public LyricsView()
        {
            InitializeComponent();
            SizeUpDown.Value = 14;
        }
        Media Media;
        public void Load(Media file)
        {
            Media = file;
            File file2 = File.Create(file.Path);
            MainViewer.Text = file2.Tag.Lyrics;
            MainViewer.Clear();
            MainViewer.AppendText(Lyrics.Get(Media.Path));
            Show();
        }
        private void FontSizeChange(object sender, double e) => MainViewer.FontSize = e;

        private void UnderlineToggled(object sender, RoutedEventArgs e) => MainViewer.TextDecorations = MainViewer.TextDecorations == TextDecorations.Baseline ? null : TextDecorations.Baseline;
        private void ItalicToggled(object sender, RoutedEventArgs e) => MainViewer.FontStyle = MainViewer.FontStyle == FontStyles.Normal ? FontStyles.Italic : FontStyles.Normal;
        private void BoldToggled(object sender, RoutedEventArgs e) => MainViewer.FontWeight = MainViewer.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
        private void FontChange(object sender, SelectionChangedEventArgs e) => MainViewer.FontFamily = new FontFamily(((ComboBoxItem)FontCombo.SelectedItem).Content.ToString());
        
    }
}
