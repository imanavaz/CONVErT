﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;


namespace CONVErT
{
    /// <summary>
    /// A pie piece shape
    /// </summary>
    public class NamedPiePiece : Shape
    {
        #region dependency properties

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("RadiusProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The radius of this pie piece
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty PushOutProperty =
            DependencyProperty.Register("PushOutProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The distance to 'push' this pie piece out from the centre.
        /// </summary>
        public double PushOut
        {
            get { return (double)GetValue(PushOutProperty); }
            set { SetValue(PushOutProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadiusProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The inner radius of this pie piece
        /// </summary>
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty WedgeAngleProperty =
            DependencyProperty.Register("WedgeAngleProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The wedge angle of this pie piece in degrees
        /// </summary>
        public double WedgeAngle
        {
            get { return (double)GetValue(WedgeAngleProperty); }
            set {
                SetValue(WedgeAngleProperty, value);
                this.Percentage = (value / 360.0);
            }
        }

        public static readonly DependencyProperty NameLabelProperty =
            DependencyProperty.Register("NameLabelProperty", typeof(String), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The wedge angle of this pie piece in degrees
        /// </summary>
        public String NameLabel
        {
            get { return (String)GetValue(NameLabelProperty); }
            set
            {
                SetValue(NameLabelProperty, value);
            }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register("RotationAngleProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The rotation, in degrees, from the Y axis vector of this pie piece.
        /// </summary>
        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty CentreXProperty =
            DependencyProperty.Register("CentreXProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The X coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        public static readonly DependencyProperty CentreYProperty =
            DependencyProperty.Register("CentreYProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// The Y coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("PercentageProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// The percentage of a full pie that this piece occupies.
        /// </summary>
        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            private set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PieceValueProperty =
            DependencyProperty.Register("PieceValueProperty", typeof(double), typeof(NamedPiePiece),
            new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// The value that this pie piece represents.
        /// </summary>
        public double PieceValue
        {
            get { return (double)GetValue(PieceValueProperty); }
            set { SetValue(PieceValueProperty, value); }
        }


        #endregion

        public NamedPiePiece()
        {
            this.Opacity = 0.8;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // Create a StreamGeometry for describing the shape
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }

        /// <summary>
        /// Draws the pie piece
        /// </summary>
        private void DrawGeometry(StreamGeometryContext context)
        {           
            Point startPoint = new Point(CentreX, CentreY);

            Point innerArcStartPoint = ComputeCartesianCoordinate(RotationAngle, InnerRadius);
            innerArcStartPoint.Offset(CentreX, CentreY);

            Point innerArcEndPoint = ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius);
            innerArcEndPoint.Offset(CentreX, CentreY);

            Point outerArcStartPoint = ComputeCartesianCoordinate(RotationAngle, Radius);
            outerArcStartPoint.Offset(CentreX, CentreY);

            Point outerArcEndPoint = ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius);
            outerArcEndPoint.Offset(CentreX, CentreY);

            Point outerArcLabelStartPoint = ComputeCartesianCoordinate(RotationAngle + (WedgeAngle/2), Radius);
            outerArcLabelStartPoint.Offset(CentreX, CentreY);

            Point outerArcLabelEndPoint = ComputeCartesianCoordinate(RotationAngle + (WedgeAngle / 2), Radius + 20);
            outerArcLabelEndPoint.Offset(CentreX, CentreY);

            bool largeArc = WedgeAngle>180.0;

            if (PushOut > 0)
            {
                Point offset = ComputeCartesianCoordinate(RotationAngle + WedgeAngle / 2, PushOut);
                innerArcStartPoint.Offset(offset.X, offset.Y);
                innerArcEndPoint.Offset(offset.X, offset.Y);
                outerArcStartPoint.Offset(offset.X, offset.Y);
                outerArcEndPoint.Offset(offset.X, offset.Y);

            }

            Size outerArcSize = new Size(Radius, Radius);
            Size innerArcSize = new Size(InnerRadius, InnerRadius);

            context.BeginFigure(innerArcStartPoint, true, true);
            context.LineTo(outerArcStartPoint, true, true);
            context.ArcTo(outerArcLabelStartPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
            context.LineTo(outerArcLabelEndPoint, true, true);
            context.LineTo(outerArcLabelStartPoint, true, true);
            context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
            context.LineTo(innerArcEndPoint, true, true);
            context.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise, true, true);


        }

        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }


    }

}
