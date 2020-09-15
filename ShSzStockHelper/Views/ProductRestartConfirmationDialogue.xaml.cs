/*
 * @Description: the back-end code of the dialogue for confirming the product restart
 * @Version: 1.0.1.20200915
 * @Author: Arvin Zhao
 * @Date: 2020-09-14 10:59:01
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2020-09-15 11:03:17
 */

using System.Windows;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the dialogue for confirming the product restart.
    /// </summary>
    public partial class ProductRestartConfirmationDialogue
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="ProductRestartConfirmationDialogue"/> class.
        /// </summary>
        public ProductRestartConfirmationDialogue()
        {
            InitializeComponent();
        } // end constructor ProductRestartConfirmationDialogue

        #region Control Events
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        } // end method ButtonCancel_OnClick

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        } // end method ButtonOk_OnClick
        #endregion Control Events
    } // end class ProductRestartConfirmationDialogue
} // end namespace ShSzStockHelper.Views