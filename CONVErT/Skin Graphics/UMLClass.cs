using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class UMLClass : Border
    {
        #region properties

        //public System.Collections.ObjectModel.Collection<UIElement> associations = new System.Collections.ObjectModel.Collection<UIElement>();

        public static readonly DependencyProperty ClassNameProperty =
            DependencyProperty.Register("ClassNameProperty", typeof(string), typeof(UMLClass),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Name of the class
        /// </summary>
        public string ClassName
        {
            get { return (string)GetValue(ClassNameProperty); }
            set { SetValue(ClassNameProperty, value); }
        }

        public static readonly DependencyProperty ClassAccessProperty =
            DependencyProperty.Register("ClassAccessProperty", typeof(string), typeof(UMLClass),
            new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Permission of the class
        /// </summary>
        public string ClassAccess
        {
            get { return (string)GetValue(ClassAccessProperty); }
            set { SetValue(ClassAccessProperty, value); }
        }

        public Size classSize;

        private bool isLoaded = false;

        #endregion //properties

        public UMLClass()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.BorderBrush = Brushes.Black;
            this.Background = Brushes.White;
            this.BorderThickness = new Thickness(1, 1, 1, 1);
            this.CornerRadius = new CornerRadius(5, 5, 5, 5);
        }

        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.Equals(new Size(0, 0)))
            {
                base.OnRenderSizeChanged(sizeInfo);
                classSize = sizeInfo.NewSize;
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isLoaded)
                {
                    isLoaded = true;

                    if (this.Child is StackPanel)
                    {
                        //create name header
                        Border nameBorder = new Border();
                        nameBorder.BorderThickness = new Thickness(0, 0, 0, 1);
                        nameBorder.BorderBrush = Brushes.Black;

                        StackPanel stack = new StackPanel();
                        stack.Orientation = Orientation.Horizontal;

                        Label access = new Label();
                        access.Content = createAccessContent(ClassAccess);
                        stack.Children.Add(access);

                        Label name = new Label();
                        name.Content = ClassName;
                        stack.Children.Add(name);

                        nameBorder.Child = stack;

                        (this.Child as StackPanel).Children.Insert(0, nameBorder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public System.Collections.ObjectModel.Collection<UIElement> getAssociations()
        {
            System.Collections.ObjectModel.Collection<UIElement> asses = new System.Collections.ObjectModel.Collection<UIElement>();

            if ((this.Child as StackPanel).Children.Count > 1)
            {
                Canvas assocCanvas = null;
                foreach (UIElement u in (this.Child as StackPanel).Children)
                    if (u is Canvas)
                    {
                        assocCanvas = u as Canvas;
                        break;
                    }

                if (assocCanvas != null)
                {
                    foreach (UIElement u in assocCanvas.Children)
                    {
                        if ((u as VisualElement).Content is UMLAssociation)
                        {
                            asses.Add(u);
                            //((u as VisualElement).Content as UMLAssociation).drawArrow(new Point(0, 0), new Point(300, 300));
                            //Canvas.SetTop((u as VisualElement), 30);
                            //Canvas.SetLeft((u as VisualElement), 5);
                            //Canvas.SetZIndex(u, 6);
                        }
                    }

                }
            }
            return asses;
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
