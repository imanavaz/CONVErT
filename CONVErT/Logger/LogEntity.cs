using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    public class LogEntity : StackPanel
    {
        public String LogString { get; set; }

        //public LogEntity()
        //{
        //    LogString = "";
        //    this.Orientation = Orientation.Horizontal;
        //}

        public LogEntity(String s)
        {
            this.Orientation = Orientation.Horizontal;
            createEntity(s, ReportIcon.Info);
        }

        public LogEntity(String s, ReportIcon ri)
        {
            this.Orientation = Orientation.Horizontal;
            createEntity(s, ri);
        }


        private void createEntity(string msg, ReportIcon ri)
        {
            Label lb = new Label();
            lb.Content = msg;
            Image image = new Image();
            image.Width = 15;
            image.Height = 15;
            // Create source.
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            if (ri == ReportIcon.Error)
                bi.UriSource = new Uri(@"/Images/Error.png", UriKind.RelativeOrAbsolute);
            else if (ri == ReportIcon.OK)
                bi.UriSource = new Uri(@"/Images/ok.png", UriKind.RelativeOrAbsolute);
            else if (ri == ReportIcon.Warning)
                bi.UriSource = new Uri(@"/Images/Warning.png", UriKind.RelativeOrAbsolute);
            else if (ri == ReportIcon.Info)
                bi.UriSource = new Uri(@"/Images/Info.png", UriKind.RelativeOrAbsolute);

            bi.EndInit();
            // Set the image source.
            image.Source = bi;

            this.Children.Add(image);
            this.Children.Add(lb);
        }
    }
}
