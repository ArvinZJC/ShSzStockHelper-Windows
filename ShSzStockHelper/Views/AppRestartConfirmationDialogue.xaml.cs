/*
 * @Description: the back-end code of the dialogue for confirming the app restart
 * @Version: 1.0.3.20210407
 * @Author: Arvin Zhao
 * @Date: 2020-09-14 10:59:01
 * @Last Editors: Arvin Zhao
 * @LastEditTime: 2021-04-07 11:03:17
 */

using System.Windows;

namespace ShSzStockHelper.Views
{
    /// <summary>
    /// Interaction logic for the dialogue for confirming the app restart.
    /// </summary>
    public partial class AppRestartConfirmationDialogue
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="AppRestartConfirmationDialogue"/> class.
        /// </summary>
        /// <param name="settingName">The name of the setting required the app restart.</param>
        public AppRestartConfirmationDialogue(string settingName)
        {
            InitializeComponent();
            TextBlockAppRestartAlert.Text = settingName + Properties.Resources.TextBlockAppRestartAlert_Text;
        } // end constructor AppRestartConfirmationDialogue

        #region Control Events
        // Return false when the cancellation button is clicked.
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        } // end method ButtonCancel_OnClick

        // Return true when the confirmation button is clicked.
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        } // end method ButtonOk_OnClick
        #endregion Control Events
    } // end class AppRestartConfirmationDialogue
} // end namespace ShSzStockHelper.Views