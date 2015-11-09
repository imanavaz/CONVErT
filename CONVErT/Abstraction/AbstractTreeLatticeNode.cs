using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;

namespace CONVErT
{
    public class AbstractTreeLatticeNode
    {
        #region prop

        public String Name { get; set; }

        /// <summary>
        /// Either it is a model element, an attribute or a text value
        /// </summary>
        public AbstractTreeNodeType Type { get; set; }

        public String Address {
            get {
                return fullPath();
            }
        }

        /// <summary>
        /// refactored tpe of values
        /// </summary>
        private string _valueType;
        public string ValueType {
            get { return _valueType; } 
        }

        public int ruleIndex { get; set; }

        private bool _isMatched;
        public bool isMatched { 
            get {return _isMatched;}
            set {
                _isMatched = value;

                /*if (this.parent != null && _isMatched == true)
                {
                    bool check = true;
                    foreach (AbstractTreeLatticeNode n in this.parent.Children)
                    {
                        if (n.isMatched == false)
                            check = false;
                    }

                    //if all nodes in the parent has been matched, update parent.isMatched to true
                    if (check == true)
                        this.parent.isMatched = true;
                }*/
            }
        }

        public ObservableCollection<string> Values = new ObservableCollection<string>();

        private Collection<AbstractTreeLatticeNode> _children = new Collection<AbstractTreeLatticeNode>();
        public Collection<AbstractTreeLatticeNode> Children
        {
            get { return _children; }
        }

        public AbstractTreeLatticeNode parent { get; set; }

        #endregion //prop


        #region ctor

        public AbstractTreeLatticeNode(string nodeName, AbstractTreeNodeType type)
            : this(nodeName, null, type, -1)
        {
        }

        public AbstractTreeLatticeNode(string nodeName, AbstractTreeLatticeNode nodeParent, AbstractTreeNodeType type)
            : this(nodeName, nodeParent, type, -1)
        {}

        public AbstractTreeLatticeNode(String nodeName, AbstractTreeLatticeNode nodeParent, AbstractTreeNodeType type, int ruleInd)
        {
            Name = nodeName;
            parent = nodeParent;
            ruleIndex = ruleInd;
            isMatched = false;
            Type = type;
            _valueType = null;//set it to null till values are added
        }

        public AbstractTreeLatticeNode()
        {

        }

        #endregion


        #region Load from reader

        public AbstractTreeLatticeNode loadFromXmlReader(XmlReader reader)
        {
        //    if (reader != null)
        //    {
        //        switch (reader.NodeType)
        //        {
        //            case (XmlNodeType.Element):

            //            this.Name = reader.Name;
            //            ruleIndex = -1;
            //            isMatched = false;
            //            Type = AbstractTreeNodeType.Element;
            //            ValueType = null;//set it to null till values are added
            //            reader.read
            //        case XmlNodeType.Element:
            //            //MessageBox.Show("element -> " + reader.Name);
            //            Element newElement = createModelElement(reader);
            //            TreeNodeViewModel newNode = new TreeNodeViewModel(newElement, this);

            //            //check if it has children
            //            if (reader.HasAttributes)
            //            {
            //                Element newAttr = createAttributeElement(reader);
            //                TreeNodeViewModel attrNode = new TreeNodeViewModel(newAttr, this);
            //                attrNode.hasChildren = false;
            //                attrNode.IsExpanded = false;
            //                //MessageBox.Show("attribute -> " + reader.Name + " : " + reader.Value);
            //                this.Children.Add(attrNode);
            //            }
                        
            //            this.addChild(newNode);


            //            if (reader.
            //            break;

            //        case XmlNodeType.Attribute:
                        

            //            counter++;

            //            break;

            //        case XmlNodeType.Text:

            //            Element newTextElement = createValueElement(reader);
            //            TreeNodeViewModel newTextElementNode = new TreeNodeViewModel(newTextElement, this);
            //            newTextElementNode.hasChildren = false;
            //            newTextElementNode.IsExpanded = false;
            //            //MessageBox.Show("text -> " + reader.Value);
            //            this.addChild(newTextElementNode);

            //            counter++;

            //            break;
            //        default:
            //            break;
            //    }

            //            break;

            //}
            throw new NotImplementedException();
        }

        #endregion //load from reader
        
        
        #region Search node

        public AbstractTreeLatticeNode searchNode(string s)
        {
            //check same name
            if (this.Name.Equals(s))
                return this;
          
            //check in children            
            foreach (AbstractTreeLatticeNode a in Children)
            {    
                AbstractTreeLatticeNode temp = a.searchNode(s);
                if (temp != null)
                    return temp;
            }

            //check in values
            foreach (string value in Values)
            {
                if (value.Equals(s))
                    return this;
            }

            //not found
            return null;
        }

        #endregion


        #region add child

        public AbstractTreeLatticeNode addChild(string s, AbstractTreeNodeType t, int rIndex)
        {
            bool check = false;
            foreach (AbstractTreeLatticeNode c in _children)
                if (c.Name.Equals(s) && c.Type == t)
                {
                    check = true;
                    break;
                }

            if (!check)
            {
                AbstractTreeLatticeNode a = new AbstractTreeLatticeNode(s, this, t, rIndex);

                _children.Add(a);
                return a;
            }
            else
                return null;
        }

        public void addValue(string s)
        {
            if (!String.IsNullOrEmpty(s))
            {
                Values.Add(s);

                //refactor type of that value
                double number1 = 0;
                bool canConvert = double.TryParse(s, out number1);
                if (canConvert)
                {
                    addValueType("double");
                    //MessageBox.Show("found double in : " + this.Address);
                }
                else
                {
                    DateTime date = new DateTime();
                    canConvert = DateTime.TryParse(s, out date);
                    if (canConvert)
                        addValueType("date");
                    else
                    {
                        addValueType("string");
                    }
                }
            }
        }

        private void addValueType(string type)
        {
            if (string.IsNullOrEmpty(this.ValueType))  
                _valueType = type;
            
            else if (ValueType.Equals(type))
                return;
            
            else
            {
                List<string> types = new List<string>();
                types.Add("number");
                types.Add("double");
                types.Add("date");
                types.Add("string");

                int currentIndex = types.IndexOf(ValueType);
                int commingIndex = types.IndexOf(type);

                if (commingIndex < currentIndex)
                    _valueType = type;
            }
        }

        /// <summary>
        /// Add child used for Horus integration
        /// </summary>
        /// <param name="values"></param>
        /// <param name="rIndex"></param>
        public void addChild(string[] values, int rIndex)
        {
            //TreeNode tNode = new TreeNode();
            if (values.Length > 0)//there exists nodes
            {
                AbstractTreeLatticeNode temp;

                if (values[0].StartsWith("@"))
                    temp = this.addChild(values[0].Replace("@",""), AbstractTreeNodeType.Attribute, -1);
                else
                    temp = this.addChild(values[0], AbstractTreeNodeType.Element, -1);

                int j = 1;     
          
                while (j < values.Length)
                {
                    //temp = temp.addChild(values[j], -1);
                    if (values[j].StartsWith("@"))//consider attributes, not sure if it works
                        temp = temp.addChild(values[j].Replace("@", ""), AbstractTreeNodeType.Attribute, -1);
                    else
                        temp = temp.addChild(values[j], AbstractTreeNodeType.Element, -1);
                    j++;
                }

                temp.ruleIndex = rIndex; //add rule index to the last element (the actual element to be matched)

            }
            //else
              //  MessageBox.Show("No String to create Node from");

        }
            
        #endregion //add child


        #region print

        internal string printTreeNode()
        {
            string s = "";

            if (!String.IsNullOrEmpty(this.Name))
            {
                s += "\t" + this.Name;

                foreach (AbstractTreeLatticeNode a in Children)
                    s += a.printTreeNode()+ "\n";

            }

            return s;
        }

        #endregion


        #region find 
        
        private string fullPath()
        {
            string s = "";

            AbstractTreeLatticeNode temp = this;
            if (this.Type == AbstractTreeNodeType.Attribute)
                s = "@" + temp.Name;
            else
                s = temp.Name;

            temp = temp.parent;

            while (temp != null)
            {
                s = temp.Name + "/" + s;
                temp = temp.parent;
            }

            return s;
        }

        internal AbstractTreeLatticeNode findInChildrenByName(string p)//added processing fro attributes
        {
            if (this.Name.Equals(p))
                return this;
            else if ((this.Type == AbstractTreeNodeType.Attribute) && (p.StartsWith("@")))
            {
                if (this.Name.Equals(p.Substring(1)))
                    return this;            
            }
            else
                foreach (AbstractTreeLatticeNode c in Children)
                {
                    AbstractTreeLatticeNode yes = c.findInChildrenByName(p);
                    if (yes != null)
                        return yes;
                }
            
            return null;
        }

        internal AbstractTreeLatticeNode find(AbstractTreeLatticeNode toFind)
        {
            if (this.Name.Equals(toFind.Name) && this.Address.Equals(toFind.Address) && this.Type == toFind.Type)
                return this;
            else
                foreach (AbstractTreeLatticeNode c in Children)
                {
                    AbstractTreeLatticeNode yes = c.find(toFind);
                    if (yes != null)
                        return yes;
                }
            return null;
        }
        
        /// <summary>
        /// Checks for any AbstractTreeLatticeNode within children of this node having the given address 
        /// </summary>
        /// <param name="add">Relative address of the node to search for, excluding the current node</param>
        /// <returns></returns>
        public AbstractTreeLatticeNode findChildrenByAddress(string add)
        {
            //split address and get address elements
            char[] seperators = { '/' };
            string[] addressElements = add.Split(seperators);
            
            //search for node starting from root
            AbstractTreeLatticeNode temp = this;

            if (!temp.Name.Equals(addressElements[0]))
                return null;

            for (int i = 1; i < addressElements.Length; i++)//assuming first element of the address is self
                {
                    bool check = false;

                    foreach (AbstractTreeLatticeNode a in temp.Children)
                        if (a.Name.Equals(addressElements[i]))
                        {
                            temp = a;
                            check = true;
                            break;
                        }

                    if (check == false)// have not found the element address
                    {
                        //temp = temp.addChild(addressElements[i], -1);//add it and continue 
                        return null;
                    }
                }

            return temp;
        }
        
        #endregion


        #region to XML
        
        internal XmlNode ToXML()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode node = xDoc.CreateElement(this.Name);

            if (Values.Count > 0)//take one of the text elements as representatives
                node.AppendChild(xDoc.CreateTextNode(Values[0]));

            foreach (AbstractTreeLatticeNode t in this.Children)
            {
                if (t.Type == AbstractTreeNodeType.Attribute) //attribute node
                {
                    XmlAttribute attr = xDoc.CreateAttribute(t.Name);
                    if (t.Values.Count > 0)
                        attr.AppendChild(xDoc.CreateTextNode(t.Values[0]));//take representative text of the value of attribute
                    else
                        attr.AppendChild(xDoc.CreateTextNode(""));

                    node.Attributes.Append(attr);
                }
                else if (t.Type == AbstractTreeNodeType.Element) //element node
                    node.AppendChild(node.OwnerDocument.ImportNode(t.ToXML(), true));
            }

            return node;
        }
        
        #endregion
        

        #region Signature

        public bool isAllChildrenMatched()
        {
            foreach (AbstractTreeLatticeNode n in this.Children)
                if (n.isMatched == false)
                    return false;
                else
                    return n.isAllChildrenMatched();
            
            //return isMatched; //considering self as well
            return true;
        }
        
        public XmlNode createSignatureNode()
        {
            XmlDocument xDoc = new XmlDocument ();
            XmlNode xnode = xDoc.CreateElement(this.Name) as XmlNode;

            if (this.ruleIndex != -1)
            {
                XmlAttribute attr = xDoc.CreateAttribute("ruleIndex");
                attr.AppendChild(xDoc.CreateTextNode(this.ruleIndex.ToString()));//fetch the value
                
                xnode.Attributes.Append(attr);
            }

            foreach (AbstractTreeLatticeNode n in this.Children)
                xnode.AppendChild(xnode.OwnerDocument.ImportNode(n.createSignatureNode(),true));

            return xnode;
        }

        public XmlNode createSignatureNode(TreeNodeViewModel node)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode xnode = null;

            if (this.isMatched == true)
            {
                xnode = xDoc.CreateElement(this.Name) as XmlNode;
                
                if (this.ruleIndex != -1)
                {
                    XmlAttribute attr = xDoc.CreateAttribute("ruleIndex");
                    attr.AppendChild(xDoc.CreateTextNode(this.ruleIndex.ToString()));//fetch the value

                    xnode.Attributes.Append(attr);
                }

                //continue for children
                foreach (AbstractTreeLatticeNode n in this.Children)
                {
                    TreeNodeViewModel tn = node.findInChidlsByName(n.Name);
                    if (tn != null)
                        xnode.AppendChild(xnode.OwnerDocument.ImportNode(n.createSignatureNode(tn),true));
                    else
                    {
                        xnode.AppendChild(xnode.OwnerDocument.ImportNode(n.createSignatureNode(),true));
                        MessageBox.Show("Could not find \"" + n.Name + "\" in treeNode");
                    }
                }
            }
            else
            {
                xnode = xDoc.CreateElement(this.Name) as XmlNode;
                //get values (children with only one child and no further dicendant) from node

                TreeNodeViewModel temp = node.findInChidlsByName(this.Name);
                XmlAttribute attr = xDoc.CreateAttribute("value");
                //attr.AppendChild(xDoc.CreateTextNode(temp.Children.ElementAt(0).Name));//fetch the value
                attr.AppendChild(xDoc.CreateTextNode(temp.EValue));//fetch the value, not sure, added to get rid of an exception

                xnode.Attributes.Append(attr);
            }

            return xnode;
        }

        #endregion //Signature


        #region Duplicate
        
        /// <summary>
        /// Duplicate node, keep node names and children names, No values, No rule indexes and all isMatched are false
        /// </summary>
        /// <returns></returns>
        public AbstractTreeLatticeNode duplicate()
        {
            AbstractTreeLatticeNode newNode = new AbstractTreeLatticeNode(this.Name, this.Type);

            foreach (string s in this.Values)
                newNode.Values.Add(s);

            foreach (AbstractTreeLatticeNode child in this.Children)
                newNode.Children.Add(child.duplicate());

            return newNode;
        }
        
        #endregion
        
    }

    public enum AbstractTreeNodeType
    {
        Element,
        Attribute,
        Text,
        PlaceHolder //added for Horus integration
    }
}
