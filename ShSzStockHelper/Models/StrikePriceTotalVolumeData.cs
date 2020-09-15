/*
 * @Description: a data model of strike prices and total volumes
 * @Version: 1.0.4.20200829
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 12:27:11
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-29 14:09:25
 */

namespace ShSzStockHelper.Models
{
    /// <summary>
    /// A data model of strike prices and total volumes.
    /// </summary>
    internal class StrikePriceTotalVolumeData
    {
        /// <summary>
        /// A strike price of a stock.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal StrikePrice { get; set; }

        /// <summary>
        /// Total volume of a strike price of a stock.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal TotalVolume { get; set; }
    } // end class StrikePriceTotalVolumeData
} // end namespace ShSzStockHelper.Models