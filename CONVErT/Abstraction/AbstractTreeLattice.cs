using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using QuickGraph;

namespace CONVErT 
{
    public class AbstractLattice
    {

        #region properties
        
        //public Collection<AbstractTreeLatticeNode> Test { get; set; }
        private AbstractTreeLatticeNode _root;
        public AbstractTreeLatticeNode Root
        {
            get { return _root; }
        }

        public BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> abstractLatticeGraph;
        
        public double[,] abstractGraphNeighbourMatrix;

        public string ModelFile = "";

        #endregion properties

        #region ctor

        public AbstractLattice()
        {
            _root = null;
        }

        public AbstractLattice(string MF)
        {
            ModelFile = MF;

            //process XMl File
            processModelFile(ModelFile);

            prepare();
        }

        public AbstractLattice(XmlNode modelXml)
        {
            if (modelXml != null)
            {
                _root = new AbstractTreeLatticeNode(modelXml.Name,AbstractTreeNodeType.Element);

                XmlNode x = modelXml.SelectSingleNode("text()");
                if (x != null)
                {
                    if (!String.IsNullOrEmpty(x.InnerText))
                    {
                        _root.addValue(x.InnerText);
                    }
                }

                foreach (XmlAttribute at in modelXml.Attributes)//read attributes
                {
                    AbstractTreeLatticeNode attnode = _root.addChild(at.Name, AbstractTreeNodeType.Attribute, -1);
                    attnode.Values.Add(at.Value);
                }

                foreach (XmlNode xn in modelXml.ChildNodes)
                    if (xn.NodeType != XmlNodeType.Text)
                        processXmlNodes(xn);

                prepare();
            }
        }

        public AbstractLattice(AbstractTreeLatticeNode node)
        {
            if (node != null)
            {

                _root = node.duplicate();
                /*_root = new AbstractTreeLatticeNode(node.Name);

                foreach (string s in node.Values)
                    _root.Values.Add(s);

                foreach (AbstractTreeLatticeNode n in node.Children)
                    _root.Children.Add(n.duplicate());
                */

                prepare();
            }
            else
                MessageBox.Show("Initiation failed, Node is Null");
        }

        public void prepare()
        {
            //create abstract graph
            createGraph();

            //create neighbourhood matrix
            createNeighborhoodMatrix();
        }
                
        #endregion

        #region Xml interaction

        /// <summary>
        /// Reads XML model example file and creates a tree lattice based on it.
        /// </summary>
        /// <param name="modelFile"></param>
        public void processModelFile(String modelFile)
        {
            //Test = new Collection<AbstractTreeLatticeNode>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(modelFile);

            _root = new AbstractTreeLatticeNode(xdoc.DocumentElement.Name,AbstractTreeNodeType.Element);
            //MessageBox.Show(Root.Name);

            XmlNode x = xdoc.DocumentElement.SelectSingleNode("text()");
            if (x != null)
            {
                if (!String.IsNullOrEmpty(x.InnerText))
                {
                    _root.addValue(x.InnerText);
                }
            }

            foreach (XmlAttribute at in xdoc.DocumentElement.Attributes)//read attributes
            {
                AbstractTreeLatticeNode attnode = _root.addChild(at.Name, AbstractTreeNodeType.Attribute, -1);
                attnode.Values.Add(at.Value);
            }

            foreach (XmlNode xn in xdoc.DocumentElement.ChildNodes)
                if (xn.NodeType != XmlNodeType.Text)
                    processXmlNodes(xn);

            //MessageBox.Show("lattice should now be complete\n\n"+ Root.printTreeNode());
            //Test.Add(_root);
        }

        /// <summary>
        /// Reads XML node and creates a tree lattice based on it.
        /// </summary>
        /// <param name="modelFile"></param>
        public void processXmlData(XmlNode x1)
        {
            //detach x1 from its parent
            XmlNode xnode = x1.Clone();

            _root = new AbstractTreeLatticeNode(xnode.Name,AbstractTreeNodeType.Element);
            //MessageBox.Show(Root.Name);

            XmlNode x = xnode.SelectSingleNode("text()");
            if (x != null)
            {
                if (!String.IsNullOrEmpty(x.InnerText))
                {
                    _root.addValue(x.InnerText);
                }
            }

            foreach (XmlAttribute at in xnode.Attributes)//read attributes
            {
                AbstractTreeLatticeNode attnode = _root.addChild(at.Name, AbstractTreeNodeType.Attribute, -1);
                attnode.Values.Add(at.Value);
            }

            foreach (XmlNode xn in xnode.ChildNodes)
                if (xn.NodeType != XmlNodeType.Text)
                    processXmlNodes(xn);

        }

