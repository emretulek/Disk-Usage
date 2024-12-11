using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Widgets.Common;

namespace Disk_Usage
{
    public partial class DiskViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Schedule schedule = new();
        private string scheduleID = "";
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public struct SettingsStruct
        {
            public float FontSize { get; set; }
            public float FontSize2 { get; set; }
            public SolidColorBrush FontColor { get; set; }
            public SolidColorBrush BarColor { get; set; }
            public SolidColorBrush BarBackground { get; set; }
            public int BarHeight { get; set; }
            public string[] DriveLetters { get; set; } 
        }

        public static SettingsStruct Default => new()
        {
            FontSize = 24,
            FontSize2 = 14,
            FontColor = new SolidColorBrush(Colors.RosyBrown),
            BarColor = new SolidColorBrush(Colors.DarkRed),
            BarHeight = 5,
            BarBackground = new SolidColorBrush(Colors.RosyBrown),
            DriveLetters = ["C", "D"]
        };

        public required SettingsStruct Settings { get; set; } = Default;
        public ObservableCollection<DiskUsageInfo> DiskUsages { get; set; } = [];

        public async Task Start()
        {
            await Task.Run(() =>
            {
                UpdateDiskUsages();
            }, cancellationTokenSource.Token);

            scheduleID = schedule.Secondly(UpdateDiskUsages, 1);
        }

        private void UpdateDiskUsages()
        {
            foreach (var driveLetter in Settings.DriveLetters)
            {
                DriveInfo drive = new(driveLetter);
                if (!drive.IsReady) continue;

                double unUsedSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                double usedSpace = (drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024); // GB
                double totalSpace = drive.TotalSize / (1024 * 1024 * 1024); // GB
                double usagePercentage = (usedSpace / totalSpace) * 100;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var diskInfo = DiskUsages.FirstOrDefault(d => d.DriveLetter == driveLetter);
                        if (diskInfo != null)
                        {
                            diskInfo.UnUsageText = $"{driveLetter}:\\ {unUsedSpace:F1} GB";
                            diskInfo.TotalSpaceText = $"free out off {totalSpace} GB";
                            diskInfo.UsagePercentage = usagePercentage;
                        }
                        else
                        {
                            DiskUsages.Add(new DiskUsageInfo
                            {
                                DriveLetter = driveLetter,
                                UnUsageText = $"{driveLetter}:\\ {unUsedSpace:F1} GB",
                                TotalSpaceText = $"free out off {totalSpace} GB",
                                UsagePercentage = usagePercentage
                            });
                        }
                    } catch(Exception e) {
                        Logger.Error(e.Message);
                    }
                });
            }
        }

        public void Dispose()
        {
            schedule.Stop(scheduleID);
            cancellationTokenSource.Cancel();
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DiskUsageInfo
    {
        public string? DriveLetter {  get; set; }
        public string? UnUsageText { get; set; }
        public string? TotalSpaceText { get; set; }
        public double UsagePercentage { get; set; }
    }
}
