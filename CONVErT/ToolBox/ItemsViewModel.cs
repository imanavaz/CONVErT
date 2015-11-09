using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using System.Windows.Media;

namespace CONVErT
{
    public class ItemsViewModel
    {
        public ObservableCollection<ToolboxItem> items = new ObservableCollection<ToolboxItem>();

        public ItemsViewModel()
        {

        }

        public ItemsViewModel(String path)
        {
            loadItems(path);
        }

        public void loadItems(string XMLItemsFile)
        {

            if (System.IO.File.Exists(XMLItemsFile))
            {
                try
                {

                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(XMLItemsFile, readerSettings))
                    {
                        // SECTION 1. Create a DOM Document and load the XML data into it.
                        XmlDocument dom = new XmlDocument();
                        dom.Load(reader);

                        // SECTION 2. Initialize Elements
                        XmlNode inXmlNode = dom.DocumentElement;

                        // SECTION 3. Populate Items with the DOM nodes.
                        foreach (XmlNode xnode in inXmlNode.ChildNodes)
                        {
                            if (xnode.Name.Equals("item"))
                            {
                            ToolboxItem t = new ToolboxItem((XmlElement)xnode);
                            if (t!= null)
                                items.Add(t);
                            }
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    System.Windows.MessageBox.Show(xmlEx.Message);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("Could not load Items file : " + XMLItemsFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /*private ToolboxItem createItem(XmlNode itemXmlNode)
        {
            ToolboxItem item = new ToolboxItem();

            foreach (XmlNode xnode in itemXmlNode.ChildNodes)
            {

                //<shape>Rectangle</shape>
                //<color>
                //    <foreground>Black</foreground>
                //    <background>White</background>
                //</color>
                //<size>
                //    <width>10</width>
                //    <height>20</height>
                //</size>
                
                switch (xnode.Name)
                {
                    case "shape":
                        //item.Shape = xnode.FirstChild.Value;
                        //MessageBox.Show(xnode.Name + "    " + item.Shape);
                        break;
                    
                    case "color":
                        XmlNode xfor = xnode.SelectSingleNode("//foreground");
                        // best, using Color's static method
                        item.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(xfor.FirstChild.Value);

                        item.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(xfor.FirstChild.Value);
                        XmlNode xback = xnode.SelectSingleNode("//background");
                        item.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(xback.FirstChild.Value);

                        //MessageBox.Show(xnode.Name + "    " + item.Foreground.ToString() + "  " + item.Background.ToString());
                        break;
                    
                    case "size":
                        XmlNode xWidth = xnode.SelectSingleNode("//width");
                        item.Width = Convert.ToDouble(xWidth.FirstChild.Value);
                        
                        XmlNode xHeight = xnode.SelectSingleNode("//height");
                        item.Height = Convert.ToDouble(xHeight.FirstChild.Value);

                        //MessageBox.Show(xnode.Name + "    " + item.width + "  " + item.height);

                        break;
                    
                    default:
                        MessageBox.Show("Something new came in items xml file!");
                        break;
                }
            }
            

            return item;
        }*/
    }
}
