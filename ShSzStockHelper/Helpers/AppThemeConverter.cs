/*
 * @Description: a converter to convert a theme ID to a corresponding theme type
 * @Version: 1.0.1.20200904
 * @Author: Arvin Zhao
 * @Date: 2020-09-03 19:03:33
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-04 21:36:16
 */

using Syncfusion.SfSkinManager;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ShSzStockHelper.Helpers
{
    /// <summary>
    /// A converter to convert a theme ID to a corresponding <see cref="VisualStyles"/> type.
    /// </summary>
    internal class AppThemeConverter : IValueConverter
    {
        /// <summary>
        /// Convert a theme ID to a corresponding <see cref="VisualStyles"/> type.
        /// </summary>
        /// <param name="value">The theme ID produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A <see cref="VisualStyles"/> type converted from a theme ID.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? VisualStyles.MaterialLight
                : (VisualStyles) (int) value;
        } // end method Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } // end method ConvertBack
    } // end class AppThemeConverter
} // end namespace ShSzStockHelper.Helpers