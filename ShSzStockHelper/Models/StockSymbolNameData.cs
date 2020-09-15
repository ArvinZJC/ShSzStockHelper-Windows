/*
 * @Description: a data model of stocks' symbols and corresponding names
 * @Version: 1.0.3.20200903
 * @Author: Arvin Zhao
 * @Date: 2020-08-13 18:28:23
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-03 18:33:08
 */

namespace ShSzStockHelper.Models
{
    /// <summary>
    /// A data model of stocks' symbols and corresponding names.
    /// </summary>
    public class StockSymbolNameData
    {
        /// <summary>
        /// The symbol of a stock.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The name of a stock.
        /// </summary>
        public string Name { get; set; }
    } // end class StockSymbolNameData
} // end namespace ShSzStockHelper.Models