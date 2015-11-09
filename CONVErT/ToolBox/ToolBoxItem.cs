using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    public class ToolboxItem : ContentControl
    {
        XAMLRenderer xamlRenderer = new XAMLRenderer();
        SVGRenderer svgRenderer = new SVGRenderer();

        public VisualElement visualElement { get; set; }
        public VisualBase visualBase { get; set; }

        public string TName { get; set; }

        private Point? dragStartPoint = null;

        static ToolboxItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        }

        public ToolboxItem()
        {
            
        }

        /// <summary>
        /// Initiate a New ToolBoxItem from predefined items XML
        /// </summary>
        /// <param name="itemNotation"></param>
        public ToolboxItem(XmlNode itemNotation)
        {
            //create with values read from a file
            //visualElement = new VisualElement();

            if (itemNotation.Name.Equals("item"))
            {
                if (itemNotation.Attributes.GetNamedItem("type").InnerText == "XAML")
                {
                    visualElement = xamlRenderer.render(itemNotation) as VisualElement;
                    if (visualElement != null)
                        TName = visualElement.VEName;

                    visualElement.loadDataFromXaml();
                    this.Content = visualElement;
                }
                else if (itemNotation.Attributes.GetNamedItem("type").InnerText == "SVG")
                {
                    //Show SVG icon for now
                    Image simpleImage = new Image();
                    simpleImage.Width = 150;
                    simpleImage.Height = 150;
                    // Create source.
                    BitmapImage bi = new BitmapImage();

                    // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"/Images/SVGIcon.png", UriKind.RelativeOrAbsolute);
                    bi.EndInit();

                    // Set the image source.
                    simpleImage.Source = bi;

                    this.Content = simpleImage;
                }
            }
            /*else
            {
                visualElement.Data = itemNotation;
                visualElement.Content = renderer.render(visualElement);
                TName = itemNotation.Name;
            }
            */
            
             
            //ControlTemplate defaultStyle = (ControlTemplate)FindResource("VisualDecorator");
            //visualElement.Template = defaultStyle;
        }

        public ToolboxItem(VisualElement v)
        {
            //create with values read from a file
            visualElement = (v.Clone() as VisualElement);
            visualBase = null;
            visualElement.Content = xamlRenderer.render(visualElement);
            this.Content = visualElement;//20/6/2012
        }

        public ToolboxItem(VisualBase vb)
        {
            visualBase = vb;
            visualElement = null;
            this.Content = visualBase;
        }

        /// <summary>
        /// Load Functions which have been defined previously
        /// </summary>
        /// <param name="item"></param>
        /*public void loadFunctionToolBoxItem(XmlNode item)
        {
            this.visualElement = new VisualElement();

            string imageName = item.SelectSingleNode("image").InnerXml;
            // Create the image element.
            Image simpleImage = new Image();
            simpleImage.Width = 75;
            simpleImage.Height = 75;                
            
            // Create source.
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(@"/Images/"+imageName, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            simpleImage.Source = bi;

            this.visualElement.Content = simpleImage;

            string data = item.SelectSingleNode("args").OuterXml;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(data);
            this.visualElement.Data = xDoc.DocumentElement.Clone();

            string dataR = item.SelectSingleNode("argsR").OuterXml;
            xDoc = new XmlDocument();
            xDoc.LoadXml(dataR);
            this.visualElement.ReverseData = xDoc.DocumentElement.Clone();

            string vname = item.SelectSingleNode("name").InnerText;
            this.visualElement.VEName = vname;

            this.visualElement.VType = VisualElementType.Function;
    
        }*/

        /// <summary>
        /// Load Custom ToolBarItems which have been defined and saved previously
        /// </summary>
        /// <param name="item"></param>
        public void loadCustomisedToolBoxItem(XmlNode item)
        {
            this.visualElement = new VisualElement();

            XmlNode itemXml = item.SelectSingleNode("VisualElement/item");
            if (itemXml != null)
            {
                visualElement.ItemXML = itemXml.Clone();
                this.visualElement = xamlRenderer.render(itemXml) as VisualElement;
            }
            //visualElement.loadDataFromXaml();

            string data = item.SelectSingleNode("VisualElement/Data").InnerXml;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(data);
            this.visualElement.Data = xDoc.DocumentElement.Clone();

            string dataR = item.SelectSingleNode("VisualElement/DataR").InnerXml;
            xDoc = new XmlDocument();
            xDoc.LoadXml(dataR);
            this.visualElement.ReverseData = xDoc.DocumentElement.Clone();
            
            string vname = item.SelectSingleNode("VisualElement/Name").InnerText;
            this.visualElement.VEName = vname;

            string templateName = item.SelectSingleNode("Template/Name").InnerText;
            string templateAddress = item.SelectSingleNode("Template/Address").InnerXml;

            this.visualElement.templateVM.TemplateName = templateName;
            this.visualElement.templateVM.TemplateAddress = templateAddress;

            string templateCode = item.SelectSingleNode("Template/Code").InnerXml;
            xDoc = new XmlDocument();
            xDoc.LoadXml(templateCode);
            this.visualElement.templateVM.TemplateXmlNode = xDoc.DocumentElement.Clone();
            //MessageBox.Show(visualElement.Data.OuterXml+"\n\n and \n\n"+ visualElement.templateVM.TemplateXmlNode.OuterXml.ToString());
            
            string templateRName = item.SelectSingleNode("TemplateR/Name").InnerText;
            //string templateRAddress = item.SelectSingleNode("TemplateR/Address").InnerXml;

            this.visualElement.templateVMR.TemplateName = templateRName;
            //this.visualElement.templateVMR.TemplateAddress = templateRAddress;

            string templateRCode = item.SelectSingleNode("TemplateR/Code").InnerXml;
            xDoc = new XmlDocument();
            xDoc.LoadXml(templateRCode);
            this.visualElement.templateVMR.TemplateXmlNode = xDoc.DocumentElement.Clone();

            this.Content = visualElement;//20/6/2012

            //string tname = item.SelectSingleNode("Name").InnerText;
            //this.TName = tname;
        }

        /// <summary>
        /// Saves current toolBox item ito an XmlNode, for reuseability purposes and returns it
        /// </summary>
        /// <returns>XmlNode</returns>
        public XmlNode saveToolBoxItemToXml()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode output = xDoc.CreateElement("CustomItem") as XmlNode;

            XmlNode Visualnode = xDoc.CreateElement("VisualElement") as XmlNode;
            
            XmlNode VisName = xDoc.CreateElement("Name") as XmlNode;
            VisName.AppendChild(xDoc.CreateTextNode(visualElement.VEName));

            Visualnode.AppendChild(VisName);

            //Data
            XmlNode VisData = xDoc.CreateElement("Data") as XmlNode;
            VisData.AppendChild(VisData.OwnerDocument.ImportNode(visualElement.Data, true));

            Visualnode.AppendChild(VisData);

            //Reverse Data
            XmlNode VisDataR = xDoc.CreateElement("DataR") as XmlNode;
            VisDataR.AppendChild(VisDataR.OwnerDocument.ImportNode(visualElement.ReverseData, true));

            Visualnode.AppendChild(VisDataR);

            //Notation transformation
            if (visualElement.ItemXML != null)
                Visualnode.AppendChild(Visualnode.OwnerDocument.ImportNode(visualElement.ItemXML, true));

            output.AppendChild(Visualnode);

            //forward template
            XmlNode template = xDoc.CreateElement("Template") as XmlNode;

            XmlNode templateCode = xDoc.CreateElement("Code") as XmlNode;
            templateCode.AppendChild(templateCode.OwnerDocument.ImportNode(visualElement.templateVM.TemplateXmlNode, true));
            template.AppendChild(templateCode);

            XmlNode templateName = xDoc.CreateElement("Name") as XmlNode;
            templateName.AppendChild(xDoc.CreateTextNode(visualElement.templateVM.TemplateName));
            template.AppendChild(templateName);

            XmlNode templateAddress = xDoc.CreateElement("Address") as XmlNode;
            templateAddress.AppendChild(xDoc.CreateTextNode(visualElement.templateVM.TemplateAddress));
            template.AppendChild(templateAddress);

            output.AppendChild(template);

            //reverse template
            XmlNode templateR = xDoc.CreateElement("TemplateR") as XmlNode;

            XmlNode templateRCode = xDoc.CreateElement("Code") as XmlNode;
            templateRCode.AppendChild(templateRCode.OwnerDocument.ImportNode(visualElement.templateVMR.TemplateXmlNode, true));
            templateR.AppendChild(templateRCode);

            XmlNode templateRName = xDoc.CreateElement("Name") as XmlNode;
            templateRName.AppendChild(xDoc.CreateTextNode(visualElement.templateVMR.TemplateName));
            templateR.AppendChild(templateRName);

            XmlNode templateRAddress = xDoc.CreateElement("Address") as XmlNode;
            //templateRAddress.AppendChild(xDoc.CreateTextNode(visualElement.templateVMR.TemplateAddress));
            templateR.AppendChild(templateRAddress);

            output.AppendChild(templateR);

            return output;
        }

        #region toolbox item interaction

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.dragStartPoint = new Point?(e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            if (this.dragStartPoint.HasValue)
            {
                Point position = e.GetPosition(this);
                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(position.X - this.dragStartPoint.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(position.Y - this.dragStartPoint.Value.Y))))
                {
                    
                    DataObject dataObject;
                    if (this.visualElement != null)
                        dataObject = new DataObject("VisualElement", this.visualElement);
                    
                    else if (this.visualBase!= null)
                        dataObject = new DataObject("VisualBase", this.visualBase);
                    
                    else
                        dataObject = null;


                    if (dataObject != null)
                    {
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                    }
                }

                e.Handled = true;
            }
        }

        #endregion
        
        /*#region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members*/
    }

}
