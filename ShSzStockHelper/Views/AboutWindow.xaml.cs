/*
 * @Description: the back-end code of the window for showing the info about the product
 * @Version: 1.0.2.20201130
 * @Author: Arvin Zhao
 * @Date: 2020-08-30 19:26:31
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-11-30 19:36:43
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
        public AboutWindow()
        {
            InitializeComponent();

            var productName = App.GetProductName();

            WindowAbout.Title = Properties.Resources.About + productName;
            TextBlockProductName.Text = productName;
            TextBlockProductVersion.Text = App.GetProductVersion();
            TextBlockProductCopyright.Text = App.GetProductCopyright();
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