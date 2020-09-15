/*
 * @Description: the back-end code of the home window
 * @Version: 1.1.5.20200907
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-07 14:12:09
 */

using ShSzStockHelper.ViewModels;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the home window.
    /// </summary>
    public partial class HomeWindow
    {
        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly SystemFontFamilyNameViewModel _systemFontFamilyNameViewModel;
        private readonly DispatcherTimer _gcTimer;
        private readonly string _productName, _productVersion, _productCopyright;

        /// <summary>
        /// Initialise a new instance of the <see cref="HomeWindow"/> class.
        /// </summary>
        public HomeWindow()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Properties.Settings.Default.CultureInfo); // It is necessary to specify the culture info here and in the name of the resource file "Syncfusion.Tools.Wpf" to avoid the issue that some text of the tab control is not displayed in simplified Chinese.
            
            InitializeComponent(); // Do essential tasks before invoking this method.

            _stockSymbolNameViewModel = new StockSymbolNameViewModel();
            _systemFontFamilyNameViewModel = new SystemFontFamilyNameViewModel();
            _gcTimer = new DispatcherTimer();
            _productName = Properties.Resources.NullProductNameError;
            _productVersion = Properties.Resources.NullProductVersionError;
            _productCopyright = Properties.Resources.NullProductCopyrightError;

            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                _productName = FileVersionInfo.GetVersionInfo(assembly.Location).ProductName;

                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var assemblyCopyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();

                if (assemblyVersionAttribute != null)
                    _productVersion = assemblyVersionAttribute.InformationalVersion;

                if (assemblyCopyrightAttribute != null)
                    _productCopyright = assemblyCopyrightAttribute.Copyright;
            } // end if

            WindowHome.Title = _productName + " " + _productVersion; // Display the app name and the package version defined in "Properties\Package\Package version".
            MenuItemAbout.Header = Properties.Resources.About + _productName;

            // TODO: need long time not too often
            _gcTimer.Interval = TimeSpan.FromSeconds(10); // Force collecting garbage every 10 seconds.
            _gcTimer.Start();
            _gcTimer.Tick += OnGarbageCollection;
        } // end constructor HomeWindow

        ~HomeWindow()
        {
            _gcTimer.Tick -= OnGarbageCollection;
        } // end destructor HomeWindow

        #region Control Events
        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            new AboutWindow(_productName, _productVersion, _productCopyright) {Owner = this}.ShowDialog();
        } // end method MenuItemAbout_OnClick

        private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // Allow opening only 1 settings window.
            foreach(Window window in Application.Current.Windows)
                if (window.GetType() == typeof(SettingsWindow))
                {
                    window.Activate();
                    return;
                } // end if

            new SettingsWindow(_systemFontFamilyNameViewModel).Show();
        } // end method MenuItemSettings_OnClick

        // Perform actions to force collecting garbage.
        private static void OnGarbageCollection(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        } // end method OnGarbageCollection

        // Add a new tab with the specified content when the new button of the specified tab control is clicked.
        private void TabControlStrikePriceVolume_NewButtonClick(object sender, EventArgs e)
        {
            TextBlockNewTabHint.Visibility = Visibility.Hidden;

            AddNewTab();
        } // end method TabControlStrikePriceVolume_NewButtonClick

        private void TabControlStrikePriceVolume_OnCloseAllTabs(object sender, CloseTabEventArgs e)
        {
            TextBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_OnCloseAllTabs

        private void TabControlStrikePriceVolume_TabClosed(object sender, CloseTabEventArgs e)
        {
            if (TabControlStrikePriceVolume.SelectedItem == null)
                TextBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_TabClosed

        private async void WindowHome_LoadedAsync(object sender, RoutedEventArgs e)
        {
            BusyIndicatorHomeWindowLoading.IsBusy = true;

            await _stockSymbolNameViewModel.LoadDataAsync();
            await _systemFontFamilyNameViewModel.LoadDataAsync();

            TextBlockNewTabHint.Visibility = Visibility.Hidden;
            AddNewTab();
            BusyIndicatorHomeWindowLoading.IsBusy = false;
        } // end method WindowHome_LoadedAsync

        private void WindowHome_OnClosing(object sender, CancelEventArgs e)
        {
            // Close the settings window (if exists) when the home window is closed.
            foreach(Window window in Application.Current.Windows)
                if (window.GetType() == typeof(SettingsWindow))
                    window.Close();
        } // end method WindowHome_OnClosing
        #endregion Control Events

        #region Private Methods
        private void AddNewTab()
        {
            var tabItemStrikePriceVolume = new TabItemExt
            {
                Header = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                HeaderTemplate = Application.Current.Resources["TabItemHeaderTemplate"] as DataTemplate,
                ItemToolTip = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                MaxWidth = Properties.Settings.Default.MaxTabItemWidth
            };

            tabItemStrikePriceVolume.Content = new StrikePriceVolumeTab(tabItemStrikePriceVolume, _stockSymbolNameViewModel);
            TabControlStrikePriceVolume.Items.Add(tabItemStrikePriceVolume);
        } // end method AddNewTab
        #endregion Private Methods
    } // end class HomeWindow
} // end namespace ShSzStockHelper.Views