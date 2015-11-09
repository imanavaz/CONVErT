using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class JavaField : StackPanel
    {
        #region properties

        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldNameProperty", typeof(string), typeof(JavaField),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Field
        /// </summary>
        public string FieldName
        {
            get { return (string)GetValue(FieldNameProperty); }
            set { SetValue(FieldNameProperty, value); }
        }

        public static readonly DependencyProperty FieldTypeProperty =
            DependencyProperty.Register("FieldTypeProperty", typeof(string), typeof(JavaField),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Field
        /// </summary>
        public string FieldType
        {
            get { return (string)GetValue(FieldTypeProperty); }
            set { SetValue(FieldTypeProperty, value); }
        }

        public static readonly DependencyProperty FieldAccessProperty =
            DependencyProperty.Register("FieldAccessProperty", typeof(string), typeof(JavaField),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Permission of the Field
        /// </summary>
        public string FieldAccess
        {
            get { return (string)GetValue(FieldAccessProperty); }
            set { SetValue(FieldAccessProperty, value); }
        }

        public static readonly DependencyProperty MultiplicityProperty =
            DependencyProperty.Register("MultiplicityProperty", typeof(string), typeof(JavaField),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Multiplicity of the Field
        /// </summary>
        public string Multiplicity
        {
            get { return (string)GetValue(MultiplicityProperty); }
            set { SetValue(MultiplicityProperty, value); }
        }

        #endregion //properties

        public JavaField()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Orientation = Orientation.Horizontal;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            this.Children.Clear();
            // Create access
            Label accessLabel = new Label();
            accessLabel.Content = FieldAccess;
            accessLabel.Foreground = Brushes.Navy;
            this.Children.Add(accessLabel);

            Label typeLabel = new Label();
            typeLabel.Content = FieldType;
            typeLabel.Foreground = Brushes.Navy;
            this.Children.Add(typeLabel);

            Label arrayLabel = new Label();
            arrayLabel.Content = createArrayLabelContent(Multiplicity);
            this.Children.Add(arrayLabel);

            Label nameLabel = new Label();
            nameLabel.Content = FieldName;
            nameLabel.Foreground = Brushes.LightBlue;
            this.Children.Add(nameLabel);

            Label semicolon = new Label();
            semicolon.Content = ";";
            this.Children.Add(semicolon);
        }

        private object createArrayLabelContent(string multiplicity)
        {

            if (multiplicity.Equals("*"))
                return "[]";
            else if (string.IsNullOrEmpty(multiplicity))
                return "";
            else if (!double.IsNaN(double.Parse(multiplicity)))
            {
                if (double.Parse(multiplicity) == 1.0)
                    return "";
                else
                    return "[" + multiplicity + "]";
            }
            else
                return " ";

        }
    }
}
