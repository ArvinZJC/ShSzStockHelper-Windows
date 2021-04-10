/*
 * @Description: the back-end code of the tab of searching for data of strike prices and volumes
 * @Version: 1.4.8.20210410
 * @Author: Arvin Zhao
 * @Date: 2020-08-10 13:37:27
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-10 04:07:15
 */

using Microsoft.Win32;
using ShSzStockHelper.Helpers;
using ShSzStockHelper.Models;
using ShSzStockHelper.ViewModels;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the tab of searching for data of strike prices and volumes.
    /// </summary>
    public partial class StrikePriceVolumeTab
    {
        /// <summary>
        /// A regular expression for validating if a string contains Chinese characters.
        /// </summary>
        private const string RegularExpressionHasChinese = @"[\u4e00-\u9fa5]";

        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly TabItemExt _tabItemStrikePriceVolume;

        /// <summary>
        /// A flag indicating if the format of the symbol input is standard. Initial value is false.
        /// </summary>
        private bool _isStandardSymbol;

        /// <summary>
        /// A collection storing data of strike prices and volumes by row (record).
        /// </summary>
        private decimal?[][] _strikePriceVolumeRowCollection;

        /// <summary>
        /// The total count of rows in the data grid containing data of strike prices and volumes.
        /// </summary>
        private int _nodeTotalCount;

        /// <summary>
        /// The title of the data grid containing data of strike prices and volumes.
        /// </summary>
        private string _dataTableTitle;

        private ExcelExportingOptions _excelExportingOptions;
        private StrikePriceVolumeDataProcessor _strikePriceVolumeDataProcessor;
        private StrikePriceTotalVolumeViewModel _strikePriceTotalVolumeViewModel;
        private VolumeUnitViewModel _volumeUnitViewModel;

        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceVolumeTab"/> class.
        /// </summary>
        /// <param name="tabItemStrikePriceVolume">The tab item using this user control as the content.</param>
        /// <param name="stockSymbolNameViewModel">The view model containing the data of stocks' symbols and corresponding names.</param>
        public StrikePriceVolumeTab(TabItemExt tabItemStrikePriceVolume, StockSymbolNameViewModel stockSymbolNameViewModel)
        {
            _tabItemStrikePriceVolume = tabItemStrikePriceVolume;
            _stockSymbolNameViewModel = stockSymbolNameViewModel;

            InitializeComponent();
            InitialiseTab();
        } // end constructor StrikePriceVolumeTab

        #region Control Events
        // Clear row selection in the data grid.
        private void ButtonClearSelection_Click(object sender, RoutedEventArgs e)
        {
            DataGridStrikePriceVolumeTable.SelectedItems.Clear();
            ButtonClearSelection.IsEnabled = false;
        } // end method ButtonClearSelection_Click

        // Export to Excel.
        private void ButtonExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var application = DataGridStrikePriceVolumeTable.ExportToExcel(DataGridStrikePriceVolumeTable.View, _excelExportingOptions).Excel;
            var workbook = application.Workbooks[0];
            application.DataProviderType = ExcelDataProviderType.ByteArray;

            var saveFileDialog = new SaveFileDialog
            {
                FileName = _dataTableTitle,
                Filter = Properties.Resources.SaveFileDialogueExportToExcel_Filter,
                FilterIndex = Properties.Settings.Default.ExcelFileFormat
            };

            // Display a dialogue that allows the user to specify a filename to save as an Excel file.
            if (saveFileDialog.ShowDialog() != true)
                return;

            using (var stream = saveFileDialog.OpenFile())
            {
                workbook.Version = saveFileDialog.FilterIndex == 1
                    ? ExcelVersion.Excel97to2003
                    : ExcelVersion.Excel2010; // Specify the version of the exported Excel file.
                workbook.Worksheets[0].AutoFilters.FilterRange = workbook.Worksheets[0].Range["A"
                    + (DataGridStrikePriceVolumeTable.StackedHeaderRows.Count + 1)
                    + ":"
                    + workbook.Worksheets[0].UsedRange.End.AddressLocal]; // Enable filters for the exported range in the worksheet.

                // Set borders to cells.
                workbook.Worksheets[0].UsedRange.BorderAround();
                workbook.Worksheets[0].UsedRange.BorderInside();

                workbook.SaveAs(stream);
            } // end using

            Process.Start("Explorer.exe", "/select," + saveFileDialog.FileName); // Start File Explorer and locate the created Excel file.
        } // end method ButtonExportToExcel_Click

        // Show the print preview window.
        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            DataGridStrikePriceVolumeTable.ShowPrintPreview();
        } // end method ButtonPrint_Click

        // Restore the data grid's column width.
        private void ButtonRestoreColumnWidth_Click(object sender, RoutedEventArgs e)
        {
            RestoreColumnWidth();
            ButtonRestoreColumnWidth.IsEnabled = false;
        } // end method ButtonRestoreColumnWidth_Click

        // Search strike prices and volumes.
        private async void ButtonSearch_ClickAsync(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim().ToUpper();
            string stockName;

            // Display the stock name or symbol (if no corresponding name is found) in the tab title.
            if (TextBoxSymbol.SelectedItem != null && input.Equals(((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol))
                stockName = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Name;
            else
                try
                {
                    var satisfiedRecord = _stockSymbolNameViewModel.StockSymbolNameRecords.First(record => input.Equals(record.Symbol));
                    stockName = satisfiedRecord.Name;
                }
                // No matching record is found from local data.
                catch (InvalidOperationException)
                {
                    stockName = await _strikePriceVolumeDataProcessor.GetStockNameFromWebAsync() ?? input;
                } // end try...catch

            _dataTableTitle = stockName + Properties.Resources.LeftBracket + _strikePriceVolumeDataProcessor.StartDate.ToString(Properties.Settings.Default.DateDisplayFormat) + "~" + _strikePriceVolumeDataProcessor.EndDate.ToString(Properties.Settings.Default.DateDisplayFormat) + Properties.Resources.RightBracket;

            // Ensure that the specified controls are in the initial/suitable status.
            _tabItemStrikePriceVolume.Header = _dataTableTitle;
            _tabItemStrikePriceVolume.ItemToolTip = _dataTableTitle;
            TextBoxSymbol.BorderBrush = Application.Current.Resources["BorderAlt"] as SolidColorBrush;
            TextBoxSymbol.IsEnabled = false;
            DatePickerStartDate.IsEnabled = false;
            DatePickerEndDate.IsEnabled = false;
            ButtonSearch.IsEnabled = false;
            BusyIndicatorSearchResultArea.IsBusy = true;
            DataGridStrikePriceVolumeTable.Visibility = Visibility.Hidden;
            TextBlockNullData.Visibility = Visibility.Hidden;
            TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_UnknownError;
            ButtonClearSelection.IsEnabled = false;
            ButtonRestoreColumnWidth.IsEnabled = false;
            ButtonExportToExcel.IsEnabled = false;
            ButtonPrint.IsEnabled = false;

            _strikePriceVolumeRowCollection = await _strikePriceVolumeDataProcessor.GetStrikePriceVolumeDataFromWebAsync();
            ShowSearchResults();

            // Modify the specified controls' properties after showing search results.
            TextBoxSymbol.BorderBrush = Application.Current.Resources["Border"] as SolidColorBrush;
            TextBoxSymbol.IsEnabled = true;
            DatePickerStartDate.IsEnabled = true;
            DatePickerEndDate.IsEnabled = true;
            ButtonSearch.IsEnabled = true;
            BusyIndicatorSearchResultArea.IsBusy = false;
        } // end method ButtonSearch_ClickAsync

        // Set advanced filter type for unbound columns.
        private static void DataGridStrikePriceVolumeTable_FilterItemsPopulating(object sender, GridFilterItemsPopulatingEventArgs e)
        {
            if (e.Column.MappingName == "StrikePrice" && e.Column.MappingName == "TotalVolume")
                return;

            e.FilterControl.AdvancedFilterType = AdvancedFilterType.NumberFilter;
            e.FilterControl.SetColumnDataType(typeof(decimal?));
            e.FilterControl.AscendingSortString = GridResourceWrapper.SortNumberAscending;
            e.FilterControl.DescendingSortString = GridResourceWrapper.SortNumberDescending;
        } // end method DataGridStrikePriceVolumeTable_FilterItemsPopulating

        // Populate data of each day's volume for unbound columns.
        private void DataGridStrikePriceVolumeTable_QueryUnboundColumnValue(object sender, GridUnboundColumnEventsArgs e)
        {
            if (e.UnBoundAction != UnBoundActions.QueryData
                || _strikePriceVolumeRowCollection == null
                || _strikePriceVolumeRowCollection[0].Length == 1)
                return;

            var strikePrice = Convert.ToDecimal(e.Record.GetType().GetProperty("StrikePrice")?.GetValue(e.Record));

            // Round each volume value which is not null.
            for (var nodeCount = 0; nodeCount < _nodeTotalCount; nodeCount++)
                if (_strikePriceVolumeRowCollection[nodeCount][0] == strikePrice)
                {
                    var dayVolume = _strikePriceVolumeRowCollection[nodeCount][2 + Convert.ToInt32(e.Column.MappingName)];

                    /*
                     * 每日成交量（1手 = 100股）。
                     * First, round the day volume. Second, convert it to a string representation in a specified format. Then, convert it back to a decimal value. These steps are necessary to keep the specified number of decimal digits without dropping 0.
                     */
                    e.Value = dayVolume == null
                        ? null
                        : (decimal?) Convert.ToDecimal(decimal.Round((decimal) dayVolume / (Properties.Settings.Default.DayVolumeUnit * 100m), Properties.Settings.Default.DayVolumeDecimalDigits).ToString("N" + Properties.Settings.Default.DayVolumeDecimalDigits));

                    break;
                } // end if
        } // end method DataGridStrikePriceVolumeTable_QueryUnboundColumnValue

        // Enable the button for clearing selection if there is any selection in the data grid.
        private void DataGridStrikePriceVolumeTable_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            ButtonClearSelection.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_SelectionChanged

        // Enable the button for restoring column width if any column is resized by the user.
        private void DataGridStrikePriceVolumeTable_OnResizingColumns(object sender, ResizingColumnsEventArgs e)
        {
            if (e.Reason == ColumnResizingReason.Resized)
                ButtonRestoreColumnWidth.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_OnResizingColumns

        // Set the end date.
        private void DatePickerEndDate_DateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var endDateTime = e.NewValue.ToDateTime();
                _strikePriceVolumeDataProcessor.EndDate = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day);
            } // end if

            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerEndDate_DateTimeChanged

        // Set the start date.
        private void DatePickerStartDate_DateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var startDateTime = e.NewValue.ToDateTime();
                _strikePriceVolumeDataProcessor.StartDate = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day);
            } // end if

            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerStartDate_DateTimeChanged

        // A handler for styling the created workbooks.
        private static void ExcelExportingHandler(object sender, GridExcelExportingEventArgs e)
        {
            if (e.CellType == ExportCellType.HeaderCell || e.CellType == ExportCellType.StackedHeaderCell)
            {
                e.CellStyle.BackGroundBrush = Properties.Settings.Default.ExcelHeaderBackground;
                e.Style.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                e.Style.VerticalAlignment = ExcelVAlign.VAlignCenter;
            } // end if

            e.CellStyle.FontInfo.FontName = Properties.Settings.Default.ExcelCellFontFamilyName;
            e.CellStyle.FontInfo.Size = Properties.Settings.Default.ExcelCellFontSize;
            e.Handled = true;
        } // end method ExcelExportingHandler

        // Change a stock name to a corresponding symbol if the specified text box loses focus.
        private void TextBoxSymbol_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim();

            if (!Regex.IsMatch(input, RegularExpressionHasChinese))
                return;

            // When the text box loses focus, it can change a stock name to a corresponding symbol if a matching one is found.
            if (TextBoxSymbol.SelectedItem != null && input.Equals(((StockSymbolNameData) TextBoxSymbol.SelectedItem).Name))
                TextBoxSymbol.Text = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol;
            else
                try
                {
                    var satisfiedRecord = _stockSymbolNameViewModel.StockSymbolNameRecords.First(record => input.Equals(record.Name));
                    TextBoxSymbol.Text = satisfiedRecord.Symbol;
                }
                catch (InvalidOperationException)
                {
                    // Keep the current input because no matching record is found.
                } // end try...catch
        } // end method TextBoxSymbol_LostFocus

        // Change a stock name to a corresponding symbol if the selected item in the auto-complete suggestion list is changed.
        private void TextBoxSymbol_SelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (TextBoxSymbol.SelectedItem != null && Regex.IsMatch(TextBoxSymbol.Text.Trim(), RegularExpressionHasChinese))
                TextBoxSymbol.Text = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol;
        } // end method TextBoxSymbol_SelectedItemChanged
        
        // Change component properties if the text in the specified text box is changed.
        private void TextBoxSymbol_TextChanged(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim();

            if (Regex.IsMatch(input, RegularExpressionHasChinese))
            {
                _isStandardSymbol = false;
                TextBoxSymbol.Foreground = Application.Current.Resources["ErrorForeground"] as SolidColorBrush;
                TextBoxSymbol.SearchItemPath = "Name";

                if (TextBoxSymbol.SelectedItem != null && input.Equals(((StockSymbolNameData) TextBoxSymbol.SelectedItem).Name))
                    TextBoxSymbol.Text = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol; // Change a stock name to a corresponding symbol.
            }
            else
            {
                _isStandardSymbol = Regex.IsMatch(input, @"^[Ss]([Hh]|[Zz])\d{6}$");
                TextBoxSymbol.SearchItemPath = "Symbol";

                // Change the foreground colour of the specified text box to red if the format of the input for the symbol is not satisfied.
                if (_isStandardSymbol)
                {
                    TextBoxSymbol.Foreground = Application.Current.Resources["ContentForeground"] as SolidColorBrush;
                    _strikePriceVolumeDataProcessor.Symbol = input;
                }
                else
                    TextBoxSymbol.Foreground = Application.Current.Resources["ErrorForeground"] as SolidColorBrush;
            } // end if...else

            ChangeButtonSearchStatus();
        } // end method TextBoxSymbol_TextChanged
        #endregion Control Events

        #region Private Methods
        /// <summary>
        /// Change the foreground colour of data pickers to red if the start date is later than the end date.
        /// </summary>
        private void ChangeDatePickerForeground()
        {
            if (DatePickerStartDate.DateTime != null
                && DatePickerEndDate.DateTime != null
                && DatePickerStartDate.DateTime.ToDateTime().CompareTo(DatePickerEndDate.DateTime.ToDateTime()) > 0)
            {
                DatePickerStartDate.Foreground = Application.Current.Resources["ErrorForeground"] as SolidColorBrush;
                DatePickerEndDate.Foreground = Application.Current.Resources["ErrorForeground"] as SolidColorBrush;
                return;
            } // end if
            
            DatePickerStartDate.Foreground = Application.Current.Resources["ContentForeground"] as SolidColorBrush;
            DatePickerEndDate.Foreground = Application.Current.Resources["ContentForeground"] as SolidColorBrush;
        } // end method ChangeDatePickerForeground

        /// <summary>
        /// Determine if the search button should be enabled.
        /// </summary>
        private void ChangeButtonSearchStatus()
        {
            if (_isStandardSymbol
                && DatePickerStartDate.DateTime != null
                && DatePickerEndDate.DateTime != null
                && DatePickerStartDate.DateTime.ToDateTime().CompareTo(DatePickerEndDate.DateTime.ToDateTime()) <= 0)
            {
                ButtonSearch.IsEnabled = true;
                return;
            } // end if
            
            ButtonSearch.IsEnabled = false;
        } // end method ChangeButtonSearchStatus

        /// <summary>
        /// Initialise the data grid to show search results.
        /// </summary>
        private void InitialiseDataGrid()
        {
            DataGridStrikePriceVolumeTable.SortColumnDescriptions.Clear(); // Clear sorting.
            ColumnTotalVolume.HeaderText = Properties.Resources.TotalVolume
                                           + "\n"
                                           + Properties.Resources.LeftBracket
                                           + _volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.TotalVolumeUnit).Name
                                           + Properties.Resources.RightBracket; // Update the total volume column header.

            if (DataGridStrikePriceVolumeTable.StackedHeaderRows.Count > 0)
            {
                /*
                 * Clear unbound columns containing each day's volumes.
                 * A string split option is specified to drop the last entry which should be the only empty entry because each stack column name is followed by a comma (refer to the place preparing day volume data to show).
                 */
                foreach (var mappingName in DataGridStrikePriceVolumeTable.StackedHeaderRows[0].StackedColumns[0].ChildColumns.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    DataGridStrikePriceVolumeTable.Columns.Remove(DataGridStrikePriceVolumeTable.Columns[mappingName]);

                DataGridStrikePriceVolumeTable.StackedHeaderRows.Clear(); // Clear stacked headers after clearing unbound columns.
            } // end if

            RestoreColumnWidth();
            _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords.Clear();
        } // end method InitialiseDataGrid

        /// <summary>
        /// Initialise component properties.
        /// </summary>
        private void InitialiseTab()
        {
            _strikePriceVolumeDataProcessor = new StrikePriceVolumeDataProcessor();
            _strikePriceTotalVolumeViewModel = new StrikePriceTotalVolumeViewModel();
            _volumeUnitViewModel = new VolumeUnitViewModel();

            TextBoxSymbol.AutoCompleteSource = _stockSymbolNameViewModel.StockSymbolNameRecords; // Set the auto-complete data source of the text box for the symbol.

            // Set the specified property values of the data pickers.
            DatePickerStartDate.CultureInfo = new CultureInfo(Properties.Settings.Default.CultureInfo);
            DatePickerEndDate.CultureInfo = new CultureInfo(Properties.Settings.Default.CultureInfo);

            // Set the specified property values of the data grid.
            DataGridStrikePriceVolumeTable.FilterItemsPopulating += DataGridStrikePriceVolumeTable_FilterItemsPopulating;
            DataGridStrikePriceVolumeTable.PrintSettings = new PrintSettings
            {
                AllowColumnWidthFitToPrintPage = false,
                AllowPrintByDrawing = false,
                AllowPrintStyles = false,
                AllowRepeatHeaders = true,
                CanPrintStackedHeaders = true,
                PrintPageFooterHeight = Properties.Settings.Default.ContentTextFontSize,
                PrintPageFooterTemplate = Application.Current.Resources["PrintedPageFooterTemplate"] as DataTemplate,
                PrintPreviewWindowStyle = Application.Current.Resources["PrintPreviewWindowStyle"] as Style
            };
            DataGridStrikePriceVolumeTable.QueryUnboundColumnValue += DataGridStrikePriceVolumeTable_QueryUnboundColumnValue;
            ColumnStrikePrice.HeaderText += "\n" + Properties.Resources.LeftBracket + Properties.Resources.PriceUnit + Properties.Resources.RightBracket;
            ColumnStrikePrice.MaximumWidth = Properties.Settings.Default.MaxCellWidth;
            ColumnStrikePrice.MinimumWidth = Properties.Settings.Default.MinCellWidth;
            ColumnTotalVolume.MaximumWidth = Properties.Settings.Default.MaxCellWidth;
            ColumnTotalVolume.MinimumWidth = Properties.Settings.Default.MinCellWidth;

            _excelExportingOptions = new ExcelExportingOptions
            {
                ExportStackedHeaders = true,
                ExportingEventHandler = ExcelExportingHandler
            }; // Set options for exporting to Excel.
        } // end method InitialiseTab

        /// <summary>
        /// Prepare day volume data to show.
        /// </summary>
        /// <returns>A <see cref="StackedHeaderRow"/> object representing a stacked header row containing a collection of stacked columns.</returns>
        private StackedHeaderRow PrepareDayVolume()
        {
            var stackedHeaderRowDayVolumeChildColumns = new StringBuilder(); // Use StringBuilder here to improve string concatenating performance because of the potentially large number of strings.

            for (var dayVolumeColumnCount = 0; dayVolumeColumnCount < _strikePriceVolumeRowCollection[0].Length - 2; dayVolumeColumnCount++)
            {
                var dayVolumeColumnDate = _strikePriceVolumeDataProcessor.StartDate.AddDays(dayVolumeColumnCount);
                var dayVolumeColumnMappingName = dayVolumeColumnCount.ToString();
                var dayVolumeColumnWeekday = dayVolumeColumnDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => Properties.Resources.Mon,
                    DayOfWeek.Tuesday => Properties.Resources.Tue,
                    DayOfWeek.Wednesday => Properties.Resources.Wed,
                    DayOfWeek.Thursday => Properties.Resources.Thu,
                    DayOfWeek.Friday => Properties.Resources.Fri,
                    DayOfWeek.Saturday => Properties.Resources.Sat,
                    DayOfWeek.Sunday => Properties.Resources.Sun,
                    _ => Properties.Resources.QuestionMark,
                };
                stackedHeaderRowDayVolumeChildColumns.Append(dayVolumeColumnMappingName + ",");

                // Hide weekends' unbound columns.
                if (dayVolumeColumnWeekday.Equals(Properties.Resources.Sat) || dayVolumeColumnWeekday.Equals(Properties.Resources.Sun))
                {
                    DataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn
                    {
                        MappingName = dayVolumeColumnMappingName,
                        MaximumWidth = 0,
                        MinimumWidth = 0,
                        Width = 0
                    });
                    continue;
                } // end if

                DataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn
                {
                    AllowFiltering = Properties.Settings.Default.DayVolumeFiltering,
                    AllowSorting = Properties.Settings.Default.DayVolumeSorting,
                    HeaderText = dayVolumeColumnDate.ToString(Properties.Settings.Default.DateDisplayFormat
                                                              + "\n"
                                                              + Properties.Resources.LeftBracket + dayVolumeColumnWeekday
                                                              + Properties.Resources.RightBracket),
                    MappingName = dayVolumeColumnMappingName,
                    MaximumWidth = Properties.Settings.Default.MaxCellWidth,
                    MinimumWidth = Properties.Settings.Default.MinCellWidth,
                    ShowHeaderToolTip = true,
                    TextAlignment = TextAlignment.Right
                });
            } // end for

            var stackedHeaderRowDayVolume = new StackedHeaderRow();

            stackedHeaderRowDayVolume.StackedColumns.Add(new StackedColumn
            {
                ChildColumns = stackedHeaderRowDayVolumeChildColumns.ToString(),
                HeaderText = Properties.Resources.DayVolume
                             + Properties.Resources.LeftBracket
                             + _volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.DayVolumeUnit).Name
                             + Properties.Resources.RightBracket,
                MappingName = "DayVolume"
            });
            return stackedHeaderRowDayVolume;
        } // end method PrepareDayVolume

        /// <summary>
        /// Prepare strike price and total volume data to show.
        /// </summary>
        private void PrepareStrikePriceTotalVolume()
        {
            _nodeTotalCount = _strikePriceVolumeRowCollection.Length;

            for (var nodeCount = 0; nodeCount < _nodeTotalCount; nodeCount++)
                if (_strikePriceVolumeRowCollection[nodeCount][0] != null && _strikePriceVolumeRowCollection[nodeCount][1] != null)
                    _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords.Add(new StrikePriceTotalVolumeData
                    {
                        StrikePrice = (decimal) _strikePriceVolumeRowCollection[nodeCount][0],
                        /*
                         * 总成交量（1手 = 100股）。
                         * First, round the day volume. Second, convert it to a string representation in a specified format. Then, convert it back to a decimal value. These steps are necessary to keep the specified number of decimal digits without dropping 0.
                         */
                        TotalVolume = Convert.ToDecimal(decimal.Round((decimal) _strikePriceVolumeRowCollection[nodeCount][1] / (Properties.Settings.Default.TotalVolumeUnit * 100m), Properties.Settings.Default.TotalVolumeDecimalDigits)
                            .ToString("N" + Properties.Settings.Default.TotalVolumeDecimalDigits))
                    });
        } // end method PrepareStrikePriceTotalVolume

        /// <summary>
        /// Restore column width by resetting it to apply autosize calculation.
        /// </summary>
        private void RestoreColumnWidth()
        {
            foreach (var column in DataGridStrikePriceVolumeTable.Columns.Where(column => !double.IsNaN(column.Width)))
                column.Width = double.NaN;

            DataGridStrikePriceVolumeTable.GridColumnSizer.Refresh();
        } // end method RestoreColumnWidth

        /// <summary>
        /// Organise and show search results.
        /// </summary>
        private void ShowSearchResults()
        {
            // It seems to be no internet connection.
            if (_strikePriceVolumeRowCollection == null)
            {
                TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_NetworkError;
                TextBlockNullData.Visibility = Visibility.Visible;
                return;
            } // end if

            // Wrong filters (symbol/start date/end date).
            if (_strikePriceVolumeRowCollection[0] == null)
            {
                TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_WrongFilters;
                TextBlockNullData.Visibility = Visibility.Visible;
                return;
            } // end if

            switch (_strikePriceVolumeRowCollection[0].Length)
            {
                // Access is denied by the specified data source.
                case 1:
                    TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_AccessDenied;
                    TextBlockNullData.Visibility = Visibility.Visible;
                    return;

                // The date range is too long.
                case 2:
                    TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_ImproperDateRange;
                    TextBlockNullData.Visibility = Visibility.Visible;
                    return;
            } // end switch-case

            DataGridStrikePriceVolumeTable.Columns.Suspend(); // For improving performance.
            InitialiseDataGrid();
            UpdateDataGrid();
            DataGridStrikePriceVolumeTable.Columns.Resume();
            DataGridStrikePriceVolumeTable.RefreshColumns();
            DataGridStrikePriceVolumeTable.Visibility = Visibility.Visible;
            ButtonExportToExcel.IsEnabled = true;
            ButtonPrint.IsEnabled = true;
        } // end method ShowSearchResults

        /// <summary>
        /// Update the data grid to show search results.
        /// </summary>
        private void UpdateDataGrid()
        {
            PrepareStrikePriceTotalVolume();
            var stackedHeaderRowDayVolume = PrepareDayVolume();
            DataGridStrikePriceVolumeTable.ItemsSource = _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords; // Bind data to the specified data grid. It can also clear data in the 1st and 2nd columns (strike price and total volume).
            DataGridStrikePriceVolumeTable.StackedHeaderRows.Add(stackedHeaderRowDayVolume);
        } // end method UpdateDataGrid
        #endregion Private Methods
    } // end class StrikePriceVolumeTab
} // end namespace ShSzStockHelper.Views