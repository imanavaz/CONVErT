using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml.Xsl;
using System.IO;
using System.Web.UI;
using System.Collections.ObjectModel;

namespace CONVErT
{
    public class XAMLRenderer
    {
        #region prop

        Collection<XmlNode> TemplateRepository = new Collection<XmlNode>();
        Collection<String> ProcessedNodes = new Collection<string>();

        //double elementHeight = 0;
        Point canvasLayingPosition = new Point(5, 5);
        XmlDocument Xdoc = new XmlDocument();

        XMLHelper xmlHelper = new XMLHelper();

        List<UIElement> elementList = new List<UIElement>();


        XmlPrettyPrinter prettyPrinter = new XmlPrettyPrinter();

        //for mapper suggester
        private Collection<VisualElement> visualElementList = new Collection<VisualElement>();
        public Collection<VisualElement> VisualElementList 
        {
            get { return visualElementList; } 
        }

        #endregion //prop


        #region ctor

        public XAMLRenderer()
        {
            string toolboxItemsPath = DirectoryHelper.getFilePathExecutingAssembly("Resources\\ToolBoxItems.xml");
            //string toolboxItemsPath = (@"C:\Users\iavazpour\Documents\SVN_IA\CONVErT\CONVErT\Resources\ToolBoxItems.xml");
            
            loadTempaltes(toolboxItemsPath);
        }

        public XAMLRenderer(string toolboxFile)
        {
            loadTempaltes(toolboxFile);
        }
        

        #endregion //ctor


        #region temlpates

        private Collection<XmlNode> findTemlate(string name)
        {
            Collection<XmlNode> results = new Collection<XmlNode>();

            foreach (XmlNode x in TemplateRepository)
            {
                XmlNode matchNode = x.Attributes.GetNamedItem("match");
                if (matchNode.Value.Equals(name))
                    results.Add(x);
            }

            return results;
        }

