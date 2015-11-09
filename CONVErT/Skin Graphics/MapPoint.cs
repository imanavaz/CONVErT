using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONVErT
{
    public class MapPoint : Canvas
    {

        #region ctor

        public MapPoint()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.Transparent;
            //MessageBox.Show(CentreX.ToString() + " ' " + CentreY.ToString());
        }

        #endregion //ctor


        #region dependency Properties

        public static readonly DependencyProperty PointXProperty =
           DependencyProperty.Register("PointXProperty", typeof(double), typeof(MapPoint),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of the point
        /// </summary>
        public double PointX
        {
            get { return (double)GetValue(PointXProperty); }
            set { SetValue(PointXProperty, value); }
        }

        public static readonly DependencyProperty PointYProperty =
            DependencyProperty.Register("PointYProperty", typeof(double), typeof(MapPoint),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of the point
        /// </summary>
        public double PointY
        {
            get { return (double)GetValue(PointYProperty); }
            set { SetValue(PointYProperty, value); }
        }


        public static readonly DependencyProperty PointValueProperty =
            DependencyProperty.Register("PointValueProperty", typeof(double), typeof(MapPoint),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Value at point
        /// </summary>
        public double PointValue
        {
            get { return (double)GetValue(PointValueProperty); }
            set { SetValue(PointValueProperty, value); }
        }

        public static readonly DependencyProperty PointColourProperty =
            DependencyProperty.Register("PointColourProperty", typeof(String), typeof(MapPoint),
            new FrameworkPropertyMetadata("Green", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Colour of the point
        /// </summary>
        public String PointColour
        {
            get { return (String)GetValue(PointColourProperty); }
            set { SetValue(PointColourProperty, value); }
        }

        public static readonly DependencyProperty PointNameProperty =
            DependencyProperty.Register("PointNameProperty", typeof(String), typeof(MapPoint),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Point Name
        /// </summary>
        public String PointName
        {
            get { return (String)GetValue(PointNameProperty); }
            set { SetValue(PointNameProperty, value); }
        }

        public static readonly DependencyProperty PointNormalisationFactorProperty =
            DependencyProperty.Register("PointNormalisationFactorProperty", typeof(double), typeof(MapPoint),
            new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Point value will be divided by this normalisation factor
        /// </summary>
        public double PointNormalisationFactor
        {
            get { return (double)GetValue(PointNormalisationFactorProperty); }
            set { SetValue(PointNormalisationFactorProperty, value); }
        }

        #endregion //Dependency properties


        void OnLoad(object sender, RoutedEventArgs e)
        {
            string colour = (!String.IsNullOrEmpty(PointColour) ? PointColour : "Blue"); //green is default colour

            //radius of location points
            double rad = PointValue / (PointNormalisationFactor * 2);

            //set canvas bounds according to sizes
            this.Width = 1;//Math.Abs(StartX - EndX) + rstart + rend;
            this.Height = 1;//Math.Abs(StartY - EndY) + rstart + rend;

                        
            //create point
            Ellipse pointEllipse = new Ellipse();
            pointEllipse.Height = PointValue / PointNormalisationFactor;
            pointEllipse.Width = PointValue / PointNormalisationFactor;
            pointEllipse.Opacity = 0.8;
            pointEllipse.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(colour);

            Canvas.SetTop(pointEllipse, PointY); //normalized positions so that the circles are centered on each other
            Canvas.SetLeft(pointEllipse, PointX);
            Canvas.SetZIndex(pointEllipse, 1);
            this.Children.Add(pointEllipse);

            Line endLine = new Line();
            endLine.X1 = 0;
            endLine.Y1 = 0;
            endLine.X2 = 15;
            endLine.Y2 = -15;
            endLine.Stroke = Brushes.Black;
            Canvas.SetTop(endLine, PointY + rad);
            Canvas.SetLeft(endLine, PointX + rad);
            Canvas.SetZIndex(endLine, 3);
            this.Children.Add(endLine);

            Label endNameLB = new Label();
            endNameLB.Background = Brushes.White;
            endNameLB.Content = PointName;
            endNameLB.BorderBrush = Brushes.Black;
            endNameLB.Height = 25;
            endNameLB.BorderThickness = new Thickness(1);
            Canvas.SetTop(endNameLB, -25 + PointY + rad);
            Canvas.SetLeft(endNameLB, PointX + 15 + rad);
            Canvas.SetZIndex(endNameLB, 4);
            this.Children.Add(endNameLB);           
            
        }
                
    }
}