        private void processXmlNodes(XmlNode xnode)
        {
            if (xnode != null)
            {
                //get node address
                string nodeAddress = getXmlNodeAddress(xnode);
                
                //get corresponding node in lattice
                AbstractTreeLatticeNode anode = getAbstractNodeAtAddress(nodeAddress);

                if (anode != null)
                {
                    //if there is a value for xnode add it to the corresponding node
                    XmlNode x = xnode.SelectSingleNode("text()");
                    if (x != null)
                    {
                        if (!String.IsNullOrEmpty(x.InnerText))
                        {
                            anode.addValue(x.InnerText);
                        }
                    }

                    foreach (XmlAttribute at in xnode.Attributes)//read attributes
                    {
                        AbstractTreeLatticeNode attnode = anode.addChild(at.Name, AbstractTreeNodeType.Attribute ,-1);
                        if (attnode != null)
                            attnode.Values.Add(at.Value);
                    }
                }
                else
                    MessageBox.Show("hey anode is null");

                //repeat for children
                foreach (XmlNode xchild in xnode.ChildNodes)
                    if (xchild.NodeType != XmlNodeType.Text)
                        processXmlNodes(xchild);

            }
            else
                MessageBox.Show(" Why do you have null nodes here?");
        }

        #endregion

        #region abstraction

        public AbstractTreeLatticeNode getAbstractNodeAtAddress(string address)
        {
            AbstractTreeLatticeNode node = null;

            //split address and get address elements
            char[] seperators = { '/' };
            string[] addressElements = address.Split(seperators);

            //search for node starting from root
            AbstractTreeLatticeNode temp = Root;

            if ((Root != null) && (!Root.Name.Equals(addressElements[0])))
                MessageBox.Show("Error in processing addresses, \n Root element and address root element does not match!\n\n"
                    + "Root.Name = " + Root.Name + "\nfirst Address node = " + addressElements[0]);
            else
            {
                for (int i = 1; i < addressElements.Length; i++)
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
                        if (addressElements[i].StartsWith("@"))
                            temp = temp.addChild(addressElements[i], AbstractTreeNodeType.Attribute, -1);//add it and continue, change it not to include @ in the name
                        else
                            temp = temp.addChild(addressElements[i], AbstractTreeNodeType.Element, -1);//add it and continue 
                    }
                }

                node = temp;
            }

            return node;
        }

        private string getXmlNodeAddress(XmlNode xEl)
        {
            string s = "";

            XmlNode temp = xEl;
            s = xEl.Name;
            //if (xEl.NodeType == XmlNodeType.Attribute)//consider attributes
            //    s = "@" + s;

            temp = temp.ParentNode;

            while ((temp != null)&&(!temp.Name.Equals("#document")))
            {
                s = temp.Name + "/" + s;
                temp = temp.ParentNode;
            }

            return s;
        }

        public void addConcept(string[] val, int ruleIndex)
        {
            AbstractTreeLatticeNode temp = null;
            AbstractTreeLatticeNode temp2 = null;
            int i = 0;

            temp = searchNodeInTree(val[i]);
            temp2 = temp;
            i++;

            while (i < val.Length & temp != null)//find first node in tree
            {
                temp2 = temp;
                temp = temp.searchNode(val[i]);
                i++;
            }

            if (temp != null)
                temp2 = temp;

            if (temp2 == null) //could not find a match from start
            {
                if (_root != null)
                    MessageBox.Show("Error in assigning rules to root");
                else //first branch to be added
                {
                    if (val[0].StartsWith("@"))//this will not happen
                        _root = new AbstractTreeLatticeNode(val[0].Replace("@",""), AbstractTreeNodeType.Attribute);
                    else
                        _root = new AbstractTreeLatticeNode(val[0], AbstractTreeNodeType.Element);

                    string[] val2 = new string[val.Length - 1];
                    for (int j = 1; j < val.Length; j++)//shift values
                        val2[j - 1] = val[j];

                    _root.addChild(val2, ruleIndex);
                }
            }
            else  //partial matches exist
            {
                if (temp != null) //exact match
                {
                    //temp2.Name = ruleIndex.ToString(); //update rule value of the match
                    //MessageBox.Show("repeated pattern");
                    temp.ruleIndex = ruleIndex;//30/8/2011
                }
                else
                {
                    String[] newVal = new String[val.Length - (i - 1)];
                    for (int k = 0; k < newVal.Length; k++)
                        newVal[k] = val[k + (i - 1)];


                    temp2.addChild(newVal, ruleIndex);

                }
            }

        }

        private AbstractTreeLatticeNode searchNodeInTree(string p)
        {
            if (_root != null)
                return _root.searchNode(p);
            else
                return null;
        }

        public string printTree()
        {
            string s = "";

            s = _root.printTreeNode();

            return s;
        }
        
        #endregion //abstraction

        #region Signature

        /// <summary>
        /// Whether all nodes have been matched
        /// </summary>
        /// <returns></returns>
        public bool isAllNodesMatched()
        {
            if (this.Root.isAllChildrenMatched() == false)
                return false;
            else
                return this.Root.isMatched;
        }

        /// <summary>
        /// Analyse abstract Tree node and check for signature with values from treeVNodeViewModel node
        /// </summary>
        /// <param name="node">null if signature is based on AbstractLattice only</param>
        public XmlNode createSignature(TreeNodeViewModel node)
        {
            XmlDocument xDoc = new XmlDocument();
            
            if (/*this.isAllNodesMatched() ||*/ node == null)
            {
                //create signature form abstract tree
                return this.Root.createSignatureNode();
            }
            else
            {
                //create signature using tree node
                return this.Root.createSignatureNode(node);
                //throw new NotImplementedException();
            }
        }
        
        #endregion //signature

        #region Addressing

        public string findRelativeAddress(AbstractTreeLatticeNode top, AbstractTreeLatticeNode low)
        {
            //check for types of low and top
            if (top.findInChildrenByName(low.Name) != null)
            {
                string lowAddress = low.Address;
                string diff = lowAddress.Substring(lowAddress.IndexOf(top.Address) + 1 + top.Address.Length);//diff includes low.Name
                //MessageBox.Show("top:" + top.Address +"\nlow: "+low.Address+ "\nrelative address: "+diff);
                return diff;
            }
            else
            {
                MessageBox.Show("Element: " + low.Name + " is not child of: " + top.Name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string findRelativeAddress(string topNodeAddress, string lowNodeAddress)
        {
            AbstractTreeLatticeNode top = Root.findChildrenByAddress(topNodeAddress);
            AbstractTreeLatticeNode low = Root.findChildrenByAddress(lowNodeAddress);
            if (top != null && low != null)
                return findRelativeAddress(top, low);
            else
                return null;
        }

        public string RelativeAddressByName(string topNode, string lowNode)
        {
            AbstractTreeLatticeNode top = Root.findInChildrenByName(topNode);//I do not like this! if repeated names are existing, it will results in error
            AbstractTreeLatticeNode low = Root.findInChildrenByName(lowNode);

            if (top == null)
            {
                MessageBox.Show("Element: topNode -> " + topNode + " could not be found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            else if (low == null)
            {
                MessageBox.Show("Element: lowNode -> " + lowNode + " could not be found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            else
                return findRelativeAddress(top, low);
        }

        #endregion

        #region create graph

        private void createGraph()
        {
            abstractLatticeGraph = new BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>>();
            createGraphFromAbstractTreeNode(abstractLatticeGraph, Root);
        }

        private void createGraphFromAbstractTreeNode(BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> g, AbstractTreeLatticeNode n)
        {
            //create the vertex
            g.AddVertex(n);

            //Call for childs
            if (n.Children.Count > 0)
            {
                //create childs and edges
                foreach (AbstractTreeLatticeNode tNode in n.Children)
                {
                    createGraphFromAbstractTreeNode(g, tNode);
                    Edge<AbstractTreeLatticeNode> edge = new Edge<AbstractTreeLatticeNode>(n, tNode);
                    g.AddEdge(edge);
                }
            }

        }

        private void createNeighborhoodMatrix()
        {
            if (abstractLatticeGraph != null)
            {
                abstractGraphNeighbourMatrix = new double[abstractLatticeGraph.VertexCount, abstractLatticeGraph.VertexCount];

                for (int i = 0; i < abstractLatticeGraph.VertexCount; i++)
                    for (int j = 0; j < abstractLatticeGraph.VertexCount; j++)
                    {
                        Edge<AbstractTreeLatticeNode> e;

                        //not considering edges at the moment and considering the graph as an undirected graph
                        if ((abstractLatticeGraph.TryGetEdge(abstractLatticeGraph.Vertices.ElementAt(i), abstractLatticeGraph.Vertices.ElementAt(j), out e))
                            || (abstractLatticeGraph.TryGetEdge(abstractLatticeGraph.Vertices.ElementAt(j), abstractLatticeGraph.Vertices.ElementAt(i), out e)))
                            abstractGraphNeighbourMatrix[i, j] = 1;

                    }

            }
            else
                MessageBox.Show("Graph not set yet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