        private void loadTempaltes(string toolboxItemsPath)
        {
            if (System.IO.File.Exists(toolboxItemsPath))
            {
                try
                {

                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(toolboxItemsPath, readerSettings))
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.Load(reader);

                        XmlNode inXmlNode = dom.DocumentElement;

                        XmlNodeList items = inXmlNode.SelectNodes("//item[@name]");
                        foreach (XmlNode xnode in items)
                        {
                            if (xnode.Attributes.GetNamedItem("type").InnerText == "XAML")
                            {
                                XmlNode templates = xnode.SelectSingleNode("trans");
                                if (templates != null)
                                {
                                    foreach (XmlNode t in templates.ChildNodes)
                                        TemplateRepository.Add(t.Clone());
                                }
                            }

                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    MessageBox.Show("(XAMLRenderer) -> problem loading templates (XmlException) : \n\n" + xmlEx.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("(XAMLRenderer) -> problem loading templates (Exception) : \n\n" + ex.Message);
                }
            }
            else
                MessageBox.Show("(XAMLRenderer) -> Could not load Items file : " + toolboxItemsPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion//templates


        #region render elements

        public UIElement render(string modelFile)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(modelFile);

            XmlNode xnode = xdoc.DocumentElement;

            return render(xnode);
        }

        public UIElement render(XmlNode xNode)
        {
            if (xNode.Name.Equals("item"))
                if (xNode.Attributes.GetNamedItem("type").InnerText == "XAML")
                    return renderItem(xNode);
            
            //MessageBox.Show(xNode.InnerText);
            return null;
        }

        internal UIElement render(VisualElement visEl)
        {
            
            if (visEl.ItemXML == null)
            {
                //since there is no ItemXML containing transformation, to find transformation for data to visual
                //we should create one by copying data to a new XML node and fetching the transformation
                //form ToolBoxItems.xml
                XmlNode newItemXMl = visEl.Data.OwnerDocument.CreateElement("item");

                XmlAttribute nameAttr = newItemXMl.OwnerDocument.CreateAttribute("name");
                if (!String.IsNullOrEmpty(visEl.VEName))
                {
                    nameAttr.Value = visEl.VEName;
                }
                else
                    nameAttr.Value = visEl.Data.Name;
                newItemXMl.Attributes.Append(nameAttr);

                XmlNode dataNode = newItemXMl.OwnerDocument.CreateElement("data");
                dataNode.AppendChild(dataNode.OwnerDocument.ImportNode(visEl.Data.Clone(),true));
                newItemXMl.AppendChild(dataNode);

                //find transformation template
                XmlNode transNode = newItemXMl.OwnerDocument.CreateElement("trans");
                Collection<XmlNode> templates = this.findTemlate(visEl.Data.Name);
                
                //assign first template found to transformation (might become problematic with similar templates)
                transNode.AppendChild(transNode.OwnerDocument.ImportNode(templates[0].Clone(), true));
                newItemXMl.AppendChild(transNode);

                //finally assign the newItemXMl to the visual element
                visEl.ItemXML = newItemXMl.Clone();
            }

            UIElement uiResult = renderItem(visEl.ItemXML);

            //Null reference problem spotted when a visualelement was being created and automatically moved to custom toolbox elements
            //this was added on 19/5/2014 to rectify null reference to reverse data and others
            if (uiResult != null) 
            {
                if (visEl.ReverseData != null)
                    (uiResult as VisualElement).ReverseData = visEl.ReverseData.Clone()as XmlNode;
                
                if(visEl.Data != null)
                    (uiResult as VisualElement).Data = visEl.Data.Clone() as XmlNode;
                
                if(visEl.templateVM.TemplateName != null)
                    (uiResult as VisualElement).templateVM.TemplateName = visEl.templateVM.TemplateName;
                
                if(visEl.templateVMR.TemplateName != null)
                    (uiResult as VisualElement).templateVMR.TemplateName = visEl.templateVMR.TemplateName;
                
                if(visEl.templateVM.TemplateXmlNode != null)
                    (uiResult as VisualElement).templateVM.TemplateXmlNode = visEl.templateVM.TemplateXmlNode.Clone() as XmlNode;

                if (visEl.templateVMR.TemplateXmlNode != null)
                    (uiResult as VisualElement).templateVMR.TemplateXmlNode = visEl.templateVMR.TemplateXmlNode.Clone() as XmlNode;
            }

            return uiResult;

        }

        //for visual debugger
        internal UIElement renderPartialVisualisation(VisualElement visEl)
        {
            if (visEl.abstractTree != null)
            {
                XmlDocument xd = new XmlDocument();
                XmlNode xn = visEl.abstractTree.Root.ToXML(); //get root and remove left over texts
                checkNodeleftover(xn);

                xd.AppendChild(xd.ImportNode(xn, true));
                xd.Save(DirectoryHelper.getFilePathExecutingAssembly("tempVisPartialRend.xml"));
                
                return createVisualisation(DirectoryHelper.getFilePathExecutingAssembly("tempVisPartialRend.xml"));
            }

            return null;
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


        #endregion

        
        #region Renderer logic

        /// <summary>
        /// Create interactable visualisation for concrete transformation
        /// </summary>
        /// <param name="fileName">file to be visualised</param>
        /// <returns></returns>
        public UIElement createVisualisation(string fileName)
        {
            //clear previous visual elements
            visualElementList.Clear();
            

            VisualElement result = new VisualElement();

            if (System.IO.File.Exists(fileName))
            {
                try
                {

                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(fileName, readerSettings))
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.Load(reader);

                        XmlNode inXmlNode = dom.DocumentElement;

                        if (findTemlate(inXmlNode.Name).Count > 0)
                        {
                            Collection<XmlNode> templates = new Collection<XmlNode>();

                            traverseAndAddTemplates(inXmlNode, templates);

                            //templates should now form a transformation file
                            string transCode = createTransformation(inXmlNode.Name, templates);
                                                       
                            //save transformation for testing
                            XmlDocument xd = new XmlDocument();
                            xd.LoadXml(transCode);
                            xd.Save(DirectoryHelper.getFilePathExecutingAssembly("testrenderer.xsl"));

                            //run transformation
                            XmlReader xslreader = XmlReader.Create(new StringReader(transCode));

                            //transformation output
                            StringBuilder output = new StringBuilder("");
                            XmlWriter outputwriter = XmlWriter.Create(output);

                            //run transformation
                            XslCompiledTransform myXslTransform = new XslCompiledTransform();
                            myXslTransform.Load(xslreader);

                            XmlReader datareader = XmlReader.Create(new StringReader(dom.DocumentElement.OuterXml));
                            myXslTransform.Transform(datareader, outputwriter);

                            String s = prettyPrinter.PrintToString(output.ToString());
                            
                            
                            //render notation
                            try
                            {
                                byte[] byteArray = Encoding.ASCII.GetBytes(s);
                                MemoryStream stream = new MemoryStream(byteArray);

                                DependencyObject rootObject = XamlReader.Load(stream) as DependencyObject;

                                return rootObject as UIElement;

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("(XAMLRenderer.createVisualisation) -> : Something went wrong -> " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show("(XAMLRenderer.createVisualisation) -> No template found for root element", "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    MessageBox.Show("(XAMLRenderer.createVisualisation) -> creating visualisation (XmlException) : \n\n" + xmlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("(XAMLRenderer.createVisualisation) -> creating visualisation (Exception) : \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("(XAMLRenderer.createVisualisation) -> Could not load Items file : " + fileName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return null;
        }


        /// <summary>
        /// Creates a non-interactable visualisation
        /// </summary>
        /// <param name="fileName">file to be visualised</param>
        /// <returns></returns>
        public UIElement createStillVisualisation(string fileName)
        {
            //clear previous visual elements
            //visualElementList.Clear();


            UIElement result = new UIElement();

            String s = renderToStillXAML(fileName);

            if (!String.IsNullOrEmpty(s))
            {

                //render notation
                try
                {
                    byte[] byteArray = Encoding.ASCII.GetBytes(s);
                    MemoryStream stream = new MemoryStream(byteArray);

                    DependencyObject rootObject = XamlReader.Load(stream) as DependencyObject;

                    return rootObject as UIElement;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("(XAMLRenderer.createStillVisualisation) -> : Something went wrong -> " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("(XAMLRenderer.createStillVisualisation) -> Returned XAML is empty for visualisaiton file : " + fileName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return null;
        }

        private void updateTemplatesToStillVisualisationTemplates(Collection<XmlNode> templates)
        {
            foreach (XmlNode xt in templates)
            {
                if (xt.FirstChild.Name.Equals("local:VisualElement"))
                {//supress the visualElement
                    XmlNode temp = xt.FirstChild.ChildNodes[1].Clone();
                    xt.RemoveChild(xt.FirstChild);
                    xt.AppendChild(xt.OwnerDocument.ImportNode(temp, true));
                }
            }
        }

        private void traverseAndAddTemplates(XmlNode node, Collection<XmlNode> templates)
        {
            if (node.NodeType != XmlNodeType.Text) //not possible to have a transformation Rule for a text value
            {
                string address = xmlHelper.getAdressIncludingSelf(node);

                if (ProcessedNodes.IndexOf(address) == -1)
                {
                    if (node.HasChildNodes)
                        foreach (XmlNode x in node.ChildNodes)
                            traverseAndAddTemplates(x, templates);

                    Collection<XmlNode> r = new Collection<XmlNode>();
                    Collection<String> templateNames = new Collection<string>(); //this will cause multiple rules to be eliminated, in case of using conditions to have different rules for on element, it should be changed
                    r = findTemlate(node.Name);
                    if (r.Count > 0)
                        foreach (XmlNode t in r)
                        {
                            string temp = t.Attributes["match"].Value;
                            if (templateNames.IndexOf(temp) == -1)//perhaps later add a condition to check whether a conditional rule exists too
                            {
                                templates.Add(t);
                                templateNames.Add(temp);

                                //apply transformation on the node to get the visual element for mapper suggester
                                if (t.FirstChild.Name.Equals("local:VisualElement"))
                                try
                                {
                                
                                    XmlNode visualElementNode = t.FirstChild;
                                    XmlNode visualElementResources = visualElementNode.FirstChild;
                                    XmlNode visualElementXmlDataProvider = visualElementResources.FirstChild;
                                    
                                    XmlNode dataNode = visualElementXmlDataProvider.FirstChild;
                                
                                    XmlNode itemNode = t.OwnerDocument.CreateElement("item");
                                    XmlAttribute itemname = itemNode.OwnerDocument.CreateAttribute("name");
                                    itemname.Value = temp;
                                    itemNode.Attributes.Append(itemname);
                                    XmlNode itemdata = itemNode.OwnerDocument.CreateElement("data");
                                    itemdata.AppendChild(itemNode.OwnerDocument.ImportNode(dataNode.FirstChild.Clone(),true));
                                    itemNode.AppendChild(itemdata);
                                    XmlNode itemtrans = itemNode.OwnerDocument.CreateElement("trans");
                                    itemtrans.AppendChild(itemNode.OwnerDocument.ImportNode(t.Clone(),true));
                                    itemNode.AppendChild(itemtrans);
                                    
                                    //MessageBox.Show(prettyPrinter.PrintToString(itemNode.OuterXml));
                                    
                                    UIElement u = renderItem(itemNode);
                                    VisualElement element = u as VisualElement;
                                    if (element != null)
                                    {
                                        //assding VsiaulElement address for mapper suggester
                                        if (address.StartsWith("#document"))
                                            element.VAddress = address.Substring(address.IndexOf("/") + 1);
                                        else
                                            element.VAddress = address;
                                        visualElementList.Add(element);
                                        //MessageBox.Show(element.Data.OuterXml);
                                    }
                                    
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("(XAMLRenderer.traverseAndAddTemplates) -> : Something went wrong -> " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                            }
                        }
                    ProcessedNodes.Add(address);
                }
            }

        }

        /// <summary>
        /// This function will render the visualisation file into still XAML which may include annimation but the 
        /// interaction is excluded for now. The resulted visualisation will include XAML and not VIsualELements of CONVErT
        /// </summary>
        /// <param name="fileName">Visualsiaiton file (XML)</param>
        /// <returns></returns>
        public string renderToStillXAML(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                try
                {

                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(fileName, readerSettings))
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.Load(reader);

                        XmlNode inXmlNode = dom.DocumentElement;

                        if (findTemlate(inXmlNode.Name).Count > 0)
                        {
                            Collection<XmlNode> templates = new Collection<XmlNode>();

                            //to clear previouse templates
                            ProcessedNodes.Clear();

                            traverseAndAddTemplates(inXmlNode, templates);

                            //update templates to create still visualisations, not VisualElements
                            updateTemplatesToStillVisualisationTemplates(templates);

                            //templates should now form a transformation file
                            string transCode = createTransformation(inXmlNode.Name, templates);

                            //save transformation for testing
                            XmlDocument xd = new XmlDocument();
                            xd.LoadXml(transCode);
                            xd.Save(DirectoryHelper.getFilePathExecutingAssembly("testrenderer.xsl"));

                            //run transformation
                            XmlReader xslreader = XmlReader.Create(new StringReader(transCode));

                            //transformation output
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.OmitXmlDeclaration = true;
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.CloseOutput = true;

                            StringBuilder output = new StringBuilder("");
                            XmlWriter outputwriter = XmlWriter.Create(output,settings);

                            //run transformation
                            XslCompiledTransform myXslTransform = new XslCompiledTransform();
                            myXslTransform.Load(xslreader);

                            XmlReader datareader = XmlReader.Create(new StringReader(dom.DocumentElement.OuterXml));
                            myXslTransform.Transform(datareader, outputwriter);

                            String xamlResult = prettyPrinter.PrintToString(output.ToString());

                            return xamlResult;
                        }
                        else
                        {
                            MessageBox.Show("(XAMLRenderer.renderToStillXAML) -> No template found for root element", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    MessageBox.Show("(XAMLRenderer.renderToStillXAML) -> (XmlException) : \n\n" + xmlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("(XAMLRenderer.renderToStillXAML) -> (Exception) : \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("(XAMLRenderer.renderToStillXAML) -> Could not load Items file : " + fileName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return null;
        }

        /// <summary>
        /// This function will render the visualisation file into still HTML which may include annimation but the 
        /// interaction is excluded for now. The resulted visualisation will include HTML and NOT VisualELements of CONVErT
        /// </summary>
        /// <param name="fileName">Visualsiaiton file (XML)</param>
        /// <returns></returns>
        public string renderToStillHTML(string fileName, string xamlFile)
        {
            StringWriter stringWriter = new StringWriter();

	        // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {

                writer.RenderBeginTag(HtmlTextWriterTag.Html); // Begin #1 html

                writer.RenderBeginTag(HtmlTextWriterTag.Body); //Begin #4 body

                //writer.AddAttribute(HtmlTextWriterAttribute.Id, "mushroom");
                writer.AddAttribute(HtmlTextWriterAttribute.Width,"100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                string xamlfilename = xamlFile.Substring(xamlFile.LastIndexOf("\\") + 1);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, xamlfilename);
                writer.RenderBeginTag(HtmlTextWriterTag.Iframe); //Begin #5 script
                writer.RenderEndTag(); //End #5 script

                writer.RenderEndTag(); //End #4 body
                writer.RenderEndTag(); //End #1 html

                //writer.Close();
            }

            // Return the result
            return stringWriter.ToString();
        }

        private string createTransformation(string dataNodeName, Collection<XmlNode> templates)
        {
            //create transformation
            XmlDocument xdoc = new XmlDocument();
            XmlElement stylesheet = xdoc.CreateElement("stylesheet", "xsl");
            stylesheet.SetAttribute("xmlns:xsl", "http://www.w3.org/1999/XSL/Transform");
            stylesheet.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");
            stylesheet.SetAttribute("xmlns:local", "clr-namespace:CONVErT;assembly=CONVErT");
            stylesheet.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            stylesheet.SetAttribute("version", "1.0");
            stylesheet.Prefix = "xsl";

            XmlElement mainTemplate = stylesheet.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
            //mainTemplate.Prefix = "xsl";
            stylesheet.AppendChild(mainTemplate);

            XmlAttribute matchAttr = xdoc.CreateAttribute("match");
            matchAttr.Value = "/";
            mainTemplate.Attributes.Append(matchAttr);

            XmlElement callTemplate = xdoc.CreateElement("xsl", "apply-templates", "http://www.w3.org/1999/XSL/Transform");

            XmlAttribute selectAttr = xdoc.CreateAttribute("select");
            string selectStr = dataNodeName;
            selectAttr.AppendChild(xdoc.CreateTextNode(selectStr) as XmlNode);
            callTemplate.Attributes.Append(selectAttr);
            mainTemplate.AppendChild(callTemplate);

            foreach (XmlNode x in templates)
                stylesheet.AppendChild(stylesheet.OwnerDocument.ImportNode(x,true));

            String transXSL = prettyPrinter.PrintToString(stylesheet);

            return transXSL;
        }

        private UIElement renderItem(XmlNode xNode)
        {
            try
            {
                VisualElement ve = new VisualElement();

                ve.ItemXML = xNode.Clone();

                XmlNode nameAttr = xNode.Attributes.GetNamedItem("name");
                String name;
                if (nameAttr != null)
                    name = nameAttr.InnerText;
                else
                    name = "";

                XmlNode dataNode = xNode.SelectSingleNode("data");
                //if (dataNode != null)
                //{
                //    ve.Data = dataNode.FirstChild.Clone();
                //}

                XmlNode transNode = xNode.SelectSingleNode("trans");
                if ((transNode != null) && (dataNode != null))
                {
                    //MessageBox.Show(xNode.OuterXml);
                    string notationXml = renderNotation(transNode, dataNode.InnerXml);

                    if (!string.IsNullOrEmpty(notationXml))
                    {
                        byte[] byteArray = Encoding.Unicode.GetBytes(notationXml);
                        MemoryStream stream = new MemoryStream(byteArray);

                        FrameworkElement rootObject = XamlReader.Load(stream) as FrameworkElement;
                        //ve.Content = rootObject;
                        VisualElement element = rootObject as VisualElement;
                        element.Trans = transNode.Clone();
                        element.ItemXML = xNode.Clone();
                        element.loadDataFromXaml();
                        //element.Data = dataNode.Clone();
                        element.VEName = name;

                        return element as UIElement;
                    } 
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("XAMLRenderer -> render Item(Item XML): Something went wrong ->\n\n " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

        }

        private string renderNotation(XmlNode transNode, String XMLData)
        {
            try
            {
                //Load data
                XmlReader datareader = XmlReader.Create(new StringReader(XMLData));

                //create transforamtion 
                XmlDocument xdoc = new XmlDocument();

                XmlElement stylesheet = xdoc.CreateElement("stylesheet", "xsl");
                stylesheet.SetAttribute("xmlns:xsl", "http://www.w3.org/1999/XSL/Transform");
                stylesheet.SetAttribute("version", "1.0");
                stylesheet.Prefix = "xsl";

                XmlElement mainTemplate = stylesheet.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
                //mainTemplate.Prefix = "xsl";

                XmlAttribute matchAttr = xdoc.CreateAttribute("match");
                matchAttr.Value = "/";
                mainTemplate.Attributes.Append(matchAttr);

                XmlElement callTemplate = xdoc.CreateElement("xsl", "apply-templates", "http://www.w3.org/1999/XSL/Transform");

                XmlAttribute selectAttr = xdoc.CreateAttribute("select");
                string selectStr = transNode.FirstChild.Attributes.GetNamedItem("match").Value;

                if (!string.IsNullOrEmpty(selectStr))
                {
                    selectAttr.AppendChild(xdoc.CreateTextNode(selectStr) as XmlNode);
                    callTemplate.Attributes.Append(selectAttr);
                    mainTemplate.AppendChild(callTemplate);

                    stylesheet.AppendChild(mainTemplate);

                    //create transforamtion 
                    foreach (XmlNode x in transNode.ChildNodes)
                        stylesheet.AppendChild(stylesheet.OwnerDocument.ImportNode(x.Clone(), true));

                    //for removing text
                    XmlNode applytemplatesText = stylesheet.OwnerDocument.CreateElement("xsl", "template", "http://www.w3.org/1999/XSL/Transform");
                    XmlAttribute selectAttr1 = stylesheet.OwnerDocument.CreateAttribute("match");
                    selectAttr1.Value = "text()";
                    applytemplatesText.Attributes.Append(selectAttr1);

                    stylesheet.AppendChild(applytemplatesText);
                    

                    XmlReader xslreader = XmlReader.Create(new StringReader(stylesheet.OuterXml));

                    //transformation output
                    StringBuilder output = new StringBuilder("");
                    XmlWriter outputwriter = XmlWriter.Create(output);

                    //run transformation
                    XslCompiledTransform myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(xslreader);

                    myXslTransform.Transform(datareader, outputwriter);
                    String s = output.ToString();

                    return s;
                }
                else
                {
                    MessageBox.Show("XAMLRenderer -> (create Notation): No templates found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("XAMLRenderer -> (create Notation): Something went wrong -> " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        #endregion //Renderer logic


        #region Utils

        private Brush getNextColor(int i)
        {
            string[] colors = { "Blue", "Red", "Green", "Yellow", "Black" };

            return (SolidColorBrush)new BrushConverter().ConvertFromString(colors[(i % (colors.Count() - 1))]);

        }

        #endregion //Utils


        #region old stuff

        /* //this was rendering mechanism
        private UIElement renderPiechart(XmlNode pcNode)
        {
            Canvas c = new Canvas();

            c.Background = Brushes.White;
            c.Height = 320;
            c.Width = 300;

            //get chart name
            string chartName = "";
            XmlNode pcnNode = pcNode.SelectSingleNode("name");
            if (pcnNode != null)
                chartName = pcnNode.InnerText;
            else
                chartName = "name";

            Label chartNameLable = new Label();
            chartNameLable.FontSize = 8;
            chartNameLable.Height = 20;
            chartNameLable.Content = chartName;

            Canvas.SetTop(chartNameLable, 0);
            Canvas.SetLeft(chartNameLable, 120);
            c.Children.Add(chartNameLable);

            //prepare pie pieces
            double Radius = 145;
            double InnerRadius = 20;
            double CenterX = 150;
            double CenterY = 170;
            double lastRotationAngle = 0;

            //prepare bars 
            XmlNode piePieces = pcNode.SelectSingleNode("PiePieces");
            VisualElement[] pieElements = new VisualElement[piePieces.ChildNodes.Count];

            //calculate sum of pie piece values
            double sum = 0;

            for (int i = 0; i < piePieces.ChildNodes.Count; i++)
            {
                XmlNode p = piePieces.SelectNodes("PieCut")[i];

                if (p.SelectNodes("value").Count > 0)
                    sum += Convert.ToDouble(p.SelectNodes("value")[0].InnerText);
            }

            //draw them on VisualElements
            for (int i = 0; i < piePieces.ChildNodes.Count; i++)
            {

                XmlNode p = piePieces.ChildNodes.Item(i);

                UIElement temp = render(p);

                if (temp != null)
                {
                    if (temp.GetType().ToString().Equals("CONVErT.PiePiece"))
                    {
                        (temp as PiePiece).Radius = Radius;
                        (temp as PiePiece).InnerRadius = InnerRadius;
                        (temp as PiePiece).CentreX = CenterX;
                        (temp as PiePiece).CentreY = CenterY;
                        (temp as PiePiece).WedgeAngle = (temp as PiePiece).WedgeAngle * 360 / sum;
                        (temp as PiePiece).RotationAngle = lastRotationAngle;
                        (temp as PiePiece).Fill = getNextColor(i);
                        lastRotationAngle += (temp as PiePiece).WedgeAngle;

                        pieElements[i] = new VisualElement();
                        pieElements[i].IsHitTestVisible = true;
                        pieElements[i].Data = p;
                        pieElements[i].Content = temp;

                        c.Children.Add(pieElements[i]);
                    }
                    else //debugging aid
                    {
                        c.Children.Add(temp);
                    }
                }
            }


            //prepare chart element
            XmlNode tempChartData = pcNode;
            XmlNode tempNode = tempChartData.SelectSingleNode("PiePieces");
            tempNode.RemoveAll();
            tempNode.InnerText = "Pieces";

            VisualElement chart = new VisualElement();
            chart.Data = tempChartData;
            chart.Content = c;

            return chart;
        }

        private UIElement renderPiecut(XmlNode pieNode)
        {
            //name
            string name = "";
            XmlNode pnNode = pieNode.SelectSingleNode("name");
            if (pnNode != null)
                name = pnNode.InnerText;
            else
                name = "name";

            //color
            //XmlNode clnode = pieNode.SelectSingleNode("color");
            //string cl = "Blue";
            //if (clnode == null)
            //    cl = "Black";
            //else
            //    cl = clnode.InnerText;

            //value
            double value = 0;
            XmlNode pvNode = pieNode.SelectSingleNode("value");
            if (pvNode != null)
                value = Double.Parse(pvNode.InnerText);
            else
                value = 50;

            PiePiece mypiece = new PiePiece();
            //mypiece.Radius = 60;
            //mypiece.InnerRadius = 20;
            mypiece.WedgeAngle = value;
            //mypiece.CentreX = 5;
            //mypiece.CentreY = 50;
            //mypiece.RotationAngle = 40;
            //mypiece.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(cl);
            mypiece.Stroke = Brushes.Black;

            return mypiece;
        }

        private UIElement renderForm(XmlNode xNode)
        {
            Grid g = new Grid();
            g.Background = Brushes.LightGray;

            int rowIndex = 0;
            int columnIndex = 0;

            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.RowDefinitions.Add(new RowDefinition());

            String formName = xNode.SelectSingleNode("name").InnerText;

            Label lh = new Label();
            lh.Content = formName;
            lh.Height = 25;
            lh.SetValue(Grid.RowProperty, rowIndex++);
            lh.SetValue(Grid.ColumnProperty, 0);
            g.Children.Add(lh);

            XmlNode fields = xNode.SelectSingleNode("FormFields");
            int fieldsCount = fields.ChildNodes.Count;
            int maxRowsCount = 5; //maximum rows of form fields
            int columnCount = 1;
            int rowCount = 0;

            if (fieldsCount > maxRowsCount)
            {
                columnCount = Convert.ToInt32(fieldsCount / maxRowsCount) + 1;
                rowCount = maxRowsCount;
            }
            else
                rowCount = fieldsCount;

            //create form grid
            for (int i = 0; i < rowCount; i++)
                g.RowDefinitions.Add(new RowDefinition());

            for (int j = 1; j < columnCount; j++)//one has already been added
                g.ColumnDefinitions.Add(new ColumnDefinition());

            //rowindex starts from 1(row 0 is form name), column index starts from 0
            foreach (XmlNode field in fields)
            {
                if (rowIndex > maxRowsCount)
                {
                    rowIndex = 1;
                    columnIndex++;
                }

                UIElement temp = render(field);
                if (temp != null)
                {
                    temp.SetValue(Grid.RowProperty, rowIndex++);
                    temp.SetValue(Grid.ColumnProperty, columnIndex);

                    g.Children.Add(temp);
                }
            }

            fields.RemoveAll();
            fields.InnerText = "Fields";

            VisualElement mVe = new VisualElement();
            mVe.Data = xNode;
            mVe.Content = g;

            Canvas.SetLeft(mVe, canvasLayingPosition.X);
            Canvas.SetTop(mVe, canvasLayingPosition.Y);

            return mVe;
        }

        private UIElement renderFormField(XmlNode xNode)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            string label = xNode.SelectSingleNode("Label").InnerText;
            string text = xNode.SelectSingleNode("data").InnerText;

            Label l = new Label();
            l.Content = label + " : ";
            l.Height = 25;
            stack.Children.Add(l);

            TextBox tb = new TextBox();
            tb.Text = text;
            tb.Height = 25;
            stack.Children.Add(tb);

            VisualElement mVe = new VisualElement();
            mVe.Data = xNode;
            mVe.Content = stack;

            return mVe;
        }

        private UIElement renderUMLModel(XmlNode model)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;

            //create model name header
            string name = model.SelectSingleNode("Name").InnerText;

            TextBlock t0 = new TextBlock();
            t0.Height = 20;
            t0.Text = name;
            t0.FontWeight = FontWeights.Bold;
            Border b0 = new Border();
            b0.BorderBrush = Brushes.Black;
            b0.Background = Brushes.White;
            b0.BorderThickness = new Thickness(1);
            b0.Width = name.Length * 8;
            b0.Child = t0;
            stack.Children.Add(b0);

            Canvas c = new Canvas();

            XmlNodeList classes = model.SelectNodes("Classes/Class");
            VisualElement[] classElements = new VisualElement[classes.Count];

            for (int i = 0; i < classes.Count; i++)
            {
                //classElements[i] = renderUMLClass(classes.Item(i)) as VisualElement;
                UIElement temp = render(classes.Item(i)) as VisualElement;
                if (temp != null)
                    classElements[i] = temp as VisualElement;
            }

            int top = 5;
            int left = 5;
            for (int i = 0; i < classElements.Count(); i++)
            {

                Canvas.SetTop(classElements[i], top);
                Canvas.SetLeft(classElements[i], left);
                Canvas.SetZIndex(classElements[i], 2);

                if (i % 2 == 0)
                    left += 150;
                else
                    top += 150;

                c.Children.Add(classElements[i]);
            }

            XmlNodeList links = model.SelectNodes("Links/Link");
            foreach (XmlNode link in links)
            {
                string start = link.SelectSingleNode("StartClass").InnerText;
                string end = link.SelectSingleNode("EndClass").InnerText;

                VisualElement startElement = null;//new VisualElement();
                VisualElement endElement = null;//new VisualElement();

                foreach (VisualElement v in classElements)
                    if (v.Name.Equals(start))
                    {
                        startElement = v;
                        break;
                    }

                foreach (VisualElement v in classElements)
                    if (v.Name.Equals(end))
                    {
                        endElement = v;
                        break;
                    }

                if ((startElement != null) && (endElement != null))
                {
                    Line linkLine = new Line();
                    linkLine.Stroke = Brushes.Black;
                    linkLine.StrokeThickness = 2;
                    linkLine.X1 = Canvas.GetLeft(startElement);
                    linkLine.X2 = Canvas.GetLeft(endElement);
                    linkLine.Y1 = Canvas.GetTop(startElement) + 10;
                    linkLine.Y2 = Canvas.GetTop(endElement) + 10;

                    VisualElement temp = new VisualElement();
                    temp.Data = link.Clone();
                    temp.Content = linkLine;
                    Canvas.SetZIndex(temp, 1);

                    c.Children.Add(temp);
                }
            }

            //prepare model element
            XmlNode tempModelData = model;
            XmlNode tempNode = tempModelData.SelectSingleNode("Classes");
            tempNode.RemoveAll();
            tempNode.InnerText = "Classes";

            XmlNode tempNode2 = tempModelData.SelectSingleNode("Links");
            tempNode2.RemoveAll();
            tempNode2.InnerText = "Links";


            Border b1 = new Border();
            b1.BorderBrush = Brushes.Black;
            b1.Background = Brushes.White;
            b1.Width = 370;
            b1.Height = 200;
            b1.BorderThickness = new Thickness(1);
            b1.Child = c;
            stack.Children.Add(b1);

            VisualElement mVe = new VisualElement();
            mVe.Data = tempModelData;
            mVe.Content = stack;

            Canvas.SetLeft(mVe, canvasLayingPosition.X);
            Canvas.SetTop(mVe, canvasLayingPosition.Y);

            //do something for links
            //==================

            return mVe;
        }

        private UIElement renderJavaCode(XmlNode code)
        {
            int lineHeight = 25;
            int lineWidth = 370;

            ScrollViewer sv = new ScrollViewer();
            sv.Background = Brushes.White;
            sv.Height = 380;
            sv.Width = lineWidth + 20;
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            stack.Name = QualifiedNameString.Convert(code.Name);
            //create comments line
            Label comments1 = new Label();

            comments1.Content = "/* this is an automatic generated code";
            comments1.Foreground = Brushes.Green;
            comments1.Height = lineHeight;
            comments1.Width = lineWidth;
            stack.Children.Add(comments1);

            Label comments2 = new Label();
            comments2.Content = " * Using CONVERT version 0.0.1";
            comments2.Foreground = Brushes.Green;
            comments2.Height = lineHeight;
            comments2.Width = lineWidth;
            stack.Children.Add(comments2);

            Label comments3 = new Label();
            //comments3.Content = " *************************************";
            comments3.Foreground = Brushes.Green;
            comments3.Height = lineHeight;
            comments3.Width = lineWidth;
            stack.Children.Add(comments3);

            //Label emptyLabel = new Label();
            //emptyLabel.Content = "";
            //emptyLabel.Height = lineHeight;
            //emptyLabel.Width = lineWidth;
            //stack.Children.Add(emptyLabel);

            //create code header
            StackPanel codeHeader = new StackPanel();
            codeHeader.Orientation = Orientation.Horizontal;

            Label package1 = new Label();
            package1.Content = "package";
            package1.Foreground = Brushes.Navy;
            package1.Height = lineHeight;
            //package1.Width = 20;
            codeHeader.Children.Add(package1);

            Label packageName = new Label();
            packageName.Name = "packageName";
            XmlNode pName = code.SelectSingleNode("packageName");
            string packageNameStr = "";
            if (pName != null)
                packageNameStr = pName.InnerText;
            else
                packageNameStr = "default";
            packageNameStr = packageNameStr + ";";
            packageName.Content = packageNameStr;
            packageName.Foreground = Brushes.Black;
            packageName.Height = lineHeight;
            //packageName.Width = lineWidth;
            codeHeader.Children.Add(packageName);
            stack.Children.Add(codeHeader);

            //create classes
            XmlNode classes = code.SelectSingleNode("class-declarations");
            VisualElement classElement;

            //int left = 5;
            for (int i = 0; i < classes.ChildNodes.Count; i++)
            {
                //classElement = renderJavaClass(classes.Item(i), lineHeight, lineWidth) as VisualElement;
                VisualElement temp = render(classes.ChildNodes.Item(i)) as VisualElement;

                if (temp != null)
                {
                    classElement = temp;
                    Label emptyLabel2 = new Label();
                    emptyLabel2.Content = "";
                    emptyLabel2.Height = lineHeight;
                    emptyLabel2.Width = lineWidth;
                    stack.Children.Add(emptyLabel2);

                    stack.Children.Add(classElement);
                }
            }

            //close package
            //Label closeBracket = new Label();
            //closeBracket.Content = "}";
            //closeBracket.Foreground = Brushes.Black;
            //closeBracket.Height = lineHeight;
            //closeBracket.Width = lineWidth;
            //stack.Children.Add(closeBracket);
            //stack.Name = "codeHeader";
            sv.Content = stack;

            //prepare code element
            XmlNode tempModelData = code;
            XmlNode tempNode = tempModelData.SelectSingleNode("class-declarations");
            tempNode.RemoveAll();
            tempNode.InnerText = "Classes";

            VisualElement mVe = new VisualElement();
            mVe.Data = tempModelData;
            mVe.Content = sv;

            //Canvas.SetLeft(mVe, canvasLayingPosition.X);
            //Canvas.SetTop(mVe, canvasLayingPosition.Y);

            return mVe;
        }

        private UIElement renderJavaClass(XmlNode cl)
        {
            int lineHeight = 25;
            int lineWidth = 370;

            StackPanel stack = new StackPanel();
            stack.Name = QualifiedNameString.Convert(cl.Name);
            stack.Background = Brushes.White;
            stack.Orientation = Orientation.Vertical;
            string name = cl.SelectSingleNode("name").InnerText;
            string id = cl.SelectSingleNode("identifier").InnerText;

            //create class header
            StackPanel classHeader = new StackPanel();
            classHeader.Orientation = Orientation.Horizontal;

            Label idLabel = new Label();
            idLabel.Name = "identifier";
            idLabel.Content = id;
            idLabel.Foreground = Brushes.Navy;
            idLabel.Height = lineHeight;
            //idLabel.Width = 40;
            classHeader.Children.Add(idLabel);

            Label classLabel = new Label();
            classLabel.Content = " class ";
            classLabel.Foreground = Brushes.Navy;
            classLabel.Height = lineHeight;
            //classLabel.Width = 30;
            classHeader.Children.Add(classLabel);

            Label className = new Label();
            className.Name = "name";
            className.Content = name;
            className.Foreground = Brushes.Black;
            className.Height = lineHeight;
            //className.Width = lineWidth - 40;
            classHeader.Children.Add(className);

            stack.Children.Add(classHeader);

            Label openBracket = new Label();
            openBracket.Content = "{";
            openBracket.Foreground = Brushes.Black;
            openBracket.Height = lineHeight;
            openBracket.Width = lineWidth;
            stack.Children.Add(openBracket);

            //create contents, properties
            XmlNode props = cl.SelectSingleNode("properties");

            foreach (XmlNode prop in props.ChildNodes)
            {
                UIElement temp = render(prop);
                if (temp != null)
                    stack.Children.Add(temp);
            }


            //create contents, operations
            XmlNode methods = cl.SelectSingleNode("methods");

            foreach (XmlNode method in methods.ChildNodes)
            {
                UIElement temp = render(method);
                if (temp != null)
                    stack.Children.Add(temp);
            }


            //close class
            Label closeBracket = new Label();
            closeBracket.Content = "}";
            closeBracket.Foreground = Brushes.Black;
            closeBracket.Height = lineHeight;
            closeBracket.Width = lineWidth;
            stack.Children.Add(closeBracket);

            //prepare class visual element
            XmlNode tempClassData = cl;
            XmlNode tempNode = tempClassData.SelectSingleNode("properties");
            tempNode.RemoveAll();
            tempNode.InnerText = "properties";

            XmlNode tempNode2 = tempClassData.SelectSingleNode("methods");
            tempNode2.RemoveAll();
            tempNode2.InnerText = "methods";

            VisualElement cVe = new VisualElement();
            cVe.Data = tempClassData;
            cVe.Content = stack;

            cVe.Name = name;//class name as element name, for tracking and positioning links

            return cVe;
        }

        private UIElement renderJavaProperty(XmlNode prop)
        {
            int lineHeight = 25;
            StackPanel stack = new StackPanel();
            stack.Background = Brushes.White;
            stack.Height = lineHeight;
            stack.Name = QualifiedNameString.Convert(prop.Name);
            stack.Orientation = Orientation.Horizontal;

            if (prop.SelectSingleNode("identifier") != null)
            {
                Label accessLabel = new Label();
                accessLabel.Name = "identifier";
                accessLabel.Height = lineHeight;
                accessLabel.Content = prop.SelectSingleNode("identifier").InnerText;
                accessLabel.Foreground = Brushes.Navy;
                stack.Children.Add(accessLabel);
            }

            Label typeLabel = new Label();
            typeLabel.Name = "type";
            typeLabel.Height = lineHeight;
            typeLabel.Content = prop.SelectSingleNode("type").InnerText;
            typeLabel.Foreground = Brushes.Navy;
            stack.Children.Add(typeLabel);

            Label attrNameLabel = new Label();
            attrNameLabel.Name = "name";
            attrNameLabel.Height = lineHeight;
            attrNameLabel.Content = prop.SelectSingleNode("name").InnerText;
            stack.Children.Add(attrNameLabel);

            //semicolon
            Label semicolon = new Label();
            semicolon.Content = ";";
            semicolon.Foreground = Brushes.Black;
            semicolon.Height = lineHeight;
            stack.Children.Add(semicolon);

            //prepare Attribute data
            VisualElement cVe = new VisualElement();
            cVe.Data = prop;
            cVe.Content = stack;

            return cVe;
        }

        private UIElement renderJavaMethod(XmlNode op)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            stack.Name = QualifiedNameString.Convert(op.Name);
            stack.Background = Brushes.White;

            if (op.SelectSingleNode("identifier") != null)
            {
                string methodAccess = op.SelectSingleNode("identifier").InnerText + " ";
                Label accessLabel = new Label();
                accessLabel.Content = methodAccess;
                accessLabel.Name = "identifier";
                accessLabel.Foreground = Brushes.Navy;
                stack.Children.Add(accessLabel);
            }

            string opReturnType = op.SelectSingleNode("return-type").InnerText + " ";
            Label returnLabel = new Label();
            returnLabel.Content = opReturnType;
            returnLabel.Name = QualifiedNameString.Convert("return-type");
            returnLabel.Foreground = Brushes.Navy;
            stack.Children.Add(returnLabel);

            string opName = op.SelectSingleNode("name").InnerText;
            Label opNameLabel = new Label();
            opNameLabel.Name = "name";
            opNameLabel.Content = opName;
            stack.Children.Add(opNameLabel);

            XmlNode opParams = op.SelectSingleNode("parameters");

            if (opParams.ChildNodes.Count > 0)//create parametes
            {
                Label openPranLabel = new Label();
                openPranLabel.Content = "(";
                stack.Children.Add(openPranLabel);

                for (int i = 0; i < opParams.ChildNodes.Count; i++)
                {
                    if (i > 0)
                    {
                        Label separatorLabel = new Label();
                        separatorLabel.Content = ",";
                        stack.Children.Add(separatorLabel);
                    }

                    UIElement temp1 = render(opParams.ChildNodes.Item(i));
                    if (temp1 != null)
                        stack.Children.Add(temp1);
                }

                Label closedPranLabel = new Label();
                closedPranLabel.Content = ")";
                stack.Children.Add(closedPranLabel);
            }
            else//no parameters, create empy header
            {
                Label emptyParamsLabel = new Label();
                emptyParamsLabel.Content = "()";
                stack.Children.Add(emptyParamsLabel);
            }

            Label emptyMethodBodyLabel = new Label();
            emptyMethodBodyLabel.Content = "{ }";
            stack.Children.Add(emptyMethodBodyLabel);

            //prepare operation
            XmlNode tempOpData = op;
            XmlNode tempNode = tempOpData.SelectSingleNode("parameters");
            tempNode.RemoveAll();
            tempNode.InnerText = "parameters";

            VisualElement opVe = new VisualElement();
            opVe.Data = tempOpData;
            opVe.Content = stack;

            return opVe;
        }

        private UIElement renderJavaParameter(XmlNode param)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            stack.Name = QualifiedNameString.Convert(param.Name);
            stack.Background = Brushes.White;

            Label l0 = new Label();
            l0.Content = " ";
            stack.Children.Add(l0);

            string paramName = param.SelectSingleNode("name").InnerText;

            string paramType = param.SelectSingleNode("type").InnerText;

            Label l = new Label();
            l.Content = paramType;
            l.Name = "type";
            l.Foreground = Brushes.Navy;
            stack.Children.Add(l);

            Label l2 = new Label();
            l2.Content = paramName;
            l2.Name = "name";
            stack.Children.Add(l2);

            Label l3 = new Label();
            l3.Content = " ";
            stack.Children.Add(l3);

            //prepare parameter
            VisualElement pVe = new VisualElement();
            pVe.Data = param;
            pVe.Content = stack;

            return pVe;
        }

        private UIElement renderUMLClass(XmlNode cl)
        {
            StackPanel stack = new StackPanel();
            stack.Background = Brushes.White;
            stack.Orientation = Orientation.Vertical;

            string name = cl.SelectSingleNode("Name").InnerText;

            string access = "";
            XmlNode anode = cl.SelectSingleNode("Access");
            if (anode != null)
            {
                switch (anode.InnerText.ToLower())
                {
                    case ("private"):
                        access = "-";
                        break;
                    case ("public"):
                        access = "+";
                        break;
                    case ("package"):
                        access = "*";
                        break;
                    default:
                        access = " ";
                        break;
                }
            }
            else
                access = " ";

            TextBlock t0 = new TextBlock();
            t0.Height = 20;
            t0.Text = access + " " + name;
            t0.FontWeight = FontWeights.Bold;
            Border b0 = new Border();
            b0.BorderBrush = Brushes.Black;
            b0.Background = Brushes.White;
            b0.BorderThickness = new Thickness(2);
            b0.Child = t0;
            stack.Children.Add(b0);

            XmlNode attrs = cl.SelectSingleNode("Attributes");
            StackPanel attrStack = new StackPanel();
            attrStack.Orientation = Orientation.Vertical;

            foreach (XmlNode attr in attrs.ChildNodes)
            {
                UIElement temp = render(attr);
                if (temp != null)
                    attrStack.Children.Add(temp);
            }

            Border b = new Border();
            b.BorderBrush = Brushes.Black;
            b.Background = Brushes.White;
            b.BorderThickness = new Thickness(1);
            b.Child = attrStack;
            stack.Children.Add(b);

            XmlNode ops = cl.SelectSingleNode("Operations");
            StackPanel opStack = new StackPanel();
            opStack.Orientation = Orientation.Vertical;

            foreach (XmlNode op in ops.ChildNodes)
            {
                UIElement temp = render(op);
                if (temp != null)
                    opStack.Children.Add(temp);
            }

            Border b2 = new Border();
            b2.BorderBrush = Brushes.Black;
            b2.Background = Brushes.White;
            b2.BorderThickness = new Thickness(1);
            b2.Child = opStack;
            stack.Children.Add(b2);

            //prepare class element
            XmlNode tempClassData = cl;
            XmlNode tempNode = tempClassData.SelectSingleNode("Attributes");
            if (tempNode != null)
            {
                tempNode.RemoveAll();
                tempNode.InnerText = "Attributes";
            }

            XmlNode tempNode2 = tempClassData.SelectSingleNode("Operations");
            if (tempNode2 != null)
            {
                tempNode2.RemoveAll();
                tempNode2.InnerText = "Operations";
            }

            VisualElement cVe = new VisualElement();
            cVe.Data = tempClassData;
            cVe.Content = stack;

            cVe.Name = name;//class name as element name, for tracking and positioning links

            //Canvas.SetLeft(cVe, canvasLayingPosition.X);
            //Canvas.SetTop(cVe, canvasLayingPosition.Y);

            return cVe;
        }

        private UIElement renderUMLAttribute(XmlNode attr)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;


            string access = "";
            string attrAccess = attr.SelectSingleNode("Access").InnerText;
            if (attrAccess.ToLower().Equals("private"))
                access = "-";
            else if (attrAccess.ToLower().Equals("public"))
                access = "+";
            else
                access = "*";
            Label accessLabel = new Label();
            accessLabel.Content = access;
            stack.Children.Add(accessLabel);

            string attrName = attr.SelectSingleNode("Name").InnerText;
            Label attrNameLabel = new Label();
            attrNameLabel.Content = attrName;
            stack.Children.Add(attrNameLabel);

            Label colonLabel = new Label();
            colonLabel.Content = " : ";
            stack.Children.Add(colonLabel);

            string attrType = attr.SelectSingleNode("Type").InnerText;
            Label typeLabel = new Label();
            typeLabel.Content = attrType;
            typeLabel.Foreground = Brushes.Navy;
            stack.Children.Add(typeLabel);

            //prepare Attribute data
            VisualElement cVe = new VisualElement();
            cVe.Data = attr;
            cVe.Content = stack;

            return cVe;
        }

        private UIElement renderUMLOperation(XmlNode op)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            string access = "";
            string opAccess = op.SelectSingleNode("Access").InnerText;
            if (opAccess.ToLower().Equals("private"))
                access = "-";
            else if (opAccess.ToLower().Equals("public"))
                access = "+";
            else
                access = "*";
            Label accessLabel = new Label();
            accessLabel.Content = access;
            stack.Children.Add(accessLabel);

            string opReturnType = op.SelectSingleNode("ReturnType").InnerText;
            Label returnLabel = new Label();
            returnLabel.Content = opReturnType;
            returnLabel.Foreground = Brushes.Navy;
            stack.Children.Add(returnLabel);

            string opName = op.SelectSingleNode("Name").InnerText;
            Label opNameLabel = new Label();
            opNameLabel.Content = opName;
            stack.Children.Add(opNameLabel);

            XmlNodeList opParams = op.SelectNodes("Parameters/Parameter");

            if (opParams.Count > 0)//create parametes
            {
                Label openPranLabel = new Label();
                openPranLabel.Content = "(";
                stack.Children.Add(openPranLabel);

                for (int i = 0; i < opParams.Count; i++)
                {
                    if (i > 0)
                    {
                        Label separatorLabel = new Label();
                        separatorLabel.Content = ",";
                        stack.Children.Add(separatorLabel);
                    }
                    UIElement temp = render(opParams.Item(i));
                    if (temp != null)
                        stack.Children.Add(temp);
                }

                Label closedPranLabel = new Label();
                closedPranLabel.Content = ")";
                stack.Children.Add(closedPranLabel);
            }
            else//no parameters, create empy header
            {
                Label emptyParamsLabel = new Label();
                emptyParamsLabel.Content = "()";
                stack.Children.Add(emptyParamsLabel);
            }

            //prepare operation
            XmlNode tempOpData = op;
            XmlNode tempNode = tempOpData.SelectSingleNode("Parameters");
            tempNode.RemoveAll();
            tempNode.InnerText = "Parameters";

            VisualElement opVe = new VisualElement();
            opVe.Data = tempOpData;
            opVe.Content = stack;

            return opVe;
        }

        private UIElement renderUMLParameter(XmlNode param)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            string paramName = param.SelectSingleNode("Name").InnerText;

            string paramType = param.SelectSingleNode("Type").InnerText;

            Label l = new Label();
            l.Content = paramType;
            l.Foreground = Brushes.Navy;
            stack.Children.Add(l);

            Label l2 = new Label();
            l2.Content = paramName;
            stack.Children.Add(l2);

            //prepare parameter
            VisualElement pVe = new VisualElement();
            pVe.Data = param;
            pVe.Content = stack;

            return pVe;
        }

        private UIElement renderTable(XmlNode table)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;

            //create table name header
            string name = table.SelectSingleNode("name").InnerText;

            TextBlock t0 = new TextBlock();
            t0.Height = 20;
            //t0.Width = 122;
            t0.Text = name;
            Border bTabelName = new Border();
            bTabelName.BorderBrush = Brushes.Black;
            bTabelName.Background = Brushes.White;
            bTabelName.BorderThickness = new Thickness(2);
            bTabelName.Child = t0;

            //get column names
            XmlNode rows = table.SelectSingleNode("rows");
            List<string> columnNames = new List<string>();

            foreach (XmlNode row in rows.ChildNodes)
            {
                XmlNodeList cols = row.SelectNodes("columns/column");

                foreach (XmlNode col in cols)
                {
                    string colname = col.SelectSingleNode("name").InnerText;

                    if (columnNames.IndexOf(colname) == -1)
                        columnNames.Add(colname);

                }

            }

            //create column header 
            StackPanel columnNamesStack = new StackPanel();
            columnNamesStack.Orientation = Orientation.Horizontal;

            //first cell is empty
            TextBlock t = new TextBlock();
            t.Height = 20;
            t.Width = 50;
            t.Text = "";
            Border b = new Border();
            b.BorderBrush = Brushes.Black;
            b.Background = Brushes.White;
            b.BorderThickness = new Thickness(2);
            b.Child = t;
            columnNamesStack.Children.Add(b);

            foreach (string cn in columnNames)
            {
                TextBlock t1 = new TextBlock();
                t1.Height = 20;
                t1.Width = 70;
                t1.Text = cn;
                Border b1 = new Border();
                b1.BorderBrush = Brushes.Black;
                b1.Background = Brushes.White;
                b1.BorderThickness = new Thickness(2);
                b1.Child = t1;

                //prepare column element
                VisualElement colVE = new VisualElement();
                Xdoc.LoadXml("<column><name>columnName</name><value>columnValue</value></column>");
                colVE.Data = Xdoc.DocumentElement;
                colVE.Content = b1;
                columnNamesStack.Children.Add(colVE);
            }

            bTabelName.Width = columnNamesStack.Width;

            stack.Children.Add(bTabelName); //table name
            stack.Children.Add(columnNamesStack);  //table column header

            //render rows
            foreach (XmlNode row in rows.ChildNodes)
            {
                stack.Children.Add(createTableRow(row, columnNames));
            }

            //prepare table element
            XmlNode tempChartData = table;
            XmlNode tempNode = tempChartData.SelectSingleNode("rows");
            tempNode.RemoveAll();
            tempNode.InnerText = "Rows";

            VisualElement chart = new VisualElement();
            chart.Data = tempChartData;
            chart.Content = stack;

            Canvas.SetLeft(chart, canvasLayingPosition.X);
            Canvas.SetTop(chart, canvasLayingPosition.Y);

            return chart;

        }

        private UIElement createTableRow(XmlNode row, List<string> columnNames)
        {
            StackPanel rowStack = new StackPanel();
            rowStack.Orientation = Orientation.Horizontal;
            rowStack.Width = 54 + columnNames.Count * 74;

            //first cell is empty
            TextBlock t = new TextBlock();
            t.Height = 20;
            t.Width = 50;
            t.Text = "Row";
            Border b = new Border();
            b.BorderBrush = Brushes.Black;
            b.Background = Brushes.White;
            b.BorderThickness = new Thickness(2);
            b.Child = t;
            rowStack.Children.Add(b);

            foreach (string cn in columnNames)
            {
                TextBlock t1 = new TextBlock();
                t1.Height = 20;
                t1.Width = 70;

                XmlNode columnValue = row.SelectSingleNode("columns/column[name='" + cn + "']");

                if (columnValue != null)
                {
                    t1.Text = columnValue.SelectSingleNode("value").InnerText;
                }
                else
                    t1.Text = "";

                Border b1 = new Border();
                b1.BorderBrush = Brushes.Black;
                b1.Background = Brushes.White;
                b1.BorderThickness = new Thickness(2);
                b1.Child = t1;

                //prepare visual element
                VisualElement columnVis = new VisualElement();
                columnVis.Data = columnValue;
                columnVis.Content = b1;
                rowStack.Children.Add(columnVis);
            }
            //prepare chart element

            XmlNode tempRowData = row;
            XmlNode tempNode = tempRowData.SelectSingleNode("columns");
            //tempNode.RemoveAll();
            //tempNode.InnerText = "columns";

            VisualElement rowVE = new VisualElement();
            rowVE.Data = tempRowData;
            rowVE.Content = rowStack;

            return rowVE;
        }

        private VisualElement renderBarchart(XmlNode xNode)
        {
            XmlNode bars = xNode.SelectSingleNode("Bars");

            //prepare chart area=============

            Canvas c = new Canvas();
            c.Background = Brushes.White;
            c.Height = 285;
            c.Width = 40 + 5 + bars.ChildNodes.Count * 20;

            //get chart name
            string chartName = xNode.SelectNodes("name")[0].InnerText;
            Label chartNameLable = new Label();
            //chartNameLable.Height = 15;
            chartNameLable.FontSize = 8;
            chartNameLable.Content = chartName;

            Canvas.SetTop(chartNameLable, 0);
            Canvas.SetLeft(chartNameLable, 30);
            c.Children.Add(chartNameLable);

            //prepare axis
            // Draw X axis
            ArrowLine xaxis = new ArrowLine();
            xaxis.Stroke = Brushes.Black;
            xaxis.StrokeThickness = 2;
            xaxis.IsArrowClosed = true;
            xaxis.ArrowLength = 4;

            xaxis.X1 = 5;
            xaxis.Y1 = 225;
            xaxis.X2 = 30 + bars.ChildNodes.Count * 20;
            xaxis.Y2 = 225;

            Canvas.SetLeft(xaxis, 0);
            Canvas.SetTop(xaxis, 0);
            c.Children.Add(xaxis);

            string xlabel = xNode.SelectNodes("xaxis")[0].InnerText;

            Label txlabel = new Label();
            txlabel.Content = xlabel;
            //txlabel. = TextAlignment.Center;

            Canvas.SetLeft(txlabel, xaxis.X2 / 2 - 20);
            Canvas.SetTop(txlabel, 260);

            c.Children.Add(txlabel);

            // DrawY axis
            string ylabel = xNode.SelectNodes("yaxis")[0].InnerText;

            TextBlock tylabel = new TextBlock();
            tylabel.Text = ylabel;
            tylabel.TextAlignment = TextAlignment.Center;
            tylabel.LayoutTransform = new RotateTransform(90);

            Canvas.SetLeft(tylabel, 0);
            Canvas.SetTop(tylabel, 100);

            c.Children.Add(tylabel);

            ArrowLine yaxis = new ArrowLine();
            yaxis.Stroke = Brushes.Black;
            yaxis.StrokeThickness = 2;
            yaxis.IsArrowClosed = true;
            yaxis.ArrowLength = 4;

            yaxis.X1 = 20;
            yaxis.Y1 = 235;
            yaxis.X2 = 20;
            yaxis.Y2 = 25;

            Canvas.SetLeft(yaxis, 0);
            Canvas.SetTop(yaxis, 0);
            c.Children.Add(yaxis);
            //=============

            //prepare bars 

            VisualElement[] barElements = new VisualElement[bars.ChildNodes.Count];

            //get maximum value for y axis
            //double maximumValue = 0;

            for (int i = 0; i < bars.ChildNodes.Count; i++)
            {
                XmlNode bar = bars.ChildNodes.Item(i);
                double temp = 0;
                if (bar.SelectNodes("value").Count > 0)
                    temp = Convert.ToDouble(bar.SelectNodes("value")[0].InnerText);
                if (temp > maximumValue)
                    maximumValue = temp;
            }

            for (int i = 0; i < bars.ChildNodes.Count; i++)
            {

                XmlNode bar = bars.ChildNodes.Item(i);

                VisualElement temp = render(bar) as VisualElement;

                if (temp != null)
                {
                    barElements[i] = temp;
                    Canvas.SetLeft(barElements[i], (10 + 2 + i * 20));
                    Canvas.SetTop(barElements[i], 225 - elementHeight);
                    barElements[i].IsHitTestVisible = true;

                    c.Children.Add(barElements[i]);
                }
            }


            //prepare chart element
            XmlNode tempChartData = xNode;
            XmlNode tempNode = tempChartData.SelectSingleNode("Bars");
            tempNode.RemoveAll();
            tempNode.InnerText = "Bars";

            VisualElement chart = new VisualElement();
            chart.Data = tempChartData;
            chart.Content = c;

            Canvas.SetLeft(chart, canvasLayingPosition.X);
            Canvas.SetTop(chart, canvasLayingPosition.Y);

            return chart;
        }

        private UIElement createBar(XmlNode barNode, double maximumHeightValue)
        {
            string name = barNode.SelectNodes("name")[0].InnerText;
            double value = Convert.ToDouble(barNode.SelectNodes("value")[0].InnerText);

            XmlNode clnode = barNode.SelectSingleNode("color");
            string cl;
            if (clnode == null)
                cl = "Black";
            else
                cl = clnode.InnerText;

            if (maximumHeightValue == 0)
                maximumHeightValue = 100; //choose default value of 100

            //normalise value considering y axis height 200 pixels
            double normValue = (value * 200) / maximumHeightValue;

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;

            TextBlock tvalue = new TextBlock();
            tvalue.Text = value.ToString();
            tvalue.Height = 10;
            tvalue.TextAlignment = TextAlignment.Center;

            stack.Children.Add(tvalue);

            stack.Children.Add(createRectangle(10, normValue, cl));

            TextBlock tname = new TextBlock();
            tname.Text = name;
            tname.Height = 10;
            tname.TextAlignment = TextAlignment.Left;
            tname.LayoutTransform = new RotateTransform(90);

            stack.Children.Add(tname);
            elementHeight = normValue + 10;

            VisualElement barElement = new VisualElement();
            barElement.Data = barNode;

            //UIElement barcontent = createBar(bar, maximumValue);
            barElement.Content = stack;

            return barElement;
        }

        private Rectangle createRectangle(double width, double height, string c)
        {
            // in WPF you can use a BrushConverter
            SolidColorBrush myBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(c);

            Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.Fill = myBrush;

            return rect;
        }

        */

        /* //this was creating initials mechanism
       internal UIElement render(VisualElement visEl)
       {
           if (visEl.ItemXML != null)
           {
               //return createNotation(visEl);
               return renderItem(visEl.ItemXML);

           }
           else 
           {
            
               switch (visEl.Data.Name.ToLower())
               {
                   case ("piecut"):
                       return createInitialPieCut(visEl.Data);
                   case ("piechart"):
                       return createInitialPiechart(visEl.Data);
                   //case ("piepiece"):
                   //    return tryNewRenderer(visEl);
                   case ("bar"):
                       return createInitialBar(visEl.Data);
                   case ("barchart"):
                       return createInitialBarchart(visEl.Data);
                   case ("column"):
                       return createInitialColumn(visEl.Data);
                   case ("row"):
                       return createInitialRow(visEl.Data);
                   case ("table"):
                       return createInitialTable(visEl.Data);
                   case ("java-property"):
                       return createInitialJavaProperty(visEl.Data);
                   case ("attribute"):
                       return createInitialUMLAttribute(visEl.Data);
                   case ("operation"):
                       return createInitialUMLOperation(visEl.Data);
                   case ("parameter"):
                       return createInitialUMLOperationParameter(visEl.Data);
                   case ("class"):
                       return createInitialUMLClass(visEl.Data);
                   case ("link"):
                       return createInitialUMLClassLink(visEl.Data);
                   case ("model"):
                       return createInitialClassDiagramModel(visEl.Data);
                   case ("method-definition"):
                       return createInitialJavaMethod(visEl.Data);
                   case ("class-declaration"):
                       return createInitialJavaClass(visEl.Data);
                   case ("javasource"):
                       return createInitialJavaSource(visEl.Data);
                   case ("java-parameter"):
                       return createInitialJavaParameter(visEl.Data);
                   case ("canvas"):
                       return (UIElement)XamlReader.Parse(visEl.Data.OuterXml);
                   case ("formfield"):
                       return createInitialFormField(visEl.Data);
                   case ("form"):
                       return createInitialForm(visEl.Data);
                   case ("stackpanel"):
                       //VisualElement mVe = new VisualElement();
                       return (UIElement)XamlReader.Parse(visEl.Data.OuterXml);
                   default:
                       MessageBox.Show("Visual object not recognised:\n\n" + visEl.Data.Name);
                       return null;
               }
           }
       }

       private UIElement createInitialPiechart(XmlNode pcNode)
       {
           Canvas c = new Canvas();

           c.Background = Brushes.White;
           c.Height = 120;
           c.Width = 100;

           //get chart name
           string chartName = "";
           XmlNode pcnNode = pcNode.SelectSingleNode("name");
           if (pcnNode != null)
               chartName = pcnNode.InnerText;
           else
               chartName = "name";

           Label chartNameLable = new Label();
           chartNameLable.FontSize = 8;
           chartNameLable.Height = 20;
           chartNameLable.Content = chartName;

           Canvas.SetTop(chartNameLable, 0);
           Canvas.SetLeft(chartNameLable, 20);
           c.Children.Add(chartNameLable);

           //prepare pie pieces
           double Radius = 45;
           double InnerRadius = 10;
           double CenterX = 50;
           double CenterY = 70;
           double lastRotationAngle = 0;

           PiePiece mypiece1 = new PiePiece();
           mypiece1.Radius = Radius;
           mypiece1.InnerRadius = InnerRadius;
           mypiece1.WedgeAngle = 70;
           mypiece1.CentreX = CenterX;
           mypiece1.CentreY = CenterY;
           mypiece1.RotationAngle = lastRotationAngle;
           mypiece1.Fill = Brushes.Blue;
           mypiece1.Stroke = Brushes.Black;
           lastRotationAngle += 70;
           c.Children.Add(mypiece1);

           PiePiece mypiece2 = new PiePiece();
           mypiece2.Radius = Radius;
           mypiece2.InnerRadius = InnerRadius;
           mypiece2.WedgeAngle = 110;
           mypiece2.CentreX = CenterX;
           mypiece2.CentreY = CenterY;
           mypiece2.RotationAngle = lastRotationAngle;
           mypiece2.Fill = Brushes.Green;
           mypiece2.Stroke = Brushes.Black;
           lastRotationAngle += 110;
           c.Children.Add(mypiece2);

           PiePiece mypiece3 = new PiePiece();
           mypiece3.Radius = Radius;
           mypiece3.InnerRadius = InnerRadius;
           mypiece3.WedgeAngle = 90;
           mypiece3.CentreX = CenterX;
           mypiece3.CentreY = CenterY;
           mypiece3.RotationAngle = lastRotationAngle;
           mypiece3.Fill = Brushes.Red;
           mypiece3.Stroke = Brushes.Black;
           lastRotationAngle += 90;
           c.Children.Add(mypiece3);

           PiePiece mypiece4 = new PiePiece();
           mypiece4.Radius = Radius;
           mypiece4.InnerRadius = InnerRadius;
           mypiece4.WedgeAngle = 90;
           mypiece4.CentreX = CenterX;
           mypiece4.CentreY = CenterY;
           mypiece4.RotationAngle = lastRotationAngle;
           mypiece4.Fill = Brushes.Yellow;
           mypiece4.Stroke = Brushes.Black;
           lastRotationAngle += 90;
           c.Children.Add(mypiece4);

           return c;
       }

       private UIElement createInitialPieCut(XmlNode pieNode)
       {
           //name
           string name = "";
           XmlNode pnNode = pieNode.SelectSingleNode("name");
           if (pnNode != null)
               name = pnNode.InnerText;
           else
               name = "name";

           //color
           //XmlNode clnode = pieNode.SelectSingleNode("color");
           string cl = "Blue";
           //if (clnode == null)
           //    cl = "Black";
           //else
           //    cl = clnode.InnerText;

           //value
           double value = 0;
           XmlNode pvNode = pieNode.SelectSingleNode("value");
           if (pvNode != null)
               value = Double.Parse(pvNode.InnerText);
           else
               value = 50;


           //Draw Pie Piece
           PiePiece mypiece = new PiePiece();
           mypiece.Radius = 60;
           mypiece.InnerRadius = 20;
           mypiece.WedgeAngle = value;
           mypiece.CentreX = 5;
           mypiece.CentreY = 50;
           mypiece.RotationAngle = 40;
           mypiece.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(cl);
           mypiece.Stroke = Brushes.Black;

           Canvas c = new Canvas();
           c.Background = Brushes.White;
           c.Width = 70;
           c.Height = 70;

           c.Children.Add(mypiece);

           return c;
       }

       private UIElement tryNewRenderer(VisualElement VisEl)
       {
           XmlNode data = VisEl.Data.SelectSingleNode("data");
           XmlNode visual = VisEl.Data.SelectSingleNode("Visual");

           //if (data != null)
           //  VisEl.Data = data;

           if (visual != null)
               return (UIElement)XamlReader.Parse(visual.InnerXml);

           return null;
       }

       private UIElement createInitialForm(XmlNode xmlNode)
       {
           Grid g = new Grid();
           g.Background = Brushes.LightGray;

           int rowIndex = 0;

           g.ColumnDefinitions.Add(new ColumnDefinition());
           g.RowDefinitions.Add(new RowDefinition());


           Label lh = new Label();
           lh.Content = "Form Name";
           lh.Height = 25;
           lh.SetValue(Grid.RowProperty, rowIndex++);
           lh.SetValue(Grid.ColumnProperty, 0);
           g.Children.Add(lh);

           g.RowDefinitions.Add(new RowDefinition());

           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string label = "Field Label : ";
           string text = "Data";

           Label l = new Label();
           l.Content = "Field Label : ";
           l.Height = 25;
           stack.Children.Add(l);

           TextBox tb = new TextBox();
           tb.Text = "Data";
           tb.Height = 25;
           stack.Children.Add(tb);

           stack.SetValue(Grid.RowProperty, rowIndex++);
           stack.SetValue(Grid.ColumnProperty, 0);
           g.Children.Add(stack);

           g.RowDefinitions.Add(new RowDefinition());

           StackPanel stack2 = new StackPanel();
           stack2.Orientation = Orientation.Horizontal;

           Label l2 = new Label();
           l2.Content = label;
           l2.Height = 25;
           stack2.Children.Add(l2);

           TextBox tb2 = new TextBox();
           tb2.Text = text;
           tb2.Height = 25;
           stack2.Children.Add(tb2);

           stack2.SetValue(Grid.RowProperty, rowIndex++);
           stack2.SetValue(Grid.ColumnProperty, 0);
           g.Children.Add(stack2);

           return g;
       }

       private UIElement createInitialFormField(XmlNode field)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string label = "Label : ";
           string text = "Data";

           Label l = new Label();
           l.Content = label;
           l.Height = 25;
           stack.Children.Add(l);

           TextBox tb = new TextBox();
           tb.Text = text;
           tb.Height = 25;
           stack.Children.Add(tb);

           return stack;
       }

       private UIElement createInitialClassDiagramModel(XmlNode model)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;
           stack.HorizontalAlignment = HorizontalAlignment.Left;
           //stack.

           string name = model.SelectSingleNode("Name").InnerText;

           TextBlock t0 = new TextBlock();
           t0.Height = 20;
           t0.Width = 70;
           t0.Text = name;
           t0.FontWeight = FontWeights.Bold;
           Border b0 = new Border();
           b0.Width = 70;
           b0.BorderBrush = Brushes.Black;
           b0.Background = Brushes.White;
           b0.BorderThickness = new Thickness(1);
           b0.Child = t0;
           stack.Children.Add(b0);

           TextBlock t1 = new TextBlock();
           t1.Height = 60;
           t1.Width = 100;
           t1.Text = "";
           Border b = new Border();
           b.BorderBrush = Brushes.Black;
           b.Background = Brushes.White;
           b.Width = 100;
           b.BorderThickness = new Thickness(1);
           b.Child = t1;
           stack.Children.Add(b);


           return stack;
       }

       private UIElement createInitialUMLClassLink(XmlNode link)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string start = link.SelectSingleNode("StartClass").InnerText;

           string end = link.SelectSingleNode("EndClass").InnerText;

           Label l = new Label();
           l.Content = start;
           stack.Children.Add(l);

           Line line = new Line();
           line.X1 = 20;
           line.X2 = 60;
           line.Y1 = line.Y2 = 10;
           line.StrokeThickness = 2;
           line.Stroke = Brushes.Black;
           stack.Children.Add(line);

           Label l2 = new Label();
           l2.Content = end;
           stack.Children.Add(l2);

           return stack;
       }

       private UIElement createInitialUMLClass(XmlNode classNode)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;

           string name = classNode.SelectSingleNode("Name").InnerText;

           string access = "";
           XmlNode anode = classNode.SelectSingleNode("Access");
           if (anode != null)
           {
               switch (anode.InnerText.ToLower())
               {
                   case ("private"):
                       access = "-";
                       break;
                   case ("public"):
                       access = "+";
                       break;
                   case ("package"):
                       access = "*";
                       break;
                   default:
                       access = " ";
                       break;
               }
           }
           else
               access = " ";

           TextBlock t0 = new TextBlock();
           t0.Height = 20;
           t0.Width = 122;
           t0.Text = access + " " + name;
           t0.FontWeight = FontWeights.Bold;
           Border b0 = new Border();
           b0.BorderBrush = Brushes.Black;
           b0.Background = Brushes.White;
           b0.BorderThickness = new Thickness(2);
           b0.Child = t0;
           stack.Children.Add(b0);

           TextBlock t1 = new TextBlock();
           t1.Height = 20;
           t1.Width = 122;
           t1.Text = "Attributes ...";
           Border b = new Border();
           b.BorderBrush = Brushes.Black;
           b.Background = Brushes.White;
           b.BorderThickness = new Thickness(1);
           b.Child = t1;
           stack.Children.Add(b);

           TextBlock t2 = new TextBlock();
           t2.Height = 20;
           t2.Width = 122;
           t2.Text = "Operations ...";
           Border b2 = new Border();
           b2.BorderBrush = Brushes.Black;
           b2.Background = Brushes.White;
           b2.BorderThickness = new Thickness(1);
           b2.Child = t2;
           stack.Children.Add(b2);

           return stack;

       }

       private UIElement createInitialUMLOperationParameter(XmlNode param)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string paramName = param.SelectSingleNode("Name").InnerText;

           string paramType = param.SelectSingleNode("Type").InnerText;

           Label l = new Label();
           l.Content = paramType;
           l.Foreground = Brushes.Navy;
           stack.Children.Add(l);

           Label l2 = new Label();
           l2.Content = paramName;
           stack.Children.Add(l2);

           return stack;
       }

       private UIElement createInitialUMLOperation(XmlNode opp)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string opAccess = opp.SelectSingleNode("Access").InnerText;

           string opName = opp.SelectSingleNode("Name").InnerText;

           string opReturnType = opp.SelectSingleNode("ReturnType").InnerText;

           string opParams = opp.SelectSingleNode("Parameters").InnerText;

           Label l = new Label();
           l.Content = "- " + opName + " (" + opParams + "...)" + " : " + opReturnType;

           stack.Children.Add(l);

           return stack;
       }

       public UIElement createInitialBar(XmlNode barNode)
       {

           XmlNode clnode = barNode.SelectSingleNode("color");
           string cl;
           if (clnode == null)
               cl = "Black";
           else
               cl = clnode.InnerText;

           double value = 50;
           XmlNode vlnode = barNode.SelectSingleNode("value");
           if (vlnode != null)
               value = Double.Parse(vlnode.InnerText);

           XmlNode nnode = barNode.SelectSingleNode("name");
           string name = "Bar name";
           if (nnode != null)
               name = nnode.InnerText;

           //if (!String.IsNullOrEmpty(barNode.SelectNodes("value")[0].InnerText))
           //    value = Convert.ToDouble(barNode.SelectNodes("value")[0].InnerText);

           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;

           TextBlock tvalue = new TextBlock();
           tvalue.Text = value.ToString();
           tvalue.TextAlignment = TextAlignment.Center;

           stack.Children.Add(tvalue);

           stack.Children.Add(createRectangle(10, value, cl));

           TextBlock tname = new TextBlock();
           tname.Text = name;
           tname.TextAlignment = TextAlignment.Center;

           stack.Children.Add(tname);

           return stack;
       }

       public UIElement createInitialBarchart(XmlNode barchartNode)
       {
           Canvas c = new Canvas();

           c.Background = Brushes.White;
           c.Height = 135;
           c.Width = 115;

           //get chart name
           string chartName = "Chart Name";
           Label chartNameLable = new Label();
           chartNameLable.FontSize = 8;
           chartNameLable.Height = 20;
           chartNameLable.Content = chartName;

           Canvas.SetTop(chartNameLable, 0);
           Canvas.SetLeft(chartNameLable, 30);
           c.Children.Add(chartNameLable);


           //prepare axis
           // Draw X axis
           ArrowLine xaxis = new ArrowLine();
           xaxis.Stroke = Brushes.Black;
           xaxis.StrokeThickness = 2;
           xaxis.IsArrowClosed = true;
           xaxis.ArrowLength = 4;

           xaxis.X1 = 5;
           xaxis.Y1 = 100;
           xaxis.X2 = 105;// +bars.Count * 10;
           xaxis.Y2 = 100;

           Canvas.SetLeft(xaxis, 0);
           Canvas.SetTop(xaxis, 20);
           xaxis.Stroke = Brushes.Black;
           c.Children.Add(xaxis);

           string xlabel = barchartNode.SelectNodes("xaxis")[0].InnerText;

           Label txlabel = new Label();
           txlabel.Content = xlabel;
           //txlabel. = TextAlignment.Center;

           Canvas.SetLeft(txlabel, 30);
           Canvas.SetTop(txlabel, 115);

           c.Children.Add(txlabel);

           // DrawY axis
           string ylabel = barchartNode.SelectNodes("yaxis")[0].InnerText;

           TextBlock tylabel = new TextBlock();
           tylabel.Text = ylabel;
           tylabel.TextAlignment = TextAlignment.Center;
           tylabel.LayoutTransform = new RotateTransform(90);

           Canvas.SetLeft(tylabel, 0);
           Canvas.SetTop(tylabel, 40);

           c.Children.Add(tylabel);

           ArrowLine yaxis = new ArrowLine();
           yaxis.Stroke = Brushes.Black;
           yaxis.StrokeThickness = 2;
           yaxis.IsArrowClosed = true;
           yaxis.ArrowLength = 4;

           yaxis.X1 = 14;
           yaxis.Y1 = 127;
           yaxis.X2 = 14;
           yaxis.Y2 = 25;

           Canvas.SetLeft(yaxis, 0);
           Canvas.SetTop(yaxis, 0);
           c.Children.Add(yaxis);


           return c;
       }

       private UIElement createInitialJavaSource(XmlNode xmlNode)
       {
           int lineHeight = 23;

           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;
           stack.Background = Brushes.White;

           //create comments line
           Label comments1 = new Label();
           comments1.Content = @"/* this is an automatic generated code ";
           comments1.Foreground = Brushes.Green;
           comments1.Height = lineHeight;
           stack.Children.Add(comments1);

           //create contents
           //create code header
           StackPanel codeHeader = new StackPanel();
           codeHeader.Orientation = Orientation.Horizontal;

           Label package1 = new Label();
           package1.Content = "package";
           package1.Foreground = Brushes.Navy;
           package1.Height = lineHeight;
           codeHeader.Children.Add(package1);

           Label packageName = new Label();
           packageName.Content = xmlNode.SelectSingleNode("packageName").InnerText;
           packageName.Foreground = Brushes.Black;
           packageName.Height = lineHeight;
           //packageName.Width = lineWidth;
           codeHeader.Children.Add(packageName);

           stack.Children.Add(codeHeader);

           //open package
           //Label openBracket = new Label();
           //openBracket.Content = "{";
           //openBracket.Foreground = Brushes.Black;
           //openBracket.Height = lineHeight;
           //stack.Children.Add(openBracket);

           Label classes = new Label();
           classes.Content = "Classes ...";
           classes.Foreground = Brushes.Navy;
           classes.Height = lineHeight;
           stack.Children.Add(classes);

           //close package
           //Label closeBracket = new Label();
           //closeBracket.Content = "}";
           //closeBracket.Foreground = Brushes.Black;
           //closeBracket.Height = lineHeight;
           //stack.Children.Add(closeBracket);

           return stack;
       }

       private UIElement createInitialJavaClass(XmlNode xmlNode)
       {
           int lineHeight = 23;

           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;
           stack.Background = Brushes.White;

           //create contents
           //create code header
           StackPanel codeHeader = new StackPanel();
           codeHeader.Orientation = Orientation.Horizontal;

           Label identifier1 = new Label();
           identifier1.Content = xmlNode.SelectSingleNode("identifier").InnerText + " ";
           identifier1.Foreground = Brushes.Navy;
           identifier1.Height = lineHeight;
           codeHeader.Children.Add(identifier1);

           Label class1 = new Label();
           class1.Content = "class ";
           class1.Foreground = Brushes.Navy;
           class1.Height = lineHeight;
           codeHeader.Children.Add(class1);

           Label packageName = new Label();
           packageName.Content = xmlNode.SelectSingleNode("name").InnerText + " ";
           packageName.Foreground = Brushes.Black;
           packageName.Height = lineHeight;
           codeHeader.Children.Add(packageName);

           stack.Children.Add(codeHeader);

           //open class
           Label openBracket = new Label();
           openBracket.Content = "{";
           openBracket.Foreground = Brushes.Black;
           openBracket.Height = lineHeight;
           stack.Children.Add(openBracket);

           Label attrs = new Label();
           attrs.Content = "Properties ...";
           attrs.Foreground = Brushes.Navy;
           attrs.Height = lineHeight;
           stack.Children.Add(attrs);

           Label methods = new Label();
           methods.Content = "Methods ...";
           methods.Foreground = Brushes.Navy;
           methods.Height = lineHeight;
           stack.Children.Add(methods);

           //close close
           Label closeBracket = new Label();
           closeBracket.Content = "}";
           closeBracket.Foreground = Brushes.Black;
           closeBracket.Height = lineHeight;
           stack.Children.Add(closeBracket);

           return stack;
       }

       private UIElement createInitialJavaMethod(XmlNode xmlNode)
       {
           int lineHeight = 23;

           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;
           stack.Background = Brushes.White;

           //create contents
           //create code header
           StackPanel codeHeader = new StackPanel();
           codeHeader.Orientation = Orientation.Horizontal;

           Label identifier1 = new Label();
           identifier1.Content = xmlNode.SelectSingleNode("identifier").InnerText + " ";
           identifier1.Foreground = Brushes.Navy;
           identifier1.Height = lineHeight;
           codeHeader.Children.Add(identifier1);


           Label returnType = new Label();
           returnType.Content = xmlNode.SelectSingleNode("return-type").InnerText + " ";
           returnType.Foreground = Brushes.Navy;
           returnType.Height = lineHeight;
           codeHeader.Children.Add(returnType);

           Label functionName = new Label();
           functionName.Content = xmlNode.SelectSingleNode("name").InnerText + " ( Params... )";
           functionName.Foreground = Brushes.Black;
           functionName.Height = lineHeight;
           codeHeader.Children.Add(functionName);

           stack.Children.Add(codeHeader);

           //open class
           Label openBracket = new Label();
           openBracket.Content = "{";
           openBracket.Foreground = Brushes.Black;
           openBracket.Height = lineHeight;
           stack.Children.Add(openBracket);

           //close close
           Label closeBracket = new Label();
           closeBracket.Content = "}";
           closeBracket.Foreground = Brushes.Black;
           closeBracket.Height = lineHeight;
           stack.Children.Add(closeBracket);

           return stack;

       }

       private UIElement createInitialJavaProperty(XmlNode xmlNode)
       {
           int lineHeight = 23;
           StackPanel stack = new StackPanel();
           stack.Background = Brushes.White;
           stack.Height = lineHeight;
           stack.Orientation = Orientation.Horizontal;

           Label accessLabel = new Label();
           accessLabel.Content = xmlNode.SelectSingleNode("identifier").InnerText;
           accessLabel.Height = lineHeight;
           accessLabel.Foreground = Brushes.Navy;
           stack.Children.Add(accessLabel);

           Label typeLabel = new Label();
           typeLabel.Content = xmlNode.SelectSingleNode("type").InnerText;
           typeLabel.Height = lineHeight;
           typeLabel.Foreground = Brushes.Navy;
           stack.Children.Add(typeLabel);

           Label attrNameLabel = new Label();
           attrNameLabel.Height = lineHeight;
           attrNameLabel.Content = xmlNode.SelectSingleNode("name").InnerText + ";";
           stack.Children.Add(attrNameLabel);

           return stack;
       }

       private UIElement createInitialJavaParameter(XmlNode xmlNode)
       {
           int lineHeight = 23;
           StackPanel stack = new StackPanel();
           stack.Background = Brushes.White;
           stack.Height = lineHeight;
           stack.Orientation = Orientation.Horizontal;

           string paramName = xmlNode.SelectSingleNode("name").InnerText;

           string paramType = xmlNode.SelectSingleNode("type").InnerText;

           Label l = new Label();
           l.Content = paramType;
           l.Foreground = Brushes.Navy;
           stack.Children.Add(l);

           Label l2 = new Label();
           l2.Content = paramName;
           stack.Children.Add(l2);

           return stack;
       }

       private UIElement createInitialUMLAttribute(XmlNode attr)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Horizontal;

           string attrAccess = attr.SelectSingleNode("Access").InnerText;

           string attrName = attr.SelectSingleNode("Name").InnerText;

           string attrType = attr.SelectSingleNode("Type").InnerText;

           Label l = new Label();
           l.Content = "+ " + attrName + " : " + attrType;

           stack.Children.Add(l);

           return stack;
       }

       private UIElement createInitialColumn(XmlNode col)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;

           string name = col.SelectSingleNode("name").InnerText;

           string value = col.SelectSingleNode("value").InnerText;

           Label lname = new Label();
           lname.Content = name;

           Border b = new Border();
           b.BorderBrush = Brushes.Black;
           b.Background = Brushes.White;
           b.BorderThickness = new Thickness(2);
           b.Child = lname;
           stack.Children.Add(b);

           Label lvalue = new Label();
           lvalue.Content = value;

           Border b2 = new Border();
           b2.BorderBrush = Brushes.Black;
           b2.Background = Brushes.White;
           b2.BorderThickness = new Thickness(2);
           b2.Child = lvalue;
           stack.Children.Add(b2);



           return stack;
       }

       private UIElement createInitialRow(XmlNode row)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;

           StackPanel stack1 = new StackPanel();
           stack1.Orientation = Orientation.Horizontal;

           StackPanel stack2 = new StackPanel();
           stack2.Orientation = Orientation.Horizontal;

           stack.Children.Add(stack1);
           stack.Children.Add(stack2);

           TextBlock t = new TextBlock();
           t.Height = 20;
           t.Width = 54;
           t.Text = "";
           stack1.Children.Add(t);

           TextBlock t2 = new TextBlock();
           t2.Background = Brushes.White;
           t2.Height = 20;
           t2.Width = 70;
           t2.Text = "ColumnName";
           Border b2 = new Border();
           b2.BorderBrush = Brushes.Black;
           b2.Background = Brushes.White;
           b2.BorderThickness = new Thickness(2);
           b2.Child = t2;
           stack1.Children.Add(b2);

           TextBlock t3 = new TextBlock();
           t3.Background = Brushes.White;
           t3.Height = 20;
           t3.Width = 50;
           t3.Text = "Row";
           Border b3 = new Border();
           b3.BorderBrush = Brushes.Black;
           b3.Background = Brushes.White;
           b3.BorderThickness = new Thickness(2);
           b3.Child = t3;
           stack2.Children.Add(b3);

           TextBlock t4 = new TextBlock();
           t4.Background = Brushes.White;
           t4.Height = 20;
           t4.Width = 70;
           t4.Text = "Columnvalue";
           Border b4 = new Border();
           b4.BorderBrush = Brushes.Black;
           b4.Background = Brushes.White;
           b4.BorderThickness = new Thickness(2);
           b4.Child = t4;
           stack2.Children.Add(b4);

           return stack;
       }

       private UIElement createInitialTable(XmlNode table)
       {
           StackPanel stack = new StackPanel();
           stack.Orientation = Orientation.Vertical;

           StackPanel stack1 = new StackPanel();
           stack1.Orientation = Orientation.Horizontal;

           StackPanel stack2 = new StackPanel();
           stack2.Orientation = Orientation.Horizontal;

           string name = table.SelectSingleNode("name").InnerText;

           TextBlock t0 = new TextBlock();
           t0.Height = 20;
           t0.Width = 122;
           t0.Text = name;
           Border b0 = new Border();
           b0.BorderBrush = Brushes.Black;
           b0.Background = Brushes.White;
           b0.BorderThickness = new Thickness(2);
           b0.Child = t0;
           stack.Children.Add(b0);

           stack.Children.Add(stack1);
           stack.Children.Add(stack2);

           TextBlock t = new TextBlock();
           t.Background = Brushes.White;
           t.Height = 20;
           t.Width = 50;
           t.Text = "";
           Border b = new Border();
           b.BorderBrush = Brushes.Black;
           b.Background = Brushes.White;
           b.BorderThickness = new Thickness(2);
           b.Child = t;
           stack1.Children.Add(b);

           TextBlock t2 = new TextBlock();
           t2.Background = Brushes.White;
           t2.Height = 20;
           t2.Width = 70;
           t2.Text = "ColumnName";
           Border b2 = new Border();
           b2.BorderBrush = Brushes.Black;
           b2.Background = Brushes.White;
           b2.BorderThickness = new Thickness(2);
           b2.Child = t2;
           stack1.Children.Add(b2);

           TextBlock t3 = new TextBlock();
           t3.Background = Brushes.White;
           t3.Height = 20;
           t3.Width = 50;
           t3.Text = "Row";
           Border b3 = new Border();
           b3.BorderBrush = Brushes.Black;
           b3.Background = Brushes.White;
           b3.BorderThickness = new Thickness(2);
           b3.Child = t3;
           stack2.Children.Add(b3);

           TextBlock t4 = new TextBlock();
           t4.Background = Brushes.White;
           t4.Height = 20;
           t4.Width = 70;
           t4.Text = "Columnvalue";
           Border b4 = new Border();
           b4.BorderBrush = Brushes.Black;
           b4.Background = Brushes.White;
           b4.BorderThickness = new Thickness(2);
           b4.Child = t4;
           stack2.Children.Add(b4);

           return stack;
       }
       */

        /*private Ellipse CreateEllipse(Canvas c, double width, double height, double desiredCenterX, double desiredCenterY)
        {
            Ellipse ellipse = new Ellipse { Width = width, Height = height };
            double left = desiredCenterX - (width / 2);
            double top = desiredCenterY - (height / 2);

            ellipse.Margin = new Thickness(left, top, 0, 0);

            Canvas.SetLeft(ellipse, desiredCenterX - (width / 2));
            c.Children.Add(ellipse);

            return ellipse;
        }*/

        #endregion //old stuff


        #region pars XAML test

        //for traceability and skin
        public UIElement parsImanSXaml()
        {
            UIElement u;

            /*string d = @"<Bar>
                            <name>barName</name>
                            <value>50</value>
                            <color>Green</color>
                         </Bar>";

            string s = @"<StackPanel xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' linkTo='Bar' >
             <Label> 
              <Label.Name>VLabel</Label.Name> 
              <Label.Background>White</Label.Background>
              <Label.Content linkTo='value'>Test</Label.Content>
             </Label>
             <Rectangle>
              <Rectangle.Name>Bar</Rectangle.Name> 
              <Rectangle.Fill linkTo='color'>Red</Rectangle.Fill> 
              <Rectangle.Width>10</Rectangle.Width>
              <Rectangle.Height linkTo='value'>100</Rectangle.Height>
             </Rectangle>
             <Label> 
              <Label.Name>BName</Label.Name> 
              <Label.Background>White</Label.Background>
              <Label.Content linkTo='name'>Test</Label.Content>
             </Label    
            </StackPanel>";/*

            /*string sxslt = @"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
	<xsl:template match='/'>
		<xsl:apply-templates select='Bar'/>
    </xsl:template>
    <xsl:template match='Bar'>
		<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
			<Label> 
              <Label.Name>VLabel</Label.Name> 
              <Label.Background>White</Label.Background>
              <xsl:apply-templates select='value' mode='name2'/>
            </Label>
            <Rectangle>
              <Rectangle.Name>Bar</Rectangle.Name> 
              <xsl:apply-templates select='color'/>
              <Rectangle.Width>10</Rectangle.Width>
              <xsl:apply-templates select='value' mode='name1'/>
             </Rectangle>
             <Label> 
              <Label.Name>BName</Label.Name> 
              <Label.Background>White</Label.Background>
              <xsl:apply-templates select='name'/> 
             </Label>    
            </StackPanel>
	</xsl:template>
	<xsl:template match='value' mode='name2'>
		<Label.Content>
			<xsl:value-of select='.'/>
		</Label.Content>
	</xsl:template>
	<xsl:template match='value' mode='name1'>
		<Rectangle.Height>
			<xsl:value-of select='.'/>
		</Rectangle.Height>
	</xsl:template>
	<xsl:template match='color'>
		<Rectangle.Fill>
			<xsl:value-of select='.'/>
		</Rectangle.Fill> 
	</xsl:template>
	<xsl:template match='name'>
		<Label.Content>
			<xsl:value-of select='.'/>
		</Label.Content>
	</xsl:template>
</xsl:stylesheet>";*/

            //XmlDocument sourceDoc = new XmlDocument();
            //sourceDoc.LoadXml(d);
            //sourceDoc.Save("source.xml");

            //XmlDocument docxsl = new XmlDocument();
            //docxsl.LoadXml(sxslt);
            //docxsl.Save("testRender.xsl");

            XslCompiledTransform myXslTransform = new XslCompiledTransform();
            myXslTransform.Load("testRender.xsl");
            myXslTransform.Transform("source.xml", "targetVisual.xml");

            XmlDocument xd = new XmlDocument();
            xd.Load("targetVisual.xml");
            //string s2 = parseMyXaml(s, d);
            XmlPrettyPrinter prettyPrinter = new XmlPrettyPrinter();
            prettyPrinter.PrintXmlFile("targetVisual.xml");
            //prettyPrinter.PrintXmlFile("testRender.xsl");

            MessageBox.Show(xd.DocumentElement.OuterXml);
            //VisualElement mVe = new VisualElement();
            //mVe.Data = xNode;
            u = XamlReader.Parse(xd.OuterXml) as UIElement;
            //return mVe;

            return u;
        }

        private string parseMyXaml(string xaml, string data)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(data);

            AbstractLattice al = new AbstractLattice(xd.DocumentElement);

            MessageBox.Show(al.Root.Name);

            XmlElement rx = parseToXSLT(xd.DocumentElement, al);

            MessageBox.Show(rx.OuterXml);
            return rx.OuterXml;
        }

        private XmlElement parseToXSLT(XmlElement xamlElement, AbstractLattice al)
        {
            if (xamlElement.HasAttribute("linlTo"))
            {
                //String prefix = "xsl";
                //String testNamespace = "http://www.w3.org/1999/XSL/Transform";

                //XmlElement xnode = xdoc.CreateElement(prefix, "apply-templates", testNamespace);
                //XmlAttribute xattr = xdoc.CreateAttribute("select");
                //xattr.Value = tempalteName;
                //xnode.Attributes.Append(xattr);
            }
            else
            {
                foreach (XmlElement x in xamlElement.ChildNodes)
                    parseToXSLT(x, al);
            }

            return null;

        }

        #endregion //parse XAML test
        

        #region test area

        /*private UIElement createNotation(VisualElement ve)
        {
            try
            {
                //ve.ItemXML = xNode.Clone();

                XmlNode nameAttr = ve.ItemXML.Attributes.GetNamedItem("name");
                String name;
                if (nameAttr != null)
                    name = nameAttr.InnerText;
                else
                    name = "";

                ve.VEName = name;

                XmlNode transNode = ve.ItemXML.SelectSingleNode("trans");
                if (transNode != null)
                {
                    //MessageBox.Show(xNode.OuterXml);
                    string notationXml = renderNotation(transNode, ve.Data.OuterXml);

                    byte[] byteArray = Encoding.Unicode.GetBytes(notationXml);
                    MemoryStream stream = new MemoryStream(byteArray);

                    FrameworkElement rootObject = XamlReader.Load(stream) as FrameworkElement;

                    ve = rootObject as VisualElement;

                    return ve as UIElement;
                    //ve.Trans = transNode.Clone();
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Renderer -> renderItem (Visual Element): Something went wrong ->\n\n " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }


        }*/

        #region bottom-up rendering Test

       /* private UIElement draw(XmlNode X)
        {
            if (!X.HasChildNodes)
            {
                UIElement temp = createVisual(X);
                if (temp != null)
                    return temp;
                //else
                // return null;
            }
            else
            {
                //List<UIElement> elementList = new List<UIElement>();
                //process children first
                foreach (XmlNode child in X.ChildNodes)
                {
                    UIElement temp = draw(child);
                    if (temp != null)
                        elementList.Add(temp);
                }

                //process self
                UIElement temp2;
                //if (elementList.Count > 0)
                //    temp2 = createVisual(X, elementList);
                //else
                temp2 = createVisual(X);

                return temp2;
            }

            return null;
        }

        /// <summary>
        /// create visual element content and embed list of elements in the visual content based on predefined layout
        /// </summary>
        /// <param name="X">XmlNode representative of the model to be visualised</param>
        /// <param name="elementList"> list of elements to be embedded in the content</param>
        /// <returns>VisualElement content as UIElement</returns>
        private UIElement createVisual(XmlNode xnode, List<UIElement> elementList)
        {
            MessageBox.Show(xnode.Name + "\n in with list");

            switch (xnode.Name.ToLower())
            {
                //case ("bar"):
                //    return createBar(xnode, 100);
                //case ("javasource"):
                //    return renderJavaCode(xnode);
                //case ("model"):
                //    return renderUMLModel(xnode);
                //case ("table"):
                //    return renderTable(xnode);

                //case ("row"):
                //    return createTableRow(xnode);
                //case ("column"):
                //    return create(xnode);
                default:
                    return null;
            }
        }

        private UIElement drawBarchart(XmlNode xNode)
        {
            //XmlNodeList bars = xNode.SelectSingleNode("Bars").ChildNodes;

            //prepare chart area=============
            List<UIElement> bars = elementList;

            Canvas c = new Canvas();
            c.Background = Brushes.White;
            c.Height = 285;
            c.Width = 40 + 5 + bars.Count * 20;

            //get chart name
            string chartName = xNode.SelectNodes("name")[0].InnerText;
            Label chartNameLable = new Label();
            //chartNameLable.Height = 15;
            chartNameLable.FontSize = 8;
            chartNameLable.Content = chartName;

            Canvas.SetTop(chartNameLable, 0);
            Canvas.SetLeft(chartNameLable, 30);
            c.Children.Add(chartNameLable);

            //prepare axis
            // Draw X axis
            ArrowLine xaxis = new ArrowLine();
            xaxis.Stroke = Brushes.Black;
            xaxis.StrokeThickness = 2;
            xaxis.IsArrowClosed = true;
            xaxis.ArrowLength = 4;

            xaxis.X1 = 5;
            xaxis.Y1 = 225;
            xaxis.X2 = 30 + bars.Count * 20;
            xaxis.Y2 = 225;

            Canvas.SetLeft(xaxis, 0);
            Canvas.SetTop(xaxis, 0);
            c.Children.Add(xaxis);

            string xlabel = xNode.SelectNodes("xaxis")[0].InnerText;

            Label txlabel = new Label();
            txlabel.Content = xlabel;
            //txlabel. = TextAlignment.Center;

            Canvas.SetLeft(txlabel, xaxis.X2 / 2 - 20);
            Canvas.SetTop(txlabel, 260);

            c.Children.Add(txlabel);

            // DrawY axis
            string ylabel = xNode.SelectNodes("yaxis")[0].InnerText;

            TextBlock tylabel = new TextBlock();
            tylabel.Text = ylabel;
            tylabel.TextAlignment = TextAlignment.Center;
            tylabel.LayoutTransform = new RotateTransform(90);

            Canvas.SetLeft(tylabel, 0);
            Canvas.SetTop(tylabel, 100);

            c.Children.Add(tylabel);

            ArrowLine yaxis = new ArrowLine();
            yaxis.Stroke = Brushes.Black;
            yaxis.StrokeThickness = 2;
            yaxis.IsArrowClosed = true;
            yaxis.ArrowLength = 4;

            yaxis.X1 = 20;
            yaxis.Y1 = 235;
            yaxis.X2 = 20;
            yaxis.Y2 = 25;

            Canvas.SetLeft(yaxis, 0);
            Canvas.SetTop(yaxis, 0);
            c.Children.Add(yaxis);
            //=============

            //prepare bars 

            //VisualElement[] barElements = new VisualElement[bars.Count];

            //get maximum value for y axis
            //double maximumValue = 0;

            //not now, go with defaults
            //for (int i = 0; i < bars.Count; i++)
            //{
            //    XmlNode bar = bars[i];
            //    double temp = 0;
            //    if (bar.SelectNodes("value").Count > 0)
            //        temp = Convert.ToDouble(bar.SelectNodes("value")[0].InnerText);
            //    if (temp > maximumValue)
            //        maximumValue = temp;
            //}

            for (int i = 0; i < bars.Count; i++)
            {
                if ((bars[i] as VisualElement).Data.Name.Equals("Bar"))
                    elementHeight = ((((bars[i] as VisualElement).Content as StackPanel).Children[1] as Rectangle).Height) + 10;
                else
                    elementHeight = 0;

                Canvas.SetLeft(bars[i], (10 + 2 + i * 20));
                Canvas.SetTop(bars[i], 225 - elementHeight);

                bars[i].IsHitTestVisible = true;

                c.Children.Add(bars[i]);
            }


            //prepare chart element
            XmlNode tempChartData = xNode;
            XmlNode tempNode = tempChartData.SelectSingleNode("Bars");
            tempNode.RemoveAll();
            tempNode.InnerText = "Bars";

            VisualElement chart = new VisualElement();
            chart.Data = tempChartData;
            chart.Content = c;

            Canvas.SetLeft(chart, canvasLayingPosition.X);
            Canvas.SetTop(chart, canvasLayingPosition.Y);

            return chart;
        }*/

        /// <summary>
        /// Create visual element content for XmlNode.
        /// </summary>
        /// <param name="X">XmlNode representative of the model to be visualised</param>
        /// <returns>VisualELemet content as UIElement </returns>
        /*private UIElement createVisual(XmlNode xnode)
        {
            //MessageBox.Show(xnode.Name);

            switch (xnode.Name.ToLower())
            {
                case ("bar"):
                    return drawBar(xnode, 100);
                case ("barchart"):
                    return drawBarchart(xnode);
                case ("class"):
                    return drawUMLClass(xnode);
                //case ("javasource"):
                //    return renderJavaCode(xnode);
                //case ("model"):
                //    return renderUMLModel(xnode);
                //case ("table"):
                //    return renderTable(xnode);
                //case ("barchart"):
                //    return renderBarchart(xnode);
                //case ("row"):
                //    return createTableRow(xnode);
                //case ("column"):
                //    return create(xnode);
                default:
                    return null;
            }

        }

        private UIElement drawUMLClass(XmlNode cl)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;

            string name = cl.SelectSingleNode("Name").InnerText;

            TextBlock t0 = new TextBlock();
            t0.Height = 20;
            t0.Text = name;
            t0.FontWeight = FontWeights.Bold;
            Border b0 = new Border();
            b0.BorderBrush = Brushes.Black;
            b0.Background = Brushes.White;
            b0.BorderThickness = new Thickness(2);
            b0.Child = t0;
            stack.Children.Add(b0);

            XmlNodeList attrs = cl.SelectNodes("Attributes/Attribute");
            StackPanel attrStack = new StackPanel();
            attrStack.Orientation = Orientation.Vertical;

            foreach (XmlNode attr in attrs)
                attrStack.Children.Add(renderUMLAttribute(attr));

            Border b = new Border();
            b.BorderBrush = Brushes.Black;
            b.Background = Brushes.White;
            b.BorderThickness = new Thickness(1);
            b.Child = attrStack;
            stack.Children.Add(b);

            XmlNodeList ops = cl.SelectNodes("Operations/Operation");
            StackPanel opStack = new StackPanel();
            opStack.Orientation = Orientation.Vertical;

            foreach (XmlNode op in ops)
                opStack.Children.Add(renderUMLOperation(op));

            Border b2 = new Border();
            b2.BorderBrush = Brushes.Black;
            b2.Background = Brushes.White;
            b2.BorderThickness = new Thickness(1);
            b2.Child = opStack;
            stack.Children.Add(b2);

            //prepare class element
            XmlNode tempClassData = cl;
            XmlNode tempNode = tempClassData.SelectSingleNode("Attributes");
            if (tempNode != null)
            {
                tempNode.RemoveAll();
                tempNode.InnerText = "Attributes";
            }

            XmlNode tempNode2 = tempClassData.SelectSingleNode("Operations");
            if (tempNode2 != null)
            {
                tempNode2.RemoveAll();
                tempNode2.InnerText = "Operations";
            }

            VisualElement cVe = new VisualElement();
            cVe.Data = tempClassData;
            cVe.Content = stack;

            cVe.Name = name;//class name as element name, for tracking and positioning links

            //Canvas.SetLeft(cVe, canvasLayingPosition.X);
            //Canvas.SetTop(cVe, canvasLayingPosition.Y);

            return cVe;
        }

        private UIElement drawBar(XmlNode barNode, int maximumHeightValue)
        {
            string name = barNode.SelectNodes("name")[0].InnerText;
            double value = Convert.ToDouble(barNode.SelectNodes("value")[0].InnerText);

            if (maximumHeightValue == 0)
                maximumHeightValue = 100; //choose default value of 100

            //normalise value considering y axis height 200 pixels
            double normValue = (value * 200) / maximumHeightValue;

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            stack.Height = 10 + normValue + name.Count() * 8;

            TextBlock tvalue = new TextBlock();
            tvalue.Text = value.ToString();
            tvalue.Height = 10;
            tvalue.TextAlignment = TextAlignment.Center;

            stack.Children.Add(tvalue);

            Rectangle rect = new Rectangle();
            rect.Width = 10;
            rect.Height = normValue;
            rect.Fill = Brushes.Red;
            stack.Children.Add(rect);

            TextBlock tname = new TextBlock();
            tname.Text = name;
            tname.Height = 10;
            tname.Width = name.Count() * 8;

            tname.TextAlignment = TextAlignment.Left;
            tname.LayoutTransform = new RotateTransform(90);

            stack.Children.Add(tname);
            //elementHeight = normValue + 10;


            //create visual element
            VisualElement barElement = new VisualElement();
            barElement.Data = barNode;

            barElement.Content = stack;

            return barElement;
        }*/

        #endregion

        #endregion //test area

        
    }
}
