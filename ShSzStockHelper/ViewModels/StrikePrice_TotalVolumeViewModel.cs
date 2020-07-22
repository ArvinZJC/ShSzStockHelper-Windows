/*
 * @Description: a view model corresponding to the data model of strike prices and total volumes
 * @Version: 1.0.1.20200715
 * @Author: Arvin Zhao
 * @Date: 2020-07-09 12:34:20
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-07-15 14:11:25
 */

using System.Collections.ObjectModel;

namespace ShSzStockHelper
{
    /// <summary>
    /// A view model corresponding to <see cref="StrikePrice_TotalVolumeData"/>.
    /// </summary>
    class StrikePrice_TotalVolumeViewModel
    {
        public StrikePrice_TotalVolumeViewModel()
        {
            StrikePrice_TotalVolumeRecords = new ObservableCollection<StrikePrice_TotalVolumeData>();
        } // end constructor StrikePrice_TotalVolumeViewModel

        /// <summary>
        /// A collection of strike prices and total volumes.
        /// </summary>
        public ObservableCollection<StrikePrice_TotalVolumeData> StrikePrice_TotalVolumeRecords { get; set; }
    } // end class StrikePrice_TotalVolumeViewModel
} // end namespace ShSzStockHelper