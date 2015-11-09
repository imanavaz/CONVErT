using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONVErT
{
    public class  TroopsMovement : Canvas
    {

        #region ctor

        public TroopsMovement()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.Transparent;
            //Find center of the shape
            CentreX = StartX + ((EndX - StartX) / 2);
            CentreY = StartY + ((EndY - StartY) / 2);
            //MessageBox.Show(CentreX.ToString() + " ' " + CentreY.ToString());
        }

        #endregion //ctor


        #region dependency Properties

        public static readonly DependencyProperty CentreXProperty =
            DependencyProperty.Register("CentreXProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of centre of the Shape
        /// </summary>
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        public static readonly DependencyProperty CentreYProperty =
           DependencyProperty.Register("CentreYProperty", typeof(double), typeof(TroopsMovement),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of centre of the Shape
        /// </summary>
        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        public static readonly DependencyProperty StartXProperty =
            DependencyProperty.Register("StartXProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of centre of the starting point
        /// </summary>
        public double StartX
        {
            get { return (double)GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }

        public static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register("StartYProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of centre of the srart point
        /// </summary>
        public double StartY
        {
            get { return (double)GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }

        public static readonly DependencyProperty EndXProperty =
           DependencyProperty.Register("EndXProperty", typeof(double), typeof(TroopsMovement),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of centre of the end point
        /// </summary>
        public double EndX
        {
            get { return (double)GetValue(EndXProperty); }
            set { SetValue(EndXProperty, value); }
        }

        public static readonly DependencyProperty EndYProperty =
            DependencyProperty.Register("EndYProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of centre of the end point
        /// </summary>
        public double EndY
        {
            get { return (double)GetValue(EndYProperty); }
            set { SetValue(EndYProperty, value); }
        }

        public static readonly DependencyProperty TroopsStartingProperty =
            DependencyProperty.Register("TroopsStartingProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// troops at Start
        /// </summary>
        public double TroopsStarting
        {
            get { return (double)GetValue(TroopsStartingProperty); }
            set { SetValue(TroopsStartingProperty, value); }
        }

        public static readonly DependencyProperty TroopsEndingProperty =
            DependencyProperty.Register("TroopsEndingProperty", typeof(double), typeof(TroopsMovement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// troops at destination
        /// </summary>
        public double TroopsEnding
        {
            get { return (double)GetValue(TroopsEndingProperty); }
            set { SetValue(TroopsEndingProperty, value); }
        }

        public static readonly DependencyProperty ShapeColorProperty =
            DependencyProperty.Register("ShapeColorProperty", typeof(String), typeof(TroopsMovement),
            new FrameworkPropertyMetadata("Green", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Color of the movement shape
        /// </summary>
        public String ShapeColor
        {
            get { return (String)GetValue(ShapeColorProperty); }
            set { SetValue(ShapeColorProperty, value); }
        }

        public static readonly DependencyProperty StartLocationNameProperty =
            DependencyProperty.Register("StartLocationNameProperty", typeof(String), typeof(TroopsMovement),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Start Location Name
        /// </summary>
        public String StartLocationName
        {
            get { return (String)GetValue(StartLocationNameProperty); }
            set { SetValue(StartLocationNameProperty, value); }
        }

        public static readonly DependencyProperty EndLocationNameProperty =
            DependencyProperty.Register("EndLocationNameProperty", typeof(String), typeof(TroopsMovement),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// End Location Name
        /// </summary>
        public String EndLocationName
        {
            get { return (String)GetValue(EndLocationNameProperty); }
            set { SetValue(EndLocationNameProperty, value); }
        }

        #endregion //Dependency properties


        void OnLoad(object sender, RoutedEventArgs e)
        {
            double normalisationFactor = 10000;
            string color = (!String.IsNullOrEmpty(ShapeColor)? ShapeColor : "Blue"); //green is default color
            
            //radius of location points
            double rstart = TroopsStarting / (10000 * 2);
            double rend = TroopsEnding / (10000 * 2);
            
            //set canvas bounds according to sizes
            this.Width = 1;//Math.Abs(StartX - EndX) + rstart + rend;
            this.Height = 1;//Math.Abs(StartY - EndY) + rstart + rend;

            
            double Sx = StartX - CentreX;
            double Sy = StartY - CentreY;
            double Ex = EndX - CentreX;
            double Ey = EndY - CentreY;

            //create start location point
            Ellipse ValueEllipseStart = new Ellipse();
            ValueEllipseStart.Height = TroopsStarting / normalisationFactor;
            ValueEllipseStart.Width = TroopsStarting / normalisationFactor;
            ValueEllipseStart.Opacity = 0.8;
            ValueEllipseStart.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
           
            Canvas.SetTop(ValueEllipseStart, Sy - rstart); //normalized positions so that the circles are centered on each other
            Canvas.SetLeft(ValueEllipseStart, Sx - rstart);
            Canvas.SetZIndex(ValueEllipseStart, 1);
            this.Children.Add(ValueEllipseStart);

            Line startLine = new Line();
            startLine.X1 = 0;
            startLine.Y1 = 0;
            startLine.X2 = 15;
            startLine.Y2 = -15;
            startLine.Stroke = Brushes.Black;
            Canvas.SetTop(startLine, Sy);
            Canvas.SetLeft(startLine, Sx);
            Canvas.SetZIndex(startLine, 3);
            this.Children.Add(startLine);

            Label startNameLB = new Label();
            startNameLB.Background = Brushes.White;
            startNameLB.Content = StartLocationName;
            startNameLB.BorderBrush = Brushes.Black;
            startNameLB.Height = 25;
            startNameLB.BorderThickness = new Thickness(1);
            Canvas.SetTop(startNameLB, -25 + Sy);
            Canvas.SetLeft(startNameLB, Sx + 15);
            Canvas.SetZIndex(startNameLB, 4);
            this.Children.Add(startNameLB);

            //create end location point
            Ellipse ValueEllipseEnd = new Ellipse();
            ValueEllipseEnd.Height = TroopsEnding / normalisationFactor;
            ValueEllipseEnd.Width = TroopsEnding / normalisationFactor;
            ValueEllipseEnd.Opacity = 0.8;
            ValueEllipseEnd.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(color);

            Canvas.SetTop(ValueEllipseEnd, Ey - rend); //normalized positions so that the circles are centered on each other
            Canvas.SetLeft(ValueEllipseEnd, Ex - rend);
            Canvas.SetZIndex(ValueEllipseEnd, 1);
            this.Children.Add(ValueEllipseEnd);

            Line endLine = new Line();
            endLine.X1 = 0;
            endLine.Y1 = 0;
            endLine.X2 = 15;
            endLine.Y2 = -15;
            endLine.Stroke = Brushes.Black;
            Canvas.SetTop(endLine, Ey);
            Canvas.SetLeft(endLine, Ex);
            Canvas.SetZIndex(endLine, 3);
            this.Children.Add(endLine);

            Label endNameLB = new Label();
            endNameLB.Background = Brushes.White;
            endNameLB.Content = EndLocationName;
            endNameLB.BorderBrush = Brushes.Black;
            endNameLB.Height = 25;
            endNameLB.BorderThickness = new Thickness(1);
            Canvas.SetTop(endNameLB, -25 + Ey);
            Canvas.SetLeft(endNameLB, Ex + 15);
            Canvas.SetZIndex(endNameLB, 4);
            this.Children.Add(endNameLB);


            //create connecting shape ToDo
            Point centerpstart = new Point(Sx, Sy); //center point of start circle
            Point centerpend = new Point(Ex, Ey); //center point of end circle

            double dy = centerpend.Y - centerpstart.Y;
            double dx = centerpend.X - centerpstart.X;
            double theta = Math.Atan2(dy, dx);

            theta += 90 * (Math.PI / 180); //the angel of prependicular line to calculate the points

            PointCollection myPointCollection1 = new PointCollection();
            myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta));
            myPointCollection1.Add(calculatePointonCircle(centerpend, rend, theta));
            myPointCollection1.Add(calculatePointonCircle(centerpend, rend, theta + 180 * (Math.PI / 180)));
            myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta + 180 * (Math.PI / 180)));
            myPointCollection1.Add(calculatePointonCircle(centerpstart, rstart, theta));

            Polygon myPolygon1 = new Polygon();
            myPolygon1.Points = myPointCollection1;
            myPolygon1.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            myPolygon1.Opacity = 1;
            Canvas.SetZIndex(myPolygon1, 2);
            this.Children.Add(myPolygon1);

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
