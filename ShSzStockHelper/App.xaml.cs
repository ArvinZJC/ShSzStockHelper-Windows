/*
 * @Description: the back-end code of initialising the app
 * @Version: 1.0.5.20200914
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-14 14:14:55
 */

using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for initialising the app.
    /// </summary>
    public partial class App
    {
        private static Mutex _mutex; // It is important to declare the mutex here. Otherwise, it may have no effect.

        /// <summary>
        /// Initialise a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Mjg5NjU2QDMxMzgyZTMyMmUzMEMwbk5LbjVFbW5VU1ZqUDFmbkF5eXF6dURJVkZ5RDgyNjVnMXF2d0h1c0k9"); // Register Syncfusion license.

            if (ShSzStockHelper.Properties.Settings.Default.ProductTheme == (int) VisualStyles.MaterialDark)
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialDark/Theme.xaml", UriKind.Relative)));
            else
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialLight/Theme.xaml", UriKind.Relative)));
        } // end constructor App

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name);

            // Allow running only 1 instance of the product.
            if (_mutex.WaitOne(0, false))
                base.OnStartup(e);
            else
                Shutdown();
        } // end method OnStartUp
    } // end class App
} // end namespace ShSzStockHelper