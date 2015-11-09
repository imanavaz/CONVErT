using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class NamedPieChart : Canvas
    {

        public NamedPieChart()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.White;

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            //get peaces
            NamedPiePiece[] pp = new NamedPiePiece[this.Children.Count];

            double sumValues = 0;

            foreach (UIElement o in this.Children)
            {
                if (o is NamedPiePiece)
                {
                    sumValues += (o as NamedPiePiece).WedgeAngle;
                }
                else if ((o is VisualElement) && ((o as VisualElement).Content is NamedPiePiece))
                {
                    sumValues += ((o as VisualElement).Content as NamedPiePiece).WedgeAngle;
                }
            }

            double lastRotationAngle = 0;

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i] is NamedPiePiece)
                {
                    (this.Children[i] as NamedPiePiece).Radius = 145;
                    (this.Children[i] as NamedPiePiece).InnerRadius = 20;
                    (this.Children[i] as NamedPiePiece).CentreX = 150;
                    (this.Children[i] as NamedPiePiece).CentreY = 170;
                    double angle = (this.Children[i] as NamedPiePiece).WedgeAngle * 360 / sumValues;
                    (this.Children[i] as NamedPiePiece).WedgeAngle = angle;
                    (this.Children[i] as NamedPiePiece).RotationAngle = lastRotationAngle;
                    
                    //position Label according to piepiece's coordinates
                    Point outerArcLabelEndPoint = (this.Children[i] as NamedPiePiece).ComputeCartesianCoordinate((this.Children[i] as NamedPiePiece).RotationAngle + ((this.Children[i] as NamedPiePiece).WedgeAngle / 2), (this.Children[i] as NamedPiePiece).Radius + 20);
                    outerArcLabelEndPoint.Offset((this.Children[i] as NamedPiePiece).CentreX, (this.Children[i] as NamedPiePiece).CentreY);
                    Label NameLabel = new Label();
                    NameLabel.Content = (this.Children[i] as NamedPiePiece).NameLabel;
                    NameLabel.Background = Brushes.White;
                    Canvas.SetTop(NameLabel, outerArcLabelEndPoint.Y - 10);
                    Canvas.SetLeft(NameLabel, outerArcLabelEndPoint.X);
                    this.Children.Add(NameLabel);
                    
                    lastRotationAngle += angle;
                }
                else if ((this.Children[i] is VisualElement) && ((this.Children[i] as VisualElement).Content is NamedPiePiece))
                {
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).Radius = 145;
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).InnerRadius = 20;
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).CentreX = 150;
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).CentreY = 170;
                    double angle = ((this.Children[i] as VisualElement).Content as NamedPiePiece).WedgeAngle * 360 / sumValues;
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).WedgeAngle = angle;
                    ((this.Children[i] as VisualElement).Content as NamedPiePiece).RotationAngle = lastRotationAngle;

                    //position Label according to piepiece's coordinates
                    Point outerArcLabelEndPoint = ((this.Children[i] as VisualElement).Content as NamedPiePiece).ComputeCartesianCoordinate(((this.Children[i] as VisualElement).Content as NamedPiePiece).RotationAngle + (((this.Children[i] as VisualElement).Content as NamedPiePiece).WedgeAngle / 2),
                        ((this.Children[i] as VisualElement).Content as NamedPiePiece).Radius + 20);
                    outerArcLabelEndPoint.Offset(((this.Children[i] as VisualElement).Content as NamedPiePiece).CentreX, ((this.Children[i] as VisualElement).Content as NamedPiePiece).CentreY);
                    Label NameLabel = new Label();
                    NameLabel.Content = ((this.Children[i] as VisualElement).Content as NamedPiePiece).NameLabel;
                    NameLabel.Background = Brushes.White;
                    Canvas.SetTop(NameLabel, outerArcLabelEndPoint.Y-10);
                    Canvas.SetLeft(NameLabel, outerArcLabelEndPoint.X);
                    this.Children.Add(NameLabel);

                    lastRotationAngle += angle;
                }

            }

        }

    }
}
