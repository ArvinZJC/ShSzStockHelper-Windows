/*
 * @Description: a data model of volume units
 * @Version: 1.0.0.20200910
 * @Author: Arvin Zhao
 * @Date: 2020-09-10 12:58:25
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-10 13:05:21
 */

namespace ShSzStockHelper.Models
{
    /// <summary>
    /// A data model of volume units.
    /// </summary>
    internal class VolumeUnitData
    {
        /// <summary>
        /// The display name of a volume unit.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { get; set; }

        /// <summary>
        /// The coefficient of a volume unit.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal Coefficient { get; set; }
    } // end class VolumeUnitData
} // end namespace ShSzStockHelper.Models