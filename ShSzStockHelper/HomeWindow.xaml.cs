/*
 * @Description: the back-end code of the home window
 * @Version: 1.0.7.20200723
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-07-23 14:12:09
 */

using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for the home window.
    /// </summary>
    public partial class HomeWindow : ChromelessWindow
    {
        private readonly StrikePrice_VolumeDataProcessor _strikePrice_VolumeDataProcessor;
        private bool _isStandardSymbol = false;
        private const string _dateStringFormat = "yyyy-M-d";

        public HomeWindow()
        {
            InitializeComponent();

            _strikePrice_VolumeDataProcessor = new StrikePrice_VolumeDataProcessor(); // Intialise the helper class for processing data of strike prices and volumes collected.

            windowHome.Title = Properties.Resources.AppName + " " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            datePickerStartDate.MaxDate = DateTime.Now;
            datePickerEndDate.MaxDate = DateTime.Now;
        } // end constructor HomeWindow

        #region Control events
        private void TextBoxSymbol_TextChanged(object sender, RoutedEventArgs e)
        {
            _strikePrice_VolumeDataProcessor.Symbol = textBoxSymbol.Text;
            _isStandardSymbol = Regex.IsMatch(textBoxSymbol.Text.Trim(), @"[Ss]([Hh]|[Zz])\d{6}");

            // Change the foreground colour of the specified text box to red if the format of the input for the symbol is satisfied.
            if (_isStandardSymbol)
                textBoxSymbol.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            else
                textBoxSymbol.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            ChangeButtonSearchStatus();
        } // end method TextBoxSymbol_TextChanged

        private void DatePickerStartDate_ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            _strikePrice_VolumeDataProcessor.StartDate = e.NewValue.ToDateTime();
            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerStartDate_ValueChanged

        private void DatePickerEndDate_ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            _strikePrice_VolumeDataProcessor.EndDate = e.NewValue.ToDateTime();
            ChangeDatePickerForeground();
            ChangeButtonSearchStatus();
        } // end method DatePickerEndDate_ValueChanged

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            // Ensure that the specified controls are in initial status.
            dataGridStrikePrice_VolumeTable.Visibility = Visibility.Hidden;
            textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_UnknownError;
            labelNullData.Visibility = Visibility.Hidden;
            buttonExportToExcel.IsEnabled = false;

            decimal?[,] strikePrice_VolumeRows = _strikePrice_VolumeDataProcessor.GetStrikePrice_VolumeData();

            if (strikePrice_VolumeRows != null)
            {
                if (strikePrice_VolumeRows.GetLength(1) != 1)
                {
                    dataGridStrikePrice_VolumeTable.Columns.Suspend(); // Suspend all the UI updates of the specified data grid to improve performance.

                    // Bind data to the specified data grid. It can also clear data in the first and second columns (strike price and total volume).
                    StrikePrice_TotalVolumeViewModel strikePrice_TotalVolumeViewModel = new StrikePrice_TotalVolumeViewModel();
                    dataGridStrikePrice_VolumeTable.ItemsSource = strikePrice_TotalVolumeViewModel.StrikePrice_TotalVolumeRecords;

                    if (dataGridStrikePrice_VolumeTable.StackedHeaderRows.Count > 0)
                    {
                        string[] childColumns = dataGridStrikePrice_VolumeTable.StackedHeaderRows[0].StackedColumns[0].ChildColumns.Split(",");

                        // Clear unbound columns containing each day's volumes.
                        foreach (string mappingName in childColumns)
                        {
                            GridColumn column = dataGridStrikePrice_VolumeTable.Columns[mappingName];

                            if (column == null)
                                continue;

                            dataGridStrikePrice_VolumeTable.Columns.Remove(column);
                        } // end foreach

                        dataGridStrikePrice_VolumeTable.StackedHeaderRows.Clear(); // Clear stacked headers after clearing unbound columns.
                    } // end if

                    int nodeTotalCount = strikePrice_VolumeRows.GetLength(0);

                    for (int nodeCount = 0; nodeCount < nodeTotalCount; nodeCount++)
                        strikePrice_TotalVolumeViewModel.StrikePrice_TotalVolumeRecords.Add(new StrikePrice_TotalVolumeData(strikePrice_VolumeRows[nodeCount, 0], strikePrice_VolumeRows[nodeCount, 1]));

                    string stackedHeaderRowDayVolumeChildColumns = string.Empty;

                    for (int dayVolumeColumnCount = 0; dayVolumeColumnCount < strikePrice_VolumeRows.GetLength(1) - 2; dayVolumeColumnCount++)
                    {
                        string dayVolumeColumnMappingName = dayVolumeColumnCount.ToString();

                        dataGridStrikePrice_VolumeTable.Columns.Add(new GridUnBoundColumn()
                        {
                            HeaderText = _strikePrice_VolumeDataProcessor.StartDate.AddDays(dayVolumeColumnCount).ToString("yyyy-M-d"),
                            MappingName = dayVolumeColumnMappingName,
                            TextAlignment = TextAlignment.Right,
                            MinimumWidth = 50,
                            MaximumWidth = 400
                        });
                        stackedHeaderRowDayVolumeChildColumns += dayVolumeColumnMappingName + ",";
                    } // end for

                    StackedHeaderRow stackedHeaderRowDayVolume = new StackedHeaderRow();

                    stackedHeaderRowDayVolume.StackedColumns.Add(new StackedColumn()
                    {
                        ChildColumns = stackedHeaderRowDayVolumeChildColumns,
                        HeaderText = Properties.Resources.DayVolumeColumnHeader,
                        MappingName = "DayVolume",
                    });
                    dataGridStrikePrice_VolumeTable.StackedHeaderRows.Add(stackedHeaderRowDayVolume);

                    // Populate data of each day's volume for unbound columns.
                    dataGridStrikePrice_VolumeTable.QueryUnboundColumnValue += dataGridStrikePrice_VolumeTable_QueryUnboundColumnValue;

                    void dataGridStrikePrice_VolumeTable_QueryUnboundColumnValue(object sender, GridUnboundColumnEventsArgs e)
                    {
                        if (e.UnBoundAction == UnBoundActions.QueryData)
                        {
                            var strikePrice = Convert.ToDecimal(e.Record.GetType().GetProperty("StrikePrice").GetValue(e.Record));

                            for (int nodeCount = 0; nodeCount < nodeTotalCount; nodeCount++)
                                if (strikePrice_VolumeRows[nodeCount, 0] == strikePrice)
                                {
                                    e.Value = strikePrice_VolumeRows[nodeCount, 2 + Convert.ToInt32(e.Column.MappingName)];
                                    break;
                                } // end if
                        } // end if
                    } // end local method dataGridStrikePrice_VolumeTable_QueryUnboundColumnValue

                    // Set advanced filter type for unbound columns.
                    dataGridStrikePrice_VolumeTable.FilterItemsPopulating += dataGridStrikePrice_VolumeTabl_FilterItemsPopulating;

                    static void dataGridStrikePrice_VolumeTabl_FilterItemsPopulating(object sender, GridFilterItemsPopulatingEventArgs e)
                    {
                        if (e.Column.MappingName != "StrikePrice" || e.Column.MappingName != "TotalVolume")
                        {
                            e.FilterControl.AdvancedFilterType = AdvancedFilterType.NumberFilter;
                            e.FilterControl.SetColumnDataType(typeof(decimal));
                            e.FilterControl.AscendingSortString = GridResourceWrapper.SortNumberAscending;
                            e.FilterControl.DescendingSortString = GridResourceWrapper.SortNumberDescending;
                        } // end if
                    } // end local method dataGridStrikePrice_VolumeTabl_FilterItemsPopulating

                    dataGridStrikePrice_VolumeTable.Columns.Resume();
                    dataGridStrikePrice_VolumeTable.RefreshColumns();
                    dataGridStrikePrice_VolumeTable.Visibility = Visibility.Visible;
                    buttonExportToExcel.IsEnabled = true;
                }
                // The date range is too long.
                else
                {
                    textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_ImproperDateRange;
                    labelNullData.Visibility = Visibility.Visible;
                } // end if...else
            }
            // Wrong filters (symbol/start date/end date).
            else
            {
                textBlockNullData.Text = Properties.Resources.TextBlockNullData_Text_WrongFilters;
                labelNullData.Visibility = Visibility.Visible;
            } // end if...else
        } // end method ButtonSearch_Click

        private void ButtonExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelExportingOptions excelExportingOptions = new ExcelExportingOptions
            {
                ExportStackedHeaders = true,
                ExportingEventHandler = ExportingHandler
            };
            IWorkbook workbook = dataGridStrikePrice_VolumeTable.ExportToExcel(dataGridStrikePrice_VolumeTable.View, excelExportingOptions).Excel.Workbooks[0];
            
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = _strikePrice_VolumeDataProcessor.Symbol + "_" + _strikePrice_VolumeDataProcessor.StartDate.ToString(_dateStringFormat) + "~" + _strikePrice_VolumeDataProcessor.EndDate.ToString(_dateStringFormat),
                Filter = Properties.Resources.SaveFileDialogueExportToExcel_Filter,
                FilterIndex = 2
            };

            // Display a dialogue that allows the user to specify a filename to save an Excel file as.
            if (saveFileDialog.ShowDialog() == true)
            {
                // TODO: This is just a temporary solution to the exception System.EntryPointNotFoundException: 'Unable to find an entry point named 'CopyMemory' in DLL 'kernel32.dll'.'
                try
                {
                    using (Stream stream = saveFileDialog.OpenFile())
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Avoid throwing the exception "System.NotSupportedException: No data is available for encoding 437".

                        workbook.Version = saveFileDialog.FilterIndex switch
                        {
                            1 => ExcelVersion.Excel97to2003,
                            _ => ExcelVersion.Excel2010,
                        };
                        workbook.SaveAs(stream);
                    } // end using

                    Process.Start("Explorer.exe", "/select," + saveFileDialog.FileName); // Start File Explorer and locate the created Excel file.
                }
                catch (EntryPointNotFoundException)
                {
                    MessageBox.Show("导出失败！\n无法导出成低版本Excel文件（.xls)是已知问题，将在更新版本中修复。给您带来的不便敬请谅解。如果可以，请选择导出成高版本Excel文件（.xlsx）。", Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                } // end try...catch
            } // end if
        } // end method ButtonExportToExcel_Click

        // A handler for styling the created workbooks.
        private static void ExportingHandler(object sender, GridExcelExportingEventArgs e)
        {
            if (e.CellType == ExportCellType.HeaderCell || e.CellType == ExportCellType.StackedHeaderCell)
            {
                e.CellStyle.BackGroundBrush = new SolidColorBrush(Color.FromRgb(205, 205, 205));
                e.Style.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                e.Style.VerticalAlignment = ExcelVAlign.VAlignCenter;
            } // end if

            e.CellStyle.FontInfo.FontName = "Segoe UI";
            e.Handled = true;
        } // end method ExportingHandler
        #endregion Control events

        #region Private methods
        /// <summary>
        /// Change the foreground colour of data pickers to red if the start date is later than the end date.
        /// </summary>
        private void ChangeDatePickerForeground()
        {
            if (datePickerStartDate.Value != null
                && datePickerEndDate.Value != null
                && datePickerStartDate.Value.ToDateTime().CompareTo(datePickerEndDate.Value.ToDateTime()) > 0)
            {
                datePickerStartDate.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                datePickerEndDate.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                datePickerStartDate.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                datePickerEndDate.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            } // end if...else
        } // end method ChangeDatePickerForeground

        /// <summary>
        /// Determine if the search button should be enabled.
        /// </summary>
        private void ChangeButtonSearchStatus()
        {
            if (_isStandardSymbol
                && datePickerStartDate.Value != null
                && datePickerEndDate.Value != null
                && datePickerStartDate.Value.ToDateTime().CompareTo(datePickerEndDate.Value.ToDateTime()) <= 0)
                buttonSearch.IsEnabled = true;
            else
                buttonSearch.IsEnabled = false;
        } // end method ChangeButtonSearchStatus
        #endregion Private methods
    } // end class HomeWindow
} // end namespace ShSzStockHelper