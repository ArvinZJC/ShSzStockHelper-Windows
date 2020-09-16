/*
 * @Description: the back-end code of the settings window
 * @Version: 1.1.4.20200916
 * @Author: Arvin Zhao
 * @Date: 2020-08-31 12:22:18
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-16 12:38:34
 */

using ShSzStockHelper.Models;
using ShSzStockHelper.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Controls.Navigation;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the settings window.
    /// </summary>
    public partial class SettingsWindow
    {
        private bool _hasMouseOnTreeNavigatorSettings; // A flag indicating if the mouse is in the area of the specified tree navigator.

        /// <summary>
        /// Initialise a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        /// <param name="systemFontFamilyNameViewModel">A <see cref="SystemFontFamilyNameViewModel"/> object to load data of the system's font family names.</param>
        public SettingsWindow(SystemFontFamilyNameViewModel systemFontFamilyNameViewModel)
        {
            InitializeComponent();
            
            // Theme setting.
            TextBlockThemeExplanation.Text += Properties.Resources.DefaultSettingValue
                                              + ((VisualStyles)Properties.DefaultUserSettings.Default.ProductTheme == VisualStyles.MaterialDark
                                                  ? Properties.Resources.ComboBoxItemMaterialDark_Content
                                                  : Properties.Resources.ComboBoxItemMaterialLight_Content);
            ComboBoxTheme.SelectedItem = (VisualStyles)Properties.Settings.Default.ProductTheme == VisualStyles.MaterialDark
                ? ComboBoxItemMaterialDark
                : ComboBoxItemMaterialLight;

            // Display font family name setting.
            TextBlockDisplayFontFamilyNameExplanation.Text += Properties.Resources.DefaultSettingValue + Properties.DefaultUserSettings.Default.DisplayFontFamilyName;
            ComboBoxDisplayFontFamilyName.ItemsSource = systemFontFamilyNameViewModel.SystemFontFamilyNameRecords;
            ComboBoxDisplayFontFamilyName.SelectedItem = Properties.Settings.Default.DisplayFontFamilyName;
            
            // Date display format setting.
            TextBlockDateDisplayFormatExplanation.Text += Properties.Resources.DefaultSettingValue + Properties.DefaultUserSettings.Default.DateDisplayFormat;

            if (Properties.Settings.Default.DateDisplayFormat.Equals((string) ComboBoxItem2Y1M1DHyphen.Content))
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem2Y1M1DHyphen;
            else if (Properties.Settings.Default.DateDisplayFormat.Equals((string) ComboBoxItem2Y2M2DHyphen.Content))
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem2Y2M2DHyphen;
            else if (Properties.Settings.Default.DateDisplayFormat.Equals((string) ComboBoxItem4Y1M1DHyphen.Content))
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem4Y1M1DHyphen;
            else if (Properties.Settings.Default.DateDisplayFormat.Equals((string) ComboBoxItem4Y2M2DHyphen.Content))
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem4Y2M2DHyphen;
            else if (Properties.Settings.Default.DateDisplayFormat.Equals((string) ComboBoxItem2Y1M1DCn.Content))
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem2Y1M1DCn;
            else
                ComboBoxDateDisplayFormat.SelectedItem = ComboBoxItem4Y1M1DCn;

            // Min date setting.
            TextBlockMinDateExplanation.Text = Properties.Resources.MinDateSetting_Explanation
                                               + Properties.Resources.DefaultSettingValue
                                               + Properties.DefaultUserSettings.Default.MinDate.ToString(Properties.Settings.Default.DateDisplayFormat);
            DatePickerMinDate.CultureInfo = new CultureInfo(Properties.Settings.Default.CultureInfo);

            var tableColumnSettingsExplanation = Properties.Resources.DayVolume + Properties.Resources.TableColumnSettings_Explanation;

            // Sorting setting.
            TextBlockSortingExplanation.Text += tableColumnSettingsExplanation;
            CheckListBoxItemStrikePriceSorting.Content += Properties.Resources.LeftBracket +
                                                          Properties.Resources.DefaultSettingValue +
                                                          (Properties.DefaultUserSettings.Default.StrikePriceSorting
                                                              ? Properties.Resources.Checked
                                                              : Properties.Resources.Unchecked) +
                                                          Properties.Resources.RightBracket;
            CheckListBoxItemTotalVolumeSorting.Content += Properties.Resources.LeftBracket +
                                                          Properties.Resources.DefaultSettingValue +
                                                          (Properties.DefaultUserSettings.Default.TotalVolumeSorting
                                                              ? Properties.Resources.Checked
                                                              : Properties.Resources.Unchecked) +
                                                          Properties.Resources.RightBracket;
            CheckListBoxItemDayVolumeSorting.Content += Properties.Resources.LeftBracket +
                                                          Properties.Resources.DefaultSettingValue +
                                                          (Properties.DefaultUserSettings.Default.DayVolumeSorting
                                                              ? Properties.Resources.Checked
                                                              : Properties.Resources.Unchecked) +
                                                          Properties.Resources.RightBracket;

            // Filtering setting.
            TextBlockFilteringExplanation.Text += tableColumnSettingsExplanation;
            CheckListBoxItemStrikePriceFiltering.Content += Properties.Resources.LeftBracket +
                                                          Properties.Resources.DefaultSettingValue +
                                                          (Properties.DefaultUserSettings.Default.StrikePriceFiltering
                                                              ? Properties.Resources.Checked
                                                              : Properties.Resources.Unchecked) +
                                                          Properties.Resources.RightBracket;
            CheckListBoxItemTotalVolumeFiltering.Content += Properties.Resources.LeftBracket +
                                                          Properties.Resources.DefaultSettingValue +
                                                          (Properties.DefaultUserSettings.Default.TotalVolumeFiltering
                                                              ? Properties.Resources.Checked
                                                              : Properties.Resources.Unchecked) +
                                                          Properties.Resources.RightBracket;
            CheckListBoxItemDayVolumeFiltering.Content += Properties.Resources.LeftBracket +
                                                        Properties.Resources.DefaultSettingValue +
                                                        (Properties.DefaultUserSettings.Default.DayVolumeFiltering
                                                            ? Properties.Resources.Checked
                                                            : Properties.Resources.Unchecked) +
                                                        Properties.Resources.RightBracket;

            // Volume unit setting.
            var volumeUnitViewModel = new VolumeUnitViewModel();
            
            TextBlockTotalVolumeUnit.Text += Properties.Resources.LeftBracket
                                             + Properties.Resources.DefaultSettingValue
                                             + volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.DefaultUserSettings.Default.TotalVolumeUnit).Name
                                             + Properties.Resources.RightBracket
                                             + Properties.Resources.Colon;
            TextBlockDayVolumeUnit.Text += Properties.Resources.LeftBracket
                                           + Properties.Resources.DefaultSettingValue
                                           + volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.DefaultUserSettings.Default.DayVolumeUnit).Name
                                           + Properties.Resources.RightBracket
                                           + Properties.Resources.Colon;
            ComboBoxTotalVolumeUnit.ItemsSource = volumeUnitViewModel.VolumeUnitRecords;
            ComboBoxTotalVolumeUnit.SelectedItem = volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.TotalVolumeUnit);
            ComboBoxDayVolumeUnit.ItemsSource = volumeUnitViewModel.VolumeUnitRecords;
            ComboBoxDayVolumeUnit.SelectedItem = volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.DayVolumeUnit);

            // Volume decimal digits setting.
            TextBlockVolumeDecimalDigitsExplanation.Text += tableColumnSettingsExplanation;
            TextBlockTotalVolumeDecimalDigits.Text += Properties.Resources.LeftBracket
                                                      + Properties.Resources.DefaultSettingValue
                                                      + Properties.DefaultUserSettings.Default.TotalVolumeDecimalDigits
                                                      + Properties.Resources.RightBracket
                                                      + Properties.Resources.Colon;
            TextBlockDayVolumeDecimalDigits.Text += Properties.Resources.LeftBracket
                                                    + Properties.Resources.DefaultSettingValue
                                                    + Properties.DefaultUserSettings.Default.DayVolumeDecimalDigits
                                                    + Properties.Resources.RightBracket
                                                    + Properties.Resources.Colon;

            var volumeDecimalDigitsArray = new[] {"0", "1", "2", "3", "4", "5", "6"};

            ComboBoxTotalVolumeDecimalDigits.ItemsSource = volumeDecimalDigitsArray;
            ComboBoxTotalVolumeDecimalDigits.SelectedIndex = Properties.Settings.Default.TotalVolumeDecimalDigits;
            ComboBoxDayVolumeDecimalDigits.ItemsSource = volumeDecimalDigitsArray;
            ComboBoxDayVolumeDecimalDigits.SelectedIndex = Properties.Settings.Default.DayVolumeDecimalDigits;
            
            // Excel cell's font family name setting.
            TextBlockExcelCellFontFamilyNameExplanation.Text += Properties.Resources.DefaultSettingValue + Properties.DefaultUserSettings.Default.ExcelCellFontFamilyName;
            ComboBoxExcelCellFontFamilyName.ItemsSource = systemFontFamilyNameViewModel.SystemFontFamilyNameRecords;
            ComboBoxExcelCellFontFamilyName.SelectedItem = Properties.Settings.Default.ExcelCellFontFamilyName;

            // Excel cell's font size setting.
            TextBlockExcelCellFontSizeExplanation.Text += Properties.Resources.DefaultSettingValue + Properties.DefaultUserSettings.Default.ExcelCellFontSize;

            var excelCellFontSizeArray = new[] {"6", "7", "8", "9", "10", "11", "12", "14", "16", "18", "20"};

            ComboBoxExcelCellFontSize.ItemsSource = excelCellFontSizeArray;
            ComboBoxExcelCellFontSize.SelectedItem = excelCellFontSizeArray.First(item => item.Equals(Properties.Settings.Default.ExcelCellFontSize.ToString("F0")));

            // Excel file format setting.
            TextBlockExcelFileFormatExplanation.Text += Properties.Resources.DefaultSettingValue
                                                        + (Properties.DefaultUserSettings.Default.ExcelFileFormat == 1
                                                            ? Properties.Resources.ComboBoxItemXls_Content
                                                            : Properties.Resources.ComboBoxItemXlsx_Content);
            ComboBoxExcelFileFormat.SelectedItem = Properties.Settings.Default.ExcelFileFormat == 1
                ? ComboBoxItemXls
                : ComboBoxItemXlsx;
        } // end constructor SettingsWindow

        #region Control Events
        private void CheckListBoxFiltering_OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.StrikePrice))
            {
                if (e.Checked == Properties.Settings.Default.StrikePriceFiltering)
                    return;
                
                Properties.Settings.Default.StrikePriceFiltering = e.Checked;
            } // end if

            if (((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.TotalVolume))
            {
                if (e.Checked == Properties.Settings.Default.TotalVolumeFiltering)
                    return;

                Properties.Settings.Default.TotalVolumeFiltering = e.Checked;
            } // end if

            if (!((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.DayVolume))
                return;

            if (e.Checked == Properties.Settings.Default.DayVolumeFiltering)
                return;

            Properties.Settings.Default.DayVolumeFiltering = e.Checked;
        } // end method CheckListBoxFiltering_OnItemChecked

        private void CheckListBoxSorting_OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.StrikePrice))
            {
                if (e.Checked == Properties.Settings.Default.StrikePriceSorting)
                    return;
                
                Properties.Settings.Default.StrikePriceSorting = e.Checked;
            } // end if

            if (((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.TotalVolume))
            {
                if (e.Checked == Properties.Settings.Default.TotalVolumeSorting)
                    return;

                Properties.Settings.Default.TotalVolumeSorting = e.Checked;
            } // end if

            if (!((string) ((CheckListBoxItem) e.Item).Content).StartsWith(Properties.Resources.DayVolume))
                return;

            if (e.Checked == Properties.Settings.Default.DayVolumeSorting)
                return;

            Properties.Settings.Default.DayVolumeSorting = e.Checked;
        } // end method CheckListBoxSorting_OnItemChecked

        private void ComboBoxDateDisplayFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDateDisplayFormat.SelectedItem != null && ((ComboBoxItemAdv) ComboBoxDateDisplayFormat.SelectedItem).Content.Equals(Properties.Settings.Default.DateDisplayFormat))
                return;

            Properties.Settings.Default.DateDisplayFormat = ComboBoxDateDisplayFormat.SelectedItem == null
                ? Properties.DefaultUserSettings.Default.DateDisplayFormat
                : (string)((ComboBoxItemAdv) ComboBoxDateDisplayFormat.SelectedItem).Content;

            TextBlockMinDateExplanation.Text = Properties.Resources.MinDateSetting_Explanation
                                               + Properties.Resources.DefaultSettingValue
                                               + Properties.DefaultUserSettings.Default.MinDate.ToString(Properties.Settings.Default.DateDisplayFormat);
        } // end method ComboBoxDateDisplayFormat_OnSelectionChanged

        private void ComboBoxDayVolumeDecimalDigits_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDayVolumeDecimalDigits.SelectedItem != null && ComboBoxDayVolumeDecimalDigits.SelectedIndex == Properties.Settings.Default.DayVolumeDecimalDigits)
                return;

            Properties.Settings.Default.DayVolumeDecimalDigits = ComboBoxDayVolumeDecimalDigits.SelectedItem == null
                ? Properties.DefaultUserSettings.Default.DayVolumeDecimalDigits
                : ComboBoxDayVolumeDecimalDigits.SelectedIndex;
        } // end method ComboBoxDayVolumeDecimalDigits_OnSelectionChanged

        private void ComboBoxDayVolumeUnit_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDayVolumeUnit.SelectedItem != null && ((VolumeUnitData) ComboBoxDayVolumeUnit.SelectedItem).Coefficient == Properties.Settings.Default.DayVolumeUnit)
                return;

            Properties.Settings.Default.DayVolumeUnit = ((VolumeUnitData) ComboBoxDayVolumeUnit.SelectedItem)?.Coefficient ?? Properties.DefaultUserSettings.Default.DayVolumeUnit;
        } // end method ComboBoxDayVolumeUnit_OnSelectionChanged

        private void ComboBoxDisplayFontFamilyName_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxDisplayFontFamilyName.SelectedItem != null && ComboBoxDisplayFontFamilyName.SelectedItem.Equals(Properties.Settings.Default.DisplayFontFamilyName))
                return;

            Properties.Settings.Default.DisplayFontFamilyName = ComboBoxDisplayFontFamilyName.SelectedItem == null
                ? Properties.DefaultUserSettings.Default.DisplayFontFamilyName
                : (string)ComboBoxDisplayFontFamilyName.SelectedItem;
        } // end method ComboBoxDisplayFontFamilyName_OnSelectionChanged

        private void ComboBoxExcelCellFontFamilyName_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxExcelCellFontFamilyName.SelectedItem != null && ComboBoxExcelCellFontFamilyName.SelectedItem.Equals(Properties.Settings.Default.ExcelCellFontFamilyName))
                return;

            Properties.Settings.Default.ExcelCellFontFamilyName = ComboBoxExcelCellFontFamilyName.SelectedItem == null
                ? Properties.DefaultUserSettings.Default.ExcelCellFontFamilyName
                : (string)ComboBoxExcelCellFontFamilyName.SelectedItem;
        } // end method ComboBoxExcelCellFontFamilyName_OnSelectionChanged

        private void ComboBoxExcelCellFontSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxExcelCellFontSize.SelectedItem != null && ComboBoxExcelCellFontSize.SelectedItem.Equals(Properties.Settings.Default.ExcelCellFontSize.ToString("F0")))
                return;

            Properties.Settings.Default.ExcelCellFontSize = double.TryParse((string) ComboBoxExcelCellFontSize.SelectedItem, out var excelCellFontSize)
                ? excelCellFontSize
                : Properties.DefaultUserSettings.Default.ExcelCellFontSize;
        } // end method ComboBoxExcelCellFontSize_OnSelectionChanged

        private void ComboBoxExcelFileFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxExcelFileFormat.SelectedItem != null)
            {
                var selectedExcelFileFormat = ((ComboBoxItemAdv) ComboBoxExcelFileFormat.SelectedItem).Content.Equals(Properties.Resources.ComboBoxItemXls_Content)
                    ? 1
                    : 2;

                if (selectedExcelFileFormat == Properties.Settings.Default.ExcelFileFormat)
                    return;

                Properties.Settings.Default.ExcelFileFormat = selectedExcelFileFormat;
            }
            else
                Properties.Settings.Default.ExcelFileFormat = Properties.DefaultUserSettings.Default.ExcelFileFormat;
        } // end method ComboBoxExcelFileFormat_OnSelectionChanged

        private void ComboBoxTheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTheme.SelectedItem == null)
                return;

            var selectedTheme = ((ComboBoxItemAdv) ComboBoxTheme.SelectedItem).Content.Equals(Properties.Resources.ComboBoxItemMaterialDark_Content)
                    ? (int) VisualStyles.MaterialDark
                    : (int) VisualStyles.MaterialLight;

            if (selectedTheme == Properties.Settings.Default.ProductTheme)
                return;

            var productRestartConfirmationDialogueResult = new ProductRestartConfirmationDialogue{Owner = this}.ShowDialog();

            if (productRestartConfirmationDialogueResult == null || !(bool) productRestartConfirmationDialogueResult)
            {
                ComboBoxTheme.SelectedItem = selectedTheme == (int) VisualStyles.MaterialLight
                    ? ComboBoxItemMaterialDark
                    : ComboBoxItemMaterialLight;
                return;
            } // end if

            Properties.Settings.Default.ProductTheme = selectedTheme;

            if (Properties.Settings.Default.ProductTheme == (int) VisualStyles.MaterialDark)
            {
                Properties.Settings.Default.ThemeColour = Properties.DefaultUserSettings.Default.ThemeColour_MaterialDark;
                Properties.Settings.Default.PrimaryTextColour = Properties.DefaultUserSettings.Default.PrimaryTextColour_MaterialDark;
                Properties.Settings.Default.ContentTextColour = Properties.DefaultUserSettings.Default.ContentTextColour_MaterialDark;
                Properties.Settings.Default.MenuIconUri = Properties.DefaultUserSettings.Default.MenuIconUri_MaterialDark;
                Properties.Settings.Default.SettingsIconUri = Properties.DefaultUserSettings.Default.SettingsIconUri_MaterialDark;
                Properties.Settings.Default.AboutIconUri = Properties.DefaultUserSettings.Default.AboutIconUri_MaterialDark;
                Properties.Settings.Default.SearchIconUri = Properties.DefaultUserSettings.Default.SearchIconUri_MaterialDark;
                Properties.Settings.Default.ClearSelectionIconUri = Properties.DefaultUserSettings.Default.ClearSelectionIconUri_MaterialDark;
                Properties.Settings.Default.RestoreColumnWidthIconUri = Properties.DefaultUserSettings.Default.RestoreColumnWidthIconUri_MaterialDark;
                Properties.Settings.Default.ExportToExcelIconUri = Properties.DefaultUserSettings.Default.ExportToExcelIconUri_MaterialDark;
                Properties.Settings.Default.PrintIconUri = Properties.DefaultUserSettings.Default.PrintIconUri_MaterialDark;
            }
            else
            {
                Properties.Settings.Default.ThemeColour = Properties.DefaultUserSettings.Default.ThemeColour_MaterialLight;
                Properties.Settings.Default.PrimaryTextColour = Properties.DefaultUserSettings.Default.PrimaryTextColour_MaterialLight;
                Properties.Settings.Default.ContentTextColour = Properties.DefaultUserSettings.Default.ContentTextColour_MaterialLight;
                Properties.Settings.Default.MenuIconUri = Properties.DefaultUserSettings.Default.MenuIconUri_MaterialLight;
                Properties.Settings.Default.SettingsIconUri = Properties.DefaultUserSettings.Default.SettingsIconUri_MaterialLight;
                Properties.Settings.Default.AboutIconUri = Properties.DefaultUserSettings.Default.AboutIconUri_MaterialLight;
                Properties.Settings.Default.SearchIconUri = Properties.DefaultUserSettings.Default.SearchIconUri_MaterialLight;
                Properties.Settings.Default.ClearSelectionIconUri = Properties.DefaultUserSettings.Default.ClearSelectionIconUri_MaterialLight;
                Properties.Settings.Default.RestoreColumnWidthIconUri = Properties.DefaultUserSettings.Default.RestoreColumnWidthIconUri_MaterialLight;
                Properties.Settings.Default.ExportToExcelIconUri = Properties.DefaultUserSettings.Default.ExportToExcelIconUri_MaterialLight;
                Properties.Settings.Default.PrintIconUri = Properties.DefaultUserSettings.Default.PrintIconUri_MaterialLight;
            } // end if...else

            Properties.Settings.Default.Save();

            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
                Process.Start(Directory.GetCurrentDirectory() + "\\" + assembly.GetName().Name + ".exe");

            Application.Current.Shutdown();
        } // end method ComboBoxTheme_OnSelectionChanged

        private void ComboBoxTotalVolumeDecimalDigits_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTotalVolumeDecimalDigits.SelectedItem != null && ComboBoxTotalVolumeDecimalDigits.SelectedIndex == Properties.Settings.Default.TotalVolumeDecimalDigits)
                return;

            Properties.Settings.Default.TotalVolumeDecimalDigits = ComboBoxTotalVolumeDecimalDigits.SelectedItem == null
                ? Properties.DefaultUserSettings.Default.TotalVolumeDecimalDigits
                : ComboBoxTotalVolumeDecimalDigits.SelectedIndex;
        } // end method ComboBoxTotalVolumeDecimalDigits_OnSelectionChanged

        private void ComboBoxTotalVolumeUnit_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTotalVolumeUnit.SelectedItem != null && ((VolumeUnitData) ComboBoxTotalVolumeUnit.SelectedItem).Coefficient == Properties.Settings.Default.TotalVolumeUnit)
                return;

            Properties.Settings.Default.TotalVolumeUnit = ((VolumeUnitData) ComboBoxTotalVolumeUnit.SelectedItem)?.Coefficient ?? Properties.DefaultUserSettings.Default.TotalVolumeUnit;
        } // end method ComboBoxTotalVolumeUnit_OnSelectionChanged

        private void DatePickerMinDate_DateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            /*
             * Step 1. The date picker first holds null value.
             * Step 2. Its value is modified automatically to today because null value is not allowed.
             * Step 3. It changes to the value specified by the property "DateTime" when the program initialise controls.
             * This condition code block is to avoid saving wrong setting value in Step 2.
             */
            if (e.OldValue == null)
                return;

            var selectedDateTime = e.NewValue.ToDateTime();
            
            Properties.Settings.Default.MinDate = new DateTime(selectedDateTime.Year, selectedDateTime.Month, selectedDateTime.Day);
        } // end method DatePickerMinDate_DateTimeChanged

        private void ScrollViewerSettings_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Change the selected item of the specified tree navigator programmatically, only if the user scrolls the specified scroll viewer.
            if (_hasMouseOnTreeNavigatorSettings)
                return;

            var heightTheme = StackPanelGeneral.ActualHeight + StackPanelTheme.ActualHeight;
            var heightDisplayFontFamilyName = heightTheme + StackPanelDisplayFontFamilyName.ActualHeight;
            var heightDateDisplayFormat = heightDisplayFontFamilyName + StackPanelDateDisplayFormat.ActualHeight;
            var heightMinDate = heightDateDisplayFormat + StackPanelMinDate.ActualHeight;
            var heightTable = heightMinDate + StackPanelTable.ActualHeight;
            var heightSorting = heightTable + StackPanelSorting.ActualHeight;
            var heightFiltering = heightSorting + StackPanelFiltering.ActualHeight;
            var heightVolumeUnit = heightFiltering + StackPanelVolumeUnit.ActualHeight;
            var heightVolumeDecimalDigits = heightVolumeUnit + StackPanelVolumeDecimalDigits.ActualHeight;
            var heightExportToExcel = heightVolumeDecimalDigits + StackPanelExportToExcel.ActualHeight;
            var heightExcelCellFontFamilyName = heightExportToExcel + StackPanelExcelCellFontFamilyName.ActualHeight;
            var heightExcelCellFontSize = heightExcelCellFontFamilyName + StackPanelExcelCellFontSize.ActualHeight;


            if (ScrollViewerSettings.VerticalOffset < StackPanelGeneral.ActualHeight)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemGeneral;
            else if (ScrollViewerSettings.VerticalOffset < heightTheme)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemTheme;
            else if (ScrollViewerSettings.VerticalOffset < heightDisplayFontFamilyName)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemDisplayFontFamily;
            else if (ScrollViewerSettings.VerticalOffset < heightDateDisplayFormat)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemDateDisplayFormat;
            else if (ScrollViewerSettings.VerticalOffset < heightMinDate)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemMinDate;
            else if (ScrollViewerSettings.VerticalOffset < heightTable)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemTable;
            else if (ScrollViewerSettings.VerticalOffset < heightSorting)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemSorting;
            else if (ScrollViewerSettings.VerticalOffset < heightFiltering)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemFiltering;
            else if (ScrollViewerSettings.VerticalOffset < heightVolumeUnit)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemVolumeUnit;
            else if (ScrollViewerSettings.VerticalOffset < heightVolumeDecimalDigits)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemVolumeDecimalDigits;
            else if (ScrollViewerSettings.VerticalOffset < heightExportToExcel)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemExportToExcel;
            else if (ScrollViewerSettings.VerticalOffset < heightExcelCellFontFamilyName)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemExcelCellFontFamilyName;
            else if (ScrollViewerSettings.VerticalOffset < heightExcelCellFontSize)
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemExcelCellFontSize;
            else
                TreeNavigatorSettings.SelectedItem = TreeNavigatorItemExcelFileFormat;
        } // end method ScrollViewerSettings_OnScrollChanged

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        } // end method SettingsWindow_OnClosing

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            StackPanelSpace.Height = ScrollViewerSettings.ActualHeight - 40; // Increase the height of the space after the final part of settings to ensure that the navigator can work properly.
        } // end method SettingsWindow_OnLoaded

        private void TreeNavigatorSettings_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _hasMouseOnTreeNavigatorSettings = true;
        } // end method TreeNavigatorSettings_OnMouseEnter

        private void TreeNavigatorSettings_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _hasMouseOnTreeNavigatorSettings = false;
        } // end method TreeNavigatorSettings_OnMouseLeave

        private void TreeNavigatorSettings_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Scroll to the specified part, only if the user changes the selection.
            if (!_hasMouseOnTreeNavigatorSettings)
                return;

            var targetControl = StackPanelGeneral;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.ThemeSetting))
                targetControl = StackPanelTheme;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.DisplayFontFamilyNameSetting))
                targetControl = StackPanelDisplayFontFamilyName;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.DateDisplayFormatSetting))
                targetControl = StackPanelDateDisplayFormat;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.MinDateSetting))
                targetControl = StackPanelMinDate;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.TableSettings))
                targetControl = StackPanelTable;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.SortingSetting))
                targetControl = StackPanelSorting;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.FilteringSetting))
                targetControl = StackPanelFiltering;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.VolumeUnitSetting))
                targetControl = StackPanelVolumeUnit;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.VolumeDecimalDigitsSetting))
                targetControl = StackPanelVolumeDecimalDigits;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.ExportToExcel))
                targetControl = StackPanelExportToExcel;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.ExcelCellFontFamilyNameSetting))
                targetControl = StackPanelExcelCellFontFamilyName;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.ExcelCellFontSizeSetting))
                targetControl = StackPanelExcelCellFontSize;

            if (((SfTreeNavigatorItem) TreeNavigatorSettings.SelectedItem).Header.Equals(Properties.Resources.ExcelFileFormatSetting))
                targetControl = StackPanelExcelFileFormat;

            ScrollViewerSettings.ScrollToVerticalOffset(targetControl.TransformToVisual(ScrollViewerSettings).Transform(new Point(0, ScrollViewerSettings.VerticalOffset)).Y);
        } // end method TreeNavigatorSettings_OnSelectionChanged
        #endregion Control Events
    } // end class SettingsWindow
} // end namespace ShSzStockHelper.Views