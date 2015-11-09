using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONVErT
{
    public class TrafficShape : Canvas
    {

        #region ctor

        public TrafficShape()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Height = 2;
            this.Width = 2;
            this.Background = Brushes.Transparent;
        }

        #endregion //ctor


        #region dependency Properties

        public static readonly DependencyProperty PointXProperty =
            DependencyProperty.Register("PointXProperty", typeof(double), typeof(TrafficShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of centre of the point
        /// </summary>
        public double PointX
        {
            get { return (double)GetValue(PointXProperty); }
            set { SetValue(PointXProperty, value); }
        }

        public static readonly DependencyProperty PointYProperty =
            DependencyProperty.Register("PointYProperty", typeof(double), typeof(TrafficShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of centre of the point
        /// </summary>
        public double PointY
        {
            get { return (double)GetValue(PointYProperty); }
            set { SetValue(PointYProperty, value); }
        }


        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("ValueProperty", typeof(double), typeof(TrafficShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// First Value
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ShapeColorProperty =
            DependencyProperty.Register("ShapeColorProperty", typeof(String), typeof(TrafficShape),
            new FrameworkPropertyMetadata("Green", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Color of the point
        /// </summary>
        public String ShapeColor
        {
            get { return (String)GetValue(ShapeColorProperty); }
            set { SetValue(ShapeColorProperty, value); }
        }

        public static readonly DependencyProperty ShapeNameProperty =
            DependencyProperty.Register("ShapeProperty", typeof(String), typeof(TrafficShape),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Shape Name
        /// </summary>
        public String ShapeName
        {
            get { return (String)GetValue(ShapeNameProperty); }
            set { SetValue(ShapeNameProperty, value); }
        }

        #endregion //Dependency properties


        void OnLoad(object sender, RoutedEventArgs e)
        {
            double normalisationFactor = 10000;
            double offset = Math.Sqrt(2); //Canvas is 2 by 2 so 1+ 1 is 2
            double movement = -(((Value / normalisationFactor) / 2) - Math.Sqrt(2));

            Ellipse ValueEllipse = new Ellipse();
            ValueEllipse.Height = Value / normalisationFactor;
            ValueEllipse.Width = Value / normalisationFactor;
            ValueEllipse.Fill = Brushes.Red;
            ValueEllipse.Opacity = 0.8;
            
            if (!String.IsNullOrEmpty(ShapeColor))
            {
                ValueEllipse.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(ShapeColor); 
            }

            //Canvas.SetTop(ValueEllipse, offset - Value / (1000 * 2)); //normalized positions so that the circles are centered on each other
            //Canvas.SetLeft(ValueEllipse, offset - Value / (1000 * 2));
            Canvas.SetTop(ValueEllipse, movement); //normalized positions so that the circles are centered on each other
            Canvas.SetLeft(ValueEllipse, movement);
            Canvas.SetZIndex(ValueEllipse, 2);
            this.Children.Add(ValueEllipse);

            
            Line pointLine = new Line();


            pointLine.X1 = 0;
            pointLine.Y1 = 0;
            pointLine.X2 = 15;
            pointLine.Y2 = -15;
            pointLine.Stroke = Brushes.Black;
            Canvas.SetTop(pointLine, offset);
            Canvas.SetLeft(pointLine, offset);
            Canvas.SetZIndex(pointLine, 2);
            this.Children.Add(pointLine);

            Label nameLB = new Label();
            nameLB.Background = Brushes.White;
            nameLB.Content = ShapeName;
            nameLB.BorderBrush = Brushes.Black;
            nameLB.Height = 25;
            nameLB.BorderThickness = new Thickness(1);
            Canvas.SetTop(nameLB, -25 + offset);
            Canvas.SetLeft(nameLB, offset + 15);
            Canvas.SetZIndex(nameLB, 4);
            this.Children.Add(nameLB);

            //Canvas.SetTop(this, PointY);
            //Canvas.SetLeft(this, PointX);

        }

    }
}
