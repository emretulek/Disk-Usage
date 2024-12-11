﻿using System.ComponentModel.Composition;
using System.Windows;
using Widgets.Common;

namespace Disk_Usage
{
    [Export(typeof(IPlugin))]
    internal class DiskUsagePlugin : IPlugin
    {
        public string Name => MainWindow.WidgetName;
        public string? ConfigFile => MainWindow.SettingsFile;
        public WidgetDefaultStruct WidgetDefaultStruct()
        {
            return MainWindow.WidgetDefaultStruct();
        }
        public WidgetWindow WidgetWindow()
        {
            return new MainWindow().WidgetWindow();
        }
    }
}
