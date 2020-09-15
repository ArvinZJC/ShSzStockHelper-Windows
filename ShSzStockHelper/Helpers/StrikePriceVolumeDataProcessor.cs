/*
 * @Description: a data processor to process data of strike prices and volumes collected
 * @Version: 1.1.4.20200904
 * @Author: Arvin Zhao
 * @Date: 2020-07-15 18:25:42
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-04 19:25:42
 */

using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShSzStockHelper.Helpers
{
    /// <summary>
    /// A data processor to process data of strike prices and volumes collected.
    /// </summary>
    internal class StrikePriceVolumeDataProcessor
    {
        private readonly HtmlWeb _htmlWeb;
        private const string CellNodePath = "//tbody/tr";

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
                .LoadFromWebAsync(@"http://market.finance.sina.com.cn/pricehis.php?symbol=" + Symbol.ToLower() + "&startdate=" + startDate.ToString(Properties.Settings.Default.DateCodeFormat) + "&enddate=" + endDate.ToString(Properties.Settings.Default.DateCodeFormat)))
                .DocumentNode;
        } // end method GetStrikePriceVolumeHtmlRootNodeAsync

        /// <summary>
        /// Retrieve the name of a stock from the web.
        /// </summary>
        /// <returns>The name of a stock, or <c>null</c> if it seems to be no internet connection or no matching result is found on the web.</returns>
        public async Task<string> GetStockNameFromWebAsync()
        {
            try
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
                var originalText = (await _htmlWeb.LoadFromWebAsync(@"http://hq.sinajs.cn/list=" + Symbol.ToLower())).DocumentNode.InnerText;
                var originalData = originalText.Substring(originalText.IndexOf("\"", StringComparison.Ordinal) + 1);

                return originalData.Equals("\";") ? null : originalData.Split(",")[0];
            }
            catch (HttpRequestException)
            {
                return null;
            } // end try...catch
        } // end method GetStockNameFromWebAsync

        /// <summary>
        /// Retrieve data of strike prices and volumes from the web.
        /// </summary>
        /// <returns>
        /// A jagged array of type "decimal?" which has 1 or more array elements, and each one has at least 3 elements containing the required data;<br />
        /// OR a jagged array of type "decimal?" which has only 1 array element:<br />
        ///     * 2 elements in the array element if the date range is too long,<br />
        ///     * OR 1 element in the array element if access is denied by the specified source providing data of strike prices and volumes;<br />
        ///     * OR <c>null</c> as the array element if the filters are wrong.<br />
        /// OR <c>null</c> if it seems to be no internet connection.
        /// <br /><br />
        /// For the jagged array with at least 1 array element, each array element contains the data for the rows of a table.
        /// These array elements are in the same size. The reason for not using a rectangular 2D array is to make the sorting simpler.
        /// Each element in an array element is the data for 1 column of a table.
        /// For columns, Index 0 represents strike prices, while Index 1 represents total volumes. These two columns should not have <c>null</c> data.
        /// The other indexes represent each day's volumes.
        /// The array elements is sorted by strike prices in descending order.
        /// </returns>
        public async Task<decimal?[][]> GetStrikePriceVolumeDataFromWebAsync()
        {
            try
            {
                var rootNode = await GetStrikePriceVolumeHtmlRootNodeAsync(StartDate, EndDate);

                // The date range is too long, or access is denied by the specified data source.
                if (rootNode.SelectNodes("//body/div") == null)
                    return rootNode.SelectNodes("//body/h1") == null ? new[] {new decimal?[2]} : new[] {new decimal?[1]};

                var strikePriceTotalVolumeNodes = rootNode.SelectNodes(CellNodePath);

                // Wrong filters (symbol/start date/end date).
                if (strikePriceTotalVolumeNodes == null)
                    return new decimal?[][] {null};

                var dayTotalCount = EndDate.Subtract(StartDate).Days + 1; // Calculate the number of days (>= 1) from the start date to the end date.
                var strikePriceVolumeRowCollection = new decimal?[strikePriceTotalVolumeNodes.Count][]; // The table of strike prices and volumes should have at least 3 columns (strike price, total volume, and each day's volume).
                var nodeIndex = 0; // Represent the index of rows (start from 0).
                int elementIndex; // Represent the index of columns (start from 0).

                // Get data of strike prices and total volumes.
                foreach (var node in strikePriceTotalVolumeNodes)
                {
                    elementIndex = 0;

                    var strikePriceVolumeRow = new decimal?[2 + dayTotalCount];

                    foreach (var element in node.Elements("td"))
                    {
                        // Index 0 represents strike prices, while Index 1 represents total volumes. (Determined by the specified data source.)
                        if (elementIndex < 2)
                        {
                            var elementValue = Convert.ToDecimal(element.InnerText);
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
                    for (var dayCount = 0; dayCount < dayTotalCount; dayCount++)
                    {
                        var dayVolumeDate = Convert.ToDateTime(StartDate.ToShortDateString()).AddDays(dayCount);
                        var strikePriceDayVolumeNodes = (await GetStrikePriceVolumeHtmlRootNodeAsync(dayVolumeDate, dayVolumeDate)).SelectNodes(CellNodePath);

                        /*
                         * Execute the code block if the specified node collection is not null.
                         * Otherwise, just keep the initial element value (0 of type decimal) of the 2D array containing data of strike prices and volume.
                         */
                        if (strikePriceDayVolumeNodes != null)
                        {
                            nodeIndex = 0;

                            foreach (var node in strikePriceDayVolumeNodes)
                            {
                                elementIndex = 0;
                                var strikePrice = 0m;

                                foreach (var element in node.Elements("td"))
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
                                        for (var nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
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
                            for (var nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
                                strikePriceVolumeRowCollection[nodeCount][2 + dayCount] = null;
                    } // end for
                }
                else
                    for (var nodeCount = 0; nodeCount < strikePriceTotalVolumeNodes.Count; nodeCount++)
                        strikePriceVolumeRowCollection[nodeCount][2] = strikePriceVolumeRowCollection[nodeCount][1];

                // TODO: sorting may not be needed when getting data from the web.
                // Array.Sort(strikePriceVolumeRowCollection, (x, y) => Comparer<decimal>.Default.Compare((decimal) y[0], (decimal) x[0])); // Sort by strike prices in descending order.
                        
                return strikePriceVolumeRowCollection;
            }
            catch (HttpRequestException)
            {
                return null;
            } // end try...catch
        } // end method GetStrikePriceVolumeDataFromWebAsync
    } // end class StrikePriceVolumeDataProcessor
} // end namespace ShSzStockHelper.Helpers