/*
 * @Description: a view model corresponding to the data model of stocks' symbols and corresponding names
 * @Version: 1.0.8.20200903
 * @Author: Arvin Zhao
 * @Date: 2020-08-13 18:34:25
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-03 18:38:01
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShSzStockHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShSzStockHelper.ViewModels
{
    /// <summary>
    /// A view model corresponding to <see cref="StockSymbolNameData"/>.
    /// </summary>
    public class StockSymbolNameViewModel
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
        /// <returns>A <see cref="Task"/> object that represents the work.</returns>
        public async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                var stream = Application.GetResourceStream(new Uri("/Resources/StockSymbolNameData.json", UriKind.Relative))?.Stream;

                if (stream == null)
                    return;

                var buffer = new byte[stream.Length];

                stream.Read(buffer);

                var jsonArray = (JArray)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

                if (jsonArray == null)
                    return;

                foreach (var jsonToken in jsonArray)
                {
                    var tsCode = jsonToken["ts_code"]?.ToString().Split("."); // Split the original value of the key "ts_code" (e.g., "601006.SZ" => {"601006", "SZ"}).

                    if (tsCode != null)
                        StockSymbolNameRecords.Add(new StockSymbolNameData()
                            {
                                Symbol = tsCode[1] + tsCode[0],
                                Name = jsonToken["name"]?.ToString()
                            }
                        );
                } // end foreach
            });
        } // end method LoadData
    } // end class StockSymbolNameViewModel
} // end namespace ShSzStockHelper.ViewModels