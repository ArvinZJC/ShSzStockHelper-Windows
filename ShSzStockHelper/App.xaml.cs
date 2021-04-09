/*
 * @Description: the back-end code of initialising the app
 * @Version: 1.1.0.20210409
 * @Author: Arvin Zhao
 * @Date: 2020-07-08 10:17:48
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-09 14:14:55
 */

using Bluegrams.Application;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.MaterialDark.WPF;
using Syncfusion.Themes.MaterialLight.WPF;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ShSzStockHelper
{
    /// <summary>
    /// Interaction logic for initialising the app.
    /// </summary>
    public partial class App : IDisposable
    {
        private static string _productCompany;
        private static string _productCopyright = ShSzStockHelper.Properties.Resources.NullProductCopyrightError;
        private static string _productName = ShSzStockHelper.Properties.Resources.NullProductNameError;
        private static string _productVersion = ShSzStockHelper.Properties.Resources.NullProductVersionError;
        private string _productId = Assembly.GetExecutingAssembly().GetName().Name;
        private Mutex _mutex; // It is important to declare the mutex here. Otherwise, it may have no effect to control running only 1 app instance.

        /// <summary>
        /// Initialise a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("NDI2NzY5QDMxMzkyZTMxMmUzMFY3Z3hHcE9WR1JzVWRoUVZldVVOYXNkL3JJTzBmQXV2ajh5b295bXRtT1k9"); // Register a Syncfusion (V19.1.0.55) license.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ShSzStockHelper.Properties.Settings.Default.CultureInfo); // It is necessary to specify the culture info here and in the name of the resource file "Syncfusion.Tools.Wpf" to avoid the issue that some text of the tab control is not displayed in simplified Chinese.
            LoadProductInfo();
            ConfigSettingsProvider();
            ApplyTheme();
        } // end constructor App

        ~App()
        {
            _mutex.Dispose();
        } // end destructor App

        public void Dispose()
        {
            _mutex.Dispose();
        } // end method Dispose

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, _productId);

            // Allow running only 1 app instance.
            if (_mutex.WaitOne(0, false))
                base.OnStartup(e);
            else
                Shutdown();
        } // end method OnStartUp

        #region Public Methods
        /// <summary>
        /// Get the product's copyright.<br />
        /// It is feasible to use this method to get a correct value if the app runs properly because the app component starts first and ends last.
        /// </summary>
        /// <returns>The copyright of the product.</returns>
        public static string GetProductCopyright()
        {
            return _productCopyright;
        } // end method GetProductCopyright

        /// <summary>
        /// Get the product's name.<br />
        /// It is feasible to use this method to get a correct value if the app runs properly because the app component starts first and ends last.
        /// </summary>
        /// <returns>The name of the product.</returns>
        public static string GetProductName()
        {
            return _productName;
        } // end method GetProductName

        /// <summary>
        /// Get the product's version.<br />
        /// It is feasible to use this method to get a correct value if the app runs properly because the app component starts first and ends last.
        /// </summary>
        /// <returns>The version of the product.</returns>
        public static string GetProductVersion()
        {
            return _productVersion;
        } // end method GetProductVersion
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Apply corresponding theme settings.
        /// </summary>
        private static void ApplyTheme()
        {
            if (ShSzStockHelper.Properties.Settings.Default.AppTheme == (int) VisualStyles.MaterialDark)
            {
                var darkThemeSettings = new MaterialDarkThemeSettings()
                {
                    BodyAltFontSize = ShSzStockHelper.Properties.Settings.Default.ContentTextFontSize,
                    BodyFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize,
                    FontFamily = new FontFamily(ShSzStockHelper.Properties.Settings.Default.DisplayFontFamilyName),
                    SubTitleFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize,
                    TitleFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize
                };
                SfSkinManager.RegisterThemeSettings("MaterialDark", darkThemeSettings);
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialDark.xaml", UriKind.Relative)));
            }
            else
            {
                var lightThemeSettings = new MaterialLightThemeSettings()
                {
                    BodyAltFontSize = ShSzStockHelper.Properties.Settings.Default.ContentTextFontSize,
                    BodyFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize,
                    FontFamily = new FontFamily(ShSzStockHelper.Properties.Settings.Default.DisplayFontFamilyName),
                    SubTitleFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize,
                    TitleFontSize = ShSzStockHelper.Properties.Settings.Default.PrimaryTextFontSize
                };
                SfSkinManager.RegisterThemeSettings("MaterialLight", lightThemeSettings);
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)LoadComponent(new Uri("Resources/MaterialLight.xaml", UriKind.Relative)));
            } // end if...else
        } // end method ApplyTheme

        /// <summary>
        /// Configure the customised settings provider.
        /// </summary>
        private void ConfigSettingsProvider()
        {
            PortableSettingsProvider.SettingsFileName = "user.config";

            if (_productId != null && _productCompany != null)
            {
                // ReSharper disable once RedundantAssignment
                var customisedSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _productCompany, _productId);

#if DEBUG
                _productId += "_debug";
                customisedSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _productCompany, _productId);
#endif

                if (!Directory.Exists(customisedSettingsDirectory))
                    Directory.CreateDirectory(customisedSettingsDirectory);

                PortableSettingsProviderBase.SettingsDirectory = customisedSettingsDirectory;
            } // end if

            PortableSettingsProvider.ApplyProvider(ShSzStockHelper.Properties.Settings.Default);
        } // end method ConfigSettingsProvider

        /// <summary>
        /// Load the product's company, copyright, name, and version.
        /// </summary>
        private static void LoadProductInfo()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                return;

            var assemblyCopyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location); // Get the package version defined in "Properties\Package\Package version".

            _productCompany = fileVersionInfo.CompanyName;
            _productName = fileVersionInfo.ProductName;

            if (assemblyCopyrightAttribute != null)
                _productCopyright = assemblyCopyrightAttribute.Copyright;

            if (assemblyVersionAttribute != null)
                _productVersion = assemblyVersionAttribute.InformationalVersion;
        } // end method LoadProductInfo
        #endregion Private Methods
    } // end class App
} // end namespace ShSzStockHelper