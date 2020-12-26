/*
 * @Description: the back-end code of the home window
 * @Version: 1.2.0.20201224
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-12-24 14:12:09
 */

using ShSzStockHelper.ViewModels;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.ComponentModel;
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

        /// <summary>
        /// Initialise a new instance of the <see cref="HomeWindow"/> class.
        /// </summary>
        public HomeWindow()
        {
            InitializeComponent(); // Do essential tasks before invoking this method.

            _stockSymbolNameViewModel = new StockSymbolNameViewModel();
            _systemFontFamilyNameViewModel = new SystemFontFamilyNameViewModel();

            var productName = App.GetProductName();

            WindowHome.Title = productName + " " + App.GetProductVersion(); // Display the app name and the package version defined in "Properties\Package\Package version".
            //MenuItemAbout.Header = Properties.Resources.About + productName;
        } // end constructor HomeWindow

        #region Control Events
        private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // Allow opening only 1 settings window.
            foreach(Window window in Application.Current.Windows)
                if (window.GetType() == typeof(SettingsWindow))
                {
                    window.Activate();
                    return;
                } // end if

            new SettingsWindow(_systemFontFamilyNameViewModel).Show();
        } // end method ButtonSettings_OnClick

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
        // Add a new tab to the specified tab control.
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