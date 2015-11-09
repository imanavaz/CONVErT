using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;

namespace CONVErT
{
    public class Suggestion : ContentControl, IComparable<Suggestion>
    {
        #region props

        //scroing and suggester parts
        public int i;
        public int j;
        public double score;
        
        //representation string
        public string SuggestionString 
        {
            get { return LHS + " -> " + RHS; }
        }
        
        //left and right
        public string LHS { get; set; }
        public string RHS { get; set; }

        //accept reject buttons
        Button btaccept = new Button();
        Button btreject = new Button();

        //private ParentWindowFinder pwFinder = new ParentWindowFinder();
        public DependencyObject ParentWindow;

        #endregion //props

        
        #region ctor

        public Suggestion()
        {
            LHS = "";
            RHS = "";
        }

        public Suggestion(String lAddress, String rAddress, DependencyObject pw)
        {
            LHS = lAddress;
            RHS = rAddress;

            ParentWindow = pw;

            //make contents
            prepareRepresentation();

            btaccept.Click += new RoutedEventHandler(acceptButtonClicked);
            btreject.Click += new RoutedEventHandler(rejectButtonClicked);
        }

        #endregion //ctor


        public void prepareRepresentation()
        {
            //prepare values
            //supposed to get element name and its parent only, not the whole address
            
            //prepare visual 
            DockPanel container = new DockPanel();
            double myMargin = 1;
            container.Margin = new Thickness(myMargin);
            container.Height = 27;
            
            //suggestion buttons
            StackPanel spbuttons = new StackPanel();
            spbuttons.Orientation = Orientation.Horizontal;

            //accept button
            btaccept.Height = 27;
            btaccept.Width = 27;
            btaccept.BorderBrush = Brushes.Transparent;
            btaccept.Background = Brushes.Transparent;
            Image image = new Image();
            image.Width = 18;
            image.Height = 18;
            // Create source.
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(@"/Images/Tick.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            image.Source = bi;
            btaccept.Content = image;
            spbuttons.Children.Add(btaccept);

            //reject button
            btreject.Height = 27;
            btreject.Width = 27;
            btreject.BorderBrush = Brushes.Transparent;
            btreject.Background = Brushes.Transparent;
            Image image2 = new Image();
            image2.Width = 18;
            image2.Height = 18;
            // Create source.
            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.UriSource = new Uri(@"/Images/Cross.png", UriKind.RelativeOrAbsolute);
            bi2.EndInit();
            // Set the image source.
            image2.Source = bi2;
            btreject.Content = image2;
            spbuttons.Children.Add(btreject);

            DockPanel.SetDock(spbuttons, Dock.Left);
            container.Children.Add(spbuttons);

            //suggestion text
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Height = 25;

            Label mapLabel = new Label();
            mapLabel.Content = "Map ";
            mapLabel.Foreground = Brushes.Black;
            sp.Children.Add(mapLabel);

            Label lLabel = new Label();
            lLabel.Content = LHS;
            lLabel.Foreground = Brushes.DarkRed;
            sp.Children.Add(lLabel);

            Label toLabel = new Label();
            toLabel.Content = " To ";
            toLabel.Foreground = Brushes.Black;
            sp.Children.Add(toLabel);

            Label rLabel = new Label();
            rLabel.Content = RHS;
            rLabel.Foreground = Brushes.DarkBlue;
            sp.Children.Add(rLabel);

            DockPanel.SetDock(sp, Dock.Right);
            container.Children.Add(sp);

            this.Content = container;
        }

        #region button pressed events

        private void acceptButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ParentWindow is Visualiser)
            {
                if ((ParentWindow as Visualiser).acceptSuggestion(this))//if successfully executed
                    disableButtons();
            }
            else if (ParentWindow is Mapper)
            {
                if ((ParentWindow as Mapper).acceptSuggestion(this))//if successfully executed
                    disableButtons();
            }

            e.Handled = true;
        }

        private void rejectButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ParentWindow is Visualiser)
            {
                (ParentWindow as Visualiser).visualiserSuggester.retrieveSuggestion(this.SuggestionString, "REJECT");
                disableButtons();
            }
            else if (ParentWindow is Mapper)
            {
                (ParentWindow as Mapper).mapperSuggester.retrieveSuggestion(this.SuggestionString, "REJECT");
                disableButtons();
            }
           

            e.Handled = true;
        }

        private void disableButtons()
        {
            //disable buttons
            btaccept.IsEnabled = false;
            btreject.IsEnabled = false;

            //change images
            Image image = new Image();
            image.Width = 18;
            image.Height = 18;
            // Create source.
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(@"/Images/TickD.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            image.Source = bi;
            btaccept.Content = image;

            Image image2 = new Image();
            image2.Width = 18;
            image2.Height = 18;
            // Create source.
            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.UriSource = new Uri(@"/Images/CrossD.png", UriKind.RelativeOrAbsolute);
            bi2.EndInit();
            // Set the image source.
            image2.Source = bi2;
            btreject.Content = image2;

        }

        #endregion //button pressed evenets

        public int CompareTo(Suggestion other)
        {
            return this.score.CompareTo(other.score);
        }


        public override string ToString()
        {
            return "LHS: " + LHS + " RHS: " + RHS;// +"\ni: " + i.ToString() + " j: " + j.ToString() + " score: " + score.ToString();
        }
    }
   
   
}
