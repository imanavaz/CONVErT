using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class UMLAssociation : Canvas
    {
        #region prop
        public static readonly DependencyProperty AssociationNameProperty =
           DependencyProperty.Register("AssociationNameProperty", typeof(string), typeof(UMLAssociation),
           new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Association
        /// </summary>
        public string AssociationName
        {
            get { return (string)GetValue(AssociationNameProperty); }
            set { SetValue(AssociationNameProperty, value); }
        }

        /*
        public static readonly DependencyProperty OwnerClassProperty =
           DependencyProperty.Register("OwnerClassProperty", typeof(string), typeof(UMLAssociation),
           new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the OwnerClass
        /// </summary>
        public string OwnerClass
        {
            get { return (string)GetValue(OwnerClassProperty); }
            set { SetValue(OwnerClassProperty, value); }
        }*/

        public static readonly DependencyProperty EndClassProperty =
           DependencyProperty.Register("EndClassProperty", typeof(string), typeof(UMLAssociation),
           new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the EndClass
        /// </summary>
        public string EndClass
        {
            get { return (string)GetValue(EndClassProperty); }
            set { SetValue(EndClassProperty, value); }
        }

        /*public static readonly DependencyProperty OwnerCardinalityProperty =
           DependencyProperty.Register("OwnerCardinalityProperty", typeof(string), typeof(UMLAssociation),
           new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Cardinality of the owner class
        /// </summary>
        public string OwnerCardinality
        {
            get { return (string)GetValue(OwnerCardinalityProperty); }
            set { SetValue(OwnerCardinalityProperty, value); }
        }*/

        public static readonly DependencyProperty EndCardinalityProperty =
           DependencyProperty.Register("EndCardinalityProperty", typeof(string), typeof(UMLAssociation),
           new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Cardinality of End class
        /// </summary>
        public string EndCardinality
        {
            get { return (string)GetValue(EndCardinalityProperty); }
            set { SetValue(EndCardinalityProperty, value); }
        }

        private bool isLoaded = false;

        #endregion

        #region ctor
        
        public UMLAssociation()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Height = 15;
            this.Width = 15;
            this.Background = Brushes.White;
            Canvas.SetZIndex(this, 10);
            //create defaut arrow

        }

        #endregion //ctor

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                //MessageBox.Show("in association");
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Vertical;

                Label CardinalityLabel = new Label();
                CardinalityLabel.Content = /*this.OwnerCardinality + ".." +*/ this.EndCardinality;
                stack.Children.Add(CardinalityLabel);
                CardinalityLabel.Height = 24;

                Label assName = new Label();
                assName.Content = AssociationName;
                stack.Children.Add(assName);
                assName.Height = 24;
                this.Children.Add(stack);

                drawArrow(new Point(-10, 0), new Point(20, 0));

                isLoaded = true;
            }
        }

        public void drawArrow(Point start, Point end)
        {
            clearArrow();

            double CentreX = Canvas.GetLeft(this);
            double CentreY = Canvas.GetTop(this);

            //Canvas.SetTop(this, 0);
            //Canvas.SetLeft(this, 0);

            ArrowLine aline1 = getArrow();
            if (aline1 == null)
                aline1 = new ArrowLine();

            aline1.Stroke = Brushes.Black;
            aline1.StrokeThickness = 1;
            aline1.IsArrowClosed = false;

            aline1.X1 = start.X;
            aline1.Y1 = start.Y;

            aline1.X2 = end.X;
            aline1.Y2 = end.Y;

            Canvas.SetLeft(aline1, 0);
            Canvas.SetTop(aline1, 0);
            //Canvas.SetZIndex(aline1, 2);

            this.Children.Add(aline1);
        }

        public ArrowLine getArrow()
        {
            //clear default arrow line
            foreach (UIElement u in this.Children)
                if (u is ArrowLine)
                {
                    return (u as ArrowLine);
                }

            return null;
        }

        public void clearArrow()
        {
            //clear default arrow line
            foreach (UIElement u in this.Children)
                if (u is ArrowLine)
                {
                    u.Visibility = System.Windows.Visibility.Hidden;
                    this.Children.Remove(u);
                }
        }

    }
}
