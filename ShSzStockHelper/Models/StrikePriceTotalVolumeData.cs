/*
 * @Description: a data model of strike prices and total volumes
 * @Version: 1.0.3.20200805
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 12:27:11
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-05 14:09:25
 */

namespace ShSzStockHelper
{
    /// <summary>
    /// A data model of strike prices and total volumes.
    /// </summary>
    class StrikePriceTotalVolumeData
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceTotalVolumeData"/> class.
        /// </summary>
        /// <param name="strikePrice">A strike price of a stock.</param>
        /// <param name="totalVolume">Total volume of a strike price of a stock.</param>
        public StrikePriceTotalVolumeData(decimal strikePrice, decimal totalVolume)
        {
            StrikePrice = strikePrice;
            TotalVolume = totalVolume;
        } // end constructor StrikePrice_TotalVolumeData

        /// <summary>
        /// A strike price of a stock.
        /// </summary>
        public decimal StrikePrice { get; set; }

        /// <summary>
        /// Total volume of a strike price of a stock.
        /// </summary>
        public decimal TotalVolume { get; set; }
    } // end class StrikePriceTotalVolumeData
} // end namespace ShSzStockHelper.Model