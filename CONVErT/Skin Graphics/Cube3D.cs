using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
//using _3DTools;

namespace CONVErT
{
    public class Cube3D : Viewport3D
    {
        public static readonly DependencyProperty CubeColorProperty =
            DependencyProperty.Register("CubeColorProperty", typeof(String), typeof(Cube3D),
            new FrameworkPropertyMetadata("Red", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Color of the cube
        /// </summary>
        public String CubeColor
        {
            get { return (String)GetValue(CubeColorProperty); }
            set { SetValue(CubeColorProperty, value); }
        }

        /***X coordinate***/
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("XProperty", typeof(String), typeof(Cube3D),
            new FrameworkPropertyMetadata("-30", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public String X
        {
            get { return (String)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /***Z coordinate***/
        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register("ZProperty", typeof(String), typeof(Cube3D),
            new FrameworkPropertyMetadata("10", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public String Z
        {
            get { return (String)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }

        /***Y coordinate***/
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("YProperty", typeof(String), typeof(Cube3D),
            new FrameworkPropertyMetadata("40", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public String Y
        {
            get { return (String)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }


        public Cube3D()
        {
            Canvas.SetLeft(this,0);
            Canvas.SetTop(this,0);
            this.Width=733;
            this.Height = 400;
            this.ClipToBounds = true;

            //add camera
            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 30, 90);
            myPCamera.LookDirection = new Vector3D(0, -20, -90);
            myPCamera.FieldOfView = 80;
            this.Camera = myPCamera;
                
            //add light
            ModelVisual3D mv3dLight = new ModelVisual3D();
            DirectionalLight myDirLight = new DirectionalLight();
            myDirLight.Color = Colors.White;
            myDirLight.Direction = new Vector3D(-2, -3, -1);
            mv3dLight.Content = myDirLight;
            this.Children.Add(mv3dLight);

            //add material and mesh geometry
            Viewport2DVisual3D vpMaterial = new Viewport2DVisual3D();
            DiffuseMaterial dm = new DiffuseMaterial();
            dm.Brush = Brushes.Transparent;
            vpMaterial.Material = dm;
            MeshGeometry3D vpg = new MeshGeometry3D();
            vpg.Positions.Add(new Point3D(-55,0,-30));
            vpg.Positions.Add(new Point3D(-55,0,30));
            vpg.Positions.Add(new Point3D(55,0,30));
            vpg.Positions.Add(new Point3D(55,0,-30));
            vpg.TextureCoordinates.Add(new Point(0,0));
            vpg.TextureCoordinates.Add(new Point(0,1));
            vpg.TextureCoordinates.Add(new Point(1,1));
            vpg.TextureCoordinates.Add(new Point(1,0));
            vpg.TriangleIndices.Add(0);
            vpg.TriangleIndices.Add(1);
            vpg.TriangleIndices.Add(2);
            vpg.TriangleIndices.Add(2);
            vpg.TriangleIndices.Add(3);
            vpg.TriangleIndices.Add(0);
            vpMaterial.Geometry = vpg;
            this.Children.Add(vpMaterial);


            this.Loaded += new RoutedEventHandler(OnLoad);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            ModelVisual3D mv3d = new ModelVisual3D();
            mv3d.Content = createCube(Convert.ToDouble(X), Convert.ToDouble(Y), Convert.ToDouble(Z), CubeColor);
            this.Children.Add(mv3d);
        }

        private Model3DGroup createCube(double _X, double _Y, double _Z, string color)
        {
            Model3DGroup cube = new Model3DGroup();

            Point3D p0 = new Point3D(-1 + _X, 0, 0 + _Z);
            Point3D p1 = new Point3D(0 + _X, 0, -1 + _Z);
            Point3D p2 = new Point3D(1 + _X, 0, 0 + _Z);
            Point3D p3 = new Point3D(0 + _X, 0, 1 + _Z);
            Point3D p4 = new Point3D(-1 + _X, _Y, 0 + _Z);
            Point3D p5 = new Point3D(0 + _X, _Y, -1 + _Z);
            Point3D p6 = new Point3D(1 + _X, _Y, 0 + _Z);
            Point3D p7 = new Point3D(0 + _X, _Y, 1 + _Z);

            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            return cube;
        }
        
        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            Material material = new DiffuseMaterial((SolidColorBrush)new BrushConverter().ConvertFromString(CubeColor));
            GeometryModel3D model = new GeometryModel3D(
                mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);

            return group;
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }
    }
}
