/*
 * @Description: the back-end code of initialising the app
 * @Version: 1.0.9.20201216
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-12-16 14:14:55
 */

using Bluegrams.Application;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private readonly string _productId = Assembly.GetExecutingAssembly().GetName().Name;
        private static string _productName = ShSzStockHelper.Properties.Resources.NullProductNameError;
        private static string _productVersion = ShSzStockHelper.Properties.Resources.NullProductVersionError;
        private static string _productCopyright = ShSzStockHelper.Properties.Resources.NullProductCopyrightError;
        private Mutex _mutex; // It is important to declare the mutex here. Otherwise, it may have no effect.

        /// <summary>
        /// Initialise a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("MzI2OTM0QDMxMzgyZTMzMmUzMGFyTEY5ai9VTGlKRXcrdlpLSjU5VUlHR1ZZZzkxeDlBYzdkMHMvY0d0LzA9"); // Register a Syncfusion license.

            string productCompany = null;
            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var assemblyCopyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();

                _productName = fileVersionInfo.ProductName;

                if (assemblyVersionAttribute != null)
                    _productVersion = assemblyVersionAttribute.InformationalVersion;

                productCompany = fileVersionInfo.CompanyName;

                if (assemblyCopyrightAttribute != null)
                    _productCopyright = assemblyCopyrightAttribute.Copyright;
            } // end if

            // Configure the customised settings provider.
            PortableSettingsProvider.SettingsFileName = "user.config";

            if (_productId != null && productCompany != null)
            {
                // ReSharper disable once RedundantAssignment
                var customisedSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), productCompany, _productId);

#if DEBUG
                customisedSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), productCompany, _productId + "_debug");
#endif

                if (!Directory.Exists(customisedSettingsDirectory))
                    Directory.CreateDirectory(customisedSettingsDirectory);

                PortableSettingsProviderBase.SettingsDirectory = customisedSettingsDirectory;
            } // end if

            PortableSettingsProvider.ApplyProvider(ShSzStockHelper.Properties.Settings.Default);

            if (ShSzStockHelper.Properties.Settings.Default.ProductTheme == (int) VisualStyles.MaterialDark)
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialDark/Theme.xaml", UriKind.Relative)));
            else
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialLight/Theme.xaml", UriKind.Relative)));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ShSzStockHelper.Properties.Settings.Default.CultureInfo); // It is necessary to specify the culture info here and in the name of the resource file "Syncfusion.Tools.Wpf" to avoid the issue that some text of the tab control is not displayed in simplified Chinese.
        } // end constructor App

        ~App()
        {
            _mutex.Dispose();
        } // end destructor App

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, _productId);

            // Allow running only 1 instance of the product.
            if (_mutex.WaitOne(0, false))
                base.OnStartup(e);
            else
                Shutdown();
        } // end method OnStartUp

        /// <summary>
        /// Get the name of the product.
        /// </summary>
        /// <returns>The name of the product.</returns>
        public static string GetProductName()
        {
            return _productName;
        } // end method GetProductName

        /// <summary>
        /// Get the version of the product.
        /// </summary>
        /// <returns>The version of the product.</returns>
        public static string GetProductVersion()
        {
            return _productVersion;
        } // end method GetProductName

        /// <summary>
        /// Get the copyright of the product.
        /// </summary>
        /// <returns>The copyright of the product.</returns>
        public static string GetProductCopyright()
        {
            return _productCopyright;
        } // end method GetProductName
    } // end class App
} // end namespace ShSzStockHelper