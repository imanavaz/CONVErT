using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;

namespace CONVErT
{
    public class UMLOperation : StackPanel
    {
        #region properties

        public static readonly DependencyProperty OperationNameProperty =
            DependencyProperty.Register("OperationNameProperty", typeof(string), typeof(UMLOperation),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the Operation
        /// </summary>
        public string OperationName
        {
            get { return (string)GetValue(OperationNameProperty); }
            set { SetValue(OperationNameProperty, value); }
        }

        public static readonly DependencyProperty ReturnTypeProperty =
            DependencyProperty.Register("ReturnTypeProperty", typeof(string), typeof(UMLOperation),
            new FrameworkPropertyMetadata(""));

        private bool isLoaded = false;

        /// <summary>
        /// Return type of the Operation 
        /// </summary>
        public string ReturnType
        {
            get { return (string)GetValue(ReturnTypeProperty); }
            set { SetValue(ReturnTypeProperty, value); }
        }

        public static readonly DependencyProperty AccessProperty =
            DependencyProperty.Register("AccessProperty", typeof(string), typeof(UMLAttribute),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Permission of the Attribute
        /// </summary>
        public string Access
        {
            get { return (string)GetValue(AccessProperty); }
            set { SetValue(AccessProperty, value); }
        }

        #endregion //properties

        #region ctor
        public UMLOperation()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Orientation = Orientation.Horizontal;
        }
        #endregion


        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                //organise parameters
                System.Collections.ObjectModel.Collection<UIElement> ps = new System.Collections.ObjectModel.Collection<UIElement>();

                if (this.Children.Count > 0)
                {
                    if (this.Children[0] is StackPanel)
                    {
                        foreach (UIElement uie in (this.Children[0] as StackPanel).Children)//to remove parameter children and assign them again later
                        {
                            if ((uie is VisualElement))//||(uie is UMLParam))
                            {//double check for parameter
                                if ((uie as VisualElement).Content is UMLParam)
                                {
                                    ps.Add(uie);
                                }
                            }
                        }

                        foreach (UIElement u in ps)
                            (this.Children[0] as StackPanel).Children.Remove(u);

                    }
                }
                this.Children.Clear();
                //this.UpdateLayout();

                Label accessLabel = new Label();
                accessLabel.Content = createAccessContent(Access);
                this.Children.Add(accessLabel);

                Label nameLabel = new Label();
                nameLabel.Content = OperationName;
                nameLabel.Foreground = Brushes.Black;
                this.Children.Add(nameLabel);

                Label openP = new Label();
                openP.Content = " (";
                this.Children.Add(openP);

                //organise parameters here
                for (int i = 0; i < ps.Count; i++)
                {
                    if ((ps[i] as VisualElement).Content is UMLParam)
                    {
                        if (i > 0) //add comma
                        {
                            Label comma = new Label();
                            comma.Content = ",";
                            this.Children.Add(comma);
                        }
                        //add parameter to operation call sign
                        this.Children.Add(ps[i]);
                    }
                }

                Label closeP = new Label();
                closeP.Content = ")";
                this.Children.Add(closeP);

                if (!String.IsNullOrEmpty(ReturnType))
                {
                    Label colon = new Label();
                    colon.Content = ":";
                    this.Children.Add(colon);
                }

                Label rtypeLabel = new Label();
                rtypeLabel.Content = ReturnType;
                rtypeLabel.Foreground = Brushes.Navy;
                this.Children.Add(rtypeLabel);

                isLoaded = true;
            }
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
