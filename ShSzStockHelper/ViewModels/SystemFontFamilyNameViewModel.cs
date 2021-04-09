/*
 * @Description: a view model to load data of the system's font family names
 * @Version: 1.0.1.20210407
 * @Author: Arvin Zhao
 * @Date: 2020-09-04 00:26:31
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-07 01:19:34
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace ShSzStockHelper.ViewModels
{
    /// <summary>
    /// A view model to load data of the system's font family names.
    /// </summary>
    public class SystemFontFamilyNameViewModel
    {
        /// <summary>
        /// A collection of the system's font family names.
        /// </summary>
        public ObservableCollection<string> SystemFontFamilyNameRecords { get; private set; }

        /// <summary>
        /// Load data of the system's font family names.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the work.</returns>
        public async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                var systemFontFamilyNameList = new List<string>();

                foreach (var systemFontFamily in Fonts.SystemFontFamilies)
                {
                    var systemFontFamilyCultureNames = systemFontFamily.FamilyNames;

                    if (systemFontFamilyCultureNames.ContainsKey(XmlLanguage.GetLanguage("zh-CN")))
                        systemFontFamilyNameList.Add(systemFontFamilyCultureNames.TryGetValue(XmlLanguage.GetLanguage("zh-CN"), out var systemFontFamilyName)
                            ? systemFontFamilyName
                            : systemFontFamily.Source);
                    else
                        systemFontFamilyNameList.Add(systemFontFamilyCultureNames.TryGetValue(XmlLanguage.GetLanguage("en-US"), out var systemFontFamilyName)
                            ? systemFontFamilyName
                            : systemFontFamily.Source);
                } // end foreach

                systemFontFamilyNameList.Sort();
                SystemFontFamilyNameRecords = new ObservableCollection<string>(systemFontFamilyNameList);
            });
        } // end method LoadDataAsync
    } // end class SystemFontFamilyNameViewModel
} // end namespace ShSzStockHelper.ViewModels