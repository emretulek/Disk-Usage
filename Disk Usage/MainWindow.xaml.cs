using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Windows;
using Widgets.Common;

namespace Disk_Usage
{
    public partial class MainWindow : Window,IWidgetWindow
    {
        public readonly static string WidgetName = "Disk Usage";
        public readonly static string SettingsFile = "settings.diskusage.json";
        private readonly Config config = new(SettingsFile);

        public DiskViewModel ViewModel { get; set; }
        private DiskViewModel.SettingsStruct Settings = DiskViewModel.Default;

        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();
            ViewModel = new()
            {
                Settings = Settings
            };
            DataContext = ViewModel;
            _ = ViewModel.Start();
            Logger.Info($"{WidgetName} is started");
        }

        public void LoadSettings()
        {
            try
            {
                var driverString = PropertyParser.ToString(config.GetValue("drivers"));
                var driverJson = JsonNode.Parse(driverString)?.AsArray() ?? [];
                string[] DriveLetters = driverJson.Select(item => item!.ToString()).ToArray();

                Settings.DriveLetters = DriveLetters;
                Settings.BarColor = PropertyParser.ToColorBrush(config.GetValue("bar_color"), Settings.BarColor.ToString());
                Settings.BarHeight = PropertyParser.ToInt(config.GetValue("bar_height"), Settings.BarHeight);
                Settings.BarBackground = PropertyParser.ToColorBrush(config.GetValue("bar_background"), Settings.BarBackground.ToString());
                Settings.FontSize = PropertyParser.ToFloat(config.GetValue("usage_font_size"));
                Settings.FontSize2 = PropertyParser.ToFloat(config.GetValue("usage_font_size2"));
                Settings.FontColor = PropertyParser.ToColorBrush(config.GetValue("usage_foreground"));
            }
            catch (Exception)
            {
                config.Add("usage_font_size", Settings.FontSize);
                config.Add("usage_font_size2", Settings.FontSize2);
                config.Add("usage_foreground", Settings.FontColor);
                config.Add("bar_color", Settings.BarColor);
                config.Add("bar_background", Settings.BarBackground);
                config.Add("bar_height", Settings.BarHeight);
                config.Add("drivers", Settings.DriveLetters);
                config.Save();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            ViewModel.Dispose();
            Logger.Info($"{WidgetName} is closed");
        }

        public WidgetWindow WidgetWindow()
        {
            return new WidgetWindow(this);
        }

        public static WidgetDefaultStruct WidgetDefaultStruct()
        {
            return new()
            {
                SizeToContent = SizeToContent.Height,
            };
        }
    }
}
