using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    public class GPSCoordinateConvertor
    {
        #region prpos
            
        //mesh coordinates
        public double LeftLatitude;
        public double RightLatitude; 
        public double TopLongitude;
        public double BottomLongitude;

        //Map properties
        public double PicWidth;
        public double PicHeight;
        //public double PicResolution;

        
        #endregion//props


        #region ctor
        public GPSCoordinateConvertor()
        {

        }

        public GPSCoordinateConvertor(double leftLatitude, double topLongitude, double rightLatitude, double bottomLongitude, BitmapSource mapImage)
        {
            LeftLatitude = leftLatitude;
            RightLatitude = rightLatitude;
            TopLongitude = topLongitude;
            BottomLongitude = bottomLongitude;
            
            PicHeight = mapImage.Height;
            PicWidth = mapImage.Width; 
        }

        public GPSCoordinateConvertor(double leftLatitude, double topLongitude, double rightLatitude, double bottomLongitude, string mapImageFile)
        {
            LeftLatitude = leftLatitude;
            RightLatitude = rightLatitude;
            TopLongitude = topLongitude;
            BottomLongitude = bottomLongitude;

            // Create source.
            BitmapImage mapImage = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            mapImage.BeginInit();
            mapImage.UriSource = new Uri(mapImageFile, UriKind.RelativeOrAbsolute);
            mapImage.EndInit();


            PicHeight = mapImage.Height;
            PicWidth = mapImage.Width;
        }

        #endregion //ctor


        #region logic

        public Point convertPoint (double longitude, double latitude)
        {
            //ToDo
            throw new NotImplementedException();
        }

        #endregion //logic

    }
}
