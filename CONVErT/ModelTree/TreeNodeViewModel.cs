using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Xml;
using System.Windows;
using MyXPathReader;

namespace CONVErT
{
    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        #region Properties

        public ObservableCollection<TreeNodeViewModel> Children
        {
            get { return _children; }

            set { _children = value; }
        }

        public string Name
        {
            get { return _element.Name; }
        }

        public string EValue
        {
            get { return _element.Value; }
        }

        public ElementType Type
        {
            get { return _element.Type; }
        }

        public bool hasChildren { get; set; }

        ObservableCollection<TreeNodeViewModel> _children;
        readonly TreeNodeViewModel _parent;
        readonly Element _element;

        #endregion // Properties


        #region Parent

        public TreeNodeViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Parent


        #region Constructors

        public TreeNodeViewModel(Element element)
            : this(element, null)
        {

        }

        public TreeNodeViewModel(Element element, TreeNodeViewModel parent)
        {
            _element = element;
            _parent = parent;

            IsExpanded = true; //defaults

            _children = new ObservableCollection<TreeNodeViewModel>(
                    (from child in _element.Children
                     select new TreeNodeViewModel(child, this))
                     .ToList<TreeNodeViewModel>());

            if (_children.Count > 0)
                hasChildren = true;
            else
                hasChildren = false;
        }


        public Element getElement()
        {
            return _element;
        }

        #endregion // Constructors


        #region XPath

        /// <summary>
        /// Calculates the unique Xpath address to this TreeNode
        /// </summary>
        /// <returns></returns>
        public string getXPath()
        {
            string xpath = "";

            if (Parent != null)
            {
                xpath = Parent.getXPath();

                if (this.Type == ElementType.InstanceValue)
                {
                    return xpath;
                }
                else if (this.Type == ElementType.ModelAttribute)
                {
                    return xpath + "/@" + this.Name;
                }
                else if (this.Type == ElementType.ModelElement)
                {
                    //calculate this element's index
                    int index = 1;// Parent.Children.IndexOf(this);
                    bool foundIt = false;

                    foreach (TreeNodeViewModel child in Parent.Children)
                    {
                        if ((child != this) && (child.Name.Equals(this.Name)))
                            index++;
                        if (child == this)
                        {
                            foundIt = true;
                            break;
                        }
                    }

                    if (foundIt == true)
                        return xpath + "/" + this.Name + "[" + index.ToString() + "]";
                    else
                        MessageBox.Show("Could not locate node : " + this.Name + ", in TreeNodeViewModel.getXPath()", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            else
                return "/" + this.Name;

            return "";
        }

        #endregion //XPath


        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion // NameContainsText


        #region Code Generation

        public string getName()
        {
            if (this.Type == ElementType.ModelAttribute)
                return "@" + this.Name;
            else
                if (this.Children.Count == 0 && this.Parent != null)
                    return this.Parent.Name;
                else
                    return this.Name;
        }

        public string generateXSLTSnippet(TreeNodeViewModel relativeParentNode)
        {
            string XSLTcode = "";
            //XSLTcode += "<";
            XSLTcode += "xsl:value-of select=\"";
            XSLTcode += getRelativeAddress(relativeParentNode);
            XSLTcode += "\"";
            //XSLTcode += "/>";

            return XSLTcode;
        }

        #endregion


        #region Presentation Members

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;

                    /*if ((_isChecked == true) && (this.Parent != null))
                    {
                        if (this.Parent.IsChecked == false)
                            this.Parent.IsChecked = true;
                    }*/

                    if (Children.Count > 0)
                    {
                        foreach (TreeNodeViewModel child in Children)
                            child.IsChecked = value;
                    }

                    this.OnPropertyChanged("IsChecked");
                }
            }
        }

        bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;

                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // Presentation Members


        #region Address calculation

        /// <summary>
        /// get address till parent if this node is a value node
        /// </summary>
        /// <returns>String full node address</returns>
        public string getFullAddress()
        {
            string address = "";

            TreeNodeViewModel temp;

            if (this.Children.Count == 0 && this.Parent != null)
            {
                temp = this.Parent;
            }
            else
            {
                temp = this;
            }
            address = temp.getName();
            temp = temp.Parent;

            while (temp != null)
            {
                address = temp.getName() + "/" + address;
                temp = temp.Parent;
            }

            return address;
        }

        /// <summary>
        /// get exact element name + its value
        /// </summary>
        /// <returns>string absolute node address</returns>
        /// 
        public string getAbsoluteAddress()
        {
            string address = "";
            TreeNodeViewModel temp = this;

            address = this.Name;

            while (temp.Parent != null)
            {
                temp = temp.Parent;
                address = temp.getName() + "/" + address;
            }

            return address;
        }

        public string getRelativeAddress(TreeNodeViewModel node)
        {
            string address = "";
            TreeNodeViewModel temp = this;

            if (this.Children.Count >= 0)
                address = this.getName();

            while (temp.Parent != null)
            {
                if (temp.Parent.isEqual(node))
                    break; // if this goes in header of while loop it will result in exception sometimes

                temp = temp.Parent;
                if (!temp.getName().Equals(address))
                    address = temp.getName() + "/" + address;
            }

            return address;
        }

