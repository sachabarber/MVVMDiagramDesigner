using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiagramDesigner
{
    /// <summary>
    /// Interaction logic for DiagramControl.xaml
    /// </summary>
    public partial class DiagramControl : UserControl
    {
        public DiagramControl()
        {
            InitializeComponent();
        }


        private void DesignerCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DesignerCanvas myDesignerCanvas = sender as DesignerCanvas;
            zoomBox.DesignerCanvas = myDesignerCanvas;
        }

    }
}
