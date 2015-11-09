using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace CONVErT
{
    public class Minard : Canvas
    {
        public Minard()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.Transparent;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            Collection<UIElement> annotations = new Collection<UIElement>();
            Collection<String> colors = new Collection<string>();

            //find distinctive colors in the map points
            foreach(UIElement u in this.Children)
            {
                if ((u as VisualElement).Content is TrafficShape)
                {
                    String c = ((u as VisualElement).Content as TrafficShape).ShapeColor;

                    bool flag = false;

                    foreach(string ctemp in colors)
                        if (String.Equals(ctemp,c))
                        {
                            flag = true;
                        }

                    if (flag == false)
                        colors.Add(c);
                }
            }

            foreach (string c in colors)
            {
                Collection<TrafficShape> points = new Collection<TrafficShape>();

                foreach (UIElement u in this.Children)
                {
                    if ((u as VisualElement).Content is TrafficShape)
                    {
                        if (String.Equals(((u as VisualElement).Content as TrafficShape).ShapeColor, c))
                            points.Add((u as VisualElement).Content as TrafficShape);

                        Canvas.SetTop((u as VisualElement), (((u as VisualElement).Content as TrafficShape).PointY ));
                        Canvas.SetLeft((u as VisualElement), (((u as VisualElement).Content as TrafficShape).PointX));
                        Canvas.SetZIndex((u as VisualElement), 2);

                    }
                }

                for (int i = 0; i < points.Count - 1; i++)
                {
                    //Canvas.SetTop(points[i], (points[i].PointY - points[i].Value / 2000));
                    //Canvas.SetLeft(points[i], (points[i].PointX - points[i].Value / 2000));
                    //Canvas.SetZIndex(points[i], 5);
                    TrafficShape start = points[i];
                    TrafficShape end = points[i+1];

                    double rstart = start.Value / (10000 * 2);
                    double rend = end.Value / (10000 * 2);

                    Point centerpstart = new Point(start.PointX+1, start.PointY+1); //center point of start circle
                    Point centerpend = new Point(end.PointX+1, end.PointY+1); //center point of end circle

                    double dy = centerpend.Y - centerpstart.Y;
                    double dx = centerpend.X - centerpstart.X;
                    double theta = Math.Atan2(dy, dx);
                    
                    if (theta != 0)
                    {
                        theta += 90 * (Math.PI / 180); //the angel of prependicular line to calculate the points
                        
                        PointCollection myPointCollection1 = new PointCollection();
                        myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta));
                        myPointCollection1.Add(calculatePointonCircle(centerpend, rend, theta));
                        myPointCollection1.Add(calculatePointonCircle(centerpend, rend, theta + 180 * (Math.PI / 180)));
                        myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta + 180 * (Math.PI / 180)));
                        myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta));

                        //myPointCollection1.Add(new Point(start.PointX+1, start.PointY+1 - (start.Value / (10000 * 2))));
                        //myPointCollection1.Add(new Point(start.PointX+1, start.PointY+1 + (start.Value / (10000 * 2))));
                        //myPointCollection1.Add(new Point(end.PointX+1, end.PointY+1 + (end.Value / (10000 * 2))));
                        //myPointCollection1.Add(new Point(end.PointX+1, end.PointY+1 - (end.Value / (10000 * 2))));
                        //myPointCollection1.Add(new Point(start.PointX+1, start.PointY+1 - (start.Value / (10000 * 2))));

                        Polygon myPolygon1 = new Polygon();
                        myPolygon1.Points = myPointCollection1;
                        myPolygon1.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(start.ShapeColor);
                        myPolygon1.Opacity = 1;
                        //myPolygon1.Width = this.Width;
                        //myPolygon1.Height = this.Height;
                        annotations.Add(myPolygon1);
                    }
                }
                                
                //clear points for another color
                points.Clear();

            }

            //this.Children.Clear();
            foreach (UIElement U in annotations)
                this.Children.Add(U);
            
        }

        private Point calculatePointonCircle(Point center, double radius, double angel)
        {
            Point p = new Point();

            p.X = center.X + radius * Math.Cos(angel);
            p.Y = center.Y + radius * Math.Sin(angel);

            return p;
        }

    }
}
