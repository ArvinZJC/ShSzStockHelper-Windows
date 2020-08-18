/*
 * @Description: the back-end code of the tab of searching for data of strike prices and volumes
 * @Version: 1.2.2.20200817
 * @Author: Arvin Zhao
 * @Date: 2020-08-10 13:37:27
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-17 13:38:24
 */

using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for the tab of searching for data of strike prices and volumes.
    /// </summary>
    public partial class StrikePriceVolumeTab : UserControl
    {
        private readonly StrikePriceVolumeDataProcessor _strikePriceVolumeDataProcessor;
        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly StylePropertyViewModel _stylePropertyViewModel;
        private readonly ExcelExportingOptions _excelExportingOptions;
        private readonly TabItemExt _tabItemStrikePriceVolume;
        private bool _isStandardSymbol = false;
        private int _nodeTotalCount = 0; // The total count of rows in the data grid containing data of strike prices and volumes.
        private string _stockName;
        private decimal?[][] _strikePriceVolumeRowCollection;
        private const string regularExpression_HasChinese = @"[\u4e00-\u9fa5]";

        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceVolumeTab"/> class.
        /// </summary>
        /// <param name="tabItemStrikePriceVolume">The tab item used this user control as the content.</param>
        public StrikePriceVolumeTab(TabItemExt tabItemStrikePriceVolume, StockSymbolNameViewModel stockSymbolNameViewModel)
        {
            InitializeComponent();

            _tabItemStrikePriceVolume = tabItemStrikePriceVolume;
            _stockSymbolNameViewModel = stockSymbolNameViewModel;
            _strikePriceVolumeDataProcessor = new StrikePriceVolumeDataProcessor(); // Intialise the helper class for processing data of strike prices and volumes collected.
            _stylePropertyViewModel = new StylePropertyViewModel(); // Initialise the view model class for using the defined style properties.

            textBoxSymbol.AutoCompleteSource = _stockSymbolNameViewModel.StockSymbolNameRecords; // Set the auto-complete data source of the text box for the symbol.

            // Set the specified property values of the data pickers.
            datePickerStartDate.CultureInfo = new CultureInfo("zh-CN");
            datePickerStartDate.MaxDateTime = _stylePropertyViewModel.MaxDate;
            datePickerStartDate.MinDateTime = _stylePropertyViewModel.MinDate;
            datePickerEndDate.CultureInfo = new CultureInfo("zh-CN");
            datePickerEndDate.MaxDateTime = _stylePropertyViewModel.MaxDate;
            datePickerEndDate.MinDateTime = _stylePropertyViewModel.MinDate;

            // Set the specified property values of the data grid.
            dataGridStrikePriceVolumeTable.FilterItemsPopulating += DataGridStrikePriceVolumeTable_FilterItemsPopulating;
            dataGridStrikePriceVolumeTable.PrintSettings = new PrintSettings
            {
                AllowColumnWidthFitToPrintPage = false,
                AllowPrintByDrawing = false,
                AllowPrintStyles = false,
                AllowRepeatHeaders = true,
                CanPrintStackedHeaders = true,
                PrintPageFooterHeight = _stylePropertyViewModel.ContentTextFontSize,
                PrintPageFooterTemplate = Application.Current.Resources["PrintedPageFooterTemplate"] as DataTemplate
            };
            dataGridStrikePriceVolumeTable.QueryUnboundColumnValue += DataGridStrikePriceVolumeTable_QueryUnboundColumnValue;
            dataGridStrikePriceVolumeTable_columnStrikePrice.HeaderText = Properties.Resources.StrikePrice + "\n" + Properties.Resources.LeftBracket + Properties.Resources.PriceUnit + Properties.Resources.RightBracket;
            dataGridStrikePriceVolumeTable_columnStrikePrice.MaximumWidth = _stylePropertyViewModel.MaxCellWidth;
            dataGridStrikePriceVolumeTable_columnStrikePrice.MinimumWidth = _stylePropertyViewModel.MinCellWidth;
            dataGridStrikePriceVolumeTable_columnTotalVolume.HeaderText = Properties.Resources.TotalVolume + "\n" + Properties.Resources.LeftBracket + Properties.Resources.VolumeUnit_10000 + Properties.Resources.RightBracket;
            dataGridStrikePriceVolumeTable_columnTotalVolume.MaximumWidth = _stylePropertyViewModel.MaxCellWidth;
            dataGridStrikePriceVolumeTable_columnTotalVolume.MinimumWidth = _stylePropertyViewModel.MinCellWidth;
            
            _excelExportingOptions = new ExcelExportingOptions
            {
                ExportStackedHeaders = true,
                ExportingEventHandler = ExcelExportingHandler
            }; // Set options for exporting to Excel.
        } // end constructor StrikePriceVolumeTab

        #region Control Events
        private void ButtonClearSelection_Click(object sender, RoutedEventArgs e)
        {
            dataGridStrikePriceVolumeTable.SelectedItems.Clear();
            buttonClearSelection.IsEnabled = false;
        } // end method ButtonClearSelection_Click

        private void ButtonExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: uncomment the following line to replace the 3 lines below the line when Syncfusion is updated (https://www.syncfusion.com/forums/156187/strange-performance-of-data-grid-when-exporting-to-excel).
            // IWorkbook workbook = dataGridStrikePriceVolumeTable.ExportToExcel(dataGridStrikePriceVolumeTable.View, _excelExportingOptions).Excel.Workbooks[0];
            IApplication application = dataGridStrikePriceVolumeTable.ExportToExcel(dataGridStrikePriceVolumeTable.View, _excelExportingOptions).Excel;
            IWorkbook workbook = application.Workbooks[0];
            application.DataProviderType = ExcelDataProviderType.ByteArray;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = _stockName + " (" + _strikePriceVolumeDataProcessor.StartDate.ToString(_stylePropertyViewModel.DateFormat) + "~" + _strikePriceVolumeDataProcessor.EndDate.ToString(_stylePropertyViewModel.DateFormat) + ")",
                Filter = Properties.Resources.SaveFileDialogueExportToExcel_Filter,
                FilterIndex = 2
            };

            // Display a dialogue that allows the user to specify a filename to save an Excel file as.
            if (saveFileDialog.ShowDialog() == true)
            {
                using (Stream stream = saveFileDialog.OpenFile())
                {
                    workbook.Version = saveFileDialog.FilterIndex switch
                    {
                        1 => ExcelVersion.Excel97to2003,
                        _ => ExcelVersion.Excel2010,
                    }; // Specify the version of the exported Excel file.
                    workbook.Worksheets[0].AutoFilters.FilterRange = workbook.Worksheets[0].Range["A"
                        + (dataGridStrikePriceVolumeTable.StackedHeaderRows.Count + 1).ToString()
                        + ":"
                        + workbook.Worksheets[0].UsedRange.End.AddressLocal]; // Enable filters for the exported range in the worksheet.

                    // Set borders to cells.
                    workbook.Worksheets[0].UsedRange.BorderAround();
                    workbook.Worksheets[0].UsedRange.BorderInside();

                    workbook.SaveAs(stream);
                } // end using

                Process.Start("Explorer.exe", "/select," + saveFileDialog.FileName); // Start File Explorer and locate the created Excel file.
            } // end if
        } // end method ButtonExportToExcel_Click

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            dataGridStrikePriceVolumeTable.ShowPrintPreview();
        } // end method ButtonPrint_Click

        private void ButtonRestoreColumnWidth_Click(object sender, RoutedEventArgs e)
        {
            RestoreColumnWidth();
            buttonRestoreColumnWidth.IsEnabled = false;
        } // end method ButtonRestoreColumnWidth_Click

        private async void ButtonSearch_ClickAsync(object sender, RoutedEventArgs e)
        {
            string tabItemTitle = _stockName + " (" + _strikePriceVolumeDataProcessor.StartDate.ToString(_stylePropertyViewModel.DateFormat) + "~" + _strikePriceVolumeDataProcessor.EndDate.ToString(_stylePropertyViewModel.DateFormat) + ")";

            // Ensure that the specified controls are in the initial/suitable status.
            _tabItemStrikePriceVolume.Header = tabItemTitle;
            _tabItemStrikePriceVolume.ItemToolTip = tabItemTitle;
            buttonSearch.IsEnabled = false;
            busyIndicatorSearchResultArea.IsBusy = true;
            dataGridStrikePriceVolumeTable.Visibility = Visibility.Hidden;
            textBlockNullData.Visibility = Visibility.Hidden;
            textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_UnknownError;
            buttonClearSelection.IsEnabled = false;
            buttonRestoreColumnWidth.IsEnabled = false;
            buttonExportToExcel.IsEnabled = false;
            buttonPrint.IsEnabled = false;

            _strikePriceVolumeRowCollection = await _strikePriceVolumeDataProcessor.GetStrikePriceVolumeDataFromWebAsync();

            if (_strikePriceVolumeRowCollection != null)
            {
                if (_strikePriceVolumeRowCollection[0].Length >= 3)
                {
                    dataGridStrikePriceVolumeTable.Columns.Suspend(); // Suspend all the UI updates of the specified data grid to improve performance.
                    dataGridStrikePriceVolumeTable.SortColumnDescriptions.Clear(); // Clear sorting.

                    if (dataGridStrikePriceVolumeTable.StackedHeaderRows.Count > 0)
                    {
                        string[] childColumns = dataGridStrikePriceVolumeTable.StackedHeaderRows[0].StackedColumns[0].ChildColumns.Split(",");

                        // Clear unbound columns containing each day's volumes.
                        foreach (string mappingName in childColumns)
                        {
                            GridColumn column = dataGridStrikePriceVolumeTable.Columns[mappingName];

                            if (column == null)
                                continue;

                            dataGridStrikePriceVolumeTable.Columns.Remove(column);
                        } // end foreach

                        dataGridStrikePriceVolumeTable.StackedHeaderRows.Clear(); // Clear stacked headers after clearing unbound columns.
                    } // end if

                    RestoreColumnWidth();

                    dataGridStrikePriceVolumeTable.GridColumnSizer.Refresh();

                    StrikePriceTotalVolumeViewModel strikePriceTotalVolumeViewModel = new StrikePriceTotalVolumeViewModel();
                    _nodeTotalCount = _strikePriceVolumeRowCollection.Length;

                    for (int nodeCount = 0; nodeCount < _nodeTotalCount; nodeCount++)
                        strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords.Add(new StrikePriceTotalVolumeData(
                            (decimal)_strikePriceVolumeRowCollection[nodeCount][0],
                            (decimal)_strikePriceVolumeRowCollection[nodeCount][1] / 1000000m)); // 总成交量：1万手 = 100万股。

                    StringBuilder stackedHeaderRowDayVolumeChildColumns = new StringBuilder();

                    for (int dayVolumeColumnCount = 0; dayVolumeColumnCount < _strikePriceVolumeRowCollection[0].Length - 2; dayVolumeColumnCount++)
                    {
                        string dayVolumeColumnMappingName = dayVolumeColumnCount.ToString();
                        DateTime dayVolumeColumnDate = _strikePriceVolumeDataProcessor.StartDate.AddDays(dayVolumeColumnCount);
                        string dayVolumeColumnWeekday = dayVolumeColumnDate.DayOfWeek switch
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

                        if (dayVolumeColumnWeekday.Equals(Properties.Resources.Sat) || dayVolumeColumnWeekday.Equals(Properties.Resources.Sun))
                            dataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn()
                            {
                                HeaderText = dayVolumeColumnDate.ToString(_stylePropertyViewModel.DateFormat + "\n" + Properties.Resources.LeftBracket + dayVolumeColumnWeekday + Properties.Resources.RightBracket),
                                MappingName = dayVolumeColumnMappingName,
                                MaximumWidth = 0,
                                MinimumWidth = 0,
                                ShowHeaderToolTip = true,
                                TextAlignment = TextAlignment.Right,
                                Width = 0
                            });
                        else
                            dataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn()
                            {
                                HeaderText = dayVolumeColumnDate.ToString(_stylePropertyViewModel.DateFormat + "\n" + Properties.Resources.LeftBracket + dayVolumeColumnWeekday + Properties.Resources.RightBracket),
                                MappingName = dayVolumeColumnMappingName,
                                MaximumWidth = _stylePropertyViewModel.MaxCellWidth,
                                MinimumWidth = _stylePropertyViewModel.MinCellWidth,
                                ShowHeaderToolTip = true,
                                TextAlignment = TextAlignment.Right
                            });

                        stackedHeaderRowDayVolumeChildColumns.Append(dayVolumeColumnMappingName + ",");
                    } // end for

                    StackedHeaderRow stackedHeaderRowDayVolume = new StackedHeaderRow();

                    stackedHeaderRowDayVolume.StackedColumns.Add(new StackedColumn()
                    {
                        ChildColumns = stackedHeaderRowDayVolumeChildColumns.ToString(),
                        HeaderText = Properties.Resources.DayVolume + "（" + Properties.Resources.VolumeUnit_1 + "）",
                        MappingName = "DayVolume"
                    });
                    
                    dataGridStrikePriceVolumeTable.ItemsSource = strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords; // Bind data to the specified data grid. It can also clear data in the first and second columns (strike price and total volume).
                    dataGridStrikePriceVolumeTable.StackedHeaderRows.Add(stackedHeaderRowDayVolume);
                    dataGridStrikePriceVolumeTable.Columns.Resume();
                    dataGridStrikePriceVolumeTable.RefreshColumns();
                    dataGridStrikePriceVolumeTable.Visibility = Visibility.Visible;
                    buttonExportToExcel.IsEnabled = true;
                    buttonPrint.IsEnabled = true;
                }
                // Access is denied by the specified data source.
                else if (_strikePriceVolumeRowCollection[0].Length == 1)
                {
                    textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_AccessDenied;
                    textBlockNullData.Visibility = Visibility.Visible;
                }
                // The date range is too long.
                else
                {
                    textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_ImproperDateRange;
                    textBlockNullData.Visibility = Visibility.Visible;
                } // end if...else
            }
            // Wrong filters (symbol/start date/end date).
            else
            {
                textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_WrongFilters;
                textBlockNullData.Visibility = Visibility.Visible;
            } // end if...else

            buttonSearch.IsEnabled = true;
            busyIndicatorSearchResultArea.IsBusy = false;
        } // end method ButtonSearch_ClickAsync

        // Set advanced filter type for unbound columns.
        private void DataGridStrikePriceVolumeTable_FilterItemsPopulating(object sender, GridFilterItemsPopulatingEventArgs e)
        {
            if (e.Column.MappingName != "StrikePrice" || e.Column.MappingName != "TotalVolume")
            {
                e.FilterControl.AdvancedFilterType = AdvancedFilterType.NumberFilter;
                e.FilterControl.SetColumnDataType(typeof(decimal));
                e.FilterControl.AscendingSortString = GridResourceWrapper.SortNumberAscending;
                e.FilterControl.DescendingSortString = GridResourceWrapper.SortNumberDescending;
            } // end if
        } // end method DataGridStrikePriceVolumeTabl_FilterItemsPopulating

        // Populate data of each day's volume for unbound columns.
        private void DataGridStrikePriceVolumeTable_QueryUnboundColumnValue(object sender, GridUnboundColumnEventsArgs e)
        {
            if (e.UnBoundAction == UnBoundActions.QueryData
                && _strikePriceVolumeRowCollection != null
                && _strikePriceVolumeRowCollection[0].Length != 1)
            {
                decimal strikePrice = Convert.ToDecimal(e.Record.GetType().GetProperty("StrikePrice").GetValue(e.Record));

                // Round each volume value which is not null to the nearest integer.
                for (int nodeCount = 0; nodeCount < _nodeTotalCount; nodeCount++)
                    if (_strikePriceVolumeRowCollection[nodeCount][0] == strikePrice)
                    {
                        decimal? dayVolume = _strikePriceVolumeRowCollection[nodeCount][2 + Convert.ToInt32(e.Column.MappingName)];
                        e.Value = dayVolume == null ? null : (decimal?)decimal.Round((decimal)dayVolume / 100m); // 每日成交量：1手 = 100股。
                        break;
                    } // end if
            } // end if
        } // end method DataGridStrikePriceVolumeTable_QueryUnboundColumnValue

        private void DataGridStrikePriceVolumeTable_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            buttonClearSelection.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_SelectionChanged

        private void DataGridStrikePriceVolumeTable_OnResizingColumns(object sender, ResizingColumnsEventArgs e)
        {
            if (e.Reason == ColumnResizingReason.Resized)
                buttonRestoreColumnWidth.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_OnResizingColumns

        private void DatePickerEndDate_DateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DateTime endDateTime = e.NewValue.ToDateTime();
                _strikePriceVolumeDataProcessor.EndDate = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day);
            } // end if

            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerEndDate_DateTimeChanged

        private void DatePickerStartDate_DateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DateTime startDateTime = e.NewValue.ToDateTime();
                _strikePriceVolumeDataProcessor.StartDate = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day);
            } // end if

            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerStartDate_DateTimeChanged

        // A handler for styling the created workbooks.
        private void ExcelExportingHandler(object sender, GridExcelExportingEventArgs e)
        {
            if (e.CellType == ExportCellType.HeaderCell || e.CellType == ExportCellType.StackedHeaderCell)
            {
                e.CellStyle.BackGroundBrush = _stylePropertyViewModel.ExcelHeaderBackground;
                e.Style.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                e.Style.VerticalAlignment = ExcelVAlign.VAlignCenter;
            } // end if

            e.CellStyle.FontInfo.FontName = _stylePropertyViewModel.GlobalFontFamilyName;
            e.CellStyle.FontInfo.Size = _stylePropertyViewModel.ContentTextFontSize;
            e.Handled = true;
        } // end method ExcelExportingHandler

        private async void TextBoxSymbol_LostFocusAsync(object sender, RoutedEventArgs e)
        {
            string input = textBoxSymbol.Text.Trim();

            _stockName = input;

            if (Regex.IsMatch(input, regularExpression_HasChinese))
            {
                // When the text box loses focus, it can change the name of the stock you entered to the corresponding symbol if a matching one is found.
                if (textBoxSymbol.SelectedItem != null && input.Equals((textBoxSymbol.SelectedItem as StockSymbolNameData).Name))
                    textBoxSymbol.Text = (textBoxSymbol.SelectedItem as StockSymbolNameData).Symbol;
                else
                    try
                    {
                        StockSymbolNameData satisfiedRecord = _stockSymbolNameViewModel.StockSymbolNameRecords.First(record => input.Equals(record.Name));

                        textBoxSymbol.Text = satisfiedRecord.Symbol;
                    }
                    catch (InvalidOperationException)
                    {
                        // Keep the current input because no matching record is found.
                    } // end try...catch
            }
            else if (_isStandardSymbol)
            {
                if (textBoxSymbol.SelectedItem != null && _stockName.Equals((textBoxSymbol.SelectedItem as StockSymbolNameData).Symbol))
                    _stockName = (textBoxSymbol.SelectedItem as StockSymbolNameData).Name;
                else
                    try
                    {
                        StockSymbolNameData satisfiedRecord = _stockSymbolNameViewModel.StockSymbolNameRecords.First(record => _stockName.Equals(record.Symbol));

                        _stockName = satisfiedRecord.Name;
                    }
                    // No matching record is found from local data.
                    catch (InvalidOperationException)
                    {
                        _stockName = await _strikePriceVolumeDataProcessor.GetStockNameFromWebAsync() ?? input.ToUpper();
                    } // end try...catch
            } // end nested if...else
        } // end method TextBoxSymbol_LostFocusAsync

        // Perform actions when necessary if the selected item in the auto-complete suggestion list is changed.
        private void TextBoxSymbol_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (textBoxSymbol.SelectedItem != null && Regex.IsMatch(textBoxSymbol.Text.Trim(), regularExpression_HasChinese))
                textBoxSymbol.Text = (textBoxSymbol.SelectedItem as StockSymbolNameData).Symbol;
        } // end method TextBoxSymbol_SelectedItemChanged
        
        private void TextBoxSymbol_TextChanged(object sender, RoutedEventArgs e)
        {
            string input = textBoxSymbol.Text.Trim();

            if (Regex.IsMatch(input, regularExpression_HasChinese))
            {
                _isStandardSymbol = false;
                textBoxSymbol.Foreground = _stylePropertyViewModel.ColourDanger;
                textBoxSymbol.SearchItemPath = "Name";

                if (textBoxSymbol.SelectedItem != null && input.Equals((textBoxSymbol.SelectedItem as StockSymbolNameData).Name))
                    textBoxSymbol.Text = (textBoxSymbol.SelectedItem as StockSymbolNameData).Symbol;
            }
            else
            {
                _isStandardSymbol = Regex.IsMatch(input, @"^[Ss]([Hh]|[Zz])\d{6}$");
                textBoxSymbol.SearchItemPath = "Symbol";

                // Change the foreground colour of the specified text box to red if the format of the input for the symbol is not satisfied.
                if (_isStandardSymbol)
                {
                    textBoxSymbol.Foreground = Brushes.Black;
                    _strikePriceVolumeDataProcessor.Symbol = input;
                }
                else
                    textBoxSymbol.Foreground = _stylePropertyViewModel.ColourDanger;
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
            if (datePickerStartDate.DateTime != null
                && datePickerEndDate.DateTime != null
                && datePickerStartDate.DateTime.ToDateTime().CompareTo(datePickerEndDate.DateTime.ToDateTime()) > 0)
            {
                datePickerStartDate.Foreground = _stylePropertyViewModel.ColourDanger;
                datePickerEndDate.Foreground = _stylePropertyViewModel.ColourDanger;
            }
            else
            {
                datePickerStartDate.Foreground = Brushes.Black;
                datePickerEndDate.Foreground = Brushes.Black;
            } // end if...else
        } // end method ChangeDatePickerForeground

        /// <summary>
        /// Determine if the search button should be enabled.
        /// </summary>
        private void ChangeButtonSearchStatus()
        {
            if (_isStandardSymbol
                && datePickerStartDate.DateTime != null
                && datePickerEndDate.DateTime != null
                && datePickerStartDate.DateTime.ToDateTime().CompareTo(datePickerEndDate.DateTime.ToDateTime()) <= 0)
                buttonSearch.IsEnabled = true;
            else
                buttonSearch.IsEnabled = false;
        } // end method ChangeButtonSearchStatus

        /// <summary>
        /// Restore column width by resetting it to apply autosize calculation.
        /// </summary>
        private void RestoreColumnWidth()
        {
            foreach (GridColumn column in dataGridStrikePriceVolumeTable.Columns)
                if (!double.IsNaN(column.Width))
                    column.Width = double.NaN; // Reset column width to apply autosize calculation.

            dataGridStrikePriceVolumeTable.GridColumnSizer.Refresh();
        } // end method RestoreColumnWidth
        #endregion Private Methods
    } // end class StrikePriceVolumeTab
} // end namespace ShSzStockHelper