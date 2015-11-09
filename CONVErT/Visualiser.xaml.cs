using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Xsl;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    /// <summary>
    /// Interaction logic for Visualiser.xaml
    /// </summary>
    public partial class Visualiser : ContentControl, INotifyPropertyChanged
    {
        #region properties

        //TreeViewViewModel tvvm;
        //System.Windows.Point modelTreeViewMousePos;
        ItemsViewModel itemsVM;

        Toolbox myToolbox;
        Toolbox customToolbox;
        Toolbox functionsToolBox;

        XmlDocument dom;//for custom toolbox items
        XmlDocument fdom;//for functions toolbox items
        VisualElement visEl = new VisualElement();

        XSLTTemplateRepository templateRepo = new XSLTTemplateRepository();
        XSLTTemplateRepository templateRepoR = new XSLTTemplateRepository();

        XAMLRenderer xrenderer = new XAMLRenderer();
        SVGRenderer srenderer = new SVGRenderer();

        public string modelFile
        {
            get
            {
                if (ModelTab.HasItems)
                {
                    return (ModelTab.SelectedItem as CloseableTabItem).ModelFile;
                }
                else
                    return null;
            }
        }

        string xslFile = "";
        string xslFileR = "";

        string modelVisualFile = "";
        string modelFileRegenerated = "";

        public AbstractLattice sourceASTL
        {
            get
            {

                if (ModelTab.HasItems)
                {
                    return (ModelTab.SelectedItem as CloseableTabItem).abstractLattice;
                }
                else
                    return null;
            }
        }


        public Suggester visualiserSuggester;

        public ObservableCollection<Suggestion> suggestions { get; set; }

        XmlPrettyPrinter prettyPrinter;

        private bool _showSample;
        public bool ShowSample
        {
            get
            {
                return _showSample;
            }
            set
            {
                _showSample = value;
                OnPropertyChanged("ShowSample");
            }
        }

        public Logger logger;

        #endregion


        #region ctor

        public Visualiser()
        {
            InitializeComponent();

            //sourceFile = "../../Resources/salestable.xml";
            //tvvm = new TreeViewViewModel(sourceFile);
            //ModelTreeView.DataContext = tvvm;
            myToolbox = new Toolbox();
            //string elementsFile = DirectoryHelper.getFilePath("Resources\\ToolBoxItems.xml");
            //loadToolboxes(elementsFile);

            suggestions = new ObservableCollection<Suggestion>();
            suggestionsListBox.ItemsSource = suggestions;


            //initiate the scheduling canvas
            initiateSchedulingCanvas();

            prettyPrinter = new XmlPrettyPrinter();

            ShowSample = false;//set popup for sample to false

            popupTimer = new System.Windows.Threading.DispatcherTimer();
            popupTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            popupTimer.Interval = new TimeSpan(0, 0, 2);

            logger = new Logger("VisualiserLogger");
            logsTab.Content = logger;

        }

        public void loadToolBoxItem(string toolboxfile)
        {

            itemsVM = new ItemsViewModel(DirectoryHelper.getFilePathExecutingAssembly(toolboxfile));


            foreach (ToolboxItem i in itemsVM.items)
                myToolbox.Items.Add(i);

            ItemsExpander.Content = myToolbox;
        }

        public void loadToolboxes(string toolboxfile)
        {

            loadToolBoxItem(toolboxfile);

            //where customise items will be
            customToolbox = new Toolbox();

            //load customtoolbox Items
            //string customElementsFile = "../../Resources/CustomToolBoxItems.xml";
            string customElementsFile = DirectoryHelper.getFilePathExecutingAssembly("Resources\\CustomToolBoxItems.xml");
            loadCutomToolbox(customElementsFile, customToolbox);

            CustomVisualisationExpander.Content = customToolbox;
            CustomVisualisationExpander.IsExpanded = false;


            //load functions toolbox
            functionsToolBox = new Toolbox();
            //string functionsFile = "../../Resources/Functions.xml";
            string functionsFile = DirectoryHelper.getFilePathExecutingAssembly("Resources\\Functions.xml");
            loadFunctionsToolbox(functionsFile, functionsToolBox);

            FunctionsExpander.Content = functionsToolBox;
            FunctionsExpander.IsExpanded = false;
        }

        #endregion


        #region popup Timer

        public System.Windows.Threading.DispatcherTimer popupTimer;


        public void showSamplePopup(Point x)
        {
            samplePopup.HorizontalOffset = x.X + 100;
            samplePopup.VerticalOffset = x.Y + 100;
            //this.ShowSample = true;
            samplePopup.IsOpen = true;
            popupTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.samplePopup.IsOpen = false;
            this.ShowSample = false;
            popupTimer.Stop();

        }

        #endregion


        #region Functions toolbox

        private void loadFunctionsToolbox(string functionsFile, Toolbox fToolbox)
        {
            //something should be done for templte repository
            if (System.IO.File.Exists(functionsFile))
            {
                try
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(functionsFile, readerSettings))
                    {
                        // SECTION 1. Create a DOM Document and load the XML data into it.
                        fdom = new XmlDocument();
                        fdom.Load(reader);

                        // SECTION 2. Initialize Elements
                        XmlNodeList arfunctions = fdom.SelectNodes("Functions/Arithmetic/Function");
                        XmlNodeList reffunctions = fdom.SelectNodes("Functions/Reference/Condition");
                        //MessageBox.Show(customNodes.Count.ToString());
                        // SECTION 3. Populate Items with the DOM nodes.
                        foreach (XmlNode func in arfunctions)
                        {
                            VisualFunction f = new VisualFunction(func);
                            ToolboxItem item = new ToolboxItem(f);
                            //item.Content = f;
                            //item.loadFunctionToolBoxItem(func);
                            fToolbox.Items.Add(item);
                        }

                        foreach (XmlNode func in reffunctions)
                        {
                            VisualCondition f = new VisualCondition(func);
                            ToolboxItem item = new ToolboxItem(f);
                            //item.Content = f;
                            //item.loadFunctionToolBoxItem(func);
                            fToolbox.Items.Add(item);
                        }

                        reader.Close();
                    }

                }
                catch (XmlException xmlEx)
                {
                    reportMessage(xmlEx.Message, ReportIcon.Error);
                }
                catch (Exception ex)
                {
                    reportMessage(ex.Message, ReportIcon.Error);
                }
            }
            else
                reportMessage("Could not load Function ToolboxItems file : " + functionsFile, ReportIcon.Error);
        }

        #endregion


        #region CustomToolBox Load and Save

        private void loadCutomToolbox(string customElementsFile, Toolbox customToolbox)
        {
            //something should be done for templte repository
            if (System.IO.File.Exists(customElementsFile))
            {
                try
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.IgnoreComments = true;
                    using (XmlReader reader = XmlReader.Create(customElementsFile, readerSettings))
                    {
                        // SECTION 1. Create a DOM Document and load the XML data into it.
                        dom = new XmlDocument();
                        dom.Load(reader);

                        // SECTION 2. Initialize Elements
                        XmlNodeList customNodes = dom.SelectNodes("CustomItems/CustomItem");
                        //MessageBox.Show(customNodes.Count.ToString());
                        // SECTION 3. Populate Items with the DOM nodes.
                        foreach (XmlNode xnode in customNodes)
                        {
                            ToolboxItem item = new ToolboxItem();
                            item.loadCustomisedToolBoxItem(xnode);
                            customToolbox.Items.Add(item);
                        }

                        reader.Close();
                    }

                }
                catch (XmlException xmlEx)
                {
                    System.Windows.MessageBox.Show(xmlEx.Message);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
                reportMessage("Could not load Custom ToolboxItems file : " + customElementsFile, ReportIcon.Error);
        }

        /// <summary>
        /// Save custom Toolbox item to file for future use
        /// </summary>
        /// <param name="item">custom item to be saved</param>
        private void saveCustomTooboxItem(ToolboxItem item)
        {
            XmlNode customItems = dom.SelectSingleNode("CustomItems");

            if (customItems == null)
            {
                customItems = dom.CreateElement("CustomItems") as XmlNode;
                dom.AppendChild(customItems);
            }

            XmlNode itemToBeAdded = item.saveToolBoxItemToXml();

            if (itemToBeAdded != null)
            {
                customItems.AppendChild(customItems.OwnerDocument.ImportNode(itemToBeAdded, true));

                string customElementsFile = getDirectory("Resources\\CustomToolBoxItems.xml");
                customElementsFile = (customElementsFile.Replace("file:\\", ""));
                dom.Save(customElementsFile);
            }
            else
                MessageBox.Show("Custom Item to be added came back Null!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private string getDirectory(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            return System.IO.Path.Combine(pbase, c);
        }

        #endregion //CustomToolbox Load and Save


        #region designer cavas interaction

        private void VisElementCanvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!string.IsNullOrEmpty(modelFile) && sourceASTL != null) //sourceASTL was tvvm (before 15/08/2012)
            {
                System.Windows.Point pos = e.GetPosition(VisElementCanvas);

                /*if (e.Data.GetDataPresent("ToolboxItem"))
                {
                    //clear canvas children
                    VisElementCanvas.Children.Clear();

                    //get toolbox item
                    ToolboxItem t = e.Data.GetData("ToolboxItem") as ToolboxItem;

                    visEl = new VisualElement();
                    visEl = (t.visualElement.Clone()) as VisualElement;//must be call by value

                    visEl.Position = pos;

                    Canvas.SetLeft(visEl, Math.Max(0, pos.X - 100 / 2));
                    Canvas.SetTop(visEl, Math.Max(0, pos.Y - 100 / 2));
                    visEl.Content = t.visualElement.Content;

                    VisElementCanvas.Children.Add(visEl);

                    //suggestions
                    visEl.abstractTree.prepare();
                    this.sourceASTL.prepare();

                    SuggesterConfig config = new SuggesterConfig();
                    config.UseNameSimSuggester = true;
                    config.UseTypeSimSuggester = true;
                    config.UseIsoRankSimSuggester = false;
                    config.UseValueSimSuggester = true;
                    config.UseStructSimSuggester = false;
                    this.visualiserSuggester = new Suggester(sourceASTL, visEl.abstractTree, config);
                    this.updateSuggestions(this.visualiserSuggester.getSuggestionsAsStrings(this.visualiserSuggester.imFeelingLucky()));

                }
                else*/
                if (e.Data.GetDataPresent("VisualElement"))
                {
                    //clear canvas children
                    VisElementCanvas.Children.Clear();

                    VisualElement temp = new VisualElement();
                    temp = (e.Data.GetData("VisualElement") as VisualElement).Clone() as VisualElement;

                    visEl = xrenderer.render(temp.ItemXML) as VisualElement; //render visual elements, no need for return valu, the notation contents will be added to same visual element 6/11/2012
                    visEl.templateVM.TemplateXmlNode = temp.templateVM.TemplateXmlNode.Clone() as XmlNode;
                    // visEl.templateVMR.TemplateXmlNode = temp.templateVMR.TemplateXmlNode.Clone() as XmlNode;
                    visEl.templateVM.TemplateName = temp.templateVM.TemplateName;
                    visEl.templateVMR.TemplateName = temp.templateVMR.TemplateName;
                    visEl.Data = temp.Data.Clone() as XmlNode;
                    //visEl.ReverseData = temp.ReverseData.Clone() as XmlNode;

                    visEl.Position = pos;

                    //prepare view box for large items
                    Viewbox vb = new Viewbox();
                    vb.MaxHeight = 180;
                    vb.MaxWidth = 180;
                    vb.StretchDirection = StretchDirection.DownOnly;
                    vb.Stretch = System.Windows.Media.Stretch.Fill;
                    vb.Child = visEl;

                    Canvas.SetLeft(vb, Math.Max(0, pos.X - 100 / 2));
                    Canvas.SetTop(vb, Math.Max(0, pos.Y - 100 / 2));

                    if (visEl != null)
                    {
                        VisElementCanvas.Children.Add(vb);

                        //suggestions
                        visEl.abstractTree.prepare();
                        this.sourceASTL.prepare();

                        SuggesterConfig config = new SuggesterConfig();
                        config.UseNameSimSuggester = true;
                        config.UseTypeSimSuggester = true;
                        config.UseIsoRankSimSuggester = false;
                        config.UseValueSimSuggester = true;
                        config.UseStructSimSuggester = false;

                        this.visualiserSuggester = new Suggester(sourceASTL, visEl.abstractTree, config);
                        this.updateSuggestions(this.visualiserSuggester.getSuggestionsAsStrings(this.visualiserSuggester.imFeelingLucky()));

                        //log event
                        logger.log("Visual Element \"" + visEl.VEName + "\" droppped on Visualisation canvas.");

                    }
                    else
                    {
                        reportMessage("Unable to render visual", ReportIcon.Error);
                    }

                }
                else if (e.Data.GetDataPresent("VisualFunction"))
                {
                    VisualFunction vf = e.Data.GetData("VisualFunction") as VisualFunction;

                    if (VisElementCanvas.Children.Count > 0)//if there exists visual element, match functions header node with the element header node
                    {
                        foreach (UIElement u in VisElementCanvas.Children)//assign same header node to the function as well
                            if (u is Viewbox)
                            {
                                if (((u as Viewbox).Child as VisualElement).templateVM.HeaderNode != null)//header node has been set
                                {
                                    //set visualfunction's target metamodel as this visual element 
                                    vf.targetASTL = new AbstractLattice(((u as Viewbox).Child as VisualElement).abstractTree.Root.duplicate());
                                    //set visualfunction's source abstraction from source model
                                    vf.sourceASTL = new AbstractLattice(this.sourceASTL.getAbstractNodeAtAddress(((u as Viewbox).Child as VisualElement).templateVM.TemplateAddress).duplicate());

                                    vf.sourceRootNode = ((u as Viewbox).Child as VisualElement).templateVM.HeaderNode;

                                    Canvas.SetLeft(vf, Math.Max(0, pos.X - 100 / 2));
                                    Canvas.SetTop(vf, Math.Max(0, pos.Y - 100 / 2));

                                    VisElementCanvas.Children.Add(vf);

                                    //log event
                                    logger.log("Visual Finction droppped on Visualisation canvas.", ReportIcon.Info);

                                    break;
                                }
                                else
                                {
                                    reportMessage("Set header node for visual Element first", ReportIcon.Error);
                                }
                            }
                    }
                    else

                        reportMessage("No visual element has been set yet", ReportIcon.Error);
                }
                else if (e.Data.GetDataPresent("VisualCondition"))
                {
                    VisualCondition vc = e.Data.GetData("VisualCondition") as VisualCondition;

                    if (VisElementCanvas.Children.Count > 0)//if there exists visual element, match functions header node with the element header node
                    {
                        foreach (UIElement u in VisElementCanvas.Children)//assign same header node to the condition as well
                            if (u is Viewbox)
                            {
                                if (((u as Viewbox).Child as VisualElement).templateVM.HeaderNode != null)//header node has been set
                                {
                                    //set visualfunction's target metamodel as this visual element 
                                    vc.targetASTL = new AbstractLattice(((u as Viewbox).Child as VisualElement).abstractTree.Root.duplicate());
                                    //set visualfunction's source abstraction from source model
                                    vc.sourceASTL = new AbstractLattice(this.sourceASTL.getAbstractNodeAtAddress(((u as Viewbox).Child as VisualElement).templateVM.TemplateAddress).duplicate());

                                    vc.sourceRootNode = ((u as Viewbox).Child as VisualElement).templateVM.HeaderNode;

                                    Canvas.SetLeft(vc, Math.Max(0, pos.X - 100 / 2));
                                    Canvas.SetTop(vc, Math.Max(0, pos.Y - 100 / 2));

                                    VisElementCanvas.Children.Add(vc);

                                    //log event
                                    logger.log("Visual Condition droppped on Visualisation canvas.", ReportIcon.Info);

                                    break;
                                }
                                else
                                {
                                    reportMessage("Set header node for visual Element first", ReportIcon.Error);
                                }
                            }
                    }
                    else
                        reportMessage("No visual element has been set yet", ReportIcon.Error);
                }

            }
            else
                reportMessage("Please open input model first!", ReportIcon.Error);
        }

        //to be used for suggestion highlighting
        public void highlightVisualElements(Collection<string> elementAddresses)
        {

        }

        #endregion


        #region Status Bar

        public void reportMessage(string msg, ReportIcon ri)
        {
            ReportStatusBar.ShowStatus(msg, ri);
        }

        public void clearReportMessage()
        {
            ReportStatusBar.clearReportMessage();
        }

        #endregion //status bar


        #region Scheduling canvas interaction

        private void SchedulingCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("VisualElement"))
            {

                VisualElement temp = new VisualElement();
                temp = (e.Data.GetData("VisualElement") as VisualElement).Clone() as VisualElement;

                visEl = new VisualElement();
                visEl = xrenderer.render(temp.ItemXML) as VisualElement;
                //visEl.ReverseData = temp.ReverseData.Clone();
                visEl.Data = temp.Data.Clone() as XmlNode;
                visEl.ReverseData = temp.ReverseData.Clone() as XmlNode;
                visEl.templateVM.TemplateName = temp.templateVM.TemplateName;
                visEl.templateVMR.TemplateName = temp.templateVMR.TemplateName;
                visEl.templateVM.TemplateXmlNode = temp.templateVM.TemplateXmlNode.Clone() as XmlNode;
                visEl.templateVMR.TemplateXmlNode = temp.templateVMR.TemplateXmlNode.Clone() as XmlNode;


                System.Windows.Point pos = e.GetPosition(schedulingCanvas);
                visEl.Position = pos;

                //prepare view box for large items
                Viewbox vb = new Viewbox();
                vb.MaxHeight = 180;
                vb.MaxWidth = 180;
                vb.StretchDirection = StretchDirection.DownOnly;
                vb.Stretch = System.Windows.Media.Stretch.Fill;
                vb.Child = visEl;

                Canvas.SetLeft(vb, Math.Max(0, pos.X - 100 / 2));
                Canvas.SetTop(vb, Math.Max(0, pos.Y - 100 / 2));
                schedulingCanvas.Children.Add(vb);

                //log event
                logger.log("Visual Element \"" + visEl.VEName + "\" droppped on Scheduling canvas.");

                //add visual template to the repository
                templateRepo.templates.Add(visEl.templateVM);
                templateRepoR.templates.Add(visEl.templateVMR);

                //suggestions
                //visEl.abstractTree.prepare();
                //this.sourceASTL.prepare();
                //this.visualiserSuggester = new Suggester(sourceASTL, visEl.abstractTree);
                //this.updateSuggestions(this.visualiserSuggester.getSuggestionsAsStrings(this.visualiserSuggester.imFeelingLucky()));

            }
        }

        #endregion


        #region scheduling process

        private VisualElement createSchedulingStartVisElement()
        {
            VisualElement v = new VisualElement();

            //set data
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml("<Start>start</Start>");

            v.Data = xDoc.DocumentElement;
            v.VEName = "/";
            v.templateVM.TemplateName = "/";

            //set data for reverse
            xDoc = new XmlDocument();
            xDoc.LoadXml("<Start>start</Start>");

            v.ReverseData = xDoc.DocumentElement;
            v.VEName = "/";
            v.templateVMR.TemplateName = "/";
            v.elementListPopup.IsOpen = true;

            Ellipse e = new Ellipse();
            e.Width = 10;
            e.Height = 10;
            e.Fill = System.Windows.Media.Brushes.Black;

            v.Content = e;

            Canvas.SetTop(v, 20);
            Canvas.SetLeft(v, 10);
            Canvas.SetZIndex(v, 1);

            return v;
        }

        private void initiateSchedulingCanvas()
        {
            //Clear Canvas
            schedulingCanvas.Children.Clear();

            //clear templates
            templateRepo.templates.Clear();

            //Create start element
            VisualElement startVE = createSchedulingStartVisElement();
            schedulingCanvas.Children.Add(startVE);
            //startVE.createRunPopup();

        }

        internal string generateCode(XSLTTemplate template, XSLTTemplateRepository repo)
        {
            string xslCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
            xslCode += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n";

            xslCode += template.generateXSLTTemplateCode();

            foreach (XSLTTemplate xt in repo.templates)
                xslCode += xt.generateXSLTTemplateCode() + "\n";

            xslCode += "</xsl:stylesheet>\n";

            return prettyPrinter.PrintToString(xslCode);
        }

        #endregion //scheduling process


        #region suggestions

        /// <summary>
        /// Clears previous suggestions and lists new ones
        /// </summary>
        /// <param name="collection">new suggestions string collection</param>
        internal void updateSuggestions(Collection<string> collection)
        {
            suggestions.Clear();

            foreach (string s in collection)
            {
                string[] splitSt = { " -> " };
                string[] strArray = s.Split(splitSt, StringSplitOptions.RemoveEmptyEntries);

                Suggestion sg = new Suggestion(strArray[0], strArray[1], this);
                suggestions.Add(sg);
            }

            reportMessage("Suggestions updated.", ReportIcon.OK);
        }

        public bool acceptSuggestion(Suggestion sg)
        {
            if (visEl != null)
            {
                //update weights in Suggester
                this.visualiserSuggester.retrieveSuggestion(sg.SuggestionString, "ACCEPT");

                if (sg.RHS.Equals(visEl.Data.Name))// map to Visual element
                {
                    AbstractTreeLatticeNode atln = sourceASTL.getAbstractNodeAtAddress(sg.LHS);

                    if (atln != null)
                    {
                        XmlNode tx = atln.ToXML();
                        TreeViewViewModel tv = new TreeViewViewModel(tx);
                        visEl.processVisual_TreeNodeDrop(tv.Root, atln.Address); //like dropping a TreeNode on visEl
                        reportMessage("Suggestion \"" + sg.ToString() + "\" applied", ReportIcon.OK);
                        return true;
                    }
                }
                else //map to internal elements
                {

                    AbstractLattice sourceLattice = new AbstractLattice(visEl.ReverseData);
                    AbstractTreeLatticeNode lownode = sourceLattice.Root.findInChildrenByName(sg.LHS);

                    //MessageBox.Show("Source: " + sg.LHS);

                    if (lownode != null)
                    {
                        string lhs = visEl.abstractTree.findRelativeAddress(sourceLattice.Root, lownode);

                        if (!String.IsNullOrEmpty(lhs))
                        {
                            string rhs = sg.RHS;
                            //if (rhs.StartsWith(visEl.abstractTree.Root.Name))
                            //  rhs = rhs.Substring(visEl.abstractTree.Root.Name.Length+1);

                            //MessageBox.Show("in sugg\n\nSource: " + lhs + "\nTarget: " + rhs);

                            XmlNode tx = sourceLattice.Root.ToXML();
                            TreeViewViewModel tv = new TreeViewViewModel(tx);
                            TreeNodeViewModel tn = tv.findInTreeByName(sg.LHS);
                            if (tn != null)//which should be
                                //MessageBox.Show("in sugg: " + tv.Root.ToXML().OuterXml);
                                if (visEl.processElement_TreeNodeDrop(lhs, rhs, tn) == true)
                                {//like dropping a tree node on element of the list
                                    reportMessage("Suggestion \"" + sg.ToString() + "\" applied", ReportIcon.OK);
                                    return true;
                                }
                            //MessageBox.Show("in sugg\n\n"+visEl.templateVM.TemplateXmlNode.OuterXml + "\n\n" + visEl.templateVMR.TemplateXmlNode.OuterXml);
                        }

                        return false;
                    }
                }

                reportMessage("Could not find suggestion in source model", ReportIcon.Error);
                return false;

            }
            else//else will never happen
            {
                reportMessage("No visual element to apply suggestions to", ReportIcon.Error);
                return false;
            }

        }


        #endregion //suggestions


        #region menu buttons

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dObj = LogicalTreeHelper.GetParent(this);

            while ((dObj != null) && (!(dObj is MainWindow)))
                dObj = LogicalTreeHelper.GetParent(dObj);

            if (dObj != null)
                (dObj as MainWindow).executeClose();
        }

        private void schedulingClearMenu_Click(object sender, RoutedEventArgs e)
        {
            initiateSchedulingCanvas();

            //clear templates
            templateRepo.templates.Clear();
            templateRepoR.templates.Clear();
        }

        private void schedulingRunMenu_Click(object sender, RoutedEventArgs e)
        {
            //string xslCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
            //xslCode += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n";

            //xslCode += "<xsl:template match=\"/\">\n";

            VisualElement startingVisEl = schedulingCanvas.Children[0] as VisualElement;

            //use start code
            string xslCodeF = generateCode(startingVisEl.templateVM, templateRepo);
            string xslCodeR = generateCode(startingVisEl.templateVMR, templateRepoR);

            //MessageBox.Show("Forward: \n\n" + xslCodeF);
            //MessageBox.Show("Reverse: \n\n" + xslCodeR);


            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save XSLT Template";
            saveDialog.ShowDialog();

            if (!saveDialog.FileName.Equals(""))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xslCodeF);

                XmlDocument docR = new XmlDocument();
                docR.LoadXml(xslCodeR);

                xslFile = saveDialog.FileName;
                if (!xslFile.ToLower().EndsWith(".xsl"))
                    xslFile += ".xsl";

                xslFile = xslFile.ToLower();
                xslFileR = xslFile.Replace(".xsl", "R.xsl");

                //log event
                logger.log("Transformation file \"" + xslFile + "\" created.", ReportIcon.OK);
                logger.log("Reverse transformation file \"" + xslFileR + "\" created.", ReportIcon.OK);

                doc.Save(xslFile);
                docR.Save(xslFileR);
                prettyPrinter.PrintXmlFile(xslFile);
                prettyPrinter.PrintXmlFile(xslFileR);

                //Run it
                saveDialog.Title = "Save Target file";
                saveDialog.FileName = "";
                saveDialog.ShowDialog();

                if (!saveDialog.FileName.Equals(""))
                {
                    //resulted visualisation model file
                    modelVisualFile = saveDialog.FileName;
                    //where the reaulted reverse will be saved

                    if (!modelVisualFile.ToLower().EndsWith(".xml"))
                        modelVisualFile += ".xml";

                    modelFileRegenerated = modelFile.ToLower().Replace(".xml", "R.xml");
                    //put it in test directory instead
                    modelFileRegenerated = modelFileRegenerated.Substring(modelFileRegenerated.LastIndexOf("\\") + 1);//get new file name
                    modelFileRegenerated = modelVisualFile.Replace(modelVisualFile.Substring(modelVisualFile.LastIndexOf("\\") + 1), modelFileRegenerated);

                    XslCompiledTransform myXslTransform;
                    myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(xslFile);
                    myXslTransform.Transform(modelFile, modelVisualFile);
                    prettyPrinter.PrintXmlFile(modelVisualFile);

                    XslCompiledTransform myXslTransformR = new XslCompiledTransform();
                    myXslTransformR.Load(xslFileR);
                    myXslTransformR.Transform(modelVisualFile, modelFileRegenerated);
                    prettyPrinter.PrintXmlFile(modelFileRegenerated);

                    //clear defined visual templates
                    //customToolbox.Items.Clear();

                    //clear template repositories
                    templateRepo.templates.Clear();
                    templateRepoR.templates.Clear();

                    //log event
                    logger.log("Transformation \"" + xslFile + "\" applied on \"" + modelFile + "\" and output \"" + modelVisualFile + "\" is created.", ReportIcon.OK);
                    logger.log("Reverse transformation \"" + xslFileR + "\" applied on \"" + modelVisualFile + "\" and output \"" + modelFileRegenerated + "\" is created.", ReportIcon.OK);

                    reportMessage("Transformation Completed!", ReportIcon.OK);

                    renderVisualisation(modelVisualFile);
                    initiateSchedulingCanvas();
                }
            }
        }

        private void TestXSLT_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open XSLT File";
            openDialog.DefaultExt = "*.XSL";
            openDialog.ShowDialog();

            if (!openDialog.FileName.Equals(""))
            {
                xslFile = openDialog.FileName;

                //Run it
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save Target file";
                saveDialog.FileName = "";
                saveDialog.ShowDialog();

                if (!saveDialog.FileName.Equals(""))
                {
                    //resulted model xml file
                    modelVisualFile = saveDialog.FileName;

                    XslCompiledTransform myXslTransform;
                    myXslTransform = new XslCompiledTransform();
                    XsltSettings xsltSettings = new XsltSettings();
                    xsltSettings.EnableDocumentFunction = true;

                    myXslTransform.Load(xslFile, xsltSettings, null);
                    double ctime = 0;
                    //repeat for 5 times and record time
                    for (int i = 0; i < 5; i++)
                    {
                        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                        myXslTransform.Transform(modelFile, modelVisualFile);
                        sw.Stop();
                        ctime += sw.ElapsedMilliseconds;
                    }

                    //MessageBox.Show(sw.Elapsed.TotalSeconds + " Sec    / " + ((float)sw.Elapsed.TotalSeconds / (float)60).ToString("N2") + " min");
                    MessageBox.Show((ctime / 5).ToString());

                    prettyPrinter.PrintXmlFile(modelVisualFile);

                    reportMessage("Testing XSLT complete! " + modelVisualFile + " is created.", ReportIcon.OK);
                }

            }
        }

        private void SaveVisualElementTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (VisElementCanvas.Children.Count > 0)
            {
                if (visEl.templateVM != null)
                {

                    foreach (UIElement u in VisElementCanvas.Children)//look for functions
                        if (u.GetType().ToString().Equals("CONVErT.VisualFunction"))//add functions to templates
                        {
                            visEl.templateVM.functions.Add((u as VisualFunction).getFunctionCode());
                            visEl.templateVMR.functions.Add((u as VisualFunction).getReverseFunctionCode());
                        }

                    visEl.templateVM.insertFunctions();//put functions to the actual code
                    visEl.templateVMR.insertFunctions();


                    //remove elements from canvas
                    VisElementCanvas.Children.Clear();

                    ToolboxItem myItem = new ToolboxItem(visEl);

                    //save to file
                    saveCustomTooboxItem(myItem);

                    //show in custom toolbox
                    customToolbox.Items.Add(myItem);

                    //check, abstraction and signature
                    XmlNode x = visEl.sourceLatticeNodeToMatch.createSignatureNode(visEl.templateVM.HeaderNode);

                    //log event
                    logger.log("Visual Element \"" + visEl.VEName + "\" created.", ReportIcon.OK);

                    reportMessage("Visual Element Rule saved!", ReportIcon.OK);

                }
            }
            else
            {
                reportMessage("No visual element exists!", ReportIcon.Error);
                logger.log("Tried saving visual element that is not existing!", ReportIcon.Error);
            }
        }

        private void RenderElements_Click(object sender, RoutedEventArgs e)
        {
            RenderCanvas.Children.Clear();

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select visualisation file";
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            openDialog.ShowDialog();

            if (!openDialog.FileName.Equals(""))
            {
                modelVisualFile = openDialog.FileName;
                renderVisualisation(modelVisualFile);
            }

        }

        public void renderVisualisation(String visualisationFile)
        {
            try
            {

                //check 
                String vistype = "";

                vistype = checkAvailableVisualisationType(visualisationFile);
                //MessageBox.Show(vistype);

                if (vistype.Equals("XAML"))
                {
                    //xrenderer = new XAMLRenderer();
                    RenderCanvas.Children.Clear();
                    UIElement visresult = xrenderer.createVisualisation(visualisationFile);
                    if (visresult != null)
                    {
                        RenderCanvas.Children.Add(visresult);
                        //log event
                        logger.log("Rendering \"" + visualisationFile + "\".");

                        DesignerTabControl.SelectedIndex = 2;
                        reportMessage("Visualisation \"" + visualisationFile + " rendered", ReportIcon.OK);
                    }
                    else
                    {
                        //log event
                        logger.log("Rendering \"" + visualisationFile + "\" failed.");

                        DesignerTabControl.SelectedIndex = 2;
                        reportMessage("Null visualisation resulted from \"" + visualisationFile + "!", ReportIcon.Error);
                    }
                }
                else if (vistype.Equals("SVG"))
                {
                    string htmlOutput = srenderer.createVisualisation(visualisationFile);

                    if (!String.IsNullOrEmpty(htmlOutput))
                    {
                        XmlDocument htmlDoc = new XmlDocument();
                        htmlDoc.LoadXml("<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\" /></head><body>"
                                        + htmlOutput
                                        + "</body></html>");

                        htmlDoc.Save(DirectoryHelper.getFilePathExecutingAssembly("testSVG.html"));

                        htmlRenderBrowser.Navigate(new Uri(DirectoryHelper.getFilePathExecutingAssembly("testSVG.html"), UriKind.RelativeOrAbsolute));

                        DesignerTabControl.SelectedIndex = 3;

                        ReportStatusBar.ShowStatus("Visualisation \"" + visualisationFile + " rendered", ReportIcon.OK);
                        logger.log("Tried rendering \"" + visualisationFile + "\".", ReportIcon.OK);
                    }
                    else
                    {
                        ReportStatusBar.ShowStatus("Rendering \"" + visualisationFile + " failed", ReportIcon.Error);
                        logger.log("Null visualisation resulted from \"" + visualisationFile + "\".", ReportIcon.Error);
                    }
                }
                else if (String.IsNullOrEmpty(vistype))
                {
                    ReportStatusBar.ShowStatus("could not locate suitable visualisation!", ReportIcon.Error);
                    logger.log("Render visualisation error -> could not locate suitable visualisation!", ReportIcon.Error);
                }

            }
            catch (Exception ex)
            {
                reportMessage(ex.ToString(), ReportIcon.Error);
                ReportStatusBar.ShowStatus("Rendering visualisation failed!", ReportIcon.Error);
                logger.log("Render visualisation error -> " + ex.ToString(), ReportIcon.Error);
            }
        }

        private void Load_ToolBox_Click(object sender, RoutedEventArgs e)
        {
            reportMessage("Loading input model", ReportIcon.Busy);

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            openDialog.Title = "Load Toolbox";
            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string tempFileName = openDialog.FileName;
                loadToolBoxItem(tempFileName);
            }
        }

        private void OpenModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reportMessage("Loading input model", ReportIcon.Busy);

                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "XML files (*.xml)|*.xml|Coma Seperated files (*.csv)|*.csv|All files (*.*)|*.*";
                openDialog.FilterIndex = 0;
                openDialog.Title = "Open model file";
                Nullable<bool> result = openDialog.ShowDialog();

                if (result == true)
                {
                    string tempFileName = openDialog.FileName;
                    //if *.csv file convert it to XML
                    if (tempFileName.ToLower().EndsWith(".csv"))
                    {
                        CSV2XMLConverter converter = new CSV2XMLConverter();
                        string XmlModelFile = tempFileName.ToLower().Replace(".csv", ".xml");
                        //MessageBox.Show("Conversion successfull");

                        converter.ConvertToXML(tempFileName, XmlModelFile);
                        tempFileName = XmlModelFile;
                    }

                    string modelName = tempFileName.Substring(tempFileName.LastIndexOf('\\') + 1);
                    modelName = modelName.Substring(0, modelName.ToLower().LastIndexOf(".xml"));
                    CloseableTabItem theTabItem = null;

                    //check if model is already open
                    bool hasOpened = false;
                    foreach (TabItem t in ModelTab.Items)
                    {
                        if ((t as CloseableTabItem).Title.Equals(modelName))
                        {
                            hasOpened = true;
                            theTabItem = (t as CloseableTabItem);
                            break;
                        }
                    }

                    //put model in new tab
                    if (hasOpened == false)//model has not been opened before
                    {
                        //load abstract lattice should be here

                        // Create new FileInfo object and get the Length.
                        FileInfo f = new FileInfo(tempFileName);
                        long s1 = f.Length;

                        if (s1 > 50000000)//file bigger than 50MB
                        {
                            //process the sample instead
                            reportMessage("Input size was : " + (s1 / 1024).ToString() + " KB, so I skipped loading!", ReportIcon.Warning);
                            //process actual file
                            //theTabItem = new CloseableTabItem();
                            //theTabItem.Title = modelName;
                            //theTabItem.InputModelTree.loadTreeView(sourceASTL.Root.ToXML());
                            //ModelTab.Items.Add(theTabItem);
                            //theTabItem.Focus();
                        }
                        else
                        {
                            //process actual file
                            theTabItem = new CloseableTabItem(tempFileName);
                            theTabItem.Title = modelName;
                            //theTabItem.ModelTree.loadTreeView(modelFile);
                            ModelTab.Items.Add(theTabItem);
                            theTabItem.Focus();

                            //create source abstraction 
                            //sourceASTL = new AbstractLattice(modelFile);

                            //tvvm = new TreeViewViewModel(modelFile);//no need with new system
                            //ModelTreeView.DataContext = tvvm;
                        }

                        //log event
                        logger.log("Model file \"" + modelName + "\" opened.", ReportIcon.OK);

                        reportMessage("Model opened successfully!", ReportIcon.OK);
                    }
                    else//model has been opened before
                    {
                        theTabItem.Focus();
                        reportMessage("Model has been opened before!", ReportIcon.Warning);
                    }


                    //clear canvas
                    RenderCanvas.Children.Clear();
                }
            }
            catch (Exception ex)
            {
                reportMessage(ex.ToString(), ReportIcon.Error);
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            logger.clearLogs();
        }
        
        private void ExportToPNG_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select visualisation file";
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string tempFileName = openDialog.FileName;
                //string tempFileName = @"C:\Users\Iman\Documents\Visual Studio 2012\Projects\CONVErT\CONVErT\Test\Working\HorBarchart.xml";
                try
                {
                    XAMLRenderer rend = new XAMLRenderer();
                    UIElement rendResult = rend.createStillVisualisation(tempFileName);


                    if (rendResult != null)
                    {
                        //get PNG file name
                        SaveFileDialog saveDialog = new SaveFileDialog();

                        saveDialog.Title = "Export to PNG";
                        saveDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                        saveDialog.FileName = "";
                        saveDialog.ShowDialog();

                        if (!saveDialog.FileName.Equals(""))
                        {
                            //resulted visualisation model file
                            string savePngFile = saveDialog.FileName;
                            //where the reaulted reverse will be saved

                            if (!savePngFile.ToLower().EndsWith(".png"))
                                savePngFile += ".png";

                            //save to file
                            using (Stream imageStream = File.Create(savePngFile))
                            {
                                Window wind = new Window();
                                wind.WindowStyle = WindowStyle.None;
                                wind.ShowInTaskbar = false;
                                wind.ShowActivated = false;

                                // Create Minimized so the window does not show. wind.WindowState = System.Windows.WindowState.Minimized;
                                wind.SizeToContent = SizeToContent.WidthAndHeight;

                                wind.Content = (UIElement)rendResult;
                                wind.Show(); // The window needs to be created for the XAML elements to be rendered. 

                                BitmapSource bitmapSrc = visualToBitmap(wind, 100, 100);//dpiX, dpiY);
                                BitmapEncoder encoder = new PngBitmapEncoder();

                                encoder.Frames.Clear();
                                encoder.Frames.Add(BitmapFrame.Create(bitmapSrc));
                                encoder.Save(imageStream);

                                imageStream.Flush();

                                wind.Hide();
                            }
                        }
                        else
                            reportMessage("Failed to save PNG file!", ReportIcon.Error);
                    }
                    else
                        reportMessage("Export to PNG -> Rendered XAML returned empty!", ReportIcon.Error);
                }
                catch (Exception ex)
                {
                    reportMessage("Exception in Visualiser.ExportToPNG -> " + ex.Message, ReportIcon.Error);
                }
            }
        }

        private BitmapSource visualToBitmap(Visual target, double dpiX, double dpiY)
        {
            if (target == null) return null;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            int width = (int)(bounds.Width * dpiX / 96.0);
            int height = (int)(bounds.Height * dpiX / 96.0);

            RenderTargetBitmap renderer = new RenderTargetBitmap(
                    width, height, dpiX,
                    dpiY, PixelFormats.Pbgra32);

            renderer.Render(target);

            return renderer;
        }

        private void ExportToXAML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select visualisation file";
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string tempFileName = openDialog.FileName;
                //string tempFileName = @"C:\Users\Iman\Documents\Visual Studio 2012\Projects\CONVErT\CONVErT\Test\Working\HorBarchart.xml";
                try
                {
                    XAMLRenderer rend = new XAMLRenderer();
                    string rendResult = rend.renderToStillXAML(tempFileName);

                    if (!String.IsNullOrEmpty(rendResult))
                    {
                        //save the xaml file
                        SaveFileDialog saveDialog = new SaveFileDialog();

                        saveDialog.Title = "Export to XAML file";
                        saveDialog.Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*";
                        saveDialog.FileName = "";
                        saveDialog.ShowDialog();

                        if (!saveDialog.FileName.Equals(""))
                        {
                            //resulted visualisation model file
                            string saveXamlFile = saveDialog.FileName;
                            //where the reaulted reverse will be saved

                            if (!saveXamlFile.ToLower().EndsWith(".xaml"))
                                saveXamlFile += ".xaml";

                            //save string to file
                            using (StreamWriter outfile = new StreamWriter(saveXamlFile))
                            {
                                outfile.Write(rendResult);
                                outfile.Close();
                                reportMessage("XAML file generated successfully", ReportIcon.OK);
                            }
                        }
                        else
                            reportMessage("Failed to save XAML file!", ReportIcon.Error);
                    }
                    else
                        reportMessage("Rendered XAML returned empty!", ReportIcon.Error);
                }
                catch (Exception ex)
                {
                    reportMessage("Exception in Visualiser.ExportToXAML -> " + ex.Message, ReportIcon.Error);
                }
            }
        }

        private void ExportToHTML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select visualisation file";
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string tempFileName = openDialog.FileName;
                try
                {
                    XAMLRenderer rend = new XAMLRenderer();
                    string rendResult = rend.renderToStillXAML(tempFileName);

                    if (!String.IsNullOrEmpty(rendResult))
                    {
                        //save the xaml file
                        SaveFileDialog saveDialog = new SaveFileDialog();

                        saveDialog.Title = "Export to HTML file";
                        saveDialog.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
                        saveDialog.FileName = "";
                        saveDialog.ShowDialog();

                        if (!saveDialog.FileName.Equals(""))
                        {
                            string dialogFile = saveDialog.FileName;
                            string saveHtmlFile = ""; //resulted visualisation html file
                            string xamlFile = ""; //where the xaml to be viewed will be saved 

                            if (!dialogFile.ToLower().EndsWith(".xaml"))
                                xamlFile = dialogFile + ".xaml";
                            else
                                xamlFile = dialogFile;

                            if (!dialogFile.ToLower().EndsWith(".html"))
                                saveHtmlFile = dialogFile + ".html";
                            else
                                saveHtmlFile = dialogFile;


                            //save xaml string to file
                            using (StreamWriter outfile = new StreamWriter(xamlFile))
                            {
                                outfile.Write(rendResult);
                                outfile.Close();
                                reportMessage("HTML files generated successfully", ReportIcon.OK);
                            }

                            string htmlResult = rend.renderToStillHTML(tempFileName, xamlFile);

                            //save xaml string to file
                            using (StreamWriter outfile = new StreamWriter(saveHtmlFile))
                            {
                                outfile.Write(htmlResult);
                                outfile.Close();
                                reportMessage("HTML files generated successfully", ReportIcon.OK);
                            }

                            //get 


                        }
                        else
                            reportMessage("Failed to save HTML file!", ReportIcon.Error);
                    }
                    else
                        reportMessage("Rendered HTML returned empty!", ReportIcon.Error);
                }
                catch (Exception ex)
                {
                    reportMessage("Exception in Visualiser.ExportToHTML -> " + ex.Message, ReportIcon.Error);
                }
            }
        }

        private void RenderStillVisualisation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openDialog.FilterIndex = 0;
            openDialog.Title = "Select visualisation file";
            Nullable<bool> result = openDialog.ShowDialog();

            if (result == true)
            {
                string tempFileName = openDialog.FileName;
                //string tempFileName = @"C:\Users\Iman\Documents\Visual Studio 2012\Projects\CONVErT\CONVErT\Test\Working\HorBarchart.xml";
                try
                {
                    XAMLRenderer rend = new XAMLRenderer();
                    UIElement rendResult = rend.createStillVisualisation(tempFileName);
                    if (rendResult != null)
                    {
                        RenderCanvas.Children.Clear();
                        RenderCanvas.Children.Add(rendResult);

                        //log event
                        logger.log("Tried rendering still visualisation \"" + tempFileName + "\".");
                        DesignerTabControl.SelectedIndex = 2;
                        reportMessage("Still visualisation \"" + tempFileName + " rendered", ReportIcon.OK);
                    }
                }
                catch (Exception ex)
                {
                    reportMessage(ex.ToString(), ReportIcon.Error);
                }
            }
        }

        private void SaveAndGenerateCode_Click(object sender, RoutedEventArgs e)
        {


            string xslCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
            xslCode += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n";

            xslCode += "<xsl:template match=\"/\">\n";

            //if (e.Data.GetDataPresent("ToolboxItem"))
            //{
            //    ToolboxItem t = e.Data.GetData("ToolboxItem") as ToolboxItem;
            //    xslCode += t.visualElement.templateVM.generateXSLTCall(t.visualElement.templateVM.TemplateName).OuterXml + "\n";
            //}
            //else
            //{
            //    VisualElement v = e.Data.GetData("VisualElement") as VisualElement;
            //    xslCode += v.templateVM.generateXSLTCall(v.templateVM.TemplateName).OuterXml + "\n";
            //}

            //XSLTTemplate tempStart = templateRepo.findTemplateByAddressID((tvvm.FirstChild.ElementAt(0).getFullAddress()));

            //if (tempStart == null)
            //{
            //    //what should go here?
            //}else
            //{
            //    xslCode += tempStart.generateXSLTCall(tempStart.TemplateName);
            //}
            //MessageBox.Show("final template : \n\n" + visEl.templateVM.generateXSLTTemplateCode());
            VisElementCanvas.Children.Remove(visEl);

            ToolboxItem myItem = new ToolboxItem(visEl);

            //save template to be embedded in the code
            templateRepo.templates.Add(visEl.templateVM);

            customToolbox.Items.Add(myItem);

            xslCode += visEl.templateVM.generateXSLTCall(visEl.templateVM.TemplateName).OuterXml + "\n";
            xslCode += "</xsl:template>\n";

            foreach (XSLTTemplate xt in templateRepo.templates)
                xslCode += xt.generateXSLTTemplateCode() + "\n";

            xslCode += "</xsl:stylesheet>\n";


            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save XSLT Template";
            saveDialog.ShowDialog();

            if (!saveDialog.FileName.Equals(""))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xslCode);
                xslFile = saveDialog.FileName;
                doc.Save(xslFile);

                //Run it
                saveDialog.Title = "Save Target file";
                saveDialog.FileName = "";
                saveDialog.ShowDialog();

                if (!saveDialog.FileName.Equals(""))
                {
                    modelVisualFile = saveDialog.FileName;
                    XslCompiledTransform myXslTransform;
                    myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(xslFile);
                    myXslTransform.Transform(modelFile, modelVisualFile);

                    //clear defined visual templates
                    //customToolbox.Items.Clear();
                    templateRepo.templates.Clear();



                    reportMessage("Transformation generated and executed successfully!", ReportIcon.OK);
                }
                else
                    reportMessage("Transformation could not be executed as the target file was empty!", ReportIcon.Error);
            }
        }

        private void ExportToDataVisualiser_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        
        #region tools

        private string checkAvailableVisualisationType(string visualisationFile)
        {
            //check file for first element name
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(visualisationFile);
            string nameElement = xdoc.DocumentElement.Name;
                        
            //check toolbox items
            string toolboxItemsPath = DirectoryHelper.getFilePathExecutingAssembly("Resources\\ToolBoxItems.xml");
            
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
                    XmlNode dataNode = xnode.SelectSingleNode("data");
                    string dataNodeName = dataNode.FirstChild.Name;
 
                    if (nameElement.Equals(dataNodeName))
                    {
                        return xnode.Attributes.GetNamedItem("type").InnerText;
                    }

                }
            }

            return null;
        }

        #endregion //tools


        #region Test

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //HorusAbstractionMaker habsmaker = new HorusAbstractionMaker(@"C:\Users\iavazpour\Desktop\TransformationDef.xml");
            
            //string elementsFile = DirectoryHelper.getFilePathExecutingAssembly("Resources\\ToolBoxItems.xml");
            //VisToHorusParser listconverter = new VisToHorusParser(elementsFile);
            
            //listconverter.parseVisualisation(@"C:\Users\iavazpour\Documents\SVN_IA\CONVErT\CONVErT\test\svgbarchart.xml");
            //listconverter.parseVisualisation();


            //srenderer = new SVGRenderer();//is at testing phase
            //string teststr = srenderer.createVisualisation(@"C:\Users\iavazpour\Documents\SVN_IA\CONVErT\CONVErT\test\svgbarchart.xml");
            //MessageBox.Show(teststr);
            //using (StreamWriter outfile = new StreamWriter(@"C:\Users\iavazpour\Documents\SVN_IA\CONVErT\CONVErT\test\dingdong.html"))
            //{
            //    outfile.Write(teststr);
            //}

            //AbstractLattice source1 = new AbstractLattice(@"C:\Test\bibtext.xml");
            //source1.prepare();
            ////MessageBox.Show(source1.Root.ToXML().OuterXml);

            //AbstractLattice target1 = new AbstractLattice(@"C:\Test\second.xml");
            //target1.prepare();

            ////MessageBox.Show(target1.Root.ToXML().OuterXml);
            //SuggesterConfig sgcon = new SuggesterConfig();
            //sgcon.UseIsoRankSimSuggester = true;
            //sgcon.UseNameSimSuggester = true;
            //sgcon.UseNeighborSimSuggester = true;
            //sgcon.UseStructSimSuggester = true;
            //sgcon.UseTypeSimSuggester = true;
            //sgcon.UseValueSimSuggester = true;

            //Suggester newsg = new Suggester(source1, target1, sgcon);
            //double[,] results = newsg.getSuggestions();
            ////MessageBox.Show(MatrixLibrary.Matrix.PrintMat(results));

            //Collection<string> temp = newsg.getSuggestionsAsStrings(newsg.imFeelingLucky());
            //foreach (string s in temp)
            //    MessageBox.Show(s);

            //VisElementCanvas.Children.Add(renderer.parsImanSXaml());
            //modelVisualFile = @"C:\Documents and Settings\iavazpour\My Documents\SVN_IA\CONVErT\CONVErT\test\mytest\Minard2.xml";
            //Renderer rend = new Renderer();

            //            RenderCanvas.Children.Add(rend.createVisualisation(modelVisualFile));
            //          DesignerTabControl.SelectedIndex = 2;
        }

        #endregion //Test


        #region tabControl interaction

        private void visualisationTab_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            CustomVisualisationExpander.IsExpanded = false;
            ItemsExpander.IsExpanded = true;
        }

        private void schedulingTab_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            CustomVisualisationExpander.IsExpanded = true;
            ItemsExpander.IsExpanded = false;
        }

        private void renderingTab_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            CustomVisualisationExpander.IsExpanded = false;
            ItemsExpander.IsExpanded = false;
        }

        private void htmlRenderingTab_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            CustomVisualisationExpander.IsExpanded = false;
            ItemsExpander.IsExpanded = false;
        }

        #endregion //tabControl interaction


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
