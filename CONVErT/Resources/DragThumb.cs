using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CONVErT
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            VisualElement designerItem = this.DataContext as VisualElement;
            Canvas designer = VisualTreeHelper.GetParent(designerItem) as Canvas;
            if (designerItem != null && designer != null && designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                //var designerItems = from item in designer.SelectedItems
                 //                   where item is DesignerItem
                 //                   select item;

                //foreach (DesignerItem item in designerItems)
                //{
               
                    double left = Canvas.GetLeft(designerItem);
                    double top = Canvas.GetTop(designerItem);

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
               // }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                //foreach (DesignerItem item in designerItems)
                //{
                    left = Canvas.GetLeft(designerItem);
                    top = Canvas.GetTop(designerItem);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    Canvas.SetLeft(designerItem, left + deltaHorizontal);
                    Canvas.SetTop(designerItem, top + deltaVertical);
                //}

                designer.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}
