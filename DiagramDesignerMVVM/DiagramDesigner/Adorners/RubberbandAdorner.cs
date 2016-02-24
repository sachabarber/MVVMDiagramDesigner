using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System;

namespace DiagramDesigner
{
    public class RubberbandAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;
        private Pen rubberbandPen;

        private DesignerCanvas designerCanvas;

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.designerCanvas = designerCanvas;
            this.startPoint = dragStartPoint;
            rubberbandPen = new Pen(Brushes.LightSlateGray, 1);
            rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired !
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this.startPoint.HasValue && this.endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, rubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
        }


        private T GetParent<T>(Type parentType, DependencyObject dependencyObject) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent.GetType() == parentType)
                return (T)parent;

            return GetParent<T>(parentType, parent);
        }



        private void UpdateSelection()
        {
            IDiagramViewModel vm = (designerCanvas.DataContext as IDiagramViewModel);
            Rect rubberBand = new Rect(startPoint.Value, endPoint.Value);
            ItemsControl itemsControl = GetParent<ItemsControl>(typeof (ItemsControl), designerCanvas);

            foreach (SelectableDesignerItemViewModelBase item in vm.Items)
            {
                if (item is SelectableDesignerItemViewModelBase)
                {
                    DependencyObject container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);

                    Rect itemRect = VisualTreeHelper.GetDescendantBounds((Visual) container);
                    Rect itemBounds = ((Visual) container).TransformToAncestor(designerCanvas).TransformBounds(itemRect);

                    if (rubberBand.Contains(itemBounds))
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                        {
                            item.IsSelected = false;
                        }
                    }
                }
            }
        }
    }
}
