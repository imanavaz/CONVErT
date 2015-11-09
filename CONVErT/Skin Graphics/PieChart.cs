using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class PieChart : Canvas
    {

        public PieChart()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.White;

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            //get peaces
            PiePiece [] pp = new PiePiece [this.Children.Count];

            if (this.Children.Count > 0)
            {
                double sumValues = 0;

                foreach (UIElement o in this.Children)
                {
                    if (o is PiePiece)
                    {
                        sumValues += (o as PiePiece).WedgeAngle;
                    }
                    else if ((o is VisualElement) && ((o as VisualElement).Content is PiePiece))
                    {
                        sumValues += ((o as VisualElement).Content as PiePiece).WedgeAngle;
                    }
                }

                double lastRotationAngle = 0;

                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (this.Children[i] is PiePiece)
                    {
                        (this.Children[i] as PiePiece).Radius = 145;
                        (this.Children[i] as PiePiece).InnerRadius = 20;
                        (this.Children[i] as PiePiece).CentreX = 150;
                        (this.Children[i] as PiePiece).CentreY = 170;
                        double angle = (this.Children[i] as PiePiece).WedgeAngle * 360 / sumValues;
                        (this.Children[i] as PiePiece).WedgeAngle = angle;
                        (this.Children[i] as PiePiece).RotationAngle = lastRotationAngle;
                        lastRotationAngle += angle;
                    }
                    else if ((this.Children[i] is VisualElement) && ((this.Children[i] as VisualElement).Content is PiePiece))
                    {
                        ((this.Children[i] as VisualElement).Content as PiePiece).Radius = 145;
                        ((this.Children[i] as VisualElement).Content as PiePiece).InnerRadius = 20;
                        ((this.Children[i] as VisualElement).Content as PiePiece).CentreX = 150;
                        ((this.Children[i] as VisualElement).Content as PiePiece).CentreY = 170;
                        double angle = ((this.Children[i] as VisualElement).Content as PiePiece).WedgeAngle * 360 / sumValues;
                        ((this.Children[i] as VisualElement).Content as PiePiece).WedgeAngle = angle;
                        ((this.Children[i] as VisualElement).Content as PiePiece).RotationAngle = lastRotationAngle;
                        lastRotationAngle += angle;
                    }

                }
            }
            /*else //no child yet, create a default child
            {
                PiePiece p = new PiePiece();
                p.Radius = 145;
                p.InnerRadius = 20;
                p.CentreX = 150;
                p.CentreY = 170;
                p.WedgeAngle = 360;
                p.RotationAngle = 10;
                p.Fill= Brushes.Red;
                p.Stroke = Brushes.Black;

                this.Children.Add(p);
            }*/
        }
        
    }
}
