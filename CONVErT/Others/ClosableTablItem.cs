using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONVErT
{
    public class CloseableTabItem : TabItem
    {
        #region prop

        CloseableHeader closeableTabHeader;

        public String ModelFile { get; set; }

        public InputModelTreeView ModelTree { get; set; }

        /// <summary>
        /// Property - Set the Title of the Tab
        /// </summary>
        public string Title
        {
            set
            {
                ((CloseableHeader)this.Header).label_TabTitle.Content = value;
            }

            get
            {
                return ((CloseableHeader)this.Header).label_TabTitle.Content as string;
            }
        }

        public AbstractLattice abstractLattice;

        #endregion //prop


        #region ctor

        // Constructor
        //public CloseableTabItem()
        //    : this(null)
        //{

        //}

        public CloseableTabItem(String fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                ModelFile = fileName;
                // Create an instance of the usercontrol
                closeableTabHeader = new CloseableHeader();
                // Assign the usercontrol to the tab header
                this.Header = closeableTabHeader;

                ModelTree = new InputModelTreeView(ModelFile);
                abstractLattice = new AbstractLattice(ModelFile);

                this.Content = ModelTree;

                // Attach to the CloseableHeader events
                // (Mouse Enter/Leave, Button Click, and Label resize)
                closeableTabHeader.button_close.Click += new RoutedEventHandler(button_close_Click);
                closeableTabHeader.label_TabTitle.SizeChanged += new SizeChangedEventHandler(label_TabTitle_SizeChanged);
            }
            else
            {
                //ModelTree = new InputModelTreeView();
                MessageBox.Show("Failed to load input file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                abstractLattice = null;
            }
        }

        #endregion //ctor


        #region events

        // Override OnSelected - Show the Close Button
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        // Override OnUnSelected - Hide the Close Button
        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
        }

        // Override OnMouseEnter - Show the Close Button
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        // Override OnMouseLeave - Hide the Close Button (If it is NOT selected)
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
            }
        }

        #endregion //events


        #region header events


        /// Button Close Click - Remove the Tab - (or raise an event indicating a "CloseTab" event has occurred)
        void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.ModelTree.kill();
            ((TabControl)this.Parent).Items.Remove(this);
        }

        /// Label SizeChanged - When the Size of the Label changes (due to setting the Title) set position of button properly
        void label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((CloseableHeader)this.Header).button_close.Margin = new Thickness(
               ((CloseableHeader)this.Header).label_TabTitle.ActualWidth + 2, 1, 1, 0);
        }

        #endregion //header events
    }
}
