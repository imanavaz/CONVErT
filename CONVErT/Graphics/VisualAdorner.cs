using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONVErT
{
   
    // Adorners must subclass the abstract base class Adorner.
    public  class VisualElementMouseAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public VisualElementMouseAdorner(UIElement adornedElement)
            : base(adornedElement) 
        {
            this.IsHitTestVisible = false;
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            
            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Green), 1);

            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
           
            //double renderRadius = 5.0;

            // Draw a circle at each corner.
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }
    }

    public class VisualElementDragAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public VisualElementDragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Red);
            renderBrush.Opacity = 0.1;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Red), 1);

            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);

        }
    }

    public class VisualSelectionAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public VisualSelectionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Blue);
            renderBrush.Opacity = 0.1;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Blue), 1);

            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);

        }
    }

    public class FrameworkElementAdorner : Adorner
    {
         public FrameworkElementAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            this.IsHitTestVisible = false;
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Yellow);
            renderBrush.Opacity = 0.3;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Blue), 1);

            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);

        }
    }
}
