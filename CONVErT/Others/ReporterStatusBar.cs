using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    public class ReporterStatusBar : System.Windows.Controls.Primitives.StatusBar
    {

        #region ctor

        public ReporterStatusBar():base()
        {

        }

        #endregion //ctor

        #region utils

        private void ShowStatus(string msg)
        {
            createMessage(msg, ReportIcon.Info);
        }

        public void ShowStatus(string msg, ReportIcon reportIcon)
        {
            createMessage(msg, reportIcon);
        }

        private void createMessage(string msg, ReportIcon ri)
        {
            clearReportMessage();

            Label lb = new Label();
            lb.Content = msg;

            StackPanel sb = new StackPanel();
            sb.Orientation = Orientation.Horizontal;

            Image image = new Image();
            image.Width = 25;
            image.Height = 25;
            // Create source.
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            switch (ri){

                case(ReportIcon.Error):
                    bi.UriSource = new Uri(@"/Images/Error.png", UriKind.RelativeOrAbsolute);
                    break;
                case (ReportIcon.Busy):
                    bi.UriSource = new Uri(@"/Images/loading.gif", UriKind.RelativeOrAbsolute);
                    break;
                case(ReportIcon.OK):
                    bi.UriSource = new Uri(@"/Images/ok.png", UriKind.RelativeOrAbsolute);
                    break;
                case(ReportIcon.Warning):
                    bi.UriSource = new Uri(@"/Images/Warning.png", UriKind.RelativeOrAbsolute);
                    break;
                case(ReportIcon.Info):
                    bi.UriSource = new Uri(@"/Images/Info.png", UriKind.RelativeOrAbsolute);
                    break;
                default: 
                    bi.UriSource = new Uri(@"/Images/Warning.png", UriKind.RelativeOrAbsolute);
                    break;
            }
            bi.EndInit();
            // Set the image source.
            image.Source = bi;

            sb.Children.Add(image);
            sb.Children.Add(lb);

            this.Items.Add(sb);
        }

        public void clearReportMessage()
        {
            this.Items.Clear();
        }

        #endregion //utils
    }

    public enum ReportIcon
    { Error, Warning, OK, Info, Busy, }
}
