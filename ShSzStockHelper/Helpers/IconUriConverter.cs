/*
 * @Description: a converter to convert a URI string to a corresponding URI object
 * @Version: 1.0.0.20200913
 * @Author: Arvin Zhao
 * @Date: 2020-09-13 23:32:31
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-13 23:39:58
 */

using System;
using System.Globalization;
using System.Windows.Data;

namespace ShSzStockHelper.Helpers
{
    /// <summary>
    /// A converter to convert a URI string to a corresponding <see cref="Uri"/> object.
    /// </summary>
    internal class IconUriConverter : IValueConverter
    {
        /// <summary>
        /// Convert a URI string to a corresponding <see cref="Uri"/> object.
        /// </summary>
        /// <param name="value">The URI string produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A <see cref="Uri"/> object converted from a URI string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? new Uri("../AppIcon.ico", UriKind.Relative)
                : new Uri((string) value, UriKind.Relative);
        } // end method Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } // end method ConvertBack
    } // end class IconUriConverter
} // end namespace ShSzStockHelper.Helpers