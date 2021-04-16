/*
 * @Description: a customised print manager to customise printing operations
 * @Version: 1.0.0.20210415
 * @Author: Arvin Zhao
 * @Date: 2021-04-15 14:41:44
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-15 14:42:13
 */

using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShSzStockHelper.Helpers
{
    /// <summary>
    /// A customised print manager to customise printing operations.
    /// </summary>
    internal class PrintManager : GridPrintManager
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="PrintManager"/> class.
        /// </summary>
        /// <param name="dataGrid">A data grid control that a print manager belongs to.</param>
        public PrintManager(SfDataGrid dataGrid) : base(dataGrid) {} // end constructor PrintManager

        protected override object GetColumnElement(object record, string mappingName)  
        {  
            var columnElement = base.GetColumnElement(record, mappingName) as TextBlock;
            
            if (columnElement != null)
                columnElement.Foreground = new SolidColorBrush(Colors.Black); // Ensure all record cells' text colour is black.

            return columnElement;  
        } // end method GetColumnElement
  
        protected override UIElement GetColumnHeaderElement(string mappingName)  
        {  
            var columnElement = base.GetColumnHeaderElement(mappingName) as TextBlock;
            
            if (columnElement != null)
                columnElement.Foreground = new SolidColorBrush(Colors.Black); // Ensure all column header (stacked header excluded) cells' text colour is black.

            return columnElement;  
        } // end method GetColumnHeaderElement

        protected override UIElement GetStackedColumnHeaderElement(string mappingName)  
        {  
            var columnElement = base.GetStackedColumnHeaderElement(mappingName) as TextBlock;
            
            if (columnElement != null)
                columnElement.Foreground = new SolidColorBrush(Colors.Black); // Ensure all stacked header cells' text colour is black.

            return columnElement;  
        } // end method GetStackedColumnHeaderElement
    } // end class PrintManager
} // end namespace ShSzStockHelper.Helpers