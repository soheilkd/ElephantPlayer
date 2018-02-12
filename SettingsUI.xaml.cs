using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using Player.Events;
using Player.Extensions;
using Player.Styling;
#pragma warning disable 1591
namespace Player
{
    public partial class SettingsUI : Window
    {
        private bool MouseDownTabs = false;
        public event EventHandler<SettingsEventArgs> SettingsChanged;
        public event EventHandler<PlaybackEventArgs> PlaybackSettingsChanged;
        public SettingsUI() => InitializeComponent();
        public Preferences Settings;
        private List<Label> Labels = new List<Label>();
        private void Reload()
        {
            PlayModeCombo.SelectedIndex = Settings.PlayMode;
            TileProgressToggle.IsChecked = Settings.TileProgress;
            TileSchemeCombo.SelectedIndex = Settings.TileScheme;
            TileFontCombo.SelectedIndex = Settings.TileFontIndex;
            TileFontSizeNumeric.Value = Settings.TileFontSize.ToInt();
            MainThemeCombo.SelectedIndex = Settings.MainThemeIndex;
            TileThemeCombo.SelectedIndex = Settings.TileThemeIndex;
            LatencyCombo.SelectedIndex = Settings.LatencyIndex;
            TilesOrientationCombo.SelectedIndex = Settings.Orientation;
            ViewModeCombo.SelectedIndex = Settings.ViewMode;
            AnySettingChanged(this, null);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings = new Preferences();
            Reload();
            SearchToggle.Checked += AnySettingChanged;
            SearchToggle.Unchecked += AnySettingChanged;
            TileProgressToggle.Checked += AnySettingChanged;
            TileProgressToggle.Unchecked += AnySettingChanged;
            TileFontCombo.SelectionChanged += AnySettingChanged;
            TileFontSizeNumeric.ValueChanged += (_, n) => AnySettingChanged(this, null);
            TileSchemeCombo.SelectionChanged += AnySettingChanged;
            TilesOrientationCombo.SelectionChanged += AnySettingChanged;
            PlayModeCombo.SelectionChanged += AnySettingChanged;
            LatencyCombo.SelectionChanged += AnySettingChanged;
            TileThemeCombo.SelectionChanged += AnySettingChanged;
            MainThemeCombo.SelectionChanged += AnySettingChanged;
            MouseDown += SettingsUI_MouseDown;
        }
        private void SettingsUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!MouseDownTabs)
                try { DragMove(); } catch (Exception) { }
        }
        private void AnySettingChanged(object sender, EventArgs e)
        {
            Settings.PlayMode = PlayModeCombo.SelectedIndex;
            Settings.Search = SearchToggle.IsChecked.Value;
            Settings.TileFontIndex = TileFontCombo.SelectedIndex;
            Settings.TileFontSize = TileFontSizeNumeric.Value;
            Settings.TileProgress = TileProgressToggle.IsChecked.Value;
            Settings.TileScheme = TileSchemeCombo.SelectedIndex;
            Settings.TileThemeIndex = TileThemeCombo.SelectedIndex;
            Settings.LatencyIndex = LatencyCombo.SelectedIndex;
            Settings.Orientation = TilesOrientationCombo.SelectedIndex;
            Settings.ViewMode = ViewModeCombo.SelectedIndex;
            Settings.MainThemeIndex = MainThemeCombo.SelectedIndex;
            Settings.TileThemeIndex = TileThemeCombo.SelectedIndex;
            SettingsChanged?.Invoke(this, new SettingsEventArgs() { NewSettings = Settings });
        }
        private async void TabController_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDownTabs = true;
            await Task.Delay(5);
            
            while (e.LeftButton == MouseButtonState.Pressed)
                await Task.Delay(5);
            MouseDownTabs = false;
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Settings = new Preferences();
            SettingsChanged?.Invoke(this, new SettingsEventArgs() { NewSettings = Settings });
            Hide();
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SettingsChanged?.Invoke(this, new SettingsEventArgs() { NewSettings = Settings });
            Settings.Save();
            Hide();
            Reload();
        }
        private void PlaybackChangedByUser(object sender, RoutedEventArgs e)
        {
            try
            {
                PlaybackSettingsChanged?.Invoke(this, new PlaybackEventArgs()
                {
                    SpeakerBalance = SpeakerSlider.Value / 10,
                    SpeedRatio = SpeedSlider.Value / 10,
                    Stretch = (Stretch)StretchModeCombo.SelectedIndex,
                    StretchDirection = (StretchDirection)StretchDirCombo.SelectedIndex
                });
            }
            catch (Exception) { }
        }
        private void SpeakerBalanceUPDOWN_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        private void PlaybackChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PlaybackChangedByUser(sender, null);
        }

        private void MainThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class Preferences
    {
        public int Orientation { get; set; }
        
        public int PlayMode { get; set; }
        public int LatencyIndex;
        public int MainThemeIndex { get; set; }
        public int TileThemeIndex { get; set; }
        public int MainKey { get; set; }
        public int TileFontIndex { get; set; }
        public int TileScheme { get; set; }
        public bool Search { get; set; }
        public bool TileProgress { get; set; }
        public double TileFontSize { get; set; }
        public int ViewMode { get; set; }
        public Theme MainTheme => Theme.Get(MainThemeIndex);
        public Theme TileTheme => Theme.Get(TileThemeIndex);
        public FontFamily TileFont => Player.User.Text.GetFont(TileFontIndex);
        public Preferences(bool AutoLoad = true)
        {
            if (AutoLoad) Reload();
        }
        public static Preferences Defaults => new Preferences(false)
        {
            MainKey = 0,
            MainThemeIndex = 1,
            PlayMode = 3,
            Search = false,
            TileFontIndex = 2,
            TileFontSize = 14,
            TileProgress = true,
            TileScheme = 1,
            TileThemeIndex = 1,
            LatencyIndex = 5,
            Orientation = 0
        };
        public void Reload()
        {
            XmlReader reader = XmlReader.Create(App.PrefPath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "MainInterface":
                            MainThemeIndex = int.Parse(reader.GetAttribute("Theme"));
                            LatencyIndex = int.Parse(reader.GetAttribute("Latency"));
                            ViewMode = int.Parse(reader.GetAttribute("ViewMode"));
                            break;
                        case "Search":
                            Search = reader.GetAttribute("Enabled") == "true";
                            break;
                        case "Playback":
                            PlayMode = int.Parse(reader.GetAttribute("Mode"));
                            break;
                        case "MainKey":
                            MainKey = int.Parse(reader.GetAttribute("Index"));
                            break;
                        case "Tile":
                            TileFontIndex = int.Parse(reader.GetAttribute("FontFamily"));
                            TileFontSize = double.Parse(reader.GetAttribute("FontSize"));
                            TileProgress = reader.GetAttribute("ProgressBar") == "true";
                            TileScheme = int.Parse(reader.GetAttribute("Scheme"));
                            TileThemeIndex = int.Parse(reader.GetAttribute("Theme"));
                            Orientation = int.Parse(reader.GetAttribute("Orientation"));
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            XmlWriter writer = XmlWriter.Create(App.PrefPath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Settings");

            writer.WriteStartElement("MainInterface");
            writer.WriteStartAttribute("Theme");
            writer.WriteValue(MainThemeIndex);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Latency");
            writer.WriteValue(LatencyIndex);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("ViewMode");
            writer.WriteValue(ViewMode);
            writer.WriteEndAttribute();
            writer.WriteEndElement();

            writer.WriteStartElement("Search");
            writer.WriteStartAttribute("Enabled");
            writer.WriteValue(Search);
            writer.WriteEndAttribute();
            writer.WriteEndElement();

            writer.WriteStartElement("Playback");
            writer.WriteStartAttribute("Mode");
            writer.WriteValue(PlayMode);
            writer.WriteEndAttribute();
            writer.WriteEndElement();

            writer.WriteStartElement("MainKey");
            writer.WriteStartAttribute("Index");
            writer.WriteValue(MainKey);
            writer.WriteEndAttribute();
            writer.WriteEndElement();

            writer.WriteStartElement("Tile");
            writer.WriteStartAttribute("Scheme");
            writer.WriteValue(TileScheme);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Theme");
            writer.WriteValue(TileThemeIndex);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("ProgressBar");
            writer.WriteValue(TileProgress);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("FontFamily");
            writer.WriteValue(TileFontIndex);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("FontSize");
            writer.WriteValue(TileFontSize);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Orientation");
            writer.WriteValue(Orientation);
            writer.WriteEndAttribute();
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
}

