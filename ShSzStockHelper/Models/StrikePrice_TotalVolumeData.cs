/*
 * @Description: a data model of strike prices and total volumes
 * @Version: 1.0.2.20200716
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 12:27:11
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-07-16 14:09:25
 */

namespace ShSzStockHelper
{
    /// <summary>
    /// A data model of strike prices and total volumes.
    /// </summary>
    class StrikePrice_TotalVolumeData
    {
        public StrikePrice_TotalVolumeData(decimal? strikePrice, decimal? totalVolume)
        {
            StrikePrice = strikePrice;
            TotalVolume = totalVolume;
        } // end constructor StrikePrice_TotalVolumeData

        /// <summary>
        /// A strike price of a stock.
        /// </summary>
        public decimal? StrikePrice { get; set; }

        /// <summary>
        /// Total volume of a strike price of a stock.
        /// </summary>
        public decimal? TotalVolume { get; set; }
    } // end class StrikePrice_TotalVolumeData
} // end namespace ShSzStockHelper.Model