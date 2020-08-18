/*
 * @Description: a view model corresponding to the data model of stocks' symbols and corresponding names
 * @Version: 1.0.4.20200817
 * @Author: Arvin Zhao
 * @Date: 2020-08-13 18:34:25
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-17 18:38:01
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShSzStockHelper
{
    /// <summary>
    /// A view model corresponding to <see cref="StockSymbolNameData"/>.
    /// </summary>
    public partial class StockSymbolNameViewModel
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="StockSymbolNameData"/> class.
        /// </summary>
        public StockSymbolNameViewModel()
        {
            StockSymbolNameRecords = new ObservableCollection<StockSymbolNameData>();
        } // end constructor StockSymbolNameViewModel

        /// <summary>
        /// A collection of stocks' symbols and corresponding names.
        /// </summary>
        public ObservableCollection<StockSymbolNameData> StockSymbolNameRecords { get; }

        /// <summary>
        /// Load data of stocks' symbols and names from the specified JSON file.
        /// </summary>
        public async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                Stream stream = Application.GetResourceStream(new Uri("/Resources/StockSymbolNameData.json", UriKind.Relative)).Stream;
                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer);

                JArray jsonArray = (JArray)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

                foreach (JToken jsonToken in jsonArray)
                {
                    string[] tsCode = jsonToken["ts_code"].ToString().Split("."); // Split the original value of the key "ts_code" (e.g., "601006.SZ" => {"601006", "SZ"}).

                    StockSymbolNameRecords.Add(new StockSymbolNameData(tsCode[1] + tsCode[0], jsonToken["name"].ToString()));
                } // end foreach
            });
        } // end method LoadData
    } // end class StockSymbolNameViewModel
} // end namespace ShSzStockHelper