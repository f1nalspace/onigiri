using Microsoft.Win32;
using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;

namespace Finalspace.Onigiri.Services
{
    class Win32DarkModeDetectionService : IDarkModeDetectionService
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";

        private enum WindowsTheme
        {
            Light,
            Dark
        }

        private WindowsTheme _currentTheme;

        public event EventHandler<bool> DarkModeChanged;

        public Win32DarkModeDetectionService()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    _currentTheme = GetWindowsTheme();
                    DarkModeChanged.Invoke(this, _currentTheme == WindowsTheme.Dark);
                };

                // Start listening for events
                watcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
            }

            _currentTheme = GetWindowsTheme();
        }

        private static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

        public bool IsDarkMode => _currentTheme == WindowsTheme.Dark;
    }
}
