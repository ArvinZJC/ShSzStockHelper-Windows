/*
 * @Description: the back-end code of the window for showing the info about the product
 * @Version: 1.0.1.20200831
 * @Author: Arvin Zhao
 * @Date: 2020-08-30 19:26:31
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-08-31 19:36:43
 */

using System.Diagnostics;
using System.Windows.Navigation;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the window for showing the info about the product.
    /// </summary>
    public partial class AboutWindow
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="AboutWindow"/> class.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <param name="productVersion">The version of the product.</param>
        /// <param name="productCopyright">The copyright of the product.</param>
        public AboutWindow(string productName, string productVersion, string productCopyright)
        {
            InitializeComponent();

            WindowAbout.Title = Properties.Resources.About + productName;
            TextBlockProductName.Text = productName;
            TextBlockProductVersion.Text = productVersion;
            TextBlockProductCopyright.Text = productCopyright;
        } // end constructor AboutWindow

        #region Control Events
        private void HyperlinkOpenSource_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", e.Uri.AbsoluteUri);
            e.Handled = true;
        } // end method HyperlinkOpenSource_OnRequestNavigate
        #endregion Control Events
    } // end class AboutWindow
} // end namespace ShSzStockHelper.Views