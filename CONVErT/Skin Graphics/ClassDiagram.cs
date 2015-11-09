using System;
using System.Windows.Controls;
using System.Windows;

namespace CONVErT
{
    public class ClassDiagram : Canvas
    {
        #region props

        public static readonly DependencyProperty DiagramNameProperty =
            DependencyProperty.Register("DiagramNameProperty", typeof(string), typeof(ClassDiagram),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Parameter
        /// </summary>
        public string DiagramName
        {
            get { return (string)GetValue(DiagramNameProperty); }
            set { SetValue(DiagramNameProperty, value); }
        }

        bool isLoaded = false;

        #endregion //props

        #region ctor

        public ClassDiagram()
        {
            this.Background = System.Windows.Media.Brushes.LightGray;
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Height = 100;
            this.Width = 100;
        }

        #endregion //ctor


        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                isLoaded = true;
                try
                {
                    if (this.Children.Count > 0)
                    {
                        Label nameLabel = new Label();
                        nameLabel.Content = this.DiagramName;
                        nameLabel.Height = 25;
                        Canvas.SetTop(nameLabel, 0);
                        Canvas.SetLeft(nameLabel, 0);
                        this.Children.Add(nameLabel);

                        Canvas clsCanvas = null;
                        foreach (UIElement u in this.Children)
                            if (u is Canvas)
                            {
                                clsCanvas = u as Canvas;
                                break;
                            }


                        //positioning
                        if (clsCanvas != null)
                        {
                            //MessageBox.Show("in class canvas");
                            //position classes
                            foreach (UIElement uClass in clsCanvas.Children)
                                if (uClass is VisualElement)
                                    if ((uClass as VisualElement).Content is UMLClass)//double check
                                    {
                                        Canvas.SetTop(uClass, 25 + (clsCanvas.Children.IndexOf(uClass) % 2) * 200);
                                        Canvas.SetLeft(uClass, 5 + (clsCanvas.Children.IndexOf(uClass) / 2) * 350);
                                        Canvas.SetZIndex(uClass, 9);
                                        Canvas.SetZIndex(((uClass as VisualElement).Content as UMLClass), 9);
                                    }

                            //reposition associations
                            foreach (UIElement uClass in clsCanvas.Children)
                                if (uClass is VisualElement)
                                    if ((uClass as VisualElement).Content is UMLClass)//double check
                                        if (((uClass as VisualElement).Content as UMLClass).getAssociations().Count > 0)
                                        {
                                            System.Collections.ObjectModel.Collection<UIElement> ass = new System.Collections.ObjectModel.Collection<UIElement>();
                                            ass = ((uClass as VisualElement).Content as UMLClass).getAssociations();
                                            foreach (UIElement assoc in ass)
                                                if ((assoc as VisualElement).Content is UMLAssociation)
                                                {
                                                    //UIElement start = uClass;
                                                    UIElement end = findClass(clsCanvas, ((assoc as VisualElement).Content as UMLAssociation).EndClass);
                                                    if ((end != null))//found end class
                                                    {
                                                        //int sindex = clsCanvas.Children.IndexOf(start);
                                                        int eindex = clsCanvas.Children.IndexOf(end);
                                                        //MessageBox.Show("found end class");
                                                        if (eindex >= 0)
                                                        {
                                                            Point sp = new Point(5, 30);
                                                            Point ep = new Point(5 + (eindex / 2) * 350, 30 + (eindex % 2) * 200);
                                                            //Point ep = new Point(Canvas.GetLeft((end as VisualElement).Content as UMLClass), Canvas.GetTop((end as VisualElement).Content as UMLClass));

                                                            //not a good approach
                                                            ((assoc as VisualElement).Content as UMLAssociation).clearArrow();
                                                            ((assoc as VisualElement).Parent as Canvas).Children.Remove(assoc as VisualElement);

                                                            this.Children.Add(assoc);

                                                            if (ep.X == 5)
                                                            {
                                                                Canvas.SetLeft(assoc, ep.X);
                                                                Canvas.SetTop(assoc, ep.Y - 60);
                                                            }
                                                            else if (ep.Y == 30)
                                                            {
                                                                Canvas.SetLeft(assoc, ep.X - 80);
                                                                Canvas.SetTop(assoc, ep.Y);
                                                            }

                                                            ArrowLine aline1 = new ArrowLine();
                                                            aline1.Stroke = System.Windows.Media.Brushes.Black;
                                                            aline1.StrokeThickness = 1;
                                                            aline1.IsArrowClosed = false;
                                                            aline1.X1 = sp.X;
                                                            aline1.Y1 = sp.Y;
                                                            aline1.X2 = ep.X;
                                                            aline1.Y2 = ep.Y;
                                                            this.Children.Add(aline1);
                                                            Canvas.SetZIndex(aline1, 1);


                                                        }
                                                    }
                                                }

                                        }
                        }


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Calculates the degree of the line connecting two points
        /// </summary>
        /// <param name="S">Center coordinate of the starting point</param>
        /// <param name="E">Center Coordinate of the ending point</param>
        /// <returns></returns>
        private double decideLinkDirection(Point centerpstart, Point centerpend)
        {
            double dy = centerpend.Y - centerpstart.Y;
            double dx = centerpend.X - centerpstart.X;
            double theta = Math.Atan2(dy, dx);

            double Rad2Deg = 180.0 / Math.PI;

            return theta * Rad2Deg;
        }

        private UIElement findClass(Canvas cs, String name)
        {
            foreach (UIElement u in cs.Children)
                if ((u is VisualElement) && ((u as VisualElement).Content is UMLClass))//double check
                {
                    if (((u as VisualElement).Content as UMLClass).ClassName.Equals(name))
                        return u;
                }

            return null;
        }
    }
}
