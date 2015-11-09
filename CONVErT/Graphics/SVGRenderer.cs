using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Xsl;

namespace CONVErT
{
    public class SVGRenderer
    {

        #region prop

        Collection<XmlNode> TemplateRepository = new Collection<XmlNode>();
        Collection<String> ProcessedNodes = new Collection<string>();

        XMLHelper xmlHelper = new XMLHelper();
        XmlPrettyPrinter prettyPrinter = new XmlPrettyPrinter();

        #endregion //prop


        #region ctor

        public SVGRenderer()
        {
            string toolboxItemsPath = DirectoryHelper.getFilePathExecutingAssembly("Resources\\ToolBoxItems.xml");
            //string toolboxItemsPath = (@"C:\Users\iavazpour\Documents\SVN_IA\CONVErT\CONVErT\Resources\ToolBoxItems.xml");

            loadTempaltes(toolboxItemsPath);
        }

        public SVGRenderer(string toolboxfile)
        {
            loadTempaltes(toolboxfile);
        }


        #endregion //ctor

                
        #region templates
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
                            try
                            {
                                string temp = t.Attributes["match"].Value;
                                if (templateNames.IndexOf(temp) == -1)//perhaps later add a condition to check whether a conditional rule exists too
                                {
                                    templates.Add(t);
                                    templateNames.Add(temp);
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("(SVGRenderer.traverseAndAddTemplates) -> : Something went wrong -> " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                        }
                    ProcessedNodes.Add(address);
                }
            }

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

                        XmlNodeList items = inXmlNode.SelectNodes("//item[@type='SVG']");

                        foreach (XmlNode xnode in items)
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
                catch (XmlException xmlEx)
                {
                    MessageBox.Show("(Renderer) -> problem loading templates (XmlException) : \n\n" + xmlEx.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("(Renderer) -> problem loading templates (Exception) : \n\n" + ex.Message);
                }
            }
            else
                MessageBox.Show("(Renderer) -> Could not load Items file : " + toolboxItemsPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion //templates
        
        
        #region render items

        public UIElement render(XmlNode xNode)
        {
            if (xNode.Name.Equals("item"))
                if (xNode.Attributes.GetNamedItem("type").InnerText == "SVG")
                    return renderItem(xNode);

            //MessageBox.Show(xNode.InnerText);
            return null;
        }

        private UIElement renderItem(XmlNode xNode)
        {
            MessageBox.Show("SVGRenderer -> render Item(Item XML) not implemented!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        #endregion //render items

        
        #region renderer logic

        private string createTransformation(string dataNodeName, Collection<XmlNode> templates)
        {
            //create transformation
            XmlDocument xdoc = new XmlDocument();
            XmlElement stylesheet = xdoc.CreateElement("stylesheet", "xsl");
            stylesheet.SetAttribute("xmlns:xsl", "http://www.w3.org/1999/XSL/Transform");
            //stylesheet.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");
            //stylesheet.SetAttribute("xmlns:local", "clr-namespace:CONVErT;assembly=CONVErT");
            //stylesheet.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
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
                stylesheet.AppendChild(stylesheet.OwnerDocument.ImportNode(x, true));

            String transXSL = prettyPrinter.PrintToString(stylesheet);

            return transXSL;
        }

        /// <summary>
        /// Create visualisation for concrete transformation
        /// </summary>
        /// <param name="fileName">file to be visualised</param>
        /// <returns></returns>
        public string createVisualisation(string fileName)
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

                            
                            //render notation
                            try
                            {
                                //byte[] byteArray = Encoding.ASCII.GetBytes(s);
                                //MemoryStream stream = new MemoryStream(byteArray);

                                //DependencyObject rootObject = XamlReader.Load(stream) as DependencyObject;

                                //return rootObject as UIElement;

                                StringWriter stringWriter = new StringWriter();

                                // Put HtmlTextWriter in using block because it needs to call Dispose.
                                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                                {
                                    writer.Write(prettyPrinter.PrintToString(output.ToString()));
                                    //writer.Close();
                                }

                                // Return the result
                                return prettyPrinter.PrintToString(stringWriter.ToString());

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("(SVGRenderer.createVisualisation) -> : Something went wrong -> " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                        }
                        else
                        {
                            MessageBox.Show("(SVGRenderer.createVisualisation) -> No template found for root element", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    MessageBox.Show("(SVGRenderer.createVisualisation) -> creating visualisation (XmlException) : \n\n" + xmlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("(SVGRenderer.createVisualisation) -> creating visualisation (Exception) : \n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("(SVGRenderer.createVisualisation) -> Could not load Items file : " + fileName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return null;
        }

        /// <summary>
        /// This function will render the visualisation file into still HTML which may include annimation but the 
        /// interaction is excluded for now. The resulted visualisation will include HTML and NOT VisualELements of CONVErT
        /// </summary>
        /// <param name="fileName">Visualsiaiton file (XML)</param>
        /// <returns>string</returns>
        public string renderToHTML(string fileName)
        {
            // Put HtmlTextWriter in using block because it needs to call Dispose.
            StringWriter stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {

                writer.RenderBeginTag(HtmlTextWriterTag.Html); // Begin #1 html

                writer.RenderBeginTag(HtmlTextWriterTag.Body); //Begin #4 body

                //writer.AddAttribute(HtmlTextWriterAttribute.Id, "mushroom");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "100%");

                writer.Write(createVisualisation(fileName));

                writer.RenderEndTag(); //End #4 body
                writer.RenderEndTag(); //End #1 html

                //writer.Close();
            }

            // Return the result
            return prettyPrinter.PrintToString(stringWriter.ToString());
        }


        //for HorusParser debugger
        internal string renderPartialVisualisation(XmlNode visXml)
        {
            if (visXml != null)
            {
                XmlDocument xd = new XmlDocument();
                
                xd.AppendChild(xd.ImportNode(visXml, true));
                xd.Save(DirectoryHelper.getFilePathExecutingAssembly("tempVisPartialRend.xml"));

                return createVisualisation(DirectoryHelper.getFilePathExecutingAssembly("tempVisPartialRend.xml"));
            }

            return null;
        }
        
        
        #endregion //renderer logic
    
    }
}
