using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Xml;
using System.Windows;
using System.Xml.XPath;
using System.IO;
using System.Xml.Linq;
using MyXPathReader;

namespace CONVErT
{
    public class TreeViewViewModel : INotifyPropertyChanged
    {
        #region Data

        private Collection<TreeNodeViewModel> _firstChild;
        private TreeNodeViewModel _rootElement;
        public TreeNodeViewModel Root
        {
            get{ return _rootElement;}
        }   


        public string modelFile;

        #endregion // Data

        #region Properties

        /// <summary>
        /// Returns a read-only collection containing the first person 
        /// in the family tree, to which the TreeView can bind.
        /// </summary>
        public Collection<TreeNodeViewModel> FirstChild
        {
            get { return _firstChild; }
            set
            {
                _firstChild = value;
                OnPropertyChanged("FirstChild");
            }
        }

        public TreeNodeViewModel SelectedItem
        {
            get { return FirstChild.FirstOrDefault(i => i.IsSelected); }
        }

        #endregion // Properties

        #region Constructor

        public TreeViewViewModel(string xmlFileName)
        {
            modelFile = xmlFileName;

            this.initiate(createElementFromXMLFile(modelFile));

            /*//lazy loader
            _rootElement = processXml(modelFile);

            FirstChild = new Collection<TreeNodeViewModel>(
                new TreeNodeViewModel[] 
                { 
                    _rootElement 
                });*/
        }

        public TreeViewViewModel(Element rootElement)
        {
            this.initiate(rootElement);
        }

        public TreeViewViewModel(XmlNode xml)
        {
            this.initiate(createElementFromXMLNode(xml));
        }

        #endregion // Constructor


        #region process XML file (Lazy loaders)

        /// <summary>
        /// Processing based on On-Demand approach
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns></returns>
        private TreeNodeViewModel processXml(string strFile)
        {
            Element root = new Element();//root element 
            TreeNodeViewModel rootNode = null;

            try
            {
                using (var reader = XmlReader.Create(strFile))
                {
                    //reader.MoveToContent();
                    
                    //pass any whitespace or useless element
                    while (reader.NodeType != XmlNodeType.Element)
                        reader.Read();

                    //first element
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        //create starting element
                        root = createModelElement(reader);
                        rootNode = new TreeNodeViewModel(root);

                        if ((reader.NodeType == XmlNodeType.Element) && (reader.HasAttributes || !reader.IsEmptyElement))
                            rootNode.hasChildren = true;
                        else if (reader.IsEmptyElement || reader.NodeType == XmlNodeType.Text)
                            rootNode.hasChildren = false;
                        
                    }
                    else 
                        MessageBox.Show("First element of Model file is not an XML Element","Exception",MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (XmlException e)
            {
                Console.WriteLine("error occured: " + e.Message);
            }

            return rootNode;
        }

        private Element createModelElement(XmlReader reader)
        {
            Element e = new Element();
            e.Name = reader.Name;
            e.Value = reader.Value;
            e.Type = ElementType.ModelElement;

            return e;
        }

        #endregion Process Xml file


        #region element and tree view instantiation

        private void initiate(Element rootElement)
        {
            _rootElement = new TreeNodeViewModel(rootElement);

            FirstChild = new Collection<TreeNodeViewModel>(
                new TreeNodeViewModel[] 
                { 
                    _rootElement 
                });

        }

        private Element createElementFromXMLNode(XmlNode xnode)
        {
            try
            {
                // Initialize Elements
                Element root = new Element();

                // Populate the TreeView with nodes
                populateNode(xnode, root);

                return root;
            }
            catch (XmlException xmlEx)
            {
                System.Windows.MessageBox.Show(xmlEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }

        private Element createElementFromXMLFile(String filename)
        {
            try
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreComments = true;
                using (XmlReader reader = XmlReader.Create(filename, readerSettings))
                {
                    // SECTION 1. Create a DOM Document and load the XML data into it.
                    XmlDocument dom = new XmlDocument();
                    dom.Load(reader);

                    // SECTION 2. Initialize Elements
                    Element root = new Element();

                    // SECTION 3. Populate the TreeView with the DOM nodes.
                    populateNode(dom.DocumentElement, root);

                    return root;
                }
            }
            catch (XmlException xmlEx)
            {
                System.Windows.MessageBox.Show(xmlEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }
        
        private Element populateNode(XmlNode inXmlNode, Element node)
        {

            if (inXmlNode.NodeType == XmlNodeType.Element)
            {
                if (inXmlNode.Attributes.Count > 0)
                {
                    foreach (XmlAttribute attr in inXmlNode.Attributes)
                    {
                        Element attNode = new Element();
                        attNode.Value = attr.Value;
                        attNode.Name = attr.Name;
                        attNode.Type = ElementType.ModelAttribute;

                        node.Children.Add(attNode);

                    }
                }

            }

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                node.Name = inXmlNode.Name;
                node.Type = ElementType.ModelElement;

                XmlNodeList nodeList = inXmlNode.ChildNodes;

                foreach (XmlNode xNode in nodeList)
                {
                    Element temp = new Element();

                    populateNode(xNode, temp);
                    node.Children.Add(temp);
                }

            }

            else
            {
                // Here you need to pull the data from the XmlNode based on the
                // type of node, whether attribute values are required, and so forth.
                if (inXmlNode.NodeType == XmlNodeType.Element)
                {
                    node.Name = inXmlNode.Name;
                    node.Type = ElementType.ModelElement;
                    node.Value = inXmlNode.Value;
                }
                else
                {
                    String value = (inXmlNode.Value);

                    if (!value.Equals(""))
                    {
                        node.Type = ElementType.InstanceValue;
                        node.Name = value;
                        node.Value = value;
                    }
                }
            }

            return node;
        }

        #endregion


        #region Find elements

        public TreeNodeViewModel findInTreeByName(String name)
        {
            TreeNodeViewModel temp = null;

            foreach (TreeNodeViewModel t in FirstChild)
            {
                temp = t.findInChidlsByName(name);
                if (temp != null)
                    return temp;
            }

            return null;
        }

        public TreeNodeViewModel findInTreeByAddress(String name)
        {
            TreeNodeViewModel temp = null;

            foreach (TreeNodeViewModel t in FirstChild)
            {
                temp = t.findInChidlsByAddress(name);
                if (temp != null)
                    return temp;
            }

            return null;
        }

        #endregion //Find elements


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
