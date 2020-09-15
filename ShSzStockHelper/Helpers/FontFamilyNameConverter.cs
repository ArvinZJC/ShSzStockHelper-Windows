/*
 * @Description: a converter to convert a font family name to a corresponding font family object
 * @Version: 1.0.0.20200904
 * @Author: Arvin Zhao
 * @Date: 2020-09-04 14:38:47
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-04 14:51:37
 */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShSzStockHelper.Helpers
{
    /// <summary>
    /// A converter to convert a font family name to a corresponding <see cref="FontFamily"/> object.
    /// </summary>
    internal class FontFamilyNameConverter : IValueConverter
    {
        /// <summary>
        /// Convert a font family name to a corresponding <see cref="FontFamily"/> object.
        /// </summary>
        /// <param name="value">The font family name produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A <see cref="FontFamily"/> object converted from a font family name.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? new FontFamily(Properties.DefaultUserSettings.Default.DisplayFontFamilyName)
                : new FontFamily((string) value);
        } // end method Convert

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } // end method ConvertBack
    } // end class FontFamilyNameConverter
} // end namespace ShSzStockHelper.Helpers