        public bool isEqual(TreeNodeViewModel node)
        {
            if (node != null)
            {
                if (this.Type == node.Type && this.Name.Equals(node.Name))//this.EValue.Equals(node.EValue) && --> gives exception
                    return true;
            }

            return false;
        }

        public TreeNodeViewModel findInChidlsByName(String n)
        {
            if (this.Name.Equals(n))
                return this;
            else if ((this.Type == ElementType.ModelAttribute)&&(n.StartsWith("@")))
            {
                if (this.Name.Equals(n.Substring(1)))
                    return this;
            }
            else
                foreach (TreeNodeViewModel t in Children)
                {
                    TreeNodeViewModel temp = t.findInChidlsByName(n);
                    if (temp != null)
                        return temp;
                }

            return null;
        }

        public TreeNodeViewModel findInChidlsByAddress(String address)
        {
            if (this.getAbsoluteAddress().Equals(address))
                return this;
            else
                foreach (TreeNodeViewModel t in Children)
                {
                    TreeNodeViewModel temp = t.findInChidlsByAddress(address);
                    if (temp != null)
                        return temp;
                }

            return null;
        }

        #endregion


        #region add child
        //testing from visual element
        public void addChild(TreeNodeViewModel t)
        {
            Children.Add(t);
            OnPropertyChanged("Children");
        }
        #endregion //add child


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members


        #region Lazy Loaders

        /// <summary>
        /// Clear Children (used in Lazy loader for space saving)
        /// </summary>
        public void clearChildren()
        {
            this.Children.Clear();
            OnPropertyChanged("Children");
        }

        public void generateChildren(XPathReader reader)
        {
            reader.MoveToContent();
            int counter = 0;

            while (reader.ReadUntilMatch())
            {
                
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //MessageBox.Show("element -> " + reader.Name);
                        Element newElement = createModelElement(reader);
                        TreeNodeViewModel newNode = new TreeNodeViewModel(newElement, this);

                        //check if it has children
                        if (reader.HasAttributes || !reader.IsEmptyElement)
                            newNode.hasChildren = true;
                        else
                            newNode.hasChildren = false;

                        newNode.IsExpanded = false;
                        this.addChild(newNode);

                        counter++;

                        break;

                    case XmlNodeType.Attribute:
                        Element newAttr = createAttributeElement(reader);
                        TreeNodeViewModel attrNode = new TreeNodeViewModel(newAttr, this);
                        attrNode.hasChildren = false;
                        attrNode.IsExpanded = false;
                        //MessageBox.Show("attribute -> " + reader.Name + " : " + reader.Value);
                        this.Children.Add(attrNode);

                        counter++;

                        break;

                    case XmlNodeType.Text:

                        Element newTextElement = createValueElement(reader);
                        TreeNodeViewModel newTextElementNode = new TreeNodeViewModel(newTextElement, this);
                        newTextElementNode.hasChildren = false;
                        newTextElementNode.IsExpanded = false;
                        //MessageBox.Show("text -> " + reader.Value);
                        this.addChild(newTextElementNode);

                        counter++;

                        break;
                    default:
                        break;
                }
                if (counter > 10)
                    break;
            }

        }

        private Element createModelElement(XmlReader reader)
        {
            Element e = new Element();
            e.Name = reader.Name;
            e.Value = reader.Value;
            e.Type = ElementType.ModelElement;

            return e;
        }

        private Element createAttributeElement(XmlReader reader)
        {
            Element e = new Element();
            e.Name = reader.Name;
            e.Value = reader.Value;
            e.Type = ElementType.ModelAttribute;

            return e;
        }

        private Element createValueElement(XmlReader reader)
        {
            Element e = new Element();
            e.Name = reader.Value;
            e.Value = reader.Value;
            e.Type = ElementType.InstanceValue;

            return e;
        }

        public int calculateDept()
        {
            //calculate node dept
            int dept = 0;
            TreeNodeViewModel temp = this;
            while (temp != null)
            {
                dept++;
                temp = temp.Parent;
            }

            return dept;
        }

        #endregion //Lazy Loaders


        #region toXML()
        //this has been used in VisualElement
        internal XmlNode ToXML()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode node = xDoc.CreateElement(this.Name);
            foreach (TreeNodeViewModel t in this.Children)
            {
                if (t.Type == ElementType.ModelAttribute) //attribute node
                {
                    XmlAttribute attr = xDoc.CreateAttribute(t.Name);
                    attr.AppendChild(xDoc.CreateTextNode(t.EValue));
                    node.Attributes.Append(attr);
                }
                else if (t.Type == ElementType.InstanceValue || t.Children.Count < 1) //value node
                    node.AppendChild(xDoc.CreateTextNode(t.Name));
                else //element node
                    node.AppendChild(node.OwnerDocument.ImportNode(t.ToXML(), true));
            }

            //there is going to be huge data transfer for big models as the reverse process does not consider the abstract and if a 
            // root element is matched the whole document will be here

            return node;
        }
        #endregion //toXML()
    }

}
