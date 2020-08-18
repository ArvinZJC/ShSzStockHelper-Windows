/*
 * @Description: a data model of stocks' symbols and corresponding names
 * @Version: 1.0.0.20200813
 * @Author: Arvin Zhao
 * @Date: 2020-08-13 18:28:23
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-13 18:33:08
 */

namespace ShSzStockHelper
{
    /// <summary>
    /// A data model of stocks' symbols and corresponding names.
    /// </summary>
    public partial class StockSymbolNameData
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="StockSymbolNameData"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of a stock.</param>
        /// <param name="name">The name of a stock.</param>
        public StockSymbolNameData(string symbol, string name)
        {
            Symbol = symbol;
            Name = name;
        } // end constructor StockSymbolNameData

        /// <summary>
        /// The symbol of a stock.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The name of a stock.
        /// </summary>
        public string Name { get; set; }
    } // end class StockSymbolNameData
} // end namespace ShSzStockHelper