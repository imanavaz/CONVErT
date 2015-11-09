using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class UMLParam : StackPanel
    {
        #region properties
        /*
        public static readonly DependencyProperty ParamNameProperty =
            DependencyProperty.Register("ParamNameProperty", typeof(string), typeof(UMLParam),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Parameter
        /// </summary>
        public string ParameterName
        {
            get { return (string)GetValue(ParamNameProperty); }
            set { SetValue(ParamNameProperty, value); }
        }

        public static readonly DependencyProperty ParamTypeProperty =
            DependencyProperty.Register("ParamTypeProperty", typeof(string), typeof(UMLParam),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Parameter
        /// </summary>
        public string ParameterType
        {
            get { return (string)GetValue(ParamTypeProperty); }
            set { SetValue(ParamTypeProperty, value); }
        }
        */

        #endregion //properties

        public UMLParam(): base()
        {
            //this.Loaded += new RoutedEventHandler(OnLoad);
            //this.Orientation = Orientation.Horizontal;
        }

        /*
        void OnLoad(object sender, RoutedEventArgs e)
        {
            Label nameLabel = new Label();
            nameLabel.Content = ParameterName;
            nameLabel.Foreground = Brushes.Black;
            this.Children.Add(nameLabel);

            Label colon = new Label();
            colon.Content = " : ";
            this.Children.Add(colon);

            Label typeLabel = new Label();
            typeLabel.Content = ParameterType;
            typeLabel.Foreground = Brushes.Navy;
            this.Children.Add(typeLabel);

        }*/

    }
}
