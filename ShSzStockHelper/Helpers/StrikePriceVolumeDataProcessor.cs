/*
 * @Description: helper class for processing data of strike prices and volumes collected
 * @Version: 1.0.9.20200817
 * @Author: Arvin Zhao
 * @Date: 2020-07-15 18:25:42
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-17 19:25:42
 */

using HtmlAgilityPack;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ShSzStockHelper
{
    /// <summary>
    /// A helper class for processing data of strike prices and volumes collected.
    /// </summary>
    class StrikePriceVolumeDataProcessor
    {
        private readonly StylePropertyViewModel _stylePropertyViewModel;
        private readonly HtmlWeb _htmlWeb;
        private const string _cellNodePath = "//tbody/tr";

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

        public StrikePriceVolumeDataProcessor()
        {
            _stylePropertyViewModel = new StylePropertyViewModel(); // Initialise the view model class for using the defined style properties.
            _htmlWeb = new HtmlWeb();
        } // end constructor StrikePriceVolumeDataProcessor

        /// <summary>
        /// Get the root node of the HTML document from the specified source providing data of strike prices and volumes.
        /// </summary>
        /// <param name="startDate">The start date of the query.</param>
        /// <param name="endDate">The end date of the query.</param>
        /// <returns>An <see cref="HtmlNode"/> object containing the root node.</returns>
        private async Task<HtmlNode> GetStrikePriceVolumeHtmlRootNodeAsync(DateTime startDate, DateTime endDate)
        {
            return (await _htmlWeb
                .LoadFromWebAsync(@"http://market.finance.sina.com.cn/pricehis.php?symbol=" + Symbol.ToLower() + "&startdate=" + startDate.ToString(_stylePropertyViewModel.DateFormat) + "&enddate=" + endDate.ToString(_stylePropertyViewModel.DateFormat)))
                .DocumentNode;
        } // end method GetStrikePriceVolumeHtmlRootNodeAsync

        /// <summary>
        /// Retrieve the name of a stock from the web.
        /// </summary>
        /// <returns>The name of a stock, or <c>null</c> if no matching result is found on the web.</returns>
        public async Task<string> GetStockNameFromWebAsync()
        {
            /*
             * Avoid throwing the exception "System.InvalidOperationException: 'The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.'".
             * The inner exception is "ArgumentException: 'GB18030' is not a supported encoding name.".
             */
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            /*
             * Sample values:
             * 1. var hq_str_sh601006="大秦铁路,6.640,2020-08-17,13:07:28,00,";
             * 2. var hq_str_sh6010065="";
             */
            string originalText = (await _htmlWeb.LoadFromWebAsync(@"http://hq.sinajs.cn/list=" + Symbol.ToLower())).DocumentNode.InnerText;
            string originalData = originalText.Substring(originalText.IndexOf("\"") + 1);

            if (originalData.Equals("\";"))
                return null;
            else
                return originalData.Split(",")[0];
        } // end method GetStockNameFromWebAsync

        /// <summary>
        /// Retrieve data of strike prices and volumes from the web.
        /// </summary>
        /// <returns>
        /// A jagged 2D array of type "decimal?" of size at least (1, 3) containing the data,
        ///     or a 2D array of size (1, 1) if access is denied by the specified source providing data of strike prices and volumes,
        ///     or a 2D array of size (1, 2) if the date range is too long,
        ///     or <c>null</c> if the filters are wrong.
        /// <br /><br />
        /// The first number of the size represents the number of rows.
        /// The second number of the size represents the number of columns.
        /// For columns, Index 0 represents strike prices, while Index 1 represents total volumes. These two columns should not have <c>null</c> data. The other indexes represent each day's volumes.
        /// <br /><br />
        /// Each "row" of the 2D array will have the same number of elements, so it can be seen as a rectangular one.
        /// The 2D array containing the data is sorted by strike prices in descending order.
        /// </returns>
        public async Task<decimal?[][]> GetStrikePriceVolumeDataFromWebAsync()
        {
            HtmlNode rootNode = await GetStrikePriceVolumeHtmlRootNodeAsync(StartDate, EndDate);
            
            if (rootNode.SelectNodes("//body/div") != null)
            {
                HtmlNodeCollection strikePriceTotalVolumeNodes = rootNode.SelectNodes(_cellNodePath);
                
                if (strikePriceTotalVolumeNodes != null)
                {
                    int dayTotalCount = EndDate.Subtract(StartDate).Days + 1; // Calculate the number of days (>= 1) from the start date to the end date.
                    decimal?[][] strikePriceVolumeRowCollection = new decimal?[strikePriceTotalVolumeNodes.Count][]; // The table of strike prices and volumes should have at least 3 columns (strike price, total volume, and each day's volume).
                    decimal?[] strikePriceVolumeRow;
                    int nodeIndex = 0; // Represent the index of rows (start from 0).
                    int elementIndex; // Represent the index of columns (start from 0).

                    // Get data of strike prices and total volumes.
                    foreach (HtmlNode node in strikePriceTotalVolumeNodes)
                    {
                        elementIndex = 0;
                        strikePriceVolumeRow = new decimal?[2 + dayTotalCount];

                        foreach (HtmlNode element in node.Elements("td"))
                        {
                            // Index 0 represents strike prices, while Index 1 represents total volumes. (Determined by the specified data source.)
                            if (elementIndex < 2)
                            {
                                decimal elementValue = Convert.ToDecimal(element.InnerText);
                                strikePriceVolumeRow[elementIndex++] = elementValue; // Here the unit of total volumes is "股".
                            }
                            else
                                break;
                        } // end foreach

                        strikePriceVolumeRowCollection[nodeIndex++] = strikePriceVolumeRow;
                    } // end foreach

                    if (dayTotalCount > 1)
                    {
                        // Get data of each day's volume.
                        for (int dayCount = 0; dayCount < dayTotalCount; dayCount++)
                        {
                            DateTime dayVolumeDate = Convert.ToDateTime(StartDate.ToShortDateString()).AddDays(dayCount);
                            HtmlNodeCollection strikePriceDayVolumeNodes = (await GetStrikePriceVolumeHtmlRootNodeAsync(dayVolumeDate, dayVolumeDate)).SelectNodes(_cellNodePath);

                            /*
                             * Execute the code block if the specified node collection is not null.
                             * Otherwise, just keep the initial element value (0 of type decimal) of the 2D array containing data of strike prices and volume.
                             */
                            if (strikePriceDayVolumeNodes != null)
                            {
                                nodeIndex = 0;

                                foreach (HtmlNode node in strikePriceDayVolumeNodes)
                                {
                                    elementIndex = 0;
                                    decimal strikePrice = 0m;

                                    foreach (HtmlNode element in node.Elements("td"))
                                    {
                                        // Index 0 represents strike prices. (Determined by the specified data source.)
                                        if (elementIndex == 0)
                                        {
                                            strikePrice = Convert.ToDecimal(element.InnerText);
                                            elementIndex++;
                                        }
                                        // Index 1 represents the specified day's volumes. (Determined by the specified data source.)
                                        else if (elementIndex == 1)
                                        {
                                            for (int nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
                                                if (strikePriceVolumeRowCollection[nodeCount][0] == strikePrice)
                                                {
                                                    strikePriceVolumeRowCollection[nodeCount][2 + dayCount] = Convert.ToDecimal(element.InnerText); // Here the unit of the specified day's volumes is "股".
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
                                for (int nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
                                    strikePriceVolumeRowCollection[nodeCount][2 + dayCount] = null;
                        } // end for
                    }
                    else
                        for (int nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
                            strikePriceVolumeRowCollection[nodeCount][2] = strikePriceVolumeRowCollection[nodeCount][1];

                    // TODO: sorting may not be needed when getting data from the web.
                    // Array.Sort(strikePriceVolumeRowCollection, (x, y) => Comparer<decimal>.Default.Compare((decimal) y[0], (decimal) x[0])); // Sort by strike prices in descending order.
                    
                    return strikePriceVolumeRowCollection;
                }
                // Wrong filters (symbol/start date/end date).
                else
                    return null;
            }
            // Access is denied by the specified data source.
            else if (rootNode.SelectNodes("//body/h1") != null)
                return new decimal?[1][] { new decimal?[1] };
            // The date range is too long.
            else
                return new decimal?[1][] { new decimal?[2] };
        } // end method GetStrikePriceVolumeDataFromWebAsync
    } // end class StrikePriceVolumeDataProcessor
} // end namespace ShSzStockHelper