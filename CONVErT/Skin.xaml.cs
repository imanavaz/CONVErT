using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Xml;
using System.Windows.Markup;
using System.IO;
using System.Xml.Xsl;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;

namespace CONVErT
{
    /// <summary>
    /// Interaction logic for Skin.xaml
    /// </summary>
    public partial class Skin : ContentControl
    {
        #region props

        XmlPrettyPrinter prettyPrinter = new XmlPrettyPrinter();

        Collection<String> callForList = new Collection<string>();

        public Logger logger;

        VisualisationType visType;

        #endregion //props


        #region ctor

        public Skin()
        {
            InitializeComponent();

            logger = new Logger("SkinLogger");
            DockPanel.SetDock(logger, Dock.Bottom);
            SkinLogPanel.Children.Add(logger);

        }

        #endregion //ctor


        #region Skin logic

       
        private Collection<XmlNode> parseAnnotatedGraphics(string graphicsCode, string dataNodeName, XmlNode XmlDataNode)
        {
            XmlDocument xdoc = new XmlDocument();

            //preparations

            XmlElement templateNode = xdoc.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
            templateNode.Prefix = "xsl";

            XmlAttribute matchAttr = xdoc.CreateAttribute("match");
            matchAttr.Value = dataNodeName;
            templateNode.Attributes.Append(matchAttr);

            if (visType == VisualisationType.XAML)//code will generate visual element
            {
                //visual element node
                XmlElement visualElementNode = xdoc.CreateElement("local", "VisualElement", "clr-namespace:CONVErT;assembly=CONVErT");
                visualElementNode.SetAttribute("xmlns:xsl", "http://www.w3.org/1999/XSL/Transform");
                visualElementNode.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");
                visualElementNode.SetAttribute("xmlns:local", "clr-namespace:CONVErT;assembly=CONVErT");
                visualElementNode.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                visualElementNode.SetAttribute("ToolTip", dataNodeName);
                templateNode.AppendChild(visualElementNode);

                //resources node of visual element
                XmlElement resources = xdoc.CreateElement("local", "VisualElement.Resources", "clr-namespace:CONVErT;assembly=CONVErT");
                visualElementNode.AppendChild(resources);

                //generate data node in Visual element
                XmlElement xmlDataProvider = xdoc.CreateElement("XmlDataProvider", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                xmlDataProvider.SetAttribute("Key", "http://schemas.microsoft.com/winfx/2006/xaml", "sourceData");
                xmlDataProvider.SetAttribute("XPath", "/" + dataNodeName);
                xmlDataProvider.SetAttribute("IsInitialLoadEnabled", "True");
                xmlDataProvider.SetAttribute("IsAsynchronous", "False");
                resources.AppendChild(xmlDataProvider);

                XmlElement xdataNode = xdoc.CreateElement("x", "XData", "http://schemas.microsoft.com/winfx/2006/xaml");

                //Add placeholder attributes to the data for differentiation in the element list. 
                addPlaceHolders(XmlDataNode);//(it should be positioned after remove callfors.)!!!!
                xdataNode.AppendChild(xdataNode.OwnerDocument.ImportNode(XmlDataNode, true));
                xmlDataProvider.AppendChild(xdataNode);

                //load template 
                XmlDocument xdoc2 = new XmlDocument();
                xdoc2.LoadXml(graphicsCode);
                XmlNode restOfTheCode = templateNode.OwnerDocument.ImportNode(xdoc2.DocumentElement, true);
                visualElementNode.AppendChild(visualElementNode.OwnerDocument.ImportNode(restOfTheCode, true));

                //MessageBox.Show(prettyPrinter.PrintToString(visualElementNode.OuterXml));
                //MessageBox.Show(prettyPrinter.PrintToString(templateNode.OuterXml));
            }
            else if (visType == VisualisationType.SVG)
            {
                //load template 
                XmlDocument xdoc2 = new XmlDocument();
                xdoc2.LoadXml(graphicsCode);
                XmlNode restOfTheCode = templateNode.OwnerDocument.ImportNode(xdoc2.DocumentElement, true);
                templateNode.AppendChild(templateNode.OwnerDocument.ImportNode(restOfTheCode, true));
            }

            //parse linktos
            removeLinkto(templateNode);

            //parse callfors
            Collection<XmlNode> templates = new Collection<XmlNode>();
            templates.Add(templateNode);
            removeCallfor(templateNode, templates);

            //put transformation on the GUI
            TextRange transTextRange = new TextRange(GeneratedTransformationTextBox.Document.ContentStart, GeneratedTransformationTextBox.Document.ContentEnd);
            transTextRange.Text = prettyPrinter.PrintToString(templateNode.OuterXml);

            //pretty print it to results
            //String result = prettyPrinter.PrintToString(templateNode);

            //foreach (XmlNode x in templates)
            //    result += prettyPrinter.PrintToString(x);

            //return result;

            return templates;
        }

        private string generate_Data(string annotatedXaml, string notationName)
        {
            XmlDocument dataDoc = new XmlDocument();
            string notationNameNoSpace = notationName.Replace(" ", "");
            XmlNode dataNode = dataDoc.CreateElement(notationNameNoSpace + "Data");

            XmlDocument xdocXaml = new XmlDocument();
            xdocXaml.LoadXml(annotatedXaml);
            XmlNode node = xdocXaml.DocumentElement;

            if (node.NodeType == XmlNodeType.Element)
            {
                try
                {
                    XmlNodeList attributedNodes = node.SelectNodes("//*[@*]");//all nodes with attributes

                    foreach (XmlNode xnode in attributedNodes)
                    {
                        foreach (XmlAttribute xattr in xnode.Attributes)//all attributes in the node
                        {
                            if (xattr.Name.Equals("linkto"))
                            {
                                //MessageBox.Show("found linkto -> " + xattr.Name +":"+ xattr.Value);
                                string linksto = xattr.Value;

                                XmlNodeList temp = dataNode.SelectNodes(linksto);
                                if (temp.Count <= 0)//if the item has not been added before
                                {
                                    XmlNode childDataNode = dataDoc.CreateElement(linksto);
                                    childDataNode.AppendChild(dataDoc.CreateTextNode(xnode.InnerText));

                                    dataNode.AppendChild(childDataNode);
                                }
                            }
                            else if (xattr.Name.Equals("callfor"))
                            {
                                //MessageBox.Show("found callfor -> " + xattr.Name + ":" + xattr.Value);
                                string callsfor = xattr.Value;

                                XmlNodeList temp = dataNode.SelectNodes(callsfor);
                                if (temp.Count <= 0)//if the item has not been added before
                                {
                                    XmlNode childDataNode = dataDoc.CreateElement(callsfor);
                                    childDataNode.AppendChild(dataDoc.CreateTextNode(callsfor.ToLower()));

                                    dataNode.AppendChild(childDataNode);
                                }
                            }
                            else if (xattr.Value.StartsWith("@linkto"))
                            {
                                //MessageBox.Show("found @linkto -> " + xattr.Name + ":" + xattr.Value);

                                //linkto generates an attribute
                                string linksto = xattr.Value.Substring(xattr.Value.IndexOf("=") + 1); //remove @linkto=, sample format is Fill="@linkto=Color|Green"
                                string linkstoAttributeValue;
                                string linkstoAttributeDefault;

                                if (linksto.Contains("|"))//"Color|Green"
                                {
                                    linkstoAttributeDefault = linksto.Substring(linksto.IndexOf("|") + 1);
                                    linkstoAttributeValue = linksto.Substring(0, linksto.IndexOf("|"));
                                }
                                else//"Color"
                                {
                                    linkstoAttributeValue = linksto;
                                    linkstoAttributeDefault = "";
                                }

                                XmlNodeList temp = dataNode.SelectNodes(linkstoAttributeValue);
                                if (temp.Count <= 0)//if the item has not been added before
                                {
                                    XmlNode childDataNode = dataDoc.CreateElement(linkstoAttributeValue);
                                    childDataNode.AppendChild(dataDoc.CreateTextNode(linkstoAttributeDefault));

                                    dataNode.AppendChild(childDataNode);
                                }

                            }
                        }

                    }

                    logger.log("Data for Notation \'" + notationName + "\' is generated.", ReportIcon.OK);

                }
                catch (Exception ex)
                {
                    ReportStatusBar.ShowStatus("Generating data failed!", ReportIcon.Error);
                    logger.log("Generating data for notation \'" + notationName + "\' failed: " + ex.ToString(), ReportIcon.OK);
                }
            }


            return dataNode.OuterXml;
        }

        private void addPlaceHolders(XmlNode XmlDataNode)
        {
            foreach (String callsfor in callForList)
            {
                //MessageBox.Show(callsfor);
                XmlNode xnode = XmlDataNode.SelectSingleNode(callsfor);
                if (xnode != null)
                {
                    XmlAttribute isPlaceHolder = xnode.OwnerDocument.CreateAttribute("IsPlaceHolder");
                    isPlaceHolder.AppendChild(xnode.OwnerDocument.CreateTextNode("true"));
                    xnode.Attributes.Append(isPlaceHolder);
                }
            }
        }

        private void removeCallfor(XmlNode node, Collection<XmlNode> templates)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                        removeCallfor(child, templates);
                }

                XmlNodeList ChildNodeswithCallfor = node.SelectNodes("*[@callfor]");
                foreach (XmlNode cnode in ChildNodeswithCallfor)
                {

                    XmlNode callforNode = cnode.Attributes.GetNamedItem("callfor");

                    if (callforNode != null)
                    {
                        string callsfor = callforNode.Value;

                        if (callsfor.Contains("@"))
                        {
                            throw new CallforUseErrorException("Callfor is a one-to-many mapping and does not support attributes!");
                        }
                        else
                        {
                            callForList.Add(callsfor);
                            cnode.Attributes.RemoveNamedItem("callfor");

                            XmlNode applytemplatesNode = cnode.OwnerDocument.CreateElement("xsl", "apply-templates", "http://www.w3.org/1999/XSL/Transform");
                            //valueofNode.Prefix = "xsl";
                            XmlAttribute selectAttr = cnode.OwnerDocument.CreateAttribute("select");
                            selectAttr.Value = callsfor;

                            applytemplatesNode.Attributes.Append(selectAttr);

                            if (cnode.ParentNode != null)
                            {
                                //replace current node with template node
                                XmlNode currentNode = cnode.Clone();
                                XmlNode parent = cnode.ParentNode;
                                parent.ReplaceChild(applytemplatesNode, cnode);


                                //prepare template
                                //XmlNode currentNode = parent.RemoveChild(node);


                                XmlNode blankapplytemp = cnode.OwnerDocument.CreateElement("xsl", "apply-templates", "http://www.w3.org/1999/XSL/Transform");//call for templates of embedded elements
                                currentNode.AppendChild(blankapplytemp);

                                XmlNode template = currentNode.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
                                XmlAttribute matchAttr = cnode.OwnerDocument.CreateAttribute("match");
                                matchAttr.Value = callsfor;
                                template.Attributes.Append(matchAttr);

                                template.AppendChild(currentNode);

                                templates.Add(template);
                                //MessageBox.Show(prettyPrinter.PrintToString(template));
                            }
                        }

                    }
                }
            }
        }

        private void removeLinkto(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                if (node.HasChildNodes)
                    foreach (XmlNode child in node.ChildNodes)
                        removeLinkto(child);

                Collection<string> attrsToBeRemoved = new Collection<string>();

                //look for linkto attributes and attributes with @linkto value
                foreach (XmlAttribute xattr in node.Attributes)
                {
                    if (xattr.Name.Equals("linkto"))//linkto attribute
                    {
                        XmlNode linktoNode = node.Attributes.GetNamedItem("linkto");

                        if (linktoNode != null)
                        {
                            string linksto = linktoNode.Value;
                            //node.Attributes.RemoveNamedItem("linkto");//cannot change foreach collection
                            attrsToBeRemoved.Add(xattr.Name);

                            XmlNode valueofNode = node.OwnerDocument.CreateElement("xsl", "value-of", "http://www.w3.org/1999/XSL/Transform");

                            XmlAttribute selectattr = node.OwnerDocument.CreateAttribute("select");
                            selectattr.Value = linksto;
                            valueofNode.Attributes.Append(selectattr);

                            XmlNode innerTextNode = node.SelectSingleNode("text()");
                            if (innerTextNode != null)
                                innerTextNode.InnerText = string.Empty;

                            node.AppendChild(valueofNode);


                            //generate tooltip -> would not work with SVG
                            /*XmlAttribute tooltipAttr = node.OwnerDocument.CreateAttribute("ToolTip");
                            tooltipAttr.Value = linksto;
                            if (node.Name.IndexOf('.') == -1)
                            {
                                node.Attributes.Append(tooltipAttr);
                            }
                            else if ((node.ParentNode != null) && (node.ParentNode.Name.IndexOf('.') == -1))
                            {
                                node.ParentNode.Attributes.Append(tooltipAttr);
                            }*/
                        }
                    }
                    else if (xattr.Value.StartsWith("@linkto"))
                    {
                        //linkto generates an attribute
                        attrsToBeRemoved.Add(xattr.Name);

                        string linkstoAttributeName = xattr.Name;
                        string linksto = xattr.Value.Substring(xattr.Value.IndexOf("=") + 1); //remove @linkto=, sample format is Fill="@linkto=Color|Green"
                        string linkstoAttributeValue;
                        string linkstoAttributeDefault;

                        if (linksto.Contains("|"))//"Color|Green"
                        {
                            linkstoAttributeDefault = linksto.Substring(linksto.IndexOf("|") + 1);
                            linkstoAttributeValue = linksto.Substring(0, linksto.IndexOf("|"));
                        }
                        else//"Color"
                        {
                            linkstoAttributeValue = linksto;
                            linkstoAttributeDefault = "";
                        }

                        //generate XSL code for the attribute
                        XmlNode attributeNode = node.OwnerDocument.CreateElement("xsl", "attribute", "http://www.w3.org/1999/XSL/Transform");
                        XmlAttribute attrName = node.OwnerDocument.CreateAttribute("name");
                        attrName.Value = linkstoAttributeName;
                        attributeNode.Attributes.Append(attrName);

                        XmlNode valueofNode = node.OwnerDocument.CreateElement("xsl", "value-of", "http://www.w3.org/1999/XSL/Transform");
                        XmlAttribute selectattr = node.OwnerDocument.CreateAttribute("select");
                        selectattr.Value = linkstoAttributeValue;
                        valueofNode.Attributes.Append(selectattr);
                        attributeNode.AppendChild(valueofNode);

                        node.InsertBefore(attributeNode, node.FirstChild);//XSL to append attribute to the parent node

                        //MessageBox.Show("Name: " + linkstoAttributeName + "\nValue: " + linkstoAttributeValue+ "\nDefault: "+linkstoAttributeDefault);    

                    }
                }

                //remove previous attributes
                foreach (string sattr in attrsToBeRemoved)
                {
                    node.Attributes.RemoveNamedItem(sattr);
                }

                //clear collection
                attrsToBeRemoved.Clear();
            }
        }

        public void showXAML(string s)
        {
            //clear rendered visual canvas
            XAMLRenderCanvas.Children.Clear();

            try
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(s);
                MemoryStream stream = new MemoryStream(byteArray);

                Assembly.Load("CONVErT");
                DependencyObject rootObject = XamlReader.Load(stream) as DependencyObject;

                XAMLRenderCanvas.Children.Add(rootObject as FrameworkElement);

                SkinOutputTabControl.SelectedIndex = 0;

                ReportStatusBar.ShowStatus("XAML Loaded", ReportIcon.OK);
                logger.log("XAML notation rendered successfully.", ReportIcon.OK);
            }
            catch (XamlParseException xe)
            {
                ReportStatusBar.ShowStatus("XAML parsing Error occured -> " + xe.Message, ReportIcon.Error);
                logger.log("Render notation error -> " + "See exception.", ReportIcon.Error);
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus("(refresh visual): Something went wrong -> " + ex.Message, ReportIcon.Error);
                logger.log("Render XAML notation error -> " + "See exception.", ReportIcon.Error);
            }
        }

        public void showSVG(string graphicsStr)
        {
            try
            {

                //string myText = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd).Text;

                if (!string.IsNullOrEmpty(graphicsStr) && (graphicsStr.IndexOf("linkto") == -1))
                {
                    XmlDocument htmlDoc = new XmlDocument();
                    htmlDoc.LoadXml("<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\" /></head><body>"
                                    + graphicsStr
                                    + "</body></html>");

                    htmlDoc.Save(DirectoryHelper.getFilePathExecutingAssembly("testSVG.html"));

                    HtmlRenderer.Navigate(new Uri(DirectoryHelper.getFilePathExecutingAssembly("testSVG.html"), UriKind.RelativeOrAbsolute));

                    SkinOutputTabControl.SelectedIndex = 1;

                    //for viewing directly on default browser
                    //Process proc = new Process();
                    //proc.StartInfo = new ProcessStartInfo(DirectoryHelper.getFilePath("testSVG.html"));
                    //proc.Start();

                    ReportStatusBar.ShowStatus("SVG Loaded", ReportIcon.OK);
                    logger.log("SVG notation rendered successfully.", ReportIcon.OK);
                }
                else if (graphicsStr.IndexOf("linkto") != -1)
                {
                    ReportStatusBar.ShowStatus("Modified SVG has to be processed first, then use \"Render output graphics\" to render", ReportIcon.Error);
                    logger.log("Render input graphics error -> " + "Modified graphic has to be processed first", ReportIcon.Error);
                }
                else
                {
                    ReportStatusBar.ShowStatus("Input graphics is empty!", ReportIcon.Error);
                    logger.log("Render input graphics error -> " + "Input graphics is empty!", ReportIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus("(Render SVG): Something went wrong -> " + ex.Message, ReportIcon.Error);
                logger.log("Render SVG error -> " + "See exception.", ReportIcon.Error);
            }

        }

        private string buildNotation(String GraphicsCode, String XMLData)
        {
            try
            {
                //Load data
                XmlReader datareader = XmlReader.Create(new StringReader(XMLData));
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(XMLData);
                XmlNode XmlDataNode = xdoc.DocumentElement;
                string dataNodeName = XmlDataNode.Name;

                xdoc.RemoveAll();


                //create transformation
                XmlElement stylesheet = xdoc.CreateElement("stylesheet", "xsl");
                stylesheet.SetAttribute("xmlns:xsl", "http://www.w3.org/1999/XSL/Transform");
                stylesheet.Prefix = "xsl";
                stylesheet.SetAttribute("version", "1.0");

                if (visType == VisualisationType.XAML)
                {
                    stylesheet.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");
                    stylesheet.SetAttribute("xmlns:local", "clr-namespace:CONVErT;assembly=CONVErT");
                    stylesheet.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                }
                else if (visType == VisualisationType.SVG)
                {
                    //don't know what will go here for now! Possibly script
                    //to use for adding interaction to SVG
                }

                XmlElement mainTemplate = stylesheet.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
                //mainTemplate.Prefix = "xsl";
                stylesheet.AppendChild(mainTemplate);

                XmlAttribute matchAttr = xdoc.CreateAttribute("match");
                matchAttr.Value = "/";
                mainTemplate.Attributes.Append(matchAttr);

                XmlElement callTemplate = xdoc.CreateElement("xsl", "apply-templates", "http://www.w3.org/1999/XSL/Transform");

                mainTemplate.AppendChild(callTemplate);

                XmlAttribute selectAttr = xdoc.CreateAttribute("select");
                string selectStr = dataNodeName;//templates.First().Attributes.GetNamedItem("match").Value;
                selectAttr.AppendChild(xdoc.CreateTextNode(selectStr) as XmlNode);
                callTemplate.Attributes.Append(selectAttr);


                //create transforamtion 
                Collection<XmlNode> transXSLTemplates = parseAnnotatedGraphics(GraphicsCode, dataNodeName, XmlDataNode);

                foreach (XmlNode x in transXSLTemplates)
                    stylesheet.AppendChild(stylesheet.OwnerDocument.ImportNode(x, true));

                //for removing text
                XmlNode applytemplatesText = stylesheet.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
                //valueofNode.Prefix = "xsl";

                XmlAttribute selectAttr1 = stylesheet.OwnerDocument.CreateAttribute("match");
                selectAttr1.Value = "text()";
                applytemplatesText.Attributes.Append(selectAttr1);

                stylesheet.AppendChild(applytemplatesText);

                String transXSL = prettyPrinter.PrintToString(stylesheet);
                //MessageBox.Show(transXSL);


                XmlReader xslreader = XmlReader.Create(new StringReader(transXSL));

                //transformation output
                StringBuilder output = new StringBuilder("");
                XmlWriter outputwriter = XmlWriter.Create(output);

                //run transformation
                XslCompiledTransform myXslTransform = new XslCompiledTransform();
                myXslTransform.Load(xslreader);

                myXslTransform.Transform(datareader, outputwriter);

                String s = prettyPrinter.PrintToString(output.ToString());

                return s;
            }
            catch (CallforUseErrorException cuex)//with new annotations for @linkto it will not happen! but keep it just in case
            {
                logger.log("Error in use of callfor: Call for does not support attribute generation", ReportIcon.Error);
                ReportStatusBar.ShowStatus("Error procesing annotated graphics: see exception!", ReportIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus("(createNotation): Something went wrong -> " + ex.Message, ReportIcon.Error);
                return null;
            }
        }

        #endregion //Skin logic


        #region Menue items

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo(DirectoryHelper.getFilePathExecutingAssembly("testSVG.html"));
            proc.Start();

        }

        private void OpenGraphics_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //clear previous xaml input text
                TextRange temp = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd);
                temp.Text = "";

                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "XAML files (*.xaml)|*.xaml|SVG graphics (*.svg)|*.svg|XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openDialog.FilterIndex = 0;
                openDialog.Title = "Open Graphics";
                Nullable<bool> result = openDialog.ShowDialog();

                if (result == true)
                {
                    string graphicsFileName = openDialog.FileName;

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(graphicsFileName);

                    string s2 = prettyPrinter.PrintToString(xdoc.OuterXml);
                    GraphicsInput.AppendText(s2);

                    if (graphicsFileName.ToLower().EndsWith(".svg"))
                    {//process SVG
                        //VisTypeComboBox.SelectedIndex = 1;//SVG
                        SVGRadio.IsChecked = true;
                        showSVG(s2);
                    }
                    else
                    {//process XAML
                        //VisTypeComboBox.SelectedIndex = 0;//XAML
                        XAMLRadio.IsChecked = true;
                        showXAML(s2);
                    }


                    logger.log("Graphics file: \"" + graphicsFileName + " loaded.", ReportIcon.OK);
                    ReportStatusBar.ShowStatus("Graphics loaded.", ReportIcon.OK);
                }
                else
                {
                    ReportStatusBar.ShowStatus("Canceled loading graphics", ReportIcon.Warning);
                    logger.log("Loading graphics canceled!", ReportIcon.Warning);
                }

            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus("Could not load graphics!", ReportIcon.Error);
                logger.log("Load graphics failed: " + ex.ToString(), ReportIcon.Error);
            }

        }

        private void RenderOutputNotation_Click(object sender, RoutedEventArgs e)
        {
            string myText = new TextRange(GraphicsOutput.Document.ContentStart, GraphicsOutput.Document.ContentEnd).Text;
            if (!String.IsNullOrEmpty(myText))
            {
                if (visType == VisualisationType.XAML)//XAML
                    showXAML(myText);
                else if (visType == VisualisationType.SVG)//SVG
                    showSVG(myText);
            }
            else
            {
                ReportStatusBar.ShowStatus("Ouput is empty!", ReportIcon.Error);
                logger.log("Render output notation error -> " + "Output is empty!", ReportIcon.Error);
            }
        }

        private void RenderInputGraphics_Click(object sender, RoutedEventArgs e)
        {
            string myText = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd).Text;

            if (!string.IsNullOrEmpty(myText) && (myText.IndexOf("linkto") == -1))
            {
                if (visType == VisualisationType.XAML)

                    showXAML(myText);

                else if (visType == VisualisationType.SVG)

                    showSVG(myText);
            }
            else if (myText.IndexOf("linkto") != -1)
            {
                ReportStatusBar.ShowStatus("Modified graphics have to be processed first, then use \"Render output graphics\" to render", ReportIcon.Error);
                logger.log("Render input graphics error -> " + "Modified graphics have to be processed first", ReportIcon.Error);
            }
            else
            {
                ReportStatusBar.ShowStatus("Input graphics is empty!", ReportIcon.Error);
                logger.log("Render input graphics error -> " + "Input is empty!", ReportIcon.Error);
            }

        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            //clear xaml input text
            TextRange temp = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd);
            temp.Text = "";

            //clear xaml output text
            temp = new TextRange(GraphicsOutput.Document.ContentStart, GraphicsOutput.Document.ContentEnd);
            temp.Text = "";

            //clear transformation code
            temp = new TextRange(GeneratedTransformationTextBox.Document.ContentStart, GeneratedTransformationTextBox.Document.ContentEnd);
            temp.Text = "";

            //clear data input text
            temp = new TextRange(XMLDataInput.Document.ContentStart, XMLDataInput.Document.ContentEnd);
            temp.Text = "";

            //clear rendered visual canvas
            XAMLRenderCanvas.Children.Clear();

            //Clear Notation name
            NotationNameTextBox.Text = "";

            //Clear callforlist
            callForList.Clear();
        }

        private void BuildNotationData_Click(object sender, RoutedEventArgs e)
        {
            ReportStatusBar.clearReportMessage();

            if (String.IsNullOrEmpty(NotationNameTextBox.Text))
            {
                ReportStatusBar.ShowStatus("Notation name is not provided!", ReportIcon.Error);
                logger.log("Building \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation name is not provided!", ReportIcon.Error);
                NotationNameTextBox.Focus();
            }
            else
            {
                string graphicsText = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd).Text;
                if (String.IsNullOrEmpty(graphicsText))
                {
                    ReportStatusBar.ShowStatus("Notation graphics code is missing!", ReportIcon.Error);
                    logger.log("Building notation data \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation graphics code is missing!", ReportIcon.Error);
                    GraphicsInput.Focus();
                }
                else
                {
                    string dataText = generate_Data(graphicsText, NotationNameTextBox.Text);
                    TextRange temp = new TextRange(XMLDataInput.Document.ContentStart, XMLDataInput.Document.ContentEnd);
                    temp.Text = "";
                    XMLDataInput.AppendText(prettyPrinter.PrintToString(dataText));

                    SkinOutputTabControl.SelectedIndex = 2;
                }
            }
        }

        private void BuildNotation_Click(object sender, RoutedEventArgs e)
        {
            ReportStatusBar.clearReportMessage();

            if (String.IsNullOrEmpty(NotationNameTextBox.Text))
            {
                ReportStatusBar.ShowStatus("Notation name is not provided!", ReportIcon.Error);
                logger.log("Building \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation name is not provided!", ReportIcon.Error);
                NotationNameTextBox.Focus();
            }
            else
            {
                string graphicsText = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd).Text;
                if (String.IsNullOrEmpty(graphicsText))
                {
                    ReportStatusBar.ShowStatus("Notation graphics code is missing!", ReportIcon.Error);
                    logger.log("Building \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation graphics code is missing!", ReportIcon.Error);
                    GraphicsInput.Focus();
                }
                else
                {
                    if ((graphicsText.IndexOf("linkto") == -1) && (graphicsText.IndexOf("callfor") == -1))
                    {
                        MessageBoxResult mbresult;
                        mbresult = MessageBox.Show("No semantic link between data and graphics is provided, if you continue the notation will be static!", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                        if (mbresult == MessageBoxResult.Cancel)
                        {
                            ReportStatusBar.ShowStatus("Build canceled!", ReportIcon.Warning);
                            logger.log("Building \'" + NotationNameTextBox.Text + "\' canceled!", ReportIcon.Error);
                            return;
                        }
                    }


                    string dataText = generate_Data(graphicsText, NotationNameTextBox.Text);
                    TextRange temp = new TextRange(XMLDataInput.Document.ContentStart, XMLDataInput.Document.ContentEnd);
                    temp.Text = "";
                    XMLDataInput.AppendText(prettyPrinter.PrintToString(dataText));

                    if (String.IsNullOrEmpty(dataText))
                    {
                        ReportStatusBar.ShowStatus("Data XML is missing!", ReportIcon.Error);
                        logger.log("Building \'" + NotationNameTextBox.Text + "\' failed -> " + "Data XML is missing!", ReportIcon.Error);
                        XMLDataInput.Focus();
                    }
                    else
                    {
                        //do build
                        string check = buildNotation(graphicsText, dataText);
                        if (!String.IsNullOrEmpty(check))
                        {
                            TextRange range = new TextRange(GraphicsOutput.Document.ContentStart, GraphicsOutput.Document.ContentEnd);
                            range.Text = "";
                            range.Text = check;
                            ReportStatusBar.ShowStatus("Noation built!", ReportIcon.OK);
                            logger.log("Notation \'" + NotationNameTextBox.Text + "\' is built.", ReportIcon.OK);

                            SkinOutputTabControl.SelectedIndex = 4;
                        }
                    }
                }
            }

        }

        private void SaveNotation_Click(object sender, RoutedEventArgs e)
        {
            ReportStatusBar.clearReportMessage();

            if (String.IsNullOrEmpty(NotationNameTextBox.Text))
            {
                ReportStatusBar.ShowStatus("Notation name is not provided!", ReportIcon.Error);
                logger.log("Generating \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation name is not provided!", ReportIcon.Error);
                NotationNameTextBox.Focus();
            }
            else
            {
                string xamlText = new TextRange(GraphicsInput.Document.ContentStart, GraphicsInput.Document.ContentEnd).Text;
                if (String.IsNullOrEmpty(xamlText))
                {
                    ReportStatusBar.ShowStatus("Notation XAML is missing!", ReportIcon.Error);
                    logger.log("Generating \'" + NotationNameTextBox.Text + "\' failed -> " + "Notation XAML is missing!", ReportIcon.Error);
                    GraphicsInput.Focus();
                }
                else
                {
                    string dataText = new TextRange(XMLDataInput.Document.ContentStart, XMLDataInput.Document.ContentEnd).Text;
                    if (String.IsNullOrEmpty(dataText))
                    {
                        ReportStatusBar.ShowStatus("Data XML is missing!", ReportIcon.Error);
                        logger.log("Generating \'" + NotationNameTextBox.Text + "\' failed -> " + "Data XML is missing!", ReportIcon.Error);
                        XMLDataInput.Focus();
                    }
                    else
                    {
                        XmlDocument xdoc = new XmlDocument();
                        XmlNode itemNode = xdoc.CreateElement("item");

                        XmlAttribute nameAttr = xdoc.CreateAttribute("name");
                        nameAttr.Value = NotationNameTextBox.Text;
                        itemNode.Attributes.Append(nameAttr);

                        XmlAttribute typeAttr = xdoc.CreateAttribute("type");

                        if (visType == VisualisationType.XAML)
                            typeAttr.Value = "XAML";
                        else if (visType == VisualisationType.SVG)
                            typeAttr.Value = "SVG";

                        itemNode.Attributes.Append(typeAttr);

                        //data XML
                        String dataXML = dataText;
                        xdoc.LoadXml(dataXML);
                        XmlNode dataNode = xdoc.CreateElement("data");
                        dataNode.AppendChild(xdoc.DocumentElement.Clone());
                        string dataNodeName = xdoc.DocumentElement.Name; //name of the data XML document element to be the name of (template match="name")
                        itemNode.AppendChild(dataNode);
                        xdoc.RemoveAll();

                        //create transforamtion 
                        Collection<XmlNode> transXSLTemplates = parseAnnotatedGraphics(xamlText, dataNodeName, dataNode.FirstChild);

                        if (transXSLTemplates.Count > 0)
                        {

                            XmlNode transformationNode = xdoc.CreateElement("trans");

                            foreach (XmlNode x in transXSLTemplates)
                                transformationNode.AppendChild(transformationNode.OwnerDocument.ImportNode(x, true));

                            itemNode.AppendChild(transformationNode);
                            xdoc.RemoveAll();

                            //save notation XML
                            //MessageBox.Show(prettyPrinter.PrintToString(itemNode.OuterXml));

                            //find ToolBoxItems.xml
                            string customElementsFile = getDirectory("Resources\\ToolBoxItems.xml");
                            customElementsFile = (customElementsFile.Replace("file:\\", ""));
                            XmlDocument customElementsDoc = new XmlDocument();
                            customElementsDoc.Load(customElementsFile);
                            XmlNode customItemsNode = customElementsDoc.SelectSingleNode("items");

                            if (customItemsNode != null)
                            {
                                customItemsNode.AppendChild(customItemsNode.OwnerDocument.ImportNode(itemNode, true));

                                customElementsDoc.Save(customElementsFile);
                               
                                //create visual element
                                if (visType == VisualisationType.XAML)
                                {
                                    XAMLRenderer rend = new XAMLRenderer();
                                    VisualElement v = rend.render(itemNode) as VisualElement;

                                    if (v != null)
                                    {
                                        XAMLRenderCanvas.Children.Clear();
                                        XAMLRenderCanvas.Children.Add(v);
                                        ReportStatusBar.ShowStatus("XAML Notation Saved", ReportIcon.OK);
                                        logger.log("XAML Notation \'" + NotationNameTextBox.Text + "\' saved in notation repository.", ReportIcon.OK);

                                        SkinOutputTabControl.SelectedIndex = 0;
                                        //Clear callforlist
                                        callForList.Clear();
                                    }
                                    else
                                    {
                                        ReportStatusBar.ShowStatus("XAML Notation failed!", ReportIcon.OK);
                                        logger.log("XAML Notation \'" + NotationNameTextBox.Text + "\' failed.", ReportIcon.Error);
                                    }
                                }
                                else if (visType == VisualisationType.SVG)
                                {
                                    ReportStatusBar.ShowStatus("SVG Notation Saved", ReportIcon.OK);
                                    logger.log("SVG Notation \'" + NotationNameTextBox.Text + "\' saved in notation repository.", ReportIcon.OK);
                                }
                            }
                            else
                            {
                                ReportStatusBar.ShowStatus("Could not locate custom items", ReportIcon.Error);
                                logger.log("Generating \'" + NotationNameTextBox.Text + "\' failed.", ReportIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dObj = LogicalTreeHelper.GetParent(this);

            while ((dObj != null) && (!(dObj is MainWindow)))
                dObj = LogicalTreeHelper.GetParent(dObj);

            if (dObj != null)
                (dObj as MainWindow).executeClose();
        }


        #endregion //menu Items


        #region tools

        /*private void VisComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*switch (VisTypeComboBox.SelectedIndex)
            {
                case 0:
                    visType = VisualisationType.XAML;
                    break;
                case 1:
                    visType = VisualisationType.SVG;
                    break;
                default:
                    //do nothing
                    break;
            }
        }*/

        private string getDirectory(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            return System.IO.Path.Combine(pbase, c);
        }

        private void XAMLRadio_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                visType = VisualisationType.XAML;
            else
                visType = VisualisationType.SVG;
        }

        private void SVGRadio_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                visType = VisualisationType.SVG;
            else
                visType = VisualisationType.XAML;
        }


        #endregion //tools

    }

    public class CallforUseErrorException : Exception
    {
        public CallforUseErrorException()
        {
        }
        public CallforUseErrorException(string message)
            : base(message)
        {
        }

        public CallforUseErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
