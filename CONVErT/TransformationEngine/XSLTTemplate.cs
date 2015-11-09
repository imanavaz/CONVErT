using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace CONVErT
{
    /// <summary>
    /// Templates will be initiated by a given code snippet
    /// Then they will be gradually updated by drag and drop on elements,
    /// hence creating correspondances
    /// </summary>
    public class XSLTTemplate
    {
        #region properties

        //to keep header of the template and check elements relative address against it
        private TreeNodeViewModel _headerNode;
        public TreeNodeViewModel HeaderNode
        {
            get { return _headerNode; }
            set
            {
                _headerNode = value;
                TemplateName = _headerNode.getName();
            }
        }

        XmlPrettyPrinter printer = new XmlPrettyPrinter();

        //this will become the tempalet code at the end
        private XmlNode _xnode;
        public XmlNode TemplateXmlNode
        {
            get { return _xnode; }
            set  { 
                _xnode = value;
                
               //nsmgr.AddNamespace(value.Prefix, value.NamespaceURI);
            }// prepareTemplateFromXmlNode(value); }
        }

        //name of the template
        public string TemplateName { get; set; }

        //address ID of template
        public string TemplateAddress { get; set; }

        XmlDocument xdoc;// = new XmlDocument ();
        XmlNamespaceManager nsmgr;

        public Collection<XmlNode> functions = new Collection<XmlNode>();//to save functions

        #endregion

        #region ctor

        public XSLTTemplate()
            : this(null)
        {
        }

        public XSLTTemplate(XmlNode xn)
        {
            TemplateXmlNode = xn;
            //XmlNode importNode = txnode.OwnerDocument.ImportNode(i, true);
            //txnode.AppendChild(importNode);

            xdoc = new XmlDocument();
            nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
            if (xn != null) 
                nsmgr.AddNamespace(xn.Prefix, xn.NamespaceURI);
        }

        #endregion


        /// <summary>
        /// This function clears any  NOt default text that is left in the template
        /// </summary>
        public void checkForLeftovers()
        {
            checkNodeleftover(TemplateXmlNode);
        }

        private void checkNodeleftover(XmlNode x)
        {
            if (x.NodeType != XmlNodeType.Element)
                return;

            if (x.ChildNodes.Count > 1) //has more than one child
            {
                //check and remove texts
                XmlNode textNode = x.SelectSingleNode("text()");
                if (textNode != null)
                    textNode.InnerText = string.Empty;
            }

            foreach (XmlNode xc in x.ChildNodes)
            {
                if (xc.NodeType == XmlNodeType.Element)
                    checkNodeleftover(xc);
            }
        }

        /*public XmlNode prepareTemplateFromXmlNode(XmlNode xnode)
        {
            if (xnode.Name.Equals("item")){
                
                XmlNode txnode = (xdoc.CreateElement(xnode.Attributes.GetNamedItem("shape").InnerText)) as XmlNode;

                foreach (XmlNode i in xnode.ChildNodes){
                    XmlNode importNode = txnode.OwnerDocument.ImportNode(i,true);
                    txnode.AppendChild(importNode);
                }

                //MessageBox.Show(txnode.OuterXml);
                return txnode;
            }else
                return xnode;
        }*/

        public void updateXmlNodeByValue(string node, string value)
        {

            if (node.IndexOf("@") != -1) //is an attribute
            {
                string selectString = node.Substring(0, node.IndexOf("@") - 1);
                string nodeString = node.Substring(node.IndexOf("@") + 1);

                if (selectString.Equals(TemplateXmlNode.Name))
                    selectString = ".";
                else if (selectString.StartsWith(TemplateXmlNode.Name))
                    selectString = selectString.Substring(TemplateXmlNode.Name.Count() + 1);//have not tested this part, is supposed to omit root element and the "/" from selectstring

                //MessageBox.Show("Select : " + selectString + "\n attribute Name : " + nodeString + "\n\n" + TemplateXmlNode.OuterXml);

                XmlNode temp = TemplateXmlNode.SelectSingleNode(selectString);

                if (temp != null)
                {
                    temp.Attributes.RemoveNamedItem(nodeString);

                    XmlNode importNode = temp.OwnerDocument.ImportNode(generateValueOfForAttribute(nodeString, value), true);

                    temp.InsertBefore(importNode, temp.FirstChild);///////
                }
                else
                {
                    MessageBox.Show("Could not locate parent of the attribute", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else //node is element
            {
                nsmgr.AddNamespace(TemplateXmlNode.Prefix, TemplateXmlNode.NamespaceURI);
                XmlNode temp = TemplateXmlNode.SelectSingleNode(node,nsmgr);

                if (temp != null)
                {
                    temp.RemoveAll();
                    XmlNode importNode = temp.OwnerDocument.ImportNode(generateValueOf(value), true);
                    temp.AppendChild(importNode);
                }

                //MessageBox.Show("changed\n\n" + TemplateXmlNode.OuterXml);

                else
                    MessageBox.Show("XSLTTemplate.updateXmlNodeByValue -> Element not found : " + node + "\n\n" + TemplateXmlNode.OuterXml, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void updateXmlNodeByVariableName(string node, string value)
        {

            XmlNode temp = TemplateXmlNode.SelectSingleNode(node);

            if (temp != null)
            {
                temp.RemoveAll();
                XmlNode importNode = temp.OwnerDocument.ImportNode(generateVariableCall(value), true);
                temp.AppendChild(importNode);
            }
            else
                MessageBox.Show("XSLTTemplate.updateXmlNodeByVariableName -> Element not found : " + node + "\n\n" + TemplateXmlNode.OuterXml, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        public void updateXmlNodeByExactValue(string target, string source, string valueMatch)
        {
            //MessageBox.Show("target : " + target + "\n\nSource : " + source + "\n\nValue match : " + valueMatch + "\n\n" + TemplateXmlNode.OuterXml);
            XmlNodeList temp = TemplateXmlNode.SelectNodes(target);
            if (temp.Count > 0)
            {
                //temp.InnerText = (generateValueOf(value));
                foreach (XmlNode x in temp)
                {
                    if (x.InnerText.Equals(valueMatch))
                    {
                        //x.RemoveAll();//no longer needed, check for left overs will delete remaining texts

                        XmlNode importNode = x.OwnerDocument.ImportNode(generateXSLTCall(source), true);
                        x.AppendChild(importNode);

                        //MessageBox.Show("changed\n\n" + TemplateXmlNode.OuterXml);

                        return;
                    }
                    MessageBox.Show("No  " + valueMatch + " :  " + x.OuterXml);

                }

            }
            else
                MessageBox.Show("XSLTTemplate.updateXmlNodeByExactValue -> Element not found : " + target + "\n\n" + TemplateXmlNode.OuterXml);
        }

        public void updateXmlNodeByTempalte(string node, string template, bool replaceElement)
        {

            if (TemplateXmlNode.Name.ToLower().Equals(node.ToLower()) && node.ToLower().Equals("start")) //dangerous, process start element, added 16/05/2012
            {
                TemplateXmlNode.RemoveAll();
                //MessageBox.Show((template));
                XmlNode importNode = TemplateXmlNode.OwnerDocument.ImportNode(generateXSLTCall(template), true);
                TemplateXmlNode = (importNode);//the danger is here, replacing the <start> element with the call
            }
            else
            {
                XmlNode temp = TemplateXmlNode.SelectSingleNode(node);

                if (temp != null)
                {
                    if (replaceElement == true)//replace temp with template call, happens when element (temp) has the same name as the first element of calling template
                    {
                        XmlNode parent = temp.ParentNode;
                        parent.RemoveChild(temp);
                        XmlNode importNode = temp.OwnerDocument.ImportNode(generateXSLTCall(template), true);
                        parent.AppendChild(importNode);
                    }
                    else
                    {
                        XmlNodeList tempEmbeddedText = temp.SelectNodes("text()");

                        //remove all text childs only to let multiple rules to be embedded
                        foreach (XmlNode t in tempEmbeddedText)
                        {
                            temp.RemoveChild(t);
                        }

                        //check this one later
                        temp.RemoveAll(); //might cause problems, does not allow multiple rulesl

                        XmlNode importNode = temp.OwnerDocument.ImportNode(generateXSLTCall(template), true);
                        temp.AppendChild(importNode);
                    }
                }
                else
                    MessageBox.Show("Element not found in XSLT template-updateXmlNodeByTempalte: Element -> " + node + "\n in TemplateXMlNode -> \n" + TemplateXmlNode.OuterXml);
            }
        }

        internal void updateNodeByCondition(string node, XmlNode xNode)
        {
            XmlNode temp = TemplateXmlNode.SelectSingleNode(node);

            if (temp != null)
            {
                temp.RemoveAll();
                temp.AppendChild(temp.OwnerDocument.ImportNode(xNode,true));
                //XmlNode parent = temp.ParentNode;
                //parent.RemoveChild(temp);
                
                //    xNode.AppendChild(xNode.OwnerDocument.ImportNode(temp,true));
            //    parent.AppendChild(parent.OwnerDocument.ImportNode(xNode,true));            
            }
        }

        #region code generation

        public string generateXSLTHeader()
        {
            string XSLTcode = "";
            XSLTcode += "<";
            XSLTcode += "xsl:template match=\"";
            XSLTcode += TemplateName;
            XSLTcode += "\">";
            return XSLTcode;
        }

        public XmlNode generateXSLTCall(string tempalteName)
        {
            //string XSLTcode = "";
            //XSLTcode += "<";
            //XSLTcode += "xsl:apply-templates select=\"";
            //XSLTcode += TemplateName;
            //XSLTcode += "\"";
            //XSLTcode += "/>";

            //return XSLTcode;
            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";

            XmlElement xnode = xdoc.CreateElement(prefix, "apply-templates", testNamespace);
            XmlAttribute xattr = xdoc.CreateAttribute("select");
            xattr.Value = tempalteName;
            xnode.Attributes.Append(xattr);
            //MessageBox.Show("template call : \n" + xnode.OuterXml);

            return xnode;
        }

        private XmlNode generateVariableCall(string value)
        {
            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";

            XmlElement xnode = xdoc.CreateElement(prefix, "copy-of", testNamespace);
            XmlAttribute xattr = xdoc.CreateAttribute("select");
            xattr.AppendChild(xdoc.CreateTextNode(value));
            xnode.Attributes.Append(xattr);

            return xnode;
        }

        

        public string generateXSLTHeaderClose()
        {
            string XSLTcode = "";
            XSLTcode += "</xsl:template>";

            return XSLTcode;
        }

        public XmlNode generateValueOf(string nodeStr)
        {

            //if (!nodeStr.StartsWith("@"))
            //    code = "<xsl:value-of select=\".\"/>";
            //else
            //    code = "<xsl:value-of select=\"" + SourceHeader + "\"/>";
            //code = "<";
            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";

            XmlElement xnode = xdoc.CreateElement(prefix, "value-of", testNamespace);

            XmlAttribute xattr = xdoc.CreateAttribute("select");
            xattr.Value = nodeStr;
            xnode.Attributes.Append(xattr);

            //MessageBox.Show("This is outer xml: \n" + xnode.OuterXml);

            return xnode;
            //code += "xsl:value-of select=\"" + nodeStr; //+ "\"";
            //code += "/>";

            //code += "\n";

            //return code;
        }

        public XmlNode generateValueOfForAttribute(string attrName, string value)
        {

            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";

            XmlElement attributeNode = xdoc.CreateElement(prefix, "attribute", testNamespace);
            XmlAttribute attr = xdoc.CreateAttribute("name");
            attr.AppendChild(xdoc.CreateTextNode(attrName));
            attributeNode.Attributes.Append(attr);

            XmlElement xnode = xdoc.CreateElement(prefix, "value-of", testNamespace);

            XmlAttribute xattr = xdoc.CreateAttribute("select");
            xattr.Value = value;
            xnode.Attributes.Append(xattr);

            attributeNode.AppendChild(xnode);

            return attributeNode;

        }

        public string generateXSLTTemplateCode()
        {
            string code = "";

            code += generateXSLTHeader();
            code += "\n";

            //add Functions to string code
            //foreach (XmlNode f in functions)
            //    code += f.InnerXml;

            code += TemplateXmlNode.OuterXml;//updated code
            code += "\n";
            code += generateXSLTHeaderClose();

            return code;
        }

        /*public string generateXSLTInclude()
        {
            string XSLTcode = "";
            //XSLTcode += "<";
            XSLTcode += "xsl:include href=\"" + TemplateName + ".xsl\"";
            //XSLTcode += "/>";

            return XSLTcode;
        }*/


        #endregion

        /*#region load and save templates

        public bool saveTemplateToFile()
        {
            if (!String.IsNullOrEmpty(Template))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Template);
                doc.Save(@"C:\Documents and Settings\iavazpour\My Documents\Visual Studio 2010\Projects\PrototypeZero\PZeroTest\bin\Debug\CodeTemplates\" + TemplateName + ".xsl");

                return true;
            }

            return false;
        }

        public bool readTemplateFromFile(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {

                string temp = File.ReadAllText(fileName);
                this.Template = temp;

                //get the name from file (could get the name from XMl document as well)
                string tempname = fileName.Substring((fileName.LastIndexOf("\\") + 1), (fileName.IndexOf(".xsl") - (fileName.LastIndexOf("\\"))) - 1);
                this.TemplateName = tempname;

                return true;
            }

            return false;
        }

        #endregion*/


        internal void insertFunctions()
        {
            foreach (XmlNode f in functions)
            {
                XmlNode temp = TemplateXmlNode.FirstChild;

                foreach (XmlNode x in f.ChildNodes)
                    TemplateXmlNode.InsertBefore(TemplateXmlNode.OwnerDocument.ImportNode(x,true),temp);
            }
        }

        
    }
}
