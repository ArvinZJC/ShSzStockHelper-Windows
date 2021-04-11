/*
 * @Description: a data processor to process data of strike prices and volumes collected
 * @Version: 1.2.0.20210411
 * @Author: Arvin Zhao
 * @Date: 2020-07-15 18:25:42
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-11 09:25:42
 */

using HtmlAgilityPack;
using System;
using System.Globalization;
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
        /// <summary>
        /// A stock symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The query's start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The query's end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The XPath expression for checking if access is denied by the specified data source.
        /// </summary>
        private const string AccessDenied = "//body/h1";

        /// <summary>
        /// The XPath expression for selecting data nodes.
        /// </summary>
        private const string Data = "//tbody/tr";

        /// <summary>
        /// The name of the data nodes' child nodes containing a row of data.
        /// </summary>
        private const string DataRow = "td";

        /// <summary>
        /// The XPath expression for checking if the date range is too long.
        /// </summary>
        private const string DateRangeTooLong = "//body/div";

        private readonly HtmlWeb _htmlWeb;

        /// <summary>
        /// A collection storing data of strike prices and volumes by row (record).
        /// </summary>
        private decimal?[][] _strikePriceVolumeRowCollection;

        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceVolumeDataProcessor"/> class.
        /// </summary>
        public StrikePriceVolumeDataProcessor()
        {
            _htmlWeb = new HtmlWeb();
        } // end constructor StrikePriceVolumeDataProcessor

        #region Public Methods
        /// <summary>
        /// Retrieve a stock name from the web.
        /// </summary>
        /// <returns>A retrieved stock name, or <c>null</c> if it seems to be no internet connection or no matching result is found on the web.</returns>
        public async Task<string> GetStockNameFromWebAsync()
        {
            try
            {
                /*
                 * The code is to avoid throwing the exception "System.InvalidOperationException: 'The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set.'".
                 * The inner exception is "ArgumentException: 'GB18030' is not a supported encoding name.".
                 */
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                /*
                 * You can refer to the following websites for sample original text.
                 * http://hq.sinajs.cn/list=sh601006
                 * http://hq.sinajs.cn/list=sh6010065
                 */
                var originalText = (await _htmlWeb.LoadFromWebAsync(@"http://hq.sinajs.cn/list=" + Symbol.ToLower(CultureInfo.InvariantCulture))).DocumentNode.InnerText;
                var originalData = originalText[(originalText.IndexOf("\"", StringComparison.Ordinal) + 1)..];
                return originalData.Equals("\";")
                    ? null // No matching result is found on the web.
                    : originalData.Split(',')[0]; // A retrieved stock name.
            }
            catch (HttpRequestException)
            {
                return null; // It seems to be no internet connection.
            } // end try...catch
        } // end method GetStockNameFromWebAsync

        /// <summary>
        /// Retrieve data of strike prices and volumes from the web.
        /// </summary>
        /// <returns>
        /// A jagged array of type "decimal?" which has 1 or more array elements in the same size, and:<br />
        /// * each array element representing a row of data has at least 3 elements for the strike price, total volume, and day volume columns;<br />
        /// * the array elements are sorted by strike prices in descending order;<br />
        /// * the strike price (Index 0) and total volume (Index 1) columns should NOT have <c>null</c> data;<br />
        /// * all values of the 1st day volume column (Index 2) are -1 if the specified data source partially denies access;<br />
        /// * the reason for NOT using a rectangular 2D array is to make the sorting simpler;<br /><br />
        /// 
        /// OR a jagged array of type "decimal?" which has only 1 array element, and this array element is/has:<br />
        /// * <c>null</c> if the filters are wrong;<br />
        /// * 1 element if the specified data source denies access;<br />
        /// * 2 elements if the date range is too long;<br /><br />
        /// 
        /// OR <c>null</c> if it seems to be no internet connection.<br /><br />
        /// </returns>
        public async Task<decimal?[][]> GetStrikePriceVolumeDataFromWebAsync()
        {
            try
            {
                var rootNode = await GetStrikePriceVolumeHtmlRootNodeAsync(StartDate, EndDate);

                // The specified data source denies access.
                if (rootNode.SelectNodes(AccessDenied) != null)
                    return new[] { new decimal?[1] };

                // The date range is too long.
                if (rootNode.SelectNodes(DateRangeTooLong) == null)
                    return new[] { new decimal?[2] };

                var strikePriceVolumeNodes = rootNode.SelectNodes(Data); // Data nodes.

                // Wrong filters (symbol/start date/end date).
                if (strikePriceVolumeNodes == null)
                    return new decimal?[][] { null };

                _strikePriceVolumeRowCollection = new decimal?[strikePriceVolumeNodes.Count][];
                var dayTotalCount = EndDate.Subtract(StartDate).Days + 1; // Calculate the number of days (>= 1) from the start date to the end date.
                GetStrikePriceTotalVolumeRows(strikePriceVolumeNodes, dayTotalCount);
                await GetDayVolumeRowsAsync(strikePriceVolumeNodes, dayTotalCount);
                return _strikePriceVolumeRowCollection;
            }
            catch (HttpRequestException)
            {
                return null; // It seems to be no internet connection.
            } // end try...catch
        } // end method GetStrikePriceVolumeDataFromWebAsync
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Store day volume data into a specified collection.
        /// </summary>
        /// <param name="strikePriceVolumeNodes">The data nodes.</param>
        /// <param name="dayTotalCount">The total number of days in the date range.</param>
        /// <returns>A <see cref="Task"/> object.</returns>
        private async Task GetDayVolumeRowsAsync(HtmlNodeCollection strikePriceVolumeNodes, int dayTotalCount)
        {
            // Day volumes equal total volumes if the date range is 1 day.
            if (dayTotalCount == 1)
            {
                for (var nodeCount = 0; nodeCount < strikePriceVolumeNodes.Count; nodeCount++)
                    _strikePriceVolumeRowCollection[nodeCount][2] = _strikePriceVolumeRowCollection[nodeCount][1];

                return;
            } // end if

            // Get columns of day volumes.
            for (var dayCount = 0; dayCount < dayTotalCount; dayCount++)
            {
                var dayVolumeDate = Convert.ToDateTime(StartDate.ToShortDateString()).AddDays(dayCount);
                var rootNode = await GetStrikePriceVolumeHtmlRootNodeAsync(dayVolumeDate, dayVolumeDate);

                // The specified data source denies access (access can be seen as partially denied for the entire search task).
                if (rootNode.SelectNodes(AccessDenied) != null)
                {
                    for (var nodeCount = 0; nodeCount < strikePriceVolumeNodes.Count; nodeCount++)
                        _strikePriceVolumeRowCollection[nodeCount][2] = -1;

                    return;
                } // end if

                var strikePriceDayVolumeNodes = rootNode.SelectNodes(Data);

                // Wrong filters (in this case, it should only occur when a specified stock market is closed on a specified day).
                if (strikePriceDayVolumeNodes == null)
                    continue;

                // Get a column of day volumes (rows of a specified day's volumes).
                foreach (var node in strikePriceDayVolumeNodes)
                {
                    var elementIndex = 0; // Represent the index of columns (start from 0).
                    var strikePrice = 0m;

                    // Get a row of a specified day's volumes.
                    foreach (var element in node.Elements(DataRow))
                    {
                        // Index 0 represents strike prices (determined by the specified data source).
                        if (elementIndex == 0)
                        {
                            strikePrice = Convert.ToDecimal(element.InnerText);
                            elementIndex++;
                            continue;
                        } // end if

                        // Index 1 represents total volumes (determined by the specified data source). They equal a specified day's volumes because each query's date range is 1 day.
                        for (var nodeCount = 0; nodeCount < strikePriceVolumeNodes.Count; nodeCount++)
                            // Store a row of a specified day's volumes as per its strike price.
                            if (_strikePriceVolumeRowCollection[nodeCount][0] == strikePrice)
                            {
                                _strikePriceVolumeRowCollection[nodeCount][2 + dayCount] = Convert.ToDecimal(element.InnerText); // Here the unit of a specified day's volumes is "股".
                                break;
                            } // end if

                        break; // Only Indexes 0 and 1 are needed to process.
                    } // end foreach
                } // end foreach
            } // end for
        } // end method GetDayVolumeRowsAsync

        /// <summary>
        /// Store strike price and total volume data into a specified collection.
        /// </summary>
        /// <param name="strikePriceVolumeNodes">The data nodes.</param>
        /// <param name="dayTotalCount">The total number of days in the date range.</param>
        private void GetStrikePriceTotalVolumeRows(HtmlNodeCollection strikePriceVolumeNodes, int dayTotalCount)
        {
            var nodeIndex = 0; // Represent the index of rows (start from 0).

            // Get rows of strike prices and total volumes.
            foreach (var node in strikePriceVolumeNodes)
            {
                var elementIndex = 0; // Represent the index of columns (start from 0).
                var strikePriceVolumeRow = new decimal?[2 + dayTotalCount]; // The size includes the number of days to store day volumes later.

                // Get a row of strike prices and total volumes.
                foreach (var element in node.Elements(DataRow))
                {
                    var elementValue = Convert.ToDecimal(element.InnerText);
                    strikePriceVolumeRow[elementIndex++] = elementValue; // Here the unit of total volumes is "股".

                    // Index 0 represents strike prices, while Index 1 represents total volumes (determined by the specified data source). Only these two are needed to process.
                    if (elementIndex == 2)
                        break;
                } // end foreach

                _strikePriceVolumeRowCollection[nodeIndex++] = strikePriceVolumeRow;
            } // end foreach
        } // end method GetStrikePriceTotalVolumeRows

        /// <summary>
        /// Get the HTML document's root node from the specified data source.
        /// </summary>
        /// <param name="startDate">The query's start date.</param>
        /// <param name="endDate">The query's end date.</param>
        /// <returns>An <see cref="HtmlNode"/> object containing the root node.</returns>
        private async Task<HtmlNode> GetStrikePriceVolumeHtmlRootNodeAsync(DateTime startDate, DateTime endDate)
        {
            return (await _htmlWeb
                    .LoadFromWebAsync(@"http://market.finance.sina.com.cn/pricehis.php?symbol=" + Symbol.ToLower(CultureInfo.InvariantCulture) + "&startdate=" + startDate.ToString(Properties.Settings.Default.DateCodeFormat) + "&enddate=" + endDate.ToString(Properties.Settings.Default.DateCodeFormat)))
                .DocumentNode; // Sample URL: http://market.finance.sina.com.cn/pricehis.php?symbol=sh600519&startdate=2016-10-28&enddate=2016-10-28
        } // end method GetStrikePriceVolumeHtmlRootNodeAsync
        #endregion Private Methods
    } // end class StrikePriceVolumeDataProcessor
} // end namespace ShSzStockHelper.Helpers