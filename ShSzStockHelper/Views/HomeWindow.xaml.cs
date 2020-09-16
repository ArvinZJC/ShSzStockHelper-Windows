/*
 * @Description: the back-end code of the home window
 * @Version: 1.1.6.20200916
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-16 14:12:09
 */

using ShSzStockHelper.ViewModels;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the home window.
    /// </summary>
    public partial class HomeWindow
    {
        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly SystemFontFamilyNameViewModel _systemFontFamilyNameViewModel;
        private readonly string _productName, _productVersion, _productCopyright;

        /// <summary>
        /// Initialise a new instance of the <see cref="HomeWindow"/> class.
        /// </summary>
        public HomeWindow()
        {
            InitializeComponent(); // Do essential tasks before invoking this method.

            _stockSymbolNameViewModel = new StockSymbolNameViewModel();
            _systemFontFamilyNameViewModel = new SystemFontFamilyNameViewModel();
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
        } // end constructor HomeWindow

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