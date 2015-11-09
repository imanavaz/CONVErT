using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CONVErT
{
    public class MapCanvas : Canvas
    {
        public MapCanvas()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.Transparent;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            foreach (UIElement u in this.Children)
            {
                if ((u as VisualElement).Content is MapPoint)
                {
                    Canvas.SetTop((u as VisualElement), (((u as VisualElement).Content as MapPoint).PointY));
                    Canvas.SetLeft((u as VisualElement), (((u as VisualElement).Content as MapPoint).PointX));
                    Canvas.SetZIndex((u as VisualElement), -1);
                }
                
                if ((u as VisualElement).Content is MapPieChart)
                {
                    Canvas.SetTop((u as VisualElement), (((u as VisualElement).Content as MapPieChart).ChartY));
                    Canvas.SetLeft((u as VisualElement), (((u as VisualElement).Content as MapPieChart).ChartX));
                    Canvas.SetZIndex((u as VisualElement), -1);
                }
            }
        }



    }
}