/*
 * @Description: the back-end code of the home window
 * @Version: 1.0.7.20200816
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-16 14:12:09
 */

using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for the home window.
    /// </summary>
    public partial class HomeWindow : ChromelessWindow
    {
        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly StylePropertyViewModel _stylePropertyViewModel;
        private readonly DispatcherTimer _gcTimer;

        /// <summary>
        /// Initialise a new instance of the <see cref="HomeWindow"/> class.
        /// </summary>
        public HomeWindow()
        {
            InitializeComponent();

            _stockSymbolNameViewModel = new StockSymbolNameViewModel(); // Initialise the view model class for getting data of stocks' symbols and corresponding names.
            _stylePropertyViewModel = new StylePropertyViewModel(); // Initialise the view model class for using the defined style properties.
            _gcTimer = new DispatcherTimer();

            windowHome.Title = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductName + " " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion; // Display the app name and the package version defined in "Properties\Package\Package version".

            // TODO: need long time not too often
            _gcTimer.Interval = TimeSpan.FromSeconds(10); // Force collecting garbage every 10 seconds.
            _gcTimer.Start();
            _gcTimer.Tick += new EventHandler(OnGarbageCollection);
        } // end constructor HomeWindow

        ~HomeWindow()
        {
            _gcTimer.Tick -= new EventHandler(OnGarbageCollection);
        } // end deconstructor HomeWindow

        #region Control Events
        // Perform actions to force collecting garbage.
        private void OnGarbageCollection(object sender, EventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        } // end method OnGarbageCollection

        // Add a new tab with the specified content when the new button of the specified tab control is clicked.
        private void TabControlStrikePriceVolume_NewButtonClick(object sender, EventArgs e)
        {
            textBlockNewTabHint.Visibility = Visibility.Hidden;

            AddNewTab();
        } // end method TabControlStrikePriceVolume_NewButtonClick

        private void TabControlStrikePriceVolume_OnCloseAllTabs(object sender, CloseTabEventArgs e)
        {
            textBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_OnCloseAllTabs

        private void TabControlStrikePriceVolume_TabClosed(object sender, CloseTabEventArgs e)
        {
            if (tabControlStrikePriceVolume.SelectedItem == null)
                textBlockNewTabHint.Visibility = Visibility.Visible;
        } // end method TabControlStrikePriceVolume_TabClosed

        private async void WindowHome_LoadedAsync(object sender, RoutedEventArgs e)
        {
            busyIndicatorHomeWindowLoading.IsBusy = true;

            await _stockSymbolNameViewModel.LoadDataAsync();

            textBlockNewTabHint.Visibility = Visibility.Hidden;
            AddNewTab();
            busyIndicatorHomeWindowLoading.IsBusy = false;
        } // end method WindowHome_LoadedAsync
        #endregion Control Events

        #region Private Methods
        private void AddNewTab()
        {
            TabItemExt tabItemStrikePriceVolume = new TabItemExt
            {
                Header = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                HeaderTemplate = Application.Current.Resources["TabItemHeaderTemplate"] as DataTemplate,
                ItemToolTip = Properties.Resources.TabItemStrikePriceVolume_Header_NewTab,
                MaxWidth = _stylePropertyViewModel.MaxTabItemWidth
            };

            tabItemStrikePriceVolume.Content = new StrikePriceVolumeTab(tabItemStrikePriceVolume, _stockSymbolNameViewModel);
            tabControlStrikePriceVolume.Items.Add(tabItemStrikePriceVolume);
        } // end method AddNewTab
        #endregion Private Methods
    } // end class HomeWindow
} // end namespace ShSzStockHelper