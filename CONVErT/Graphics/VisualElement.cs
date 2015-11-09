using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Data;
using System.Xml.Linq;
using System.Linq;

namespace CONVErT
{

    //    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    public class VisualElement : UserControl, INotifyPropertyChanged, ICloneable
    {
        #region properties

        //skin
        public XmlNode Trans { get; set; }
        public XmlNode ItemXML { get; set; }
        
        //the rest
        public static int RULEINDEX = 1;

        public string VEName { get; set; }

        public Point Position { get; set; }

        Point? elementDragStartPoint;

        VisualElementMouseAdorner visualMouseAdorner;
        VisualElementDragAdorner visualDragAdorner;

        Point? visualElementPosition;

        ListBox MyItemsHost;

        Point mouseClickPoint;

        public Popup elementListPopup = new Popup();

        XmlPrettyPrinter xmlPrinter = new XmlPrettyPrinter();

        private XmlNode _data;

        /// <summary>
        /// start portion of the code/Actual data
        /// Setting it will initiate the Abstract Tree as well as XSLT Template data
        /// </summary>
        public XmlNode Data
        {
            get { return _data; }
            set
            {
                this.templateVM.TemplateXmlNode = value;
                this._data = value.Clone();
                abstractTree.processXmlData(value);
            }
        }

        private XmlNode _dataR;

        /// <summary>
        /// start portion of the code/Actual reverse data
        /// Setting it will initiate the Reverse element Abstract Tree as well as XSLT Template data for reverse transformation
        /// </summary>
        public XmlNode ReverseData
        {
            get { return _dataR; }
            set
            {
                this.templateVMR.TemplateXmlNode = value;
                this._dataR = value.Clone();
                //abstractTreeReverse.processXmlData(value);
            }
        }

        public XSLTTemplate templateVM = new XSLTTemplate();

        public XSLTTemplate templateVMR = new XSLTTemplate();

        public AbstractLattice abstractTree;

        public List<Element> elementList { get; set; }
                
        /// <summary>
        /// used for source in visualFunctions as well
        /// </summary>
        public AbstractTreeLatticeNode sourceLatticeNodeToMatch; 

        XAMLRenderer rend = new XAMLRenderer();

        private ParentWindowFinder pwFinder = new ParentWindowFinder();
        
        public DependencyObject ParentWindow
        {
            get
            {
                return pwFinder.getParentWindow(this);
            }
        }

        /// <summary>
        /// Indicator of the view type, XAML/SVG/etc
        /// </summary>
        public VisualisationType viewType;

        /// <summary>
        /// for use in mapper suggestion accept/reject, will be assigned in Renderer
        /// </summary>
        public string VAddress;

        //public XmlNode representativeData { get; set; }
        //public bool showList { get; set; }
        //public bool isDragOver { get; set; }
        //public AbstractLattice abstractTreeReverse; //one directioanl suggestions for this time only.
        //public VisualElementType VType { get; set; }

        #endregion


        #region ctor

        static VisualElement()
        {

            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VisualElement), new FrameworkPropertyMetadata(typeof(VisualElement)));

        }

        public VisualElement()
        {
            //this.Loaded += new RoutedEventHandler(VisualELement_Loaded);

            //add event handler for droping elements on visual element
            this.Drop += new DragEventHandler(VisualElement_Drop);

            this.MouseDown += new MouseButtonEventHandler(VisualElement_MouseDown);

            this.MouseMove += new MouseEventHandler(VisualELement_MouseMove);

            this.MouseRightButtonDown += new MouseButtonEventHandler(VisualElement_RightClick);

            this.MouseEnter += new MouseEventHandler(VisualElement_MouseEnter);

            this.MouseLeave += new MouseEventHandler(VisualElement_MouseLeave);

            this.DragEnter += new DragEventHandler(VisualElement_DragEnter);

            this.DragLeave += new DragEventHandler(VisualElement_DragLeave);

            this.TouchDown += new EventHandler<TouchEventArgs>(VisualElement_TouchDrag);

            //this.MouseDoubleClick += new MouseButtonEventHandler(VisualElement_MouseDoubleClick);//suggestion representation

            elementList = new List<Element>();

            abstractTree = new AbstractLattice();
            //abstractTreeReverse = new AbstractLattice();

            visualMouseAdorner = new VisualElementMouseAdorner(this);
            visualDragAdorner = new VisualElementDragAdorner(this);

            MyItemsHost = new ListBox();

            this.Unloaded += VisualElement_Unloaded;
        }

        private void VisualElement_Unloaded(object sender, RoutedEventArgs e)
        {
            this.elementListPopup.IsOpen = false;
        }

        #endregion


        #region on apply template

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //MyItemsHost = (ListBox)this.Template.FindName("ElementsListBox", this);

            Style myStyle = new Style();

            //MyItemsHost.ItemContainerStyle.
            EventSetter esDrop = new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ItemElement_Drop));
            EventSetter esMouseDown = new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ItemElement_PreviewMouseDown));
            EventSetter esMouseMove = new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(ItemElement_MouseMove));
            EventSetter esTouchDrag = new EventSetter(ListBoxItem.TouchDownEvent, new EventHandler<TouchEventArgs>(ItemElement_TouchDrag));
            //EventSetter esDragOver = new EventSetter(ListBoxItem.DragOverEvent, new DragEventHandler(ItemElement_DragOver));

            myStyle.Setters.Add(esDrop);
            myStyle.Setters.Add(esMouseDown);
            myStyle.Setters.Add(esMouseMove);
            //myStyle.Setters.Add(esDragOver);

            MyItemsHost.ItemContainerStyle = myStyle;
            MyItemsHost.ItemTemplate = FindResource("ListBoxItemTEmplate") as DataTemplate;

            if (this.Data == null)
                loadDataFromXaml();


        }

        #endregion


        #region data part

        public void loadDataFromXaml()//this loader omits IsPlaceHolderElements
        {
            Object obj = this.Resources["sourceData"];

            if ((obj as XmlDataProvider) != null)
            {
                (obj as XmlDataProvider).IsAsynchronous = false;
                (obj as XmlDataProvider).IsInitialLoadEnabled = true;
                XmlNode xnode = (obj as XmlDataProvider).Document.DocumentElement.Clone();
                if (xnode != null)
                {
                    XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xnode.OuterXml));
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(xmlDocumentWithoutNs.ToString());
                    XmlNode x = xdoc.DocumentElement.Clone() as XmlNode;//not to mess with original one
                    x = removePlaceHolder(x);
                    //MessageBox.Show(x.OuterXml);
                    this.Data = x;
                }

            }
        }

        private XmlNode removePlaceHolder(XmlNode xnode)
        {
            XDocument xdoc = XDocument.Parse(xnode.OuterXml);
            foreach (var node in xdoc.Descendants().Where(e => e.Attribute("IsPlaceHolder") != null))
            {
                node.Attribute("IsPlaceHolder").Remove();
            }

            string result = xdoc.ToString();

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(result);
            return xd.DocumentElement;

            //XmlNodeList xnlNodes = xnode.OwnerDocument.GetElementsByTagName("*");
            //foreach (XmlElement el in xnlNodes)
            //{
            //    if (el.HasAttribute("IsPlaceHolder"))
            //    {
            //        el.RemoveAttribute("IsPlaceHolder");
            //    }
            //    if (el.HasChildNodes)
            //    {
            //        foreach (XmlNode child in el.ChildNodes)
            //        {
            //            if (child is XmlElement && (child as XmlElement).HasAttribute("IsPlaceHolder"))
            //            {
            //                (child as XmlElement).RemoveAttribute("IsPlaceHolder");
            //            }
            //        }
            //    }
            //}
        }

        public XmlNode loadPureDataFromXaml()//this data loader does not omit IsPlaceHolder Elements
        {
            Object obj = this.Resources["sourceData"];

            if ((obj as XmlDataProvider) != null)
            {
                (obj as XmlDataProvider).IsAsynchronous = false;
                (obj as XmlDataProvider).IsInitialLoadEnabled = true;
                XmlNode xnode = (obj as XmlDataProvider).Document.DocumentElement.Clone();
                if (xnode != null)
                {
                    XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xnode.OuterXml));
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(xmlDocumentWithoutNs.ToString());
                    return xdoc.DocumentElement.Clone() as XmlNode;//not to mess with 
                }
            }
            return null;
        }

        private XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        #endregion


        #region Touch

        /// <summary>
        /// Dragging visual elements by touch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualElement_TouchDrag(object sender, TouchEventArgs e)//to be tested on a touch device
        {
            DataObject dataObject = new DataObject("VisualElement", this);

            if (dataObject != null)
            {
                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
            }

            e.Handled = true;
            
        }

        /// <summary>
        /// Dargging item elements by touch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemElement_TouchDrag(object sender, TouchEventArgs e)//to be tested on a touch device
        {
            Element ele = (e.Source as ContentPresenter).Content as Element;

            DataObject dataObject = new DataObject("Element", ele);

            if (dataObject != null)
            {
                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
            }

            e.Handled = true;
        }

        #endregion //Touch
        

        #region drag and drop

        private void VisualElement_Drop(object sender, DragEventArgs e)
        {
            if (ParentWindow != null)//could locate parent form (visualiser or mapper for now)
            {

                if (e.Data.GetDataPresent("TreeNodeViewModel") && (ParentWindow is Visualiser))//for visualiser
                {
                    TreeNodeViewModel treeNode = e.Data.GetData("TreeNodeViewModel") as TreeNodeViewModel;

                    //process drop
                    processVisual_TreeNodeDrop(treeNode, treeNode.getFullAddress());

                    e.Handled = true;

                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                    myAdornerLayer.Remove(visualDragAdorner);


                }
                else if (e.Data.GetDataPresent("VisualElement") && (ParentWindow is Mapper))//for mapper
                {
                    VisualElement source = (e.Data.GetData("VisualElement") as VisualElement);

                    processVisual_VisualNotationDrop(source, ParentWindow);

                    e.Handled = true;

                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                    myAdornerLayer.Remove(visualDragAdorner);
                }
            }
        }

        public void processVisual_VisualNotationDrop(VisualElement source, DependencyObject pWindow)
        {
            if (pWindow is Mapper)//double check
            {
                (pWindow as Mapper).ReportStatusBar.ShowStatus("Mapping " + source.Data.Name + " -> " + this.Data.Name, ReportIcon.Info);

                XSLTTemplate tem = new XSLTTemplate();
                Element header = new Element();
                header.Name = source.Data.Name;

                tem.HeaderNode = new TreeNodeViewModel(header);
                tem.TemplateXmlNode = this.Data.Clone() as XmlNode;

                (pWindow as Mapper).NewTemplate = tem;
                (pWindow as Mapper).sourceData = source.Data;

                //for reverse
                this.ReverseData = source.Data.Clone() as XmlNode;
                this.templateVMR.TemplateName = this.Data.Name.Clone() as string;

                (pWindow as Mapper).NewTemplateR = this.templateVMR;
                (pWindow as Mapper).NewTemplateR.TemplateName = templateVMR.TemplateName.Clone() as string;


                //suggester initiation
                source.abstractTree.prepare();
                this.abstractTree.prepare();
                (pWindow as Mapper).mapperSuggester = new Suggester(source.abstractTree, this.abstractTree);

                (pWindow as Mapper).updateSuggestions((pWindow as Mapper).mapperSuggester.getSuggestionsAsStrings((pWindow as Mapper).mapperSuggester.imFeelingLucky()), false);

                //MessageBox.Show("Reverse Data: \n\n"+ReverseData.OuterXml);
                //maybe do this for source as well
                //make sure it does not overright the model to visualisation reverse code

                //move elements to Mapper rule designer
                (pWindow as CONVErT.Mapper).RuleDesignerCanvas.Children.Clear();

                (pWindow as CONVErT.Mapper).LHS = source.Clone() as VisualElement;
                (pWindow as CONVErT.Mapper).RHS = this.Clone() as VisualElement;

                (pWindow as CONVErT.Mapper).LHS.Content = (pWindow as Mapper).renderer.render((pWindow as CONVErT.Mapper).LHS);
                (pWindow as CONVErT.Mapper).RHS.Content = (pWindow as Mapper).renderer.render((pWindow as CONVErT.Mapper).RHS);

                Canvas.SetTop((pWindow as CONVErT.Mapper).LHS, 40);
                Canvas.SetLeft((pWindow as CONVErT.Mapper).LHS, 40);

                Canvas.SetTop((pWindow as CONVErT.Mapper).RHS, 40);
                Canvas.SetLeft((pWindow as CONVErT.Mapper).RHS, 470);

                (pWindow as CONVErT.Mapper).RuleDesignerCanvas.Children.Add((pWindow as CONVErT.Mapper).LHS);
                (pWindow as CONVErT.Mapper).RuleDesignerCanvas.Children.Add((pWindow as CONVErT.Mapper).RHS);

                //log event
                (pWindow as Mapper).logger.log("\"" + source.Data.Name + "\" was dropped on \"" + this.Data.Name + "\"", ReportIcon.OK);
                (pWindow as Mapper).RuleDesignStatusBar.ShowStatus("Rule : " + source.Data.Name + " -> " + this.Data.Name, ReportIcon.Info);
            }
        }

        public void processVisual_TreeNodeDrop(TreeNodeViewModel treeN, String FullAddress)
        {
            //set header for template XSLT
            templateVM.HeaderNode = treeN; // I do not think these will be used, they have been used in visualiser to check and set visualfunction abstraction

            //set address Id for template XSLT
            templateVM.TemplateAddress = FullAddress; // I do not think these will be used

            AbstractTreeLatticeNode matchingNode = (ParentWindow as Visualiser).sourceASTL.getAbstractNodeAtAddress(FullAddress);

            //define correspondence
            if (matchingNode != null)//if you have found the node
            {
                if (matchingNode.ruleIndex == -1)
                {
                    sourceLatticeNodeToMatch = matchingNode;
                }
                else if (matchingNode.parent != null)
                {
                    //create a duplicate****************************** duplicate not implemented yet
                    sourceLatticeNodeToMatch = matchingNode.duplicate();
                    matchingNode.parent.Children.Add(sourceLatticeNodeToMatch);
                }

                sourceLatticeNodeToMatch.isMatched = true;
                sourceLatticeNodeToMatch.ruleIndex = RULEINDEX;//assign an index for current rule

                //assign same rule index for this.abstractTree
                this.abstractTree.Root.ruleIndex = sourceLatticeNodeToMatch.ruleIndex;
                this.abstractTree.Root.isMatched = true;

                //increment index for other correspondences
                RULEINDEX++;

                //go for reverse, the top part will be obsolete
                //this.abstractTreeReverse = new AbstractLattice(matchingNode);//might not need abstract tree for reverse, as one way suggestion will eb shown
                this.ReverseData = matchingNode.ToXML();// treeNode.ToXML();//matchingNode might have worked as well but the signature problems might get in the way
                this.templateVMR.TemplateName = this.Data.Name;
            }

            //suggester initiation
            AbstractLattice sourceAST = new AbstractLattice(matchingNode);
            this.abstractTree.prepare();
            SuggesterConfig config = new SuggesterConfig();
            config.UseNameSimSuggester = true;
            config.UseTypeSimSuggester = true;
            config.UseIsoRankSimSuggester = true;
            config.UseValueSimSuggester = true;
            config.UseStructSimSuggester = true;
            (ParentWindow as Visualiser).visualiserSuggester = new Suggester(sourceAST, this.abstractTree, config);

            (ParentWindow as Visualiser).updateSuggestions((ParentWindow as Visualiser).visualiserSuggester.getSuggestionsAsStrings((ParentWindow as Visualiser).visualiserSuggester.imFeelingLucky()));

            //log event
            (ParentWindow as Visualiser).logger.log("\"" + FullAddress + "\" was dropped on \"" + this.VEName + "\"", ReportIcon.OK);
            (ParentWindow as Visualiser).ReportStatusBar.ShowStatus("Mapping " + treeN.Name + " to " + this.Data.Name, ReportIcon.Info);
        }

        private void VisualElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            visualElementPosition = new Point?(e.GetPosition(this));
        }

        private void VisualELement_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                visualElementPosition = null;
            }

            if (visualElementPosition.HasValue)
            {
                Point pos = e.GetPosition(this);

                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(pos.X - visualElementPosition.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(pos.Y - visualElementPosition.Value.Y))))
                {
                    DataObject dataObject = new DataObject("VisualElement", this);

                    if (dataObject != null)
                    {
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                    }
                }

                e.Handled = true;
            }
        }

        private void VisualElement_DragEnter(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent("VisualElement"))
            //    if ((e.Data.GetData("VisualElement") as VisualElement) != this)
            {
                //highlight element
                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (visualDragAdorner.Parent == null && myAdornerLayer != null)
                    myAdornerLayer.Add(visualDragAdorner);
                e.Handled = true;
            }
        }

        private void VisualElement_DragLeave(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent("VisualElement"))
            //   if ((e.Data.GetData("VisualElement") as VisualElement) != this)
            {
                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (visualDragAdorner != null && myAdornerLayer != null)
                    myAdornerLayer.Remove(visualDragAdorner);

                e.Handled = true;
            }
        }

        #endregion


        #region highlight

        private void VisualElement_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            //traverse logical tree to get to visualiser or mapper
            if (ownerForm == null)
            {
                ownerForm = LogicalTreeHelper.GetParent(this);
                while ((ownerForm != null) && (!ownerForm.GetType().ToString().Equals("CONVErT.Visualiser")) && (!ownerForm.GetType().ToString().Equals("CONVErT.Mapper")))
                {
                    ownerForm = LogicalTreeHelper.GetParent(ownerForm);
                }
            }

            if (ownerForm.GetType().ToString().Equals("CONVErT.Mapper"))//could locate parent form and is mapper
            {
                //find what is the source
                if ((ownerForm as Mapper).sourceASTL != null)
                {
                    string address = (ownerForm as Mapper).sourceASTL.Root.findInChildrenByName(this.Data.Name).Address;
                    if (!string.IsNullOrEmpty(address))
                    {
                        (ownerForm as Mapper).highlightSuggestedCorrespondeces(address);
                    }
                }
            }
            
            //highlight element
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (visualMouseAdorner.Parent == null && myAdornerLayer != null)
                myAdornerLayer.Add(visualMouseAdorner);

            e.Handled = true;*/
        }

        private void VisualElement_MouseLeave(object sender, MouseEventArgs e)
        {
            //highlight element
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (visualMouseAdorner != null && myAdornerLayer != null)
                myAdornerLayer.Remove(visualMouseAdorner);

            e.Handled = true;
        }


        #endregion


        #region Item element

        private void ItemElement_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TreeNodeViewModel"))//Visualiser
            {
                TreeNodeViewModel sourceNode = e.Data.GetData("TreeNodeViewModel") as TreeNodeViewModel;

                if (templateVM.HeaderNode != null && sourceLatticeNodeToMatch != null)
                {
                    //string source = "";
                    //if (sourceNode.Name.Equals(templateVM.HeaderNode.Name))
                    //    source = ".";
                    // else
                    string source = sourceNode.getRelativeAddress(templateVM.HeaderNode);

                    string target = ((sender as ListBoxItem).DataContext as Element).Address;//was t

                    //string target = t.Substring(t.IndexOf('/') + 1);

                    //MessageBox.Show("in element\n\nDragged\nSource: " + source + "\nTarget: " + target);
                    //MessageBox.Show("in element: "+ sourceNode.ToXML().OuterXml);
                    //============
                    processElement_TreeNodeDrop(source, target, sourceNode);
                    //============

                    //log event
                    (ParentWindow as Visualiser).logger.log("\"" + sourceNode.getRelativeAddress(templateVM.HeaderNode) + "\" was dropped on \"" + target + "\" attribute of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);

                }
                else
                    ShowStatus("Please set the header node first!", ReportIcon.Error);

                e.Handled = true;

                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                myAdornerLayer.Remove(visualDragAdorner);
            }

            else if ((e.Data.GetDataPresent("Element")) && (ParentWindow is Mapper))//Mapper
            {
                Element sourceElement = e.Data.GetData("Element") as Element;
                Element targetElement = (sender as ListBoxItem).DataContext as Element;
                //string rule = (sourceElement.Name + " : " + sourceElement.Address + " -> " + targetElement.Name + " : " + targetElement.Address);

                string targetMatch = targetElement.Address.Substring(targetElement.Address.IndexOf('/') + 1);

                string sourceMatch = sourceElement.Address.Substring(sourceElement.Address.IndexOf('/') + 1);

                if ((ParentWindow as Mapper).NewTemplate != null)
                    (ParentWindow as Mapper).NewTemplate.updateXmlNodeByExactValue(targetMatch, sourceMatch, targetElement.Value);

                //for reverse
                if ((ParentWindow as Mapper).NewTemplateR != null)
                    (ParentWindow as Mapper).NewTemplateR.updateXmlNodeByExactValue(sourceMatch, targetMatch, sourceElement.Value);

                //log event
                (ParentWindow as Mapper).logger.log("Element \"" + sourceElement.Address + "\" was dropped on Element \"" + targetElement.Address + "\".", ReportIcon.OK);

                e.Handled = true;

                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                myAdornerLayer.Remove(visualDragAdorner);

            }

            else if (e.Data.GetDataPresent("VisualElement"))//scheduling 
            {

                VisualElement v = e.Data.GetData("VisualElement") as VisualElement;

                string targetBase = ((sender as ListBoxItem).DataContext as Element).Address;
                //what to replace
                string target = targetBase.Substring(targetBase.IndexOf('/') + 1);

                bool replaceElement = false;
                string forwardTemplateCallName = "";

                //get metamodel of source model example to find template element restructure

                if (!target.ToLower().Equals("start"))//not start element of the scheduling
                {
                    //update abstractions
                    AbstractTreeLatticeNode reference = this.abstractTree.getAbstractNodeAtAddress(targetBase);
                    reference.Children.Add(v.abstractTree.Root);
                    //update metamodel(create a partial model example)
                    v.abstractTree.Root.parent = reference;

                    
                    if (ParentWindow != null)
                    {
                        if (ParentWindow is Visualiser)
                        {
                            try
                            {
                                string test = (ParentWindow as Visualiser).sourceASTL.RelativeAddressByName(this.templateVM.TemplateName, v.templateVM.TemplateName);
                                if (!String.IsNullOrEmpty(test))
                                    forwardTemplateCallName = test;

                                //show new abstraction rendered in sample popup - commented due to error 
                                //debugger
                                /*(this.ParentWindow as Visualiser).samplePopup.Child = rend.renderPartialVisualisation(this);
                                (this.ParentWindow as Visualiser).showSamplePopup(e.GetPosition(this.ParentWindow as Visualiser));*/
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Exception occured in ItemElement_Drop -> " + ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                        }
                        else if (ParentWindow is Mapper)//!! will not get trigered, mapper does not have start!
                        {
                            string test = (ParentWindow as Mapper).sourceASTL.RelativeAddressByName(this.templateVM.TemplateName, v.templateVM.TemplateName);
                            if (!String.IsNullOrEmpty(test))
                                forwardTemplateCallName = test;
                        }
                    }
                    else
                        forwardTemplateCallName = v.templateVM.TemplateName;

                    if (target.Equals(v.Data.Name))
                    {
                        ShowStatus("VisualElement-ItemElementDrop: setting replacement to true -> target = " + target + " v.Data.Name = " + v.Data.Name, ReportIcon.Warning);
                        replaceElement = true;
                    }
                    else
                        replaceElement = false;
                }
                else
                {
                    forwardTemplateCallName = v.templateVM.TemplateName;
                    replaceElement = false;
                }

                templateVM.updateXmlNodeByTempalte(target, forwardTemplateCallName, replaceElement);

                //log event
                (ParentWindow as Visualiser).logger.log("\"" + v.VEName + "\" is set to be placed in \"" + target + "\" of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);


                //******for reverse*******
                //************************
                if (!target.ToLower().Equals("start"))//not start element of the scheduling
                {
                    string reverseTemplateCallName = "";

                    string test = this.abstractTree.RelativeAddressByName(this.templateVMR.TemplateName, v.templateVMR.TemplateName);
                    if (!String.IsNullOrEmpty(test))
                        reverseTemplateCallName = test;

                    if (string.IsNullOrEmpty(reverseTemplateCallName))
                        reverseTemplateCallName = v.templateVMR.TemplateName;

                    if (v.templateVM.TemplateName.Equals(v.ReverseData.Name))
                        replaceElement = true;
                    else
                        replaceElement = false;

                    AbstractLattice tempReverseASTL = new AbstractLattice(this.ReverseData);
                    string toReplace = tempReverseASTL.RelativeAddressByName(this.templateVM.TemplateName, v.templateVM.TemplateName);

                    if (string.IsNullOrEmpty(toReplace))
                        toReplace = v.templateVM.TemplateName;

                    this.templateVMR.updateXmlNodeByTempalte(toReplace, reverseTemplateCallName, replaceElement);

                }
                else //we were at the start of scheduling
                {
                    //create start for reverse template
                    this.templateVMR.updateXmlNodeByTempalte("start", v.templateVMR.TemplateName, false);
                }


                //generate arrow
                if ((ParentWindow != null) && (ParentWindow is Visualiser))
                {
                    //create Arrow
                    ArrowLine aline1 = new ArrowLine();
                    aline1.Stroke = Brushes.Black;
                    aline1.StrokeThickness = 2;
                    aline1.IsArrowClosed = true;


                    //get visual element center point
                    Point vePos = v.TransformToVisual((ParentWindow as Visualiser).schedulingCanvas).Transform(new Point(0, 0));
                    if (v.ActualWidth > 180)
                        aline1.X1 = vePos.X + 180 / 2;
                    else
                        aline1.X1 = vePos.X + v.ActualWidth / 2;

                    if (v.ActualHeight > 180)
                        aline1.Y1 = vePos.Y + 180 / 2;
                    else
                        aline1.Y1 = vePos.Y + v.ActualHeight / 2;

                    //Point lePos = (sender as ListBoxItem).TransformToVisual((current as Visualiser).schedulingCanvas).Transform(new Point(0, 0));
                    aline1.X2 = e.GetPosition((ParentWindow as Visualiser).schedulingCanvas).X + (sender as ListBoxItem).ActualWidth / 2;
                    aline1.Y2 = e.GetPosition((ParentWindow as Visualiser).schedulingCanvas).Y;

                    Canvas.SetZIndex(aline1, 2);
                    (ParentWindow as Visualiser).schedulingCanvas.Children.Add(aline1);

                }

                e.Handled = true;

                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                myAdornerLayer.Remove(visualDragAdorner);
            }

            else if (e.Data.GetDataPresent("VisualFunctionElementDragObject"))//Visualiser functions
            {
                VisualFunctionElementDragObject eleDrag = e.Data.GetData("VisualFunctionElementDragObject") as VisualFunctionElementDragObject;

                if (ParentWindow is Visualiser)
                {
                    string t = ((sender as ListBoxItem).DataContext as Element).Address;
                    string target = t.Substring(t.IndexOf('/') + 1);//target is where the output goes

                    templateVM.updateXmlNodeByVariableName(target, "$" + eleDrag.element.Value);

                    //for reverse generation
                    eleDrag.function.updateOutput(eleDrag.element.Value, target);

                    //draw the line conecting both elements

                    //for reverse
                    //needs to first check if all args of the input have been matched before dragging the output

                    //get elements participating in function
                    XmlNode functionData = eleDrag.function.Data;
                    XmlNode forwardInputs = functionData.SelectSingleNode("inputs");

                    foreach (XmlNode x in forwardInputs.ChildNodes)
                    {
                        //look for output variable name
                        string outputString = "output" + x.Name.Substring(3);//x.Name is like arg1
                        templateVMR.updateXmlNodeByVariableName(x.InnerText, "$" + outputString);
                    }

                    //log event
                    (ParentWindow as Visualiser).logger.log("Function element \"" + eleDrag.element.Address + "\" was dropped on \"" + target + "\" of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);

                    e.Handled = true;
                }
                else if (ParentWindow is Mapper)
                {
                    string t = ((sender as ListBoxItem).DataContext as Element).Address;
                    string target = t.Substring(t.IndexOf('/') + 1);//target is where the output goes

                    (ParentWindow as Mapper).NewTemplate.updateXmlNodeByVariableName(target, "$" + eleDrag.element.Value);

                    //for reverse generation
                    eleDrag.function.updateOutput(eleDrag.element.Value, target);

                    //draw the line conecting both elements

                    //for reverse
                    //needs to first check if alll args of the input have been matched before dragging the output

                    //get elements participating in function
                    XmlNode functionData = eleDrag.function.Data;
                    XmlNode forwardInputs = functionData.SelectSingleNode("inputs");

                    foreach (XmlNode x in forwardInputs.ChildNodes)
                    {
                        //look for output variable name
                        string outputString = "output" + x.Name.Substring(3);//x.Name is like arg1

                        //if X is constant value, then it has come from nowhere! (from th pannel of constant values)
                        //therefore it will not be considered for puttin back to its place becouse it is considered
                        //as lossy data -> for futur processing
                        //so:
                        double Num;
                        bool isNum = double.TryParse(x.InnerText, out Num);

                        if (!(isNum) && !(x.InnerText.StartsWith("'"))) //is not a constant number or string
                            (ParentWindow as Mapper).NewTemplateR.updateXmlNodeByVariableName(x.InnerText, "$" + outputString);
                    }

                    //log event
                    (ParentWindow as Mapper).logger.log("Function element \"" + eleDrag.element.Address + "\" was dropped on \"" + target + "\" of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);

                    e.Handled = true;
                }

                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                myAdornerLayer.Remove(visualDragAdorner);
            }
            else if (e.Data.GetDataPresent("VisualCondition"))//Visualiser Conditions
            {
                VisualCondition cond = (e.Data.GetData("VisualCondition") as VisualCondition);

                string t = ((sender as ListBoxItem).DataContext as Element).Address;

                string target = t.Substring(t.IndexOf('/') + 1);//target is where the output goes

                if (ParentWindow is Visualiser)
                {
                    templateVM.updateNodeByCondition(target, cond.getConditionCode());

                    //log event
                    (ParentWindow as Visualiser).logger.log("Condition \"" + cond.Name + "\" was dropped on \"" + target + "\" of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);
                }
                else if (ParentWindow is Mapper)
                {
                    (ParentWindow as Mapper).NewTemplate.updateNodeByCondition(target, cond.getConditionCode());
                    //log event
                    (ParentWindow as Mapper).logger.log("Condition \"" + cond.Name + "\" was dropped on \"" + target + "\" of Visual Element \"" + this.VEName + "\".", ReportIcon.OK);
                }

                //no reverse for conditions yet

                e.Handled = true;

                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                myAdornerLayer.Remove(visualDragAdorner);
            }
            else
                ShowStatus("Unrecognised drag element -> " + e.Data.GetType().ToString(), ReportIcon.Error);

        }

        public bool processElement_TreeNodeDrop(string sourceElement, string targetElement, TreeNodeViewModel sourceNode)
        {
            if (templateVM.HeaderNode != null && sourceLatticeNodeToMatch != null)
            {
                string source = sourceElement;
                if (sourceNode.Name.Equals(templateVM.HeaderNode.Name))//attribute
                    source = ".";
                //else
                //source = sourceNode.getRelativeAddress(templateVM.HeaderNode);

                string target = targetElement.Substring(targetElement.IndexOf('/') + 1);

                templateVM.updateXmlNodeByValue(target, sourceElement);

                //for reverse
                string rsource = sourceNode.getRelativeAddress(templateVM.HeaderNode); //this might give wrong ones, but will not be a problem

                if (rsource.IndexOf("@") == 0) //is an attribute, so send it with its parent get its parent name behind it
                    rsource = sourceNode.Parent.Name + "/" + rsource;//if @ index is more than 0, the parent has already been included

                //MessageBox.Show(rsource + "\n\n" + target);
                templateVMR.updateXmlNodeByValue(rsource, target);

                //MessageBox.Show(templateVM.TemplateXmlNode.OuterXml + "\n\n" + templateVMR.TemplateXmlNode.OuterXml);

                //Abstraction mechanism and rule indexing (no use)
                AbstractTreeLatticeNode toMatch = sourceLatticeNodeToMatch.findChildrenByAddress(sourceNode.getRelativeAddress(templateVM.HeaderNode));
                AbstractTreeLatticeNode targetMatch = abstractTree.getAbstractNodeAtAddress(targetElement);

                if (toMatch != null && targetMatch != null)
                {
                    //assign an index for source rule correspondence
                    toMatch.isMatched = true;
                    toMatch.ruleIndex = RULEINDEX;

                    //assign same rule index for this.abstractTree
                    targetMatch.isMatched = true;
                    targetMatch.ruleIndex = RULEINDEX;

                    //increment index for other correspondences
                    RULEINDEX++;

                }

                return true;
            }
            else
            {
                ShowStatus("Please set the top element correspondence first!", ReportIcon.Error);
                return false;
            }
        }

        private void ItemElement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            elementDragStartPoint = new Point?(e.GetPosition(this));
        }

        private void ItemElement_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                elementDragStartPoint = null;
            }

            if (elementDragStartPoint.HasValue)
            {
                Point pos = e.GetPosition(this);

                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(pos.X - elementDragStartPoint.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(pos.Y - elementDragStartPoint.Value.Y))))
                {
                    if ((e.Source as ContentPresenter) != null)
                    {
                        Element ele = (e.Source as ContentPresenter).Content as Element;

                        DataObject dataObject = new DataObject("Element", ele);

                        if (dataObject != null)
                        {
                            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion


        #region DClick & RClick

        public void VisualElement_RightClick(object sender, MouseButtonEventArgs e)
        {

            //createRunPopup();
            mouseClickPoint = e.GetPosition(this);

            elementListPopup.Placement = PlacementMode.Custom;
            elementListPopup.PlacementTarget = this;
            elementListPopup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(placePopup);

            if (elementListPopup.IsOpen == true)
            {
                elementListPopup.IsOpen = false;

            }
            else
            {
                elementList.Clear();
                MyItemsHost.Items.Clear();
                XmlNode xData = null;

                if (this.Data != null)
                {
                    if (this.Data.Name.Equals("Start")) //start element (in composition)
                        xData = this.Data.Clone();
                }
                
                if (xData == null)
                    xData = loadPureDataFromXaml();


                if (xData != null)
                {
                    getElementListFromNotation(xData, false);

                    elementListPopup.Child = MyItemsHost;
                    elementListPopup.Visibility = Visibility.Visible;
                    elementListPopup.IsOpen = true;

                    //MessageBox.Show(this.Data.OuterXml);

                }

            }

            e.Handled = true;

        }

        public CustomPopupPlacement[] placePopup(Size popupSize,
                                           Size targetSize,
                                           Point offset)
        {
            CustomPopupPlacement placement1 =
               new CustomPopupPlacement(mouseClickPoint, PopupPrimaryAxis.Vertical);

            CustomPopupPlacement placement2 =
                new CustomPopupPlacement(new Point(2, 2), PopupPrimaryAxis.Horizontal);

            CustomPopupPlacement[] ttplaces =
                    new CustomPopupPlacement[] { placement1, placement2 };
            return ttplaces;
        }

        //public void createRunPopup()
        //{
        //    //clear previous popUp
        //    if (showList == true)
        //    {
        //        showList = false;
        //        OnPropertyChanged("showList");
        //    }
        //    else
        //    {
        //        elementList.Clear();

        //        if (Data == null)
        //        {
        //            loadDataFromXaml();
        //        }
        //        //else
        //        //{
        //        getElementListFromNotation(Data);
        //        //}

        //        OnPropertyChanged("elementList");
        //        showList = true;
        //        OnPropertyChanged("showList");

        //    }
        //}

        private void getElementListFromNotation(XmlNode notation, bool isPlaceHolder)
        {
            if ((!notation.HasChildNodes) && (!String.IsNullOrEmpty(notation.Value)))
            {

                Element e = new Element();
                e.Name = notation.ParentNode.Name;
                e.Value = notation.InnerText;
                e.IsPlaceHolder = isPlaceHolder;
                //e.Value = notation.ParentNode.Name;
                //e.Name = notation.InnerText;

                //e.Address = getRelativeAddress(Data,notation.Name);
                e.Address = getFullAddress(notation);
                //elementList.Add(e);
                MyItemsHost.Items.Add(e);

            }
            else
                if (!notation.Name.Equals("IsPlaceHolder"))//if notation is placeholder attribute, ignore it
                {
                    XmlNode node = notation.Attributes["IsPlaceHolder"]; //if xml element has isplaceholder attribute
                    bool checkPlaceHolder = false;

                    if (node != null)
                        checkPlaceHolder = true;

                    foreach (XmlNode x in notation.ChildNodes)
                        getElementListFromNotation(x, checkPlaceHolder);
                }
        }

        /*private void VisualElement_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //find the matching source node to make the rule
            if ((ParentWindow != null) && (ParentWindow is Mapper))
            {
                //suggester initiation
                //MessageBox.Show(this.);
                if ((ParentWindow as Mapper).suggestions.Count > 0)
                {
                    this.abstractTree.prepare();
                    (ParentWindow as Mapper).mapperSuggester = new Suggester((ParentWindow as Mapper).targetASTL, this.abstractTree);

                    (ParentWindow as Mapper).updateSuggestions((ParentWindow as Mapper).mapperSuggester.getSuggestionsAsStrings((ParentWindow as Mapper).mapperSuggester.imFeelingLucky()),true);
                }
            }
        }*/

        #endregion


        #region addressings

        private string getRelativeAddress(XmlNode x, string s)
        {

            string address = "";

            if (x.Name.Equals(s))//are the same
                address = ".";
            else
            {
                XmlNode temp;

                if (!x.HasChildNodes && x.ParentNode != null && !x.ParentNode.Name.Equals(s))
                {
                    temp = x.ParentNode;
                }
                else
                {
                    temp = x;
                }

                address = temp.Name;
                temp = temp.ParentNode;

                while (temp != null && !temp.Name.Equals(s))
                {
                    address = temp.Name + "/" + address;
                    temp = temp.ParentNode;
                }
            }

            return address;
        }

        private string getFullAddress(XmlNode x)
        {
            string s = "";
            XmlNode temp;

            if ((!x.HasChildNodes) && (x.ParentNode != null))
                temp = x.ParentNode;
            else
                temp = x;

            while (temp != null)
            {
                if (temp.Name.Equals("#document"))
                    temp = null;
                else
                {
                    if (s.Equals(""))
                        s = s + temp.Name;
                    else
                        s = temp.Name + "/" + s;

                    temp = temp.ParentNode;
                }
            }

            return s;
        }

        #endregion //addressings


        #region IsSelected Property

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected",
                                       typeof(bool),
                                       typeof(VisualElement),
                                       new FrameworkPropertyMetadata(false));

        #endregion


        #region move Thumb template

        //public static readonly DependencyProperty MoveThumbTemplateProperty =
        //    DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(VisualElement));

        //public static ControlTemplate GetMoveThumbTemplate(UIElement element)
        //{
        //    return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
        //}

        //public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
        //{
        //    element.SetValue(MoveThumbTemplateProperty, value);
        //}

        #endregion


        #region ConnectorDecoratorTemplate Property

        // can be used to replace the default template for the ConnectorDecorator
        //public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
        //    DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(VisualElement));

        //public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        //{
        //    return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
        //}

        //public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        //{
        //    element.SetValue(ConnectorDecoratorTemplateProperty, value);
        //}

        #endregion


        #region DragThumbTemplate Property

        // can be used to replace the default template for the DragThumb
        //public static readonly DependencyProperty DragThumbTemplateProperty =
        //    DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(VisualElement));

        //public static ControlTemplate GetDragThumbTemplate(UIElement element)
        //{
        //    return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        //}

        //public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        //{
        //    element.SetValue(DragThumbTemplateProperty, value);
        //}

        #endregion


        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        //public bool IsDragConnectionOver
        //{
        //    get { return (bool)GetValue(IsDragConnectionOverProperty); }
        //    set { SetValue(IsDragConnectionOverProperty, value); }
        //}
        //public static readonly DependencyProperty IsDragConnectionOverProperty =
        //    DependencyProperty.Register("IsDragConnectionOver",
        //                                 typeof(bool),
        //                                 typeof(VisualElement),
        //                                 new FrameworkPropertyMetadata(false));

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members


        #region clone

        public object Clone()
        {
            VisualElement v = new VisualElement();

            if (this.Data != null)
                v.Data = (this.Data.Clone()) as XmlNode;

            if (this.Trans != null)
                v.Trans = (this.Trans.Clone()) as XmlNode;

            if (this.ItemXML != null)
                v.ItemXML = (this.ItemXML.Clone()) as XmlNode;

            if (!String.IsNullOrEmpty(this.VEName))
                v.VEName = (this.VEName.Clone()) as string;

            if (this.ReverseData != null)
                v.ReverseData = (this.ReverseData.Clone()) as XmlNode;

            if (!String.IsNullOrEmpty(this.templateVM.TemplateName))
                v.templateVM.TemplateName = this.templateVM.TemplateName.Clone() as string;

            if (this.templateVM.TemplateXmlNode != null)
                v.templateVM.TemplateXmlNode = this.templateVM.TemplateXmlNode.Clone();

            if (!String.IsNullOrEmpty(this.templateVMR.TemplateName))
                v.templateVMR.TemplateName = this.templateVMR.TemplateName.Clone() as string;

            if (this.templateVMR.TemplateXmlNode != null)
                v.templateVMR.TemplateXmlNode = this.templateVMR.TemplateXmlNode.Clone();

            if (this.VAddress != null)
                v.VAddress = this.VAddress.Clone() as string;

            if (this.viewType != null)
                v.viewType = this.viewType;

            return v;

        }

        #endregion


        #region utils

        private void ShowStatus(string p, ReportIcon reportIcon)
        {
            if (this.ParentWindow != null)
            {
                if (this.ParentWindow is Mapper)
                    (ParentWindow as Mapper).ReportStatusBar.ShowStatus(p, reportIcon);
                if (this.ParentWindow is Visualiser)
                    (ParentWindow as Visualiser).ReportStatusBar.ShowStatus(p, reportIcon);
            }
        }

        #endregion //utils


        #region comments
        //void VisualELement_Loaded(object sender, RoutedEventArgs e)
        //{
        // if DragThumbTemplate and ConnectorDecoratorTemplate properties of this class
        // are set these templates are applied; 
        // Note: this method is only executed when the Loaded event is fired, so
        // setting DragThumbTemplate or ConnectorDecoratorTemplate properties after
        // will have no effect.
        //if (base.Template != null)
        //{
        //ContentPresenter contentPresenter =
        //    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
        //if (contentPresenter != null)
        //{
        //    UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
        //    if (contentVisual != null)
        //    {
        //DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
        //Control connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;

        //if (thumb != null)
        //{
        //    ControlTemplate template = VisualElement.GetDragThumbTemplate(contentVisual) as ControlTemplate;
        //    if (template != null)
        //        thumb.Template = template;
        //}


        //if (connectorDecorator != null)
        //{
        //    ControlTemplate template = VisualElement.GetConnectorDecoratorTemplate(contentVisual) as ControlTemplate;
        //    if (template != null)
        //        connectorDecorator.Template = template;
        //}
        //    }
        //}
        //}
        //}




        //protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseUp(e);
        //    Canvas designer = VisualTreeHelper.GetParent(this) as Canvas;

        //    // update selection
        //    if (designer != null)
        //        if (this.IsSelected)
        //        {
        //            this.IsSelected = false;

        //        }

        //    e.Handled = false;
        //}
        #endregion //comments
    }

}


