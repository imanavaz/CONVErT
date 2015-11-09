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

namespace CONVErT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Visualiser vWindow;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = System.Windows.WindowState.Maximized;

            vWindow = new Visualiser();
            VisualiserTab.Content = vWindow;

            Mapper mWindow = new Mapper();
            MapperTab.Content = mWindow;

            Skin sWindow = new Skin();
            SkinTab.Content = sWindow;

            string elementsFile = DirectoryHelper.getFilePathExecutingAssembly("Resources\\ToolBoxItems.xml");
            (vWindow as Visualiser).loadToolboxes(elementsFile);

        }

        //private void VisualiserTab_GotFocus(object sender, RoutedEventArgs e)
        //{
            //if (tabControl1.SelectedIndex == 0)
              //  (vWindow as Visualiser).loadToolboxes();
        //}

        /*private void Visualiser_Click(object sender, RoutedEventArgs e)
        {
            //Visualiser eWindow = new Visualiser();

            //eWindow.Show();
        }

        private void Mapper_Click(object sender, RoutedEventArgs e)
        {
            //Mapper mWindow = new Mapper();

            //mWindow.Show();
        }*/

        public void executeClose()
        {
            this.Close();
        }

    }
}
