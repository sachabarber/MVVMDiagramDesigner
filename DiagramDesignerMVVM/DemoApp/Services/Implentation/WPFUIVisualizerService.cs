using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DemoApp
{
    public class WPFUIVisualizerService : IUIVisualizerService
    {
 
        #region Public Methods
        /// <summary>
        /// This method displays a modal dialog associated with the given key.
        /// </summary>
        /// <param name="dataContextForPopup">Object state to associate with the dialog</param>
        /// <returns>True/False if UI is displayed.</returns>
        public bool? ShowDialog(object dataContextForPopup)
        {
            Window win = new PopupWindow();
            win.DataContext = dataContextForPopup;
            win.Owner = Application.Current.MainWindow;
            if (win != null)
                return win.ShowDialog();

            return false;
        }
        #endregion

      
    }
}
