using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using MyXPathReader;
using System.Collections.ObjectModel;

namespace CONVErT
{
    /// <summary>
    /// Interaction logic for InputModelTreeView.xaml
    /// </summary>
    public partial class InputModelTreeView : UserControl, INotifyPropertyChanged
    {

        #region props

        public TreeViewViewModel ModelTree { get; set; }

        public static Collection<String> InputModelFiles = new Collection<string>();

        public string ModelFile = "";

        System.Windows.Point modelTreeViewMousePos;

        #endregion //props


        #region ctor

        //public InputModelTreeView():this(null)
        //{}

        public InputModelTreeView(String fileName)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(fileName))
            {
                ModelFile = fileName;

                loadTreeView(ModelFile);

                InputModelFiles.Add(ModelFile);
            }
        }

        #endregion //ctor


        ~InputModelTreeView()
        {
            
        }

        internal void kill()
        {
            InputModelFiles.Remove(this.ModelFile);
            //MessageBox.Show(InputModelFiles.Count.ToString());
        }

        #region destructor



        #endregion //destructor

        #region loading

        public void loadTreeView(string fileName)
        {
            ModelTree = new TreeViewViewModel(fileName);
            
            this.DataContext = ModelTree;

        }

        #endregion //loading


        #region interaction

        private void ModelTreeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //store mouse position
            modelTreeViewMousePos = e.GetPosition(null);
        }

        //used for suggesters
        private void TreeViewItem_LostFocus(object sender, RoutedEventArgs e)
        {
            TreeNodeViewModel selectedElement = ModelTreeView.SelectedItem as TreeNodeViewModel;
            //MessageBox.Show("Bye Bye " + selectedElement.getFullAddress());   

            e.Handled = true;
        }


        //used for suggesters
        private void TreeViewItem_GotFocus(object sender, RoutedEventArgs e)
        {
            //TreeNodeViewModel selectedElement = ModelTreeView.SelectedItem as TreeNodeViewModel;
            
            
            ////traverse logical tree to get to visualiser and find the abstract Lattice node form sourceASTL
            //DependencyObject current2 = LogicalTreeHelper.GetParent(this);
            //while ((!current2.GetType().ToString().Equals("CONVErT.Visualiser"))&& (current2 != null))
            //{
            //    current2 = LogicalTreeHelper.GetParent(current2);
            //}

            ////find the matching source node to make the rule
            //if ((current2 != null) && (current2.GetType().ToString().Equals("CONVErT.Visualiser")))
            //{
            //    Collection<string> tempSuggs = (current2 as CONVErT.Visualiser).visualiserSuggester.getSuggestionStringsFor(selectedElement.getFullAddress());

            //    string test = "";
            //    Canvas c = (current2 as CONVErT.Visualiser).VisElementCanvas;

            //    foreach (string s in tempSuggs)
            //    {
            //        string add = s.Substring(s.IndexOf("->") + 3);
            //        test += add + "\n";
                    
            //        foreach (UIElement u in c.Children)
            //        {
            //            MessageBox.Show(u.GetType().ToString());
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(test))
            //        MessageBox.Show(test);
            //}
            
            //e.Handled = true;
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.OriginalSource;
            TreeNodeViewModel treeNode = (TreeNodeViewModel)tvi.Header;

            if (treeNode != null)
            {
                //MessageBox.Show("collapsed -> " + treeNode.Name);
                treeNode.clearChildren();
                e.Handled = true;
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)e.Source;
            TreeNodeViewModel treeNode = (TreeNodeViewModel)tvi.Header;

            if (treeNode != null)
            {
                string xpathBase = treeNode.getXPath();
                string xpath = xpathBase + "/child::*";
                string xpath2 = xpathBase + "/attribute::*";
                string xpath3 = xpathBase + "/text()";

                XPathCollection xpc = new XPathCollection();
                int query = xpc.Add(xpath);
                int query2 = xpc.Add(xpath2);
                int query3 = xpc.Add(xpath3);

                XPathReader xreader = new XPathReader(ModelTree.modelFile, xpc);

                //MessageBox.Show("expanded -> " + xpath);

                treeNode.generateChildren(xreader);

                e.Handled = true;
            }
            else
                MessageBox.Show("Nist agha!");
            
        }
        
        private void ModelTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            System.Windows.Point mousePos = e.GetPosition(null);
            Vector diff = modelTreeViewMousePos - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||//Minimum was too small
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TreeNodeViewModel selectedElement = ModelTreeView.SelectedItem as TreeNodeViewModel;

                Object data = new DataObject("TreeNodeViewModel", selectedElement);
               
                if (data != null)
                {
                    DragDropEffects dde = DragDropEffects.Copy;
                    DragDropEffects de = DragDrop.DoDragDrop(this.ModelTreeView, data, dde);
                }

            }

        }

        #endregion //interaction


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members

       
    }
}
