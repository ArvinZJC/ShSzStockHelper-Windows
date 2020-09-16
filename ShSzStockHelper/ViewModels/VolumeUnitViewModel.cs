/*
 * @Description: a view model corresponding to the data model of volume units
 * @Version: 1.0.1.20200916
 * @Author: Arvin Zhao
 * @Date: 2020-09-10 13:06:20
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-16 13:10:33
 */

using ShSzStockHelper.Models;
using System.Collections.ObjectModel;

namespace ShSzStockHelper.ViewModels
{
    /// <summary>
    /// A view model corresponding to <see cref="VolumeUnitViewModel"/>.
    /// </summary>
    internal class VolumeUnitViewModel
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="VolumeUnitViewModel"/> class.
        /// </summary>
        public VolumeUnitViewModel()
        {
            VolumeUnitRecords = new ObservableCollection<VolumeUnitData>
            {
                new VolumeUnitData {Name = Properties.Resources.VolumeUnit_1, Coefficient = 1m},
                new VolumeUnitData {Name = Properties.Resources.VolumeUnit_10, Coefficient = 10m},
                new VolumeUnitData {Name = Properties.Resources.VolumeUnit_100, Coefficient = 100m},
                new VolumeUnitData {Name = Properties.Resources.VolumeUnit_1000, Coefficient = 1000m},
                new VolumeUnitData {Name = Properties.Resources.VolumeUnit_10000, Coefficient = 10000m}
            };
        } // end constructor VolumeUnitViewModel

        /// <summary>
        /// A collection of volume units.
        /// </summary>
        public ObservableCollection<VolumeUnitData> VolumeUnitRecords { get; }
    } // end class VolumeUnitViewModel
} // end namespace ShSzStockHelper.ViewModels