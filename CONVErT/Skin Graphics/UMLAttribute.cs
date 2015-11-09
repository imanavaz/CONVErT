using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class UMLAttribute: StackPanel
    {
        #region properties

        public static readonly DependencyProperty AttributeNameProperty =
            DependencyProperty.Register("AttributeNameProperty", typeof(string), typeof(UMLAttribute),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Attribute
        /// </summary>
        public string AttributeName
        {
            get { return (string)GetValue(AttributeNameProperty); }
            set { SetValue(AttributeNameProperty, value); }
        }

        public static readonly DependencyProperty AttributeTypeProperty =
            DependencyProperty.Register("AttributeTypeProperty", typeof(string), typeof(UMLAttribute),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Attribute
        /// </summary>
        public string AttributeType
        {
            get { return (string)GetValue(AttributeTypeProperty); }
            set { SetValue(AttributeTypeProperty, value); }
        }

        public static readonly DependencyProperty AttrAccessProperty =
            DependencyProperty.Register("AttrAccessProperty", typeof(string), typeof(UMLAttribute),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Permission of the Attribute
        /// </summary>
        public string AttrAccess
        {
            get { return (string)GetValue(AttrAccessProperty); }
            set { SetValue(AttrAccessProperty, value); }
        }



        #endregion //properties

        public UMLAttribute()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Orientation = Orientation.Horizontal;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            this.Children.Clear();
            // Create access
            Label accessLabel = new Label();
            accessLabel.Content = createAccessContent(AttrAccess);
            this.Children.Add(accessLabel);

            Label nameLabel = new Label();
            nameLabel.Content = AttributeName;
            nameLabel.Foreground = Brushes.Black;
            this.Children.Add(nameLabel);

            Label colon = new Label();
            colon.Content = " : ";
            this.Children.Add(colon);

            Label typeLabel = new Label();
            typeLabel.Content = AttributeType;
            typeLabel.Foreground = Brushes.Navy;
            this.Children.Add(typeLabel);

        }

        private string createAccessContent(string access)
        {
            switch (access.ToLower())
            {
                case ("public"):
                    return "+";

                case ("private"):
                    return "-";
                    
                case ("protected"):
                    return "#";
                    
                case ("package"):
                    return "~";
                    
                default:
                    return " ";
            }
        }
    }
}
