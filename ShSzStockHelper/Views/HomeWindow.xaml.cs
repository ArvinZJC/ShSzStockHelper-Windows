/*
 * @Description: the back-end code of the home window
 * @Version: 1.2.1.20210409
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-09 14:12:09
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

            var productName = App.GetProductName();
            WindowHome.Title = productName + " " + App.GetProductVersion();
            _stockSymbolNameViewModel = new StockSymbolNameViewModel();
            _systemFontFamilyNameViewModel = new SystemFontFamilyNameViewModel();
        } // end constructor HomeWindow

        #region Control Events
        // Show settings window.
        private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
        {
            TabControlStrikePriceVolume.Focus(); // Make the settings button lose focus.

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

        // Show the hint for adding a new tab after closing all tabs.
        private void TabControlStrikePriceVolume_OnCloseAllTabs(object sender, CloseTabEventArgs e)
        {
            TextBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_OnCloseAllTabs

        // Show the hint for adding a new tab if applicable.
        private void TabControlStrikePriceVolume_TabClosed(object sender, CloseTabEventArgs e)
        {
            if (TabControlStrikePriceVolume.SelectedItem == null)
                TextBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_TabClosed

        // Load initial content on the home window when it is loaded.
        private async void WindowHome_LoadedAsync(object sender, RoutedEventArgs e)
        {
            BusyIndicatorHomeWindowLoading.IsBusy = true;
            
            await _stockSymbolNameViewModel.LoadDataAsync();
            await _systemFontFamilyNameViewModel.LoadDataAsync();

            TextBlockNewTabHint.Visibility = Visibility.Hidden;
            AddNewTab();
            BusyIndicatorHomeWindowLoading.IsBusy = false;
        } // end method WindowHome_LoadedAsync

        // Close the settings window (if exists) when the home window is closed.
        private void WindowHome_OnClosing(object sender, CancelEventArgs e)
        {
            foreach(Window window in Application.Current.Windows)
                if (window.GetType() == typeof(SettingsWindow))
                    window.Close();
        } // end method WindowHome_OnClosing
        #endregion Control Events

        #region Private Methods
        /// <summary>
        /// Add a new tab to the specified tab control.
        /// </summary>
        private void AddNewTab()
        {
            var tabItemStrikePriceVolume = new TabItemExt
            {
                Header = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                IsTabStop = false,
                ItemToolTip = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                MaxWidth = Properties.Settings.Default.MaxTabItemWidth
            };
            tabItemStrikePriceVolume.Content = new StrikePriceVolumeTab(tabItemStrikePriceVolume, _stockSymbolNameViewModel);
            TabControlStrikePriceVolume.Items.Add(tabItemStrikePriceVolume);
        } // end method AddNewTab
        #endregion Private Methods
    } // end class HomeWindow
} // end namespace ShSzStockHelper.Views