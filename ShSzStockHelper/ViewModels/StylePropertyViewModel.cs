/*
 * @Description: a view model defining the style properties used by the app
 * @Version: 1.0.3.20200814
 * @Author: Arvin Zhao
 * @Date: 2020-08-06 13:25:33
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-14 13:36:16
 */

using Syncfusion.SfSkinManager;
using System;
using System.Windows.Media;

namespace ShSzStockHelper
{
    /// <summary>
    /// A view model defining the style properties used by the app.
    /// </summary>
    class StylePropertyViewModel
    {
        #region Colour
        /// <summary>
        /// The colour used to represent danger.
        /// </summary>
        public Brush ColourDanger { get; } = Brushes.Red;

        /// <summary>
        /// The background of the header in the exported Excel files.
        /// </summary>
        public Brush ExcelHeaderBackground { get; } = Brushes.LightGray;
        #endregion Colour

        #region Data Grid
        /// <summary>
        /// The height of the header row.
        /// </summary>
        public double HeaderRowHeight { get; } = 42.0;

        /// <summary>
        /// The maximum cell width.
        /// </summary>
        public double MaxCellWidth { get; } = 300.0;

        /// <summary>
        /// The minimum cell width.
        /// </summary>
        public double MinCellWidth { get; } = 50.0;
        #endregion Data Grid

        #region Date Picker
        /// <summary>
        /// The format of dates.
        /// </summary>
        public string DateFormat { get; } = "yyyy-M-d";

        /// <summary>
        /// The maximum date of the date picker.
        /// </summary>
        public DateTime MaxDate { get; } = DateTime.Now;

        /// <summary>
        /// The minimum date of the date picker.
        /// Shanghai Stock Exchange started working on 19 December 1990, while Shenzhen Stock Exchange opened on 1 December 1990. However, the data source does not start to record data from that time.
        /// </summary>
        public DateTime MinDate { get; } = Convert.ToDateTime("1990-12-1");
        #endregion Date Picker

        #region Font
        private const string _globalFontFamilyName = "Microsoft YaHei UI";

        /// <summary>
        /// The font size of content text.
        /// </summary>
        public double ContentTextFontSize { get; } = 12.0;

        /// <summary>
        /// The name of the global font family.
        /// </summary>
        public string GlobalFontFamilyName { get; } = _globalFontFamilyName;

        /// <summary>
        /// The global font family.
        /// </summary>
        public FontFamily GlobalFontFamily { get; } = new FontFamily(_globalFontFamilyName);

        /// <summary>
        /// The font size of primary text.
        /// </summary>
        public double PrimaryTextFontSize { get; } = 14.0;
        #endregion Font

        #region Tab Control
        /// <summary>
        /// The maximum width of a tab item.
        /// </summary>
        public double MaxTabItemWidth { get; } = 200.0;
        #endregion Tab Control

        #region Textbox
        /// <summary>
        /// The maximum height of the auto-complete suggestion list.
        /// </summary>
        public double MaxDropDownHeight { get; } = 300.0;
        /// <summary>
        /// The maximum input length.
        /// </summary>
        public int MaxInputLength { get; } = 14;
        #endregion Textbox

        #region Theme
        public VisualStyles AppTheme { get; } = VisualStyles.MaterialLight;
        #endregion Theme

        #region Window
        /// <summary>
        /// The minimum height of the window.
        /// </summary>
        public double MinWindowHeight { get; } = 600.0;

        /// <summary>
        /// The minimum width of the window.
        /// </summary>
        public double MinWindowWidth { get; } = 800.0;
        #endregion Window
    } // end class StylePropertyViewModel
} // end namespace ShSzStockHelper