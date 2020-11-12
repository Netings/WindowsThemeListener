﻿#region Copyright

/*
 * Developer    : Willy Kimura
 * Library      : Windows Theme Listener
 * License      : MIT
 * 
 * Windows Theme Listener is a helper library that was birthed 
 * after a longing to see my applications blend-in with the new 
 * Windows 10 theming modes, that is, Dark and Light themes. 
 * How I searched through countless StackOverFlow questions! 
 * Oh well, eventually the hardwork had to be done by someone, 
 * so I set out to building a nifty helper library that would 
 * do just that and probably even more. Thus came "WTL" or 
 * Windows Theme Listener, a nifty, static .NET library that 
 * lets one not only capture the default Windows theming modes, 
 * but also listen to any changes made to the theming modes and 
 * the system-wide accent color applied. This library will help 
 * developers modernize their applications to support dark/light 
 * theming options and so create a seamless end-user experience.
 * 
 * Improvements are welcome.
 * 
 */

#endregion


using System;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using WK.Libraries.WTL.Helpers;

namespace WK.Libraries.WTL
{
    /// <summary>
    /// A static class that provides helper properties and 
    /// methods for obtaining Windows 10 Theming settings.
    /// </summary>
    [DebuggerStepThrough]
    public static class ThemeListener
    {
        #region Constructor

        /// <summary>
        /// Initializes the <see cref="Theming"/> class.
        /// </summary>
        static ThemeListener()
        {
            AppMode = GetAppMode();
            WindowsMode = GetWindowsMode();
            AccentColor = GetAccentColor();

            _nwAppsThemeMode = GetAppMode();
            _nwWinThemeMode = GetWindowsMode();
            _nwAccentColor = GetAccentColor();

            TransparencyEnabled = GetTransparency();

            _watcher = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows");
            _watcher.RegistryChanged += OnRegistryChanged;

            Enabled = _enabled;

            _invoker.CreateControl();
        }

        #endregion

        #region Fields

        private static bool _enabled = true;
        private static bool _transparencyEnabled;

        private static ThemeModes _winThemeMode;
        private static ThemeModes _appsThemeMode;
        private static ThemeModes _nwWinThemeMode;
        private static ThemeModes _nwAppsThemeMode;

        private static Color _accentColor;
        private static Color _nwAccentColor;

        private static string _accentColorKey = "AccentColor";
        private static string _transparencyKey = "EnableTransparency";
        private static string _appsLightThemeKey = "AppsUseLightTheme";
        private static string _sysLightThemeKey = "SystemUsesLightTheme";
        private static string _regKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private static string _regKey2 = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM";

        private static RegistryMonitor _watcher;
        private static UserControl _invoker = new UserControl();

        #endregion

        #region Enumerations

        /// <summary>
        /// Provides the default Windows Themes.
        /// </summary>
        public enum ThemeModes
        {
            /// <summary>
            /// Windows Dark theme.
            /// </summary>
            Dark,

            /// <summary>
            /// Windows Light theme.
            /// </summary>
            Light
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="ThemeListener"/> is enabled.
        /// </summary>
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;

                if (_enabled)
                    _watcher.Start();
                else
                    _watcher.Stop();
            }
        }

        /// <summary>
        /// Gets the currently applied applications theme mode.
        /// </summary>
        public static ThemeModes AppMode
        {
            get => GetAppMode();
            private set => _appsThemeMode = value;
        }

        /// <summary>
        /// Gets the currently applied system-wide Windows theme mode.
        /// </summary>
        public static ThemeModes WindowsMode
        {
            get => GetWindowsMode();
            private set => _winThemeMode = value;
        }

        /// <summary>
        /// Gets the currently applied system accent color.
        /// </summary>
        public static Color AccentColor
        {
            get => GetAccentColor();
            private set => _accentColor = value;
        }

