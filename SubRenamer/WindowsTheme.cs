using System;
using System.Globalization;
using System.Management;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.Win32;

namespace SubRenamer;

internal class WindowsTheme
{
    public enum Theme
    {
        Light,
        Dark
    }

    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private const string RegistryValueName = "AppsUseLightTheme";

    public event EventHandler<Theme> OnWindowsThemeChanged;

    [SupportedOSPlatform("windows")]
    public void WatchTheme()
    {
        var currentUser = WindowsIdentity.GetCurrent();
        if (currentUser.User != null)
        {
            var query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    var newWindowsTheme = GetWindowsTheme();
                    OnWindowsThemeChanged?.Invoke(this, newWindowsTheme);
                };

                // Start listening for events
                watcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
            }
        }

        var initialTheme = GetWindowsTheme();
        OnWindowsThemeChanged?.Invoke(this, initialTheme);
    }

    [SupportedOSPlatform("windows")]
    private static Theme GetWindowsTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        var registryValueObject = key?.GetValue(RegistryValueName);
        if (registryValueObject == null) return Theme.Light;

        var registryValue = (int)registryValueObject;

        return registryValue > 0 ? Theme.Light : Theme.Dark;
    }
}