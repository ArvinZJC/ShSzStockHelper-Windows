/*
 * @Description: the back-end code of initialising the app
 * @Version: 1.0.2.20200810
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-10 14:14:55
 */

using Syncfusion.Licensing;
using System.Windows;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for initialising the app.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Mjg5NjU2QDMxMzgyZTMyMmUzMEMwbk5LbjVFbW5VU1ZqUDFmbkF5eXF6dURJVkZ5RDgyNjVnMXF2d0h1c0k9"); // Register Syncfusion license.
        } // end constructor App
    } // end class App
} // end namespace ShSzStockHelper