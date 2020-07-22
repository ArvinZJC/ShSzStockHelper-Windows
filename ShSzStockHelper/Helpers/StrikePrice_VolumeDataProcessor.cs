/*
 * @Description: helper class for processing data of strike prices and volumes collected
 * @Version: 1.0.3.20200719
 * @Author: Arvin Zhao
 * @Date: 2020-07-15 18:25:42
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-07-19 19:25:42
 */

using HtmlAgilityPack;
using System;

namespace ShSzStockHelper
{
    /// <summary>
    /// A helper class for processing data of strike prices and volumes collected.
    /// </summary>
    class StrikePrice_VolumeDataProcessor
    {
        private const string _cellNodePath = "//tbody/tr";
        private const string _dateStringFormat = "yyyy-M-d";

        /// <summary>
        /// The symbol of a stock.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The start date of the query.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the query.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Get data of strike prices and volumes.
        /// </summary>
        /// <returns>
        /// A 2D array of type decimal of size at least (1, 3) containing the data or of size (1, 1) if the date range is too long, or <c>null</c> if the filters are wrong.
        /// The first index of the array represents the index of rows (start from 0).
        /// The second index of the array represents the index of columns (start from 0).
        /// For the second index, Index 0 represents strike prices, while Index 1 represents total volumes. The other indexes represent each day's volumes.
        /// </returns>
        public decimal?[,] GetStrikePrice_VolumeData()
        {
            HtmlNode rootNode = GetRootNode(StartDate, EndDate);

            if (rootNode.SelectNodes("//body/div") != null)
            {
                HtmlNodeCollection strikePrice_TotalVolumeNodes = rootNode.SelectNodes(_cellNodePath);

                if (strikePrice_TotalVolumeNodes != null)
                {
                    int dayTotalCount = EndDate.Subtract(StartDate).Days + 1; // Calculate the number of days (>= 1) from the start date to the end date.
                    decimal?[,] strikePrice_VolumeRows = new decimal?[strikePrice_TotalVolumeNodes.Count, 2 + dayTotalCount]; // The table of strike prices and volumes should have at least 3 columns (strike price, total volume, and each day's volume).
                    int nodeIndex = 0; // Represent the index of rows (start from 0).
                    int elementIndex; // Represent the index of columns (start from 0).

                    // Get data of strike prices and total volumes.
                    foreach (HtmlNode node in strikePrice_TotalVolumeNodes)
                    {
                        elementIndex = 0;

                        foreach (HtmlNode element in node.Elements("td"))
                        {
                            // Index 0 represents strike price, while Index 1 represents total volume. (Determined by the specified data source.)
                            if (elementIndex < 2)
                            {
                                decimal elementValue = Convert.ToDecimal(element.InnerText);
                                strikePrice_VolumeRows[nodeIndex, elementIndex] = elementIndex == 0 ? elementValue : elementValue / 100m; // 总成交量：1手 = 100股。
                                elementIndex++;
                            }
                            else
                                break;
                        } // end foreach

                        nodeIndex++;
                    } // end foreach

                    if (dayTotalCount > 1)
                    {
                        // Get data of each day's volume.
                        for (int dayCount = 0; dayCount < dayTotalCount; dayCount++)
                        {
                            DateTime dayVolumeDate = Convert.ToDateTime(StartDate.ToShortDateString()).AddDays(dayCount);
                            HtmlNodeCollection strikePrice_DayVolumeNodes = GetRootNode(dayVolumeDate, dayVolumeDate).SelectNodes(_cellNodePath);

                            /*
                             * Execute the code block if the specified node collection is not null.
                             * Otherwise, just keep the initial element value (0 of type decimal) of the 2D array containing data of strike prices and volume.
                             */
                            if (strikePrice_DayVolumeNodes != null)
                            {
                                nodeIndex = 0;

                                foreach (HtmlNode node in strikePrice_DayVolumeNodes)
                                {
                                    elementIndex = 0;
                                    decimal strikePrice = 0m;

                                    foreach (HtmlNode element in node.Elements("td"))
                                    {
                                        // Index 0 represents strike price. (Determined by the specified data source.)
                                        if (elementIndex == 0)
                                        {
                                            strikePrice = Convert.ToDecimal(element.InnerText);
                                            elementIndex++;
                                        }
                                        // Index 1 represents the specified day's volume. (Determined by the specified data source.)
                                        else if (elementIndex == 1)
                                        {
                                            for (int nodeCount = 0; nodeCount < strikePrice_TotalVolumeNodes.Count; nodeCount++)
                                                if (strikePrice_VolumeRows[nodeCount, 0] == strikePrice)
                                                {
                                                    strikePrice_VolumeRows[nodeCount, 2 + dayCount] = Convert.ToDecimal(element.InnerText) / 100m; // 每日成交量：1手 = 100股。
                                                    break;
                                                } // end if

                                            elementIndex++;
                                        }
                                        else
                                            break;
                                    } // end foreach

                                    nodeIndex++;
                                } // end foreach
                            }
                            else
                                for (int nodeCount = 0; nodeCount < strikePrice_TotalVolumeNodes.Count; nodeCount++)
                                    strikePrice_VolumeRows[nodeCount, 2 + dayCount] = null;
                        } // end for
                    }
                    else
                        for (int nodeCount = 0; nodeCount < strikePrice_TotalVolumeNodes.Count; nodeCount++)
                            strikePrice_VolumeRows[nodeCount, 2] = strikePrice_VolumeRows[nodeCount, 1];

                    return strikePrice_VolumeRows;
                }
                // Wrong filters (symbol/start date/end date).
                else
                    return null;
            }
            // The date range is too long.
            else
                return new decimal?[1, 1];
        } // end method GetStrikePrice_VolumeData

        /// <summary>
        /// Get the root node of the HTML document from the specified source providing data of strike prices and volumes.
        /// </summary>
        /// <param name="startDate">The start date of the query.</param>
        /// <param name="endDate">The end date of the query.</param>
        /// <returns>An <see cref="HtmlNode"/> object containing the root node.</returns>
        private HtmlNode GetRootNode(DateTime startDate, DateTime endDate)
        {
            return new HtmlWeb()
                .Load(@"http://market.finance.sina.com.cn/pricehis.php?symbol=" + Symbol + "&startdate=" + startDate.ToString(_dateStringFormat) + "&enddate=" + endDate.ToString(_dateStringFormat))
                .DocumentNode;
        } // end method GetRootNode
    } // end class StrikePrice_VolumeDataProcessor
} // end namespace ShSzStockHelper