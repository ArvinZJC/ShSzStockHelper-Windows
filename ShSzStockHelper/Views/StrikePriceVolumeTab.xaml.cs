/*
 * @Description: the back-end code of the tab of searching for data of strike prices and volumes
 * @Version: 1.3.6.20200914
 * @Author: Arvin Zhao
 * @Date: 2020-08-10 13:37:27
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-14 13:38:24
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
        private readonly StrikePriceVolumeDataProcessor _strikePriceVolumeDataProcessor;
        private readonly StrikePriceTotalVolumeViewModel _strikePriceTotalVolumeViewModel;
        private readonly StockSymbolNameViewModel _stockSymbolNameViewModel;
        private readonly VolumeUnitViewModel _volumeUnitViewModel;
        private readonly ExcelExportingOptions _excelExportingOptions;
        private readonly TabItemExt _tabItemStrikePriceVolume;
        private bool _isStandardSymbol; // Use the default false value to indicate if the format of the input for the symbol is standard.
        private int _nodeTotalCount; // The total count of rows in the data grid containing data of strike prices and volumes (use the default initial value 0).
        private string _stockName, _dataTableTitle;
        private decimal?[][] _strikePriceVolumeRowCollection;
        private const string RegularExpressionHasChinese = @"[\u4e00-\u9fa5]";

        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceVolumeTab"/> class.
        /// </summary>
        /// <param name="tabItemStrikePriceVolume">The tab item using this user control as the content.</param>
        /// <param name="stockSymbolNameViewModel">The view model containing the data of stocks' symbols and corresponding names.</param>
        public StrikePriceVolumeTab(TabItemExt tabItemStrikePriceVolume, StockSymbolNameViewModel stockSymbolNameViewModel)
        {
            InitializeComponent();

            _tabItemStrikePriceVolume = tabItemStrikePriceVolume;
            _stockSymbolNameViewModel = stockSymbolNameViewModel;
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
                PrintPageFooterTemplate = Application.Current.Resources["PrintedPageFooterTemplate"] as DataTemplate
            };
            DataGridStrikePriceVolumeTable.QueryUnboundColumnValue += DataGridStrikePriceVolumeTable_QueryUnboundColumnValue;
            ColumnStrikePrice.HeaderText += "\n" + Properties.Resources.LeftBracket + Properties.Resources.PriceUnit + Properties.Resources.RightBracket;
            ColumnStrikePrice.MaximumWidth = Properties.Settings.Default.MaxCellWidth;
            ColumnStrikePrice.MinimumWidth = Properties.Settings.Default.MinCellWidth;
            ColumnStrikePrice.MaximumWidth = Properties.Settings.Default.MaxCellWidth;
            ColumnStrikePrice.MinimumWidth = Properties.Settings.Default.MinCellWidth;

            _excelExportingOptions = new ExcelExportingOptions
            {
                ExportStackedHeaders = true,
                ExportingEventHandler = ExcelExportingHandler
            }; // Set options for exporting to Excel.
        } // end constructor StrikePriceVolumeTab

        #region Control Events
        private void ButtonClearSelection_Click(object sender, RoutedEventArgs e)
        {
            DataGridStrikePriceVolumeTable.SelectedItems.Clear();
            ButtonClearSelection.IsEnabled = false;
        } // end method ButtonClearSelection_Click

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
                workbook.Version = saveFileDialog.FilterIndex switch
                {
                    1 => ExcelVersion.Excel97to2003,
                    _ => ExcelVersion.Excel2010,
                }; // Specify the version of the exported Excel file.
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

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            DataGridStrikePriceVolumeTable.ShowPrintPreview();
        } // end method ButtonPrint_Click

        private void ButtonRestoreColumnWidth_Click(object sender, RoutedEventArgs e)
        {
            RestoreColumnWidth();
            ButtonRestoreColumnWidth.IsEnabled = false;
        } // end method ButtonRestoreColumnWidth_Click

        private async void ButtonSearch_ClickAsync(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim().ToUpper();

            if (TextBoxSymbol.SelectedItem != null && input.Equals(((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol))
                _stockName = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Name;
            else
                try
                {
                    var satisfiedRecord = _stockSymbolNameViewModel.StockSymbolNameRecords.First(record => input.Equals(record.Symbol));

                    _stockName = satisfiedRecord.Name;
                }
                // No matching record is found from local data.
                catch (InvalidOperationException)
                {
                    _stockName = await _strikePriceVolumeDataProcessor.GetStockNameFromWebAsync() ?? input;
                } // end try...catch

            _dataTableTitle = _stockName + Properties.Resources.LeftBracket + _strikePriceVolumeDataProcessor.StartDate.ToString(Properties.Settings.Default.DateDisplayFormat) + "~" + _strikePriceVolumeDataProcessor.EndDate.ToString(Properties.Settings.Default.DateDisplayFormat) + Properties.Resources.RightBracket;

            // Ensure that the specified controls are in the initial/suitable status.
            _tabItemStrikePriceVolume.Header = _dataTableTitle;
            _tabItemStrikePriceVolume.ItemToolTip = _dataTableTitle;
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

            if (_strikePriceVolumeRowCollection != null)
            {
                if (_strikePriceVolumeRowCollection[0] != null)
                {
                    if (_strikePriceVolumeRowCollection[0].Length >= 3)
                    {
                        DataGridStrikePriceVolumeTable.Columns.Suspend(); // Suspend all the UI updates of the specified data grid to improve performance.
                        DataGridStrikePriceVolumeTable.SortColumnDescriptions.Clear(); // Clear sorting.

                        ColumnTotalVolume.HeaderText = Properties.Resources.TotalVolume
                                                       + "\n"
                                                       + Properties.Resources.LeftBracket
                                                       + _volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.TotalVolumeUnit).Name
                                                       + Properties.Resources.RightBracket;

                        if (DataGridStrikePriceVolumeTable.StackedHeaderRows.Count > 0)
                        {
                            var childColumns = DataGridStrikePriceVolumeTable.StackedHeaderRows[0].StackedColumns[0].ChildColumns.Split(",");

                            // Clear unbound columns containing each day's volumes.
                            foreach (var mappingName in childColumns)
                            {
                                var column = DataGridStrikePriceVolumeTable.Columns[mappingName];

                                if (column == null)
                                    continue;

                                DataGridStrikePriceVolumeTable.Columns.Remove(column);
                            } // end foreach

                            DataGridStrikePriceVolumeTable.StackedHeaderRows.Clear(); // Clear stacked headers after clearing unbound columns.
                        } // end if

                        RestoreColumnWidth();
                        DataGridStrikePriceVolumeTable.GridColumnSizer.Refresh();
                        _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords.Clear();

                        _nodeTotalCount = _strikePriceVolumeRowCollection.Length;

                        for (var nodeCount = 0; nodeCount < _nodeTotalCount; nodeCount++)
                            if (_strikePriceVolumeRowCollection[nodeCount][0] != null && _strikePriceVolumeRowCollection[nodeCount][1] != null)
                                _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords.Add(new StrikePriceTotalVolumeData()
                                {
                                    StrikePrice = (decimal) _strikePriceVolumeRowCollection[nodeCount][0],
                                    TotalVolume = (decimal) _strikePriceVolumeRowCollection[nodeCount][1] / (Properties.Settings.Default.TotalVolumeUnit * 100m) // 总成交量（1手 = 100股）。
                                });

                        var stackedHeaderRowDayVolumeChildColumns = new StringBuilder();

                        for (var dayVolumeColumnCount = 0; dayVolumeColumnCount < _strikePriceVolumeRowCollection[0].Length - 2; dayVolumeColumnCount++)
                        {
                            var dayVolumeColumnMappingName = dayVolumeColumnCount.ToString();
                            var dayVolumeColumnDate = _strikePriceVolumeDataProcessor.StartDate.AddDays(dayVolumeColumnCount);
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

                            if (dayVolumeColumnWeekday.Equals(Properties.Resources.Sat) || dayVolumeColumnWeekday.Equals(Properties.Resources.Sun))
                                DataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn()
                                {
                                    MappingName = dayVolumeColumnMappingName,
                                    MaximumWidth = 0,
                                    MinimumWidth = 0,
                                    Width = 0
                                });
                            else
                                DataGridStrikePriceVolumeTable.Columns.Add(new GridUnBoundColumn()
                                {
                                    AllowFiltering = Properties.Settings.Default.DayVolumeFiltering,
                                    AllowSorting = Properties.Settings.Default.DayVolumeSorting,
                                    HeaderText = dayVolumeColumnDate.ToString(Properties.Settings.Default.DateDisplayFormat + "\n" + Properties.Resources.LeftBracket + dayVolumeColumnWeekday + Properties.Resources.RightBracket),
                                    MappingName = dayVolumeColumnMappingName,
                                    MaximumWidth = Properties.Settings.Default.MaxCellWidth,
                                    MinimumWidth = Properties.Settings.Default.MinCellWidth,
                                    ShowHeaderToolTip = true,
                                    TextAlignment = TextAlignment.Right
                                });

                            stackedHeaderRowDayVolumeChildColumns.Append(dayVolumeColumnMappingName + ",");
                        } // end for

                        var stackedHeaderRowDayVolume = new StackedHeaderRow();

                        stackedHeaderRowDayVolume.StackedColumns.Add(new StackedColumn()
                        {
                            ChildColumns = stackedHeaderRowDayVolumeChildColumns.ToString(),
                            HeaderText = Properties.Resources.DayVolume
                                         + Properties.Resources.LeftBracket
                                         + _volumeUnitViewModel.VolumeUnitRecords.First(item => item.Coefficient == Properties.Settings.Default.DayVolumeUnit).Name
                                         + Properties.Resources.RightBracket,
                            MappingName = "DayVolume"
                        });
                        
                        DataGridStrikePriceVolumeTable.ItemsSource = _strikePriceTotalVolumeViewModel.StrikePriceTotalVolumeRecords; // Bind data to the specified data grid. It can also clear data in the first and second columns (strike price and total volume).
                        DataGridStrikePriceVolumeTable.StackedHeaderRows.Add(stackedHeaderRowDayVolume);
                        DataGridStrikePriceVolumeTable.Columns.Resume();
                        DataGridStrikePriceVolumeTable.RefreshColumns();
                        DataGridStrikePriceVolumeTable.Visibility = Visibility.Visible;
                        ButtonExportToExcel.IsEnabled = true;
                        ButtonPrint.IsEnabled = true;
                    }
                    // Access is denied by the specified data source.
                    else if (_strikePriceVolumeRowCollection[0].Length == 1)
                    {
                        TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_AccessDenied;
                        TextBlockNullData.Visibility = Visibility.Visible;
                    }
                    // The date range is too long.
                    else
                    {
                        TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_ImproperDateRange;
                        TextBlockNullData.Visibility = Visibility.Visible;
                    } // end if...else
                }
                // Wrong filters (symbol/start date/end date).
                else
                {
                    TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_WrongFilters;
                    TextBlockNullData.Visibility = Visibility.Visible;
                } // end if...else
            }
            // It seems to be no internet connection.
            else
            {
                TextBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_NetworkError;
                TextBlockNullData.Visibility = Visibility.Visible;
            } // end if...else

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
                    
                    e.Value = dayVolume == null
                        ? null
                        : (decimal?) decimal.Round((decimal) dayVolume / (Properties.Settings.Default.DayVolumeUnit * 100m), Properties.Settings.Default.DayVolumeDecimalDigits); // 每日成交量（1手 = 100股）。
                    break;
                } // end if
        } // end method DataGridStrikePriceVolumeTable_QueryUnboundColumnValue

        private void DataGridStrikePriceVolumeTable_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            ButtonClearSelection.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_SelectionChanged

        private void DataGridStrikePriceVolumeTable_OnResizingColumns(object sender, ResizingColumnsEventArgs e)
        {
            if (e.Reason == ColumnResizingReason.Resized)
                ButtonRestoreColumnWidth.IsEnabled = true;
        } // end method DataGridStrikePriceVolumeTable_OnResizingColumns

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

        private void TextBoxSymbol_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim();

            if (!Regex.IsMatch(input, RegularExpressionHasChinese))
                return;

            // When the text box loses focus, it can change the name of the stock you entered to the corresponding symbol if a matching one is found.
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

        // Perform actions when necessary if the selected item in the auto-complete suggestion list is changed.
        private void TextBoxSymbol_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (TextBoxSymbol.SelectedItem != null && Regex.IsMatch(TextBoxSymbol.Text.Trim(), RegularExpressionHasChinese))
                TextBoxSymbol.Text = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol;
        } // end method TextBoxSymbol_SelectedItemChanged
        
        private void TextBoxSymbol_TextChanged(object sender, RoutedEventArgs e)
        {
            var input = TextBoxSymbol.Text.Trim();

            if (Regex.IsMatch(input, RegularExpressionHasChinese))
            {
                _isStandardSymbol = false;
                TextBoxSymbol.Foreground = Properties.Settings.Default.ColourDanger;
                TextBoxSymbol.SearchItemPath = "Name";

                if (TextBoxSymbol.SelectedItem != null && input.Equals(((StockSymbolNameData) TextBoxSymbol.SelectedItem).Name))
                    TextBoxSymbol.Text = ((StockSymbolNameData) TextBoxSymbol.SelectedItem).Symbol;
            }
            else
            {
                _isStandardSymbol = Regex.IsMatch(input, @"^[Ss]([Hh]|[Zz])\d{6}$");
                TextBoxSymbol.SearchItemPath = "Symbol";

                // Change the foreground colour of the specified text box to red if the format of the input for the symbol is not satisfied.
                if (_isStandardSymbol)
                {
                    TextBoxSymbol.Foreground = Properties.Settings.Default.PrimaryTextColour;
                    _strikePriceVolumeDataProcessor.Symbol = input;
                }
                else
                    TextBoxSymbol.Foreground = Properties.Settings.Default.ColourDanger;
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
                DatePickerStartDate.Foreground = Properties.Settings.Default.ColourDanger;
                DatePickerEndDate.Foreground = Properties.Settings.Default.ColourDanger;
            }
            else
            {
                DatePickerStartDate.Foreground = Properties.Settings.Default.PrimaryTextColour;
                DatePickerEndDate.Foreground = Properties.Settings.Default.PrimaryTextColour;
            } // end if...else
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
                ButtonSearch.IsEnabled = true;
            else
                ButtonSearch.IsEnabled = false;
        } // end method ChangeButtonSearchStatus

        /// <summary>
        /// Restore column width by resetting it to apply autosize calculation.
        /// </summary>
        private void RestoreColumnWidth()
        {
            foreach (var column in DataGridStrikePriceVolumeTable.Columns.Where(column => !double.IsNaN(column.Width)))
                column.Width = double.NaN; // Reset column width to apply autosize calculation.

            DataGridStrikePriceVolumeTable.GridColumnSizer.Refresh();
        } // end method RestoreColumnWidth
        #endregion Private Methods
    } // end class StrikePriceVolumeTab
} // end namespace ShSzStockHelper.Views