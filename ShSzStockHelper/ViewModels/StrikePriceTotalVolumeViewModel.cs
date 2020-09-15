/*
 * @Description: a view model corresponding to the data model of strike prices and total volumes
 * @Version: 1.0.4.20200829
 * @Author: Arvin Zhao
 * @Date: 2020-07-09 12:34:20
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-29 14:11:25
 */

using ShSzStockHelper.Models;
using System.Collections.ObjectModel;

namespace ShSzStockHelper.ViewModels
{
    /// <summary>
    /// A view model corresponding to <see cref="StrikePriceTotalVolumeData"/>.
    /// </summary>
    internal class StrikePriceTotalVolumeViewModel
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="StrikePriceTotalVolumeViewModel"/> class.
        /// </summary>
        public StrikePriceTotalVolumeViewModel()
        {
            StrikePriceTotalVolumeRecords = new ObservableCollection<StrikePriceTotalVolumeData>();
        } // end constructor StrikePrice_TotalVolumeViewModel

        /// <summary>
        /// A collection of strike prices and total volumes.
        /// </summary>
        public ObservableCollection<StrikePriceTotalVolumeData> StrikePriceTotalVolumeRecords { get; }
    } // end class StrikePriceTotalVolumeViewModel
} // end namespace ShSzStockHelper.ViewModels