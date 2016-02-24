using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DiagramDesigner
{
    public static class ItemConnectProps
    {
        #region EnabledForConnection

        public static readonly DependencyProperty EnabledForConnectionProperty =
            DependencyProperty.RegisterAttached("EnabledForConnection", typeof(bool), typeof(ItemConnectProps),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForConnectionChanged)));

        public static bool GetEnabledForConnection(DependencyObject d)
        {
            return (bool)d.GetValue(EnabledForConnectionProperty);
        }

        public static void SetEnabledForConnection(DependencyObject d, bool value)
        {
            d.SetValue(EnabledForConnectionProperty, value);
        }

        private static void OnEnabledForConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)d;


            if ((bool)e.NewValue)
            {
                fe.MouseEnter += Fe_MouseEnter;
            }
            else
            {
                fe.MouseEnter -= Fe_MouseEnter;
            }
        }

        #endregion

        static void Fe_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
            {
                DesignerItemViewModelBase designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;
                designerItem.ShowConnectors = true;
            }
        }




    }
}
