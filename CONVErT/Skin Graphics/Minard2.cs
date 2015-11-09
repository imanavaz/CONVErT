using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace CONVErT
{
    public class Minard2 : Canvas
    {
        public Minard2()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.Transparent;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            foreach (UIElement u in this.Children)
            {
                if ((u as VisualElement).Content is TroopsMovement)
                {
                    Canvas.SetTop((u as VisualElement), (((u as VisualElement).Content as TroopsMovement).CentreY));
                    Canvas.SetLeft((u as VisualElement), (((u as VisualElement).Content as TroopsMovement).CentreX));
                    Canvas.SetZIndex((u as VisualElement), -1);
                    //MessageBox.Show((((u as VisualElement).Content as TroopsMovement).CentreX).ToString() + " ' " +(((u as VisualElement).Content as TroopsMovement).CentreY).ToString());
                }
            }
        }



    }
}