        /// <summary>
        /// Gets a value indicating whether transparency is enabled system-wide.
        /// </summary>
        public static bool TransparencyEnabled
        {
            get => GetTransparency();
            private set => _transparencyEnabled = value;
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Parses a Windows theme mode value.
        /// </summary>
        private static ThemeModes GetTheme(int value)
        {
            if (value == 1)
                return ThemeModes.Light;
            else
                return ThemeModes.Dark;
        }

        /// <summary>
        /// Gets the current apps theme mode.
        /// </summary>
        private static ThemeModes GetAppMode()
        {
            bool lightMode = Convert.ToBoolean(Registry.GetValue(_regKey, _appsLightThemeKey, 0));

            if (lightMode)
                _appsThemeMode = ThemeModes.Light;
            else
                _appsThemeMode = ThemeModes.Dark;

            return _appsThemeMode;
        }

        /// <summary>
        /// Gets the current Windows theme mode.
        /// </summary>
        private static ThemeModes GetWindowsMode()
        {
            bool lightMode = Convert.ToBoolean(Registry.GetValue(_regKey, _sysLightThemeKey, 0));

            if (lightMode)
                _winThemeMode = ThemeModes.Light;
            else
                _winThemeMode = ThemeModes.Dark;

            return _winThemeMode;
        }

        /// <summary>
        /// Gets the currently applied Windows accent color.
        /// </summary>
        private static Color GetAccentColor()
        {
            _accentColor = ColorTranslator.FromWin32(
                Convert.ToInt32(Registry.GetValue(_regKey2, _accentColorKey, "")));

            return _accentColor;
        }

        /// <summary>
        /// Gets a value indicating whether window transparency is enabled system-wide.
        /// </summary>
        private static bool GetTransparency()
        {
            _transparencyEnabled = Convert.ToBoolean(Registry.GetValue(_regKey, _transparencyKey, 0));

            return _transparencyEnabled;
        }

        /// <summary>
        /// Gets a Registry key value.
        /// </summary>
        private static object GetKeyValue(string path, string key)
        {
            return Registry.GetValue(path, key, 0);
        }

        #endregion

        #endregion

        #region Events

        #region Public

        #region Event Handlers

        /// <summary>
        /// Occurs whenever the <see cref="AppMode"/>, <see cref="WindowsMode"/> or 
        /// <see cref="AccentColor"/> have been changed.
        /// </summary>
        public static event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        #endregion

        #region Event Arguments

        /// <summary>
        /// Provides data for the <see cref="ThemeChanged"/> event.
        /// </summary>
        public class ThemeChangedEventArgs : EventArgs
        {
            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistryChangeEventArgs"/> class.
            /// </summary>
            /// <param name="oldAppsMode">The previously set Apps theme.</param>
            /// <param name="oldWinMode">The previously set System theme.</param>
            /// <param name="oldAccentColor">The previously set accent color.</param>
            /// <param name="newAppsMode">The newly set Apps theme.</param>
            /// <param name="newWinMode">The newly set System theme.</param>
            /// <param name="newAccentColor">The newly set accent color.</param>
            public ThemeChangedEventArgs(
                ThemeModes oldAppsMode, ThemeModes oldWinMode, Color oldAccentColor,
                ThemeModes newAppsMode, ThemeModes newWinMode, Color newAccentColor)
            {
                OldAppMode = oldAppsMode;
                OldWindowsMode = oldWinMode;
                OldAccentColor = oldAccentColor;
                NewAppMode = newAppsMode;
                NewWindowsMode = newWinMode;
                NewAccentColor = newAccentColor;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the previously applied apps theme mode.
            /// </summary>
            public ThemeModes OldAppMode { get; private set; }

            /// <summary>
            /// Gets the newly applied apps theme mode.
            /// </summary>
            public ThemeModes NewAppMode { get; private set; }

            /// <summary>
            /// Gets the previously applied Windows theme mode.
            /// </summary>
            public ThemeModes OldWindowsMode { get; private set; }

            /// <summary>
            /// Gets the newly applied Windows theme mode.
            /// </summary>
            public ThemeModes NewWindowsMode { get; private set; }

            /// <summary>
            /// Gets the previously applied Windows accent color.
            /// </summary>
            public Color OldAccentColor { get; private set; }

            /// <summary>
            /// Gets the newly applied Windows accent color.
            /// </summary>
            public Color NewAccentColor { get; private set; }

            #endregion
        }

        #endregion

        #endregion

        #region Private

        /// <summary>
        /// Raised whenever the theming Registry keys have changed.
        /// </summary>
        private static void OnRegistryChanged(object sender, EventArgs e)
        {
            object sysTheme = GetKeyValue(_regKey, _sysLightThemeKey);
            object appsTheme = GetKeyValue(_regKey, _appsLightThemeKey);
            object accentColor = GetKeyValue(_regKey2, _accentColorKey);
            
            if (sysTheme.ToString() == _sysLightThemeKey)
                _nwWinThemeMode = GetTheme((int)sysTheme);

            if (appsTheme.ToString() == _appsLightThemeKey)
                _nwAppsThemeMode = GetTheme((int)appsTheme);

            if (accentColor.ToString() == _accentColorKey)
                _nwAccentColor = ColorTranslator.FromWin32(Convert.ToInt32(accentColor));

            if (_winThemeMode != _nwWinThemeMode ||
                _appsThemeMode != _nwAppsThemeMode ||
                _accentColor != _nwAccentColor)
            {
                MessageBox.Show("Test");
                ThemeChanged?.Invoke(_watcher,
                    new ThemeChangedEventArgs(
                        _appsThemeMode, _winThemeMode,
                        _accentColor, _nwAppsThemeMode,
                        _nwWinThemeMode, _nwAccentColor));

                _winThemeMode = _nwWinThemeMode;
                _appsThemeMode = _nwAppsThemeMode;
                _accentColor = _nwAccentColor;
            }
        }

        #endregion

        #endregion
    }
}