using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;

namespace CONVErT
{
    /// <summary>
    /// Interaction logic for Mapper.xaml
    /// </summary>
    public partial class Mapper : ContentControl, INotifyPropertyChanged
    {

        #region properties

        string sourceFile = "";
        string targetFile = "";
        //string xslFileF = "";
        //string xslFileR = "";
        //string transformedFile = "";

        public XSLTTemplateRepository tempRepo = new XSLTTemplateRepository();
        public XSLTTemplateRepository tempRepoR = new XSLTTemplateRepository();

        public XSLTTemplate NewTemplate { get; set; }
        public XSLTTemplate NewTemplateR { get; set; }

        //XSLTTemplate startTemplate = null;
        //XSLTTemplate startTemplateR = null;

        public XmlNode sourceData { get; set; }

        //Point? elementDragStartPoint = null;
        Point? valueListboxDragStartPoint = null;

        //private ObservableCollection<string> _matches = new ObservableCollection<string>();
        private ObservableCollection<VisualElement> _matches = new ObservableCollection<VisualElement>();
        public ObservableCollection<VisualElement> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        public Suggester mapperSuggester;

        public ObservableCollection<Suggestion> suggestions { get; set; }

        public AbstractLattice sourceASTL = null;
        public AbstractLattice targetASTL = null;

        public XAMLRenderer renderer = new XAMLRenderer();

        //for generating rules. 
        public VisualElement LHS { get; set; }
        public VisualElement RHS { get; set; }

        //public VisualElement templateVisual;
        public ContentPresenter ruleContent;

        Toolbox functionsToolBox;

        Collection<Adorner> targetAdorners = new Collection<Adorner>();
        Collection<Adorner> sourceAdorners = new Collection<Adorner>();


        private Collection<VisualElement> sourceVisualElements = new Collection<VisualElement>();
        private Collection<VisualElement> targetVisualElements = new Collection<VisualElement>();

        public List<string> InteractModes { get; set;}

        public Logger logger;

        #endregion //properties


        #region ctor

        public Mapper()
        {
            InitializeComponent();

            //For interaction modes in Mapper.
            InteractModes = new List<string> { "Mouse", "Stylus", "Gesture", "Stylus and Gesture" };
            InteractionModeCombobox.ItemsSource = InteractModes;
            InteractionModeCombobox.SelectedIndex = 0;

            //style for values list box -> rule designer
            Style valuesListboxStyle = new Style();
            EventSetter esVSMouseDown = new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ValueListBox_PreviewMouseDown));
            EventSetter esVSMouseMove = new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ValueListBox_PreviewMouseMove));
            valuesListboxStyle.Setters.Add(esVSMouseDown);
            valuesListboxStyle.Setters.Add(esVSMouseMove);
            ValuesListBox.ItemContainerStyle = valuesListboxStyle;


            RulesListBox.ItemsSource = Matches;

            //style for defined rules list box 
            //Style rulesListboxStyle = new Style();
            //EventSetter esMouseDown = new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ListBoxItem_PreviewMouseDown));
            //EventSetter esMouseMove = new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_MouseMove));
            //rulesListboxStyle.Setters.Add(esMouseDown);
            //rulesListboxStyle.Setters.Add(esMouseMove);
            //RulesListBox.ItemContainerStyle = rulesListboxStyle;

            suggestions = new ObservableCollection<Suggestion>();
            SuggestionsListBox.ItemsSource = suggestions;

            //set them to true
            mapperSuggester = new Suggester();

            //load functions toolbox
            functionsToolBox = new Toolbox();
            string functionsFile = "../../Resources/Functions.xml";
            loadFunctionsToolbox(functionsFile, functionsToolBox);

            FunctionsExpander.Content = functionsToolBox;
            FunctionsExpander.IsExpanded = true;

            //Logger
            logger = new Logger("MapperLogger");
            logsTab.Content = logger;

        }

        #endregion


        #region Rule Design Canvas interaction

        private void RuleDesignerCanvas_Drop(object sender, DragEventArgs e)
        {
            System.Windows.Point pos = e.GetPosition(RuleDesignerCanvas);

            if (e.Data.GetDataPresent("VisualFunction"))
            {

                VisualFunction vf = e.Data.GetData("VisualFunction") as VisualFunction;

                if (RuleDesignerCanvas.Children.Count > 0)//if there exists visual element, match functions header node with the element header node
                {
                    //foreach (UIElement u in RuleDesignerCanvas.Children)//assign same header node to the function as well
                    //  if (u.GetType().ToString().Equals("VisualMapper.VisualElement"))
                    //{
                    //  if ((u as VisualElement).templateVM.HeaderNode != null)//header node has been set
                    //{
                    //set visualfunction's target metamodel as this visual element 
                    //  vf.targetASTL = new AbstractLattice((u as VisualElement).abstractTree.Root.duplicate());
                    //set visualfunction's source abstraction from source model
                    //  vf.sourceASTL = new AbstractLattice(this.sourceASTL.getAbstractNodeAtAddress((u as VisualElement).templateVM.TemplateAddress).duplicate());

                    //  vf.sourceRootNode = (u as VisualElement).templateVM.HeaderNode;

                    Canvas.SetLeft(vf, Math.Max(0, pos.X - 100 / 2));
                    Canvas.SetTop(vf, Math.Max(0, pos.Y - 100 / 2));

                    RuleDesignerCanvas.Children.Add(vf);

                    //log event
                    logger.log("Visual Function dropped on Ruledesigner canvas");

                    //break;
                    //}
                    //else
                    // {
                    //   MessageBox.Show("Set header node for visual Element first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}
                    //}
                }
            }
            else if (e.Data.GetDataPresent("VisualCondition")) //copy paste from visualiser, it might not work here
            {
                VisualCondition vc = e.Data.GetData("VisualCondition") as VisualCondition;

                if (RuleDesignerCanvas.Children.Count > 0)//if there exists visual element, match functions header node with the element header node
                {
                    //foreach (UIElement u in RuleDesignerCanvas.Children)//assign same header node to the condition as well
                    //{
                    //if (u.GetType().ToString().Equals("CONVErT.VisualElement"))
                    //{
                    //if ((u as VisualElement).templateVM.HeaderNode != null)//header node has been set
                    //{
                    //set visualfunction's target metamodel as this visual element 
                    //vc.targetASTL = new AbstractLattice((u as VisualElement).abstractTree.Root.duplicate());
                    //set visualfunction's source abstraction from source model
                    //vc.sourceASTL = new AbstractLattice(this.sourceASTL.getAbstractNodeAtAddress((u as VisualElement).templateVM.TemplateAddress).duplicate());

                    //vc.sourceRootNode = (u as VisualElement).templateVM.HeaderNode;

                    Canvas.SetLeft(vc, Math.Max(0, pos.X - 100 / 2));
                    Canvas.SetTop(vc, Math.Max(0, pos.Y - 100 / 2));

                    RuleDesignerCanvas.Children.Add(vc);

                    //log event
                    logger.log("Visual Condition dropped on Ruledesigner canvas");
                    
                    // break;
                    //}
                    //else
                    //{
                    //    ReportStatusBar.ShowMessage("Set header node for visual Element first", ReportIcon.Error);
                    //}
                    //}
                    //}    
                }
            }
            else
                ReportStatusBar.ShowStatus("No visual element has been set yet", ReportIcon.Error);


        }

        #endregion //Rule design canvas


        #region Functions toolbox

        private void loadFunctionsToolbox(string functionsFile, Toolbox fToolbox)
        {
            XmlDocument fdom = new XmlDocument();

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
                    System.Windows.MessageBox.Show(xmlEx.Message);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("Could not load Function ToolboxItems file : " + functionsFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion


        #region Templates list box interaction

        /*private void CheckBox_Checked(object sender, RoutedEventArgs e) 
        {
            if (e.Source is CheckBox)//off course it is!!
            {
                DependencyObject obj = e.Source as CheckBox;
                while ((obj != null)&& !(obj is ListBoxItem)){//go up till to find the visual element containing templates

                    if (obj is VisualElement)
                    {
                        if (!string.IsNullOrEmpty((obj as VisualElement).VEName))
                            break;
                    }
                    obj = LogicalTreeHelper.GetParent(obj);
                }

                if ((obj != null) && (obj is VisualElement))//found the visual element containing rule templates
                {
                    //Clear all other rule check boxes (if any)
                    foreach(VisualElement i in RulesListBox.Items)//does not work!
                    {
                        var ui = (CheckBox)(i.Content as StackPanel).FindName("TopRuleCheckbox");
                        if (ui!= null)
                            (ui as CheckBox).IsChecked = false;
                    }
                    //check current checkbox
                    //(e.Source as CheckBox).IsChecked = true;

                    //set start template
                    string startname = (obj as VisualElement).VEName;

                    string forwardTemplateName = startname.Substring(0, (startname.LastIndexOf("2")));
                    string reverseTempalteName = startname.Substring(startname.LastIndexOf("2") + 1);

                    startTemplate = tempRepo.findTemplateByName(forwardTemplateName);
                    startTemplateR = tempRepoR.findTemplateByName(reverseTempalteName);

                    if (startTemplate == null || startTemplateR == null)
                    {
                        ReportStatusBar.ShowStatus("Start templates have problem", ReportIcon.Error);
                    }
                    else
                    {
                        ReportStatusBar.ShowStatus("Starting rule : " + forwardTemplateName + " <-> " + reverseTempalteName, ReportIcon.Info);

                        //log event
                        logger.log("Rule: " + forwardTemplateName + " <-> " + reverseTempalteName + " is set as starting rule.", ReportIcon.Info);

                    }

                    e.Handled = true;
                }
            }
        }*/

        #endregion


        #region evaluation

        public void evaluateCombination()
        {
            List<SuggesterConfig> MaxPrecisionConfigurations = new List<SuggesterConfig>();
            List<SuggesterConfig> MaxRecallConfigurations = new List<SuggesterConfig>();
            List<SuggesterConfig> MaxFMeasureConfigurations = new List<SuggesterConfig>();
            double maxPrecision = 0;
            double maxRecall = 0;
            double maxFMeasure = 0;

            if (sourceASTL != null && targetASTL != null)
            {
                this.sourceASTL.prepare();
                this.targetASTL.prepare();

                SuggesterConfig config = new SuggesterConfig();

                //use different combintions
                for (int i = 0; i < 2; i++)
                {
                    config.UseIsoRankSimSuggester = !config.UseIsoRankSimSuggester;
                    for (int j = 0; j < 2; j++)
                    {
                        config.UseNameSimSuggester = !config.UseNameSimSuggester;
                        for (int k = 0; k < 2; k++)
                        {
                            config.UseStructSimSuggester = !config.UseStructSimSuggester;
                            for (int l = 0; l < 2; l++)
                            {
                                config.UseTypeSimSuggester = !config.UseTypeSimSuggester;
                                for (int m = 0; m < 2; m++)
                                {
                                    config.UseValueSimSuggester = !config.UseValueSimSuggester;
                                    for (int p = 0; p < 2; p++)
                                    {
                                        config.UseNeighborSimSuggester = !config.UseNeighborSimSuggester;

                                        //create suggester and test it
                                        mapperSuggester = new Suggester(sourceASTL, targetASTL, config);
                                        mapperSuggester.imFeelingLucky();
                                        //apperSuggester.getRankedSuggestions(2);

                                        SuggesterEvaluator eval = new SuggesterEvaluator(mapperSuggester);

                                        //get max precision
                                        if (eval.Precision > maxPrecision)
                                        {
                                            MaxPrecisionConfigurations.Clear();
                                            MaxPrecisionConfigurations.Add(config.Clone());
                                            maxPrecision = eval.Precision;
                                        }
                                        else if (eval.Precision == maxPrecision)
                                            MaxPrecisionConfigurations.Add(config.Clone());

                                        //get best recalls
                                        if (eval.Recall > maxRecall)
                                        {
                                            MaxRecallConfigurations.Clear();
                                            MaxRecallConfigurations.Add(config.Clone());
                                            maxRecall = eval.Recall;
                                        }
                                        else if (eval.Recall == maxRecall)
                                            MaxRecallConfigurations.Add(config.Clone());

                                        //get best FMeasures
                                        if (eval.FMeasure > maxFMeasure)
                                        {
                                            MaxFMeasureConfigurations.Clear();
                                            MaxFMeasureConfigurations.Add(config.Clone());
                                            maxFMeasure = eval.FMeasure;
                                        }
                                        else if (eval.FMeasure == maxFMeasure)
                                            MaxFMeasureConfigurations.Add(config.Clone());

                                        //SuggesterEvaluator eval = new SuggesterEvaluator(mapperSuggester);//evaluation is included in imFeelingLucky
                                    }
                                }
                            }
                        }
                    }
                }

                StreamWriter writer = new StreamWriter("Eval.csv");

                //write best precisions
                writer.WriteLine("Max precision of: " + maxPrecision + " Achieved by:");
                writer.WriteLine("");
                string header = "ISO, Name, Struct, Type, Value, Neighbors, Precision, Recall, FMeasure";
                writer.WriteLine(header);
                writer.WriteLine("");

                foreach (SuggesterConfig sc in MaxPrecisionConfigurations)
                {
                    string rl = (sc.UseIsoRankSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNameSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseStructSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseTypeSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseValueSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNeighborSimSuggester ? "1" : "0");

                    writer.WriteLine(rl);
                }
                writer.WriteLine("");
                writer.WriteLine("");

                //write best recalls
                writer.WriteLine("Max Recall of: " + maxRecall + " Achieved by:");
                writer.WriteLine("");
                writer.WriteLine(header);
                writer.WriteLine("");

                foreach (SuggesterConfig sc in MaxRecallConfigurations)
                {
                    string rl = (sc.UseIsoRankSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNameSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseStructSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseTypeSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseValueSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNeighborSimSuggester ? "1" : "0");

                    writer.WriteLine(rl);
                }
                writer.WriteLine("");
                writer.WriteLine("");

                //write best FMeasures
                writer.WriteLine("Max FMeasure of: " + maxFMeasure + " Achieved by:");
                writer.WriteLine("");
                writer.WriteLine(header);
                writer.WriteLine("");

                foreach (SuggesterConfig sc in MaxFMeasureConfigurations)
                {
                    string rl = (sc.UseIsoRankSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNameSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseStructSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseTypeSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseValueSimSuggester ? "1" : "0") + ", ";
                    rl += (sc.UseNeighborSimSuggester ? "1" : "0");

                    writer.WriteLine(rl);
                }

                writer.Flush();
                writer.Close();

                ReportStatusBar.ShowStatus("Suggestor valuation complete!", ReportIcon.Info);
            }
        }

        #endregion //evaluation


        #region menu buttons

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
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

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //AbstractLattice sabs = new AbstractLattice(@"C:\Users\iavazpour\Desktop\CitationsEndNote.xml");
            //AbstractLattice tabs = new AbstractLattice(@"C:\Users\iavazpour\Desktop\FromJobRefDocBook.xml");

            AbstractLattice tabs = new AbstractLattice("real_estate_nky.xml");
            //AbstractLattice sabs = new AbstractLattice("real_estate_texas.xml");
            //AbstractLattice tabs = new AbstractLattice("real_estate_yahoo.xml");
            //AbstractLattice tabs = new AbstractLattice("real_estate_homeseeker.xml");
            AbstractLattice sabs = new AbstractLattice("real_estate_windermere.xml");

            Suggester testSugg;

            SuggesterConfig scP = new SuggesterConfig();
            SuggesterConfig scR = new SuggesterConfig();

            double prec = 0;
            double reca = 1;

            SuggesterConfig sct = new SuggesterConfig();
            sct.UseIsoRankSimSuggester = true;
            sct.UseNameSimSuggester = true;
            sct.UseNeighborSimSuggester = true;
            sct.UseStructSimSuggester = true;
            sct.UseTypeSimSuggester = true;
            sct.UseValueSimSuggester = true;

            testSugg = new Suggester(sabs, tabs, sct);
            testSugg.imFeelingLucky();
            
            String test1 =  "Evaluation Results for test1 " + testSugg.evaluator.printAnalysisResults();  
            //MessageBox.Show(test1);

            
           
            SuggesterConfig sc = new SuggesterConfig();
            sc.UseIsoRankSimSuggester = false;
            sc.UseNameSimSuggester = false;
            sc.UseNeighborSimSuggester = false;
            sc.UseStructSimSuggester = false;
            sc.UseTypeSimSuggester = false;
            sc.UseValueSimSuggester = false;

            for (int i = 1; i < 2; i++)
            {
                sc.UseIsoRankSimSuggester = !sc.UseIsoRankSimSuggester;
                for (int j = 1; j < 2; j++)
                {
                    sc.UseNameSimSuggester = !sc.UseNameSimSuggester;
                    for (int t = 0; t < 2; t++)
                    {
                        sc.UseNeighborSimSuggester = !sc.UseNeighborSimSuggester;
                        for (int a = 0; a < 2; a++)
                        {
                            sc.UseStructSimSuggester = !sc.UseStructSimSuggester;
                            for (int b = 0; b < 2; b++)
                            {
                                sc.UseTypeSimSuggester = !sc.UseTypeSimSuggester;
                                for (int d = 0; d < 2; d++)
                                {
                                    sc.UseValueSimSuggester = !sc.UseValueSimSuggester;

                                    testSugg = new Suggester(sabs, tabs, sc);
                                    testSugg.imFeelingLucky();

                                    if (testSugg.evaluator.Precision > prec)
                                    {
                                        scP = sc;
                                        prec = testSugg.evaluator.Precision;
                                    }

                                    if (testSugg.evaluator.Recall < reca)
                                    {
                                        scR = sc;
                                        reca = testSugg.evaluator.Recall;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            testSugg = new Suggester(sabs, tabs, scP);
            testSugg.imFeelingLucky();
            //MessageBox.Show(scP.ToString() + testSugg.evaluator.printAnalysisResults(), "Evaluation Results for precision");
            string test2 = "Evaluation Results for precision" + testSugg.evaluator.printAnalysisResults(); 
            
            testSugg = new Suggester(sabs, tabs, scR);
            testSugg.imFeelingLucky();
            //MessageBox.Show(scR.ToString() + testSugg.evaluator.printAnalysisResults(), "Evaluation Results for recall");
            string test3 = "Evaluation Results for recall" + testSugg.evaluator.printAnalysisResults();
           
            MessageBox.Show(test1 + test2 + test3);


            //AbstractLattice sabsR = new AbstractLattice(@"C:\Users\iavazpour\Desktop\CitationsEndNote.xml");
            //AbstractLattice tabsR = new AbstractLattice(@"C:\Users\iavazpour\Desktop\FromJobRefDocBook.xml");


            //First Create the instance of Stopwatch Class
            //Stopwatch sw = new Stopwatch();

            // Start The StopWatch ...From 000
            //sw.Start();

            //testSugg.imFeelingLucky();

            //sw.Stop();
            
            //this.updateSuggestions(testSugg.getSuggestionsAsStrings(testSugg.LastResults));


            //MessageBox.Show(string.Format("Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, 
             //   sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds));

           

            //testSugg = new Suggester(sabs, tabs, sc);

            //sw = new Stopwatch();

            // Start The StopWatch ...From 000
            //sw.Start();

            //testSugg.imFeelingLucky();

            //sw.Stop();

            //this.updateSuggestions(testSugg.getSuggestionsAsStrings(testSugg.LastResults));


            //MessageBox.Show(string.Format("Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes,
               // sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds));

            /*Object obj = TestVisual.Resources["sourceData"];
            MessageBox.Show(obj.GetType().ToString());
            if ((obj as XmlDataProvider) != null)
            {
                XmlNode xnode = (obj as XmlDataProvider).Document.DocumentElement.Clone();
                if (xnode != null)
                {
                    XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xnode.OuterXml));
                    MessageBox.Show(xmlDocumentWithoutNs.ToString());
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(xmlDocumentWithoutNs.ToString());
                    TestVisual.Data =  xdoc.DocumentElement;
                }
                
            }*/


            //string test = @"javasource/class_declarations/class_declaration/properties/java_property/identifier";

            //MyLogicalTreeHelper helper = new MyLogicalTreeHelper();
            //Collection<object> results = helper.getVisualElementsByName(SourceCanvas, QualifiedNameString.Convert("javasource"));

            //foreach (var obj in results)
            //{
            //    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            //    FrameworkElement found = (obj) as FrameworkElement;
            //    FrameworkElementAdorner fea = new FrameworkElementAdorner(found);
            //    if (myAdornerLayer != null)
            //    {
            //        myAdornerLayer.Add(fea);

            //    }
            //}



            //var foundTextBox = LogicalTreeHelper.FindLogicalNode(SourceCanvas, "type");
            //if (foundTextBox != null)
            //{
            //    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            //    FrameworkElementAdorner fea = new FrameworkElementAdorner(foundTextBox as FrameworkElement);
            //    if (myAdornerLayer != null)
            //    {
            //        myAdornerLayer.Add(fea);
            //        //(foundTextBox as Label).Background = Brushes.Red;
            //    }
            //}
            //else
            //    ReportStatusBar.ShowMessage("not found", ReportIcon.Error);
            //evaluateCombination();
        }

        private void InteractionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(InteractionModeCombobox.SelectedIndex)
            { 
            
                case(0): //Mouse
                    SourceCanvas.EditingMode = InkCanvasEditingMode.None;
                    TargetCanvas.EditingMode = InkCanvasEditingMode.None;
                break;
            
                case (1): //Stylus
                    SourceCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    TargetCanvas.EditingMode = InkCanvasEditingMode.Ink;
                break;

                case(2): //Gesture
                    SourceCanvas.EditingMode = InkCanvasEditingMode.GestureOnly;
                    TargetCanvas.EditingMode = InkCanvasEditingMode.GestureOnly;
                break;

                case (3): //Gesture and Stylus
                    SourceCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
                    TargetCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
                break;

                default:
                    SourceCanvas.EditingMode = InkCanvasEditingMode.None;
                    TargetCanvas.EditingMode = InkCanvasEditingMode.None;
                break;
            }
        }

        private void LoadSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SourceCanvas.Children.Clear();
                sourceVisualElements.Clear();

                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Open Source file";
                openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openDialog.FilterIndex = 0;
                Nullable<bool> result = openDialog.ShowDialog();

                if (result == true)
                {
                    sourceFile = openDialog.FileName;
                    XAMLRenderer rend = new XAMLRenderer();
                    SourceCanvas.Children.Add(rend.createVisualisation(sourceFile));
                    
                    if (SourceCanvas.Children.Count > 0)//get visual elements for suggester
                        sourceVisualElements = rend.VisualElementList;
                    
                    sourceASTL = new AbstractLattice(sourceFile);
                    prepareSuggestions();
                    ReportStatusBar.ShowStatus("Source model loaded", ReportIcon.OK);

                    string modelname = sourceFile.Substring(sourceFile.LastIndexOf("\\") + 1);
                    //log event
                    logger.log("Source model \"" + modelname + "\" opened.", ReportIcon.Info);
                }
                else
                    ReportStatusBar.ShowStatus("Could not load source model", ReportIcon.Error);
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
        }

        private void LoadTarget_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TargetCanvas.Children.Clear();
                targetVisualElements.Clear();

                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Title = "Open Target file";
                openDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openDialog.FilterIndex = 0;
                Nullable<bool> result = openDialog.ShowDialog();

                if (result == true)
                {
                    targetFile = openDialog.FileName;
                    XAMLRenderer rend = new XAMLRenderer();
                    TargetCanvas.Children.Add(rend.createVisualisation(targetFile));

                    if (TargetCanvas.Children.Count > 0)//get visual elements for suggester
                        targetVisualElements = rend.VisualElementList;

                    targetASTL = new AbstractLattice(targetFile);
                    prepareSuggestions();
                    ReportStatusBar.ShowStatus("Target model loaded", ReportIcon.OK);

                    string modelname = targetFile.Substring(targetFile.LastIndexOf("\\") + 1);
                    //log event
                    logger.log("Target model \"" + modelname + "\" opened.", ReportIcon.Info);
                }
                else
                    ReportStatusBar.ShowStatus("Could not load Target model", ReportIcon.Error);
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
        }

        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //prepare rule representation
                StackPanel sp = new StackPanel();//testing new rule representation
                sp.Orientation = Orientation.Horizontal;

                Image arrowImage = new Image();
                arrowImage.Width = 50;
                arrowImage.Height = 30;
                // Create source.
                System.Windows.Media.Imaging.BitmapImage bi = new System.Windows.Media.Imaging.BitmapImage();

                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Images/doubleArrow.gif", UriKind.RelativeOrAbsolute);
                bi.EndInit();

                // Set the image source.
                arrowImage.Source = bi;

                sp.Children.Add(renderer.render(LHS.Clone() as VisualElement));
                sp.Children.Add(arrowImage);
                sp.Children.Add(renderer.render(RHS.Clone() as VisualElement));

                ruleContent = new ContentPresenter();
                ruleContent.Content = sp;

                saveTemplate();

                RuleDesignerCanvas.Children.Clear();
                RuleDesignStatusBar.clearReportMessage();
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
        }

        public void saveTemplate()
        {
            try
            {
                if (NewTemplate != null)
                {
                    foreach (UIElement u in RuleDesignerCanvas.Children)//look for functions
                        if (u.GetType().ToString().Equals("CONVErT.VisualFunction"))//add functions to templates
                        {
                            this.NewTemplate.functions.Add((u as VisualFunction).getFunctionCode());
                            this.NewTemplateR.functions.Add((u as VisualFunction).getReverseFunctionCode());
                        }

                    this.NewTemplate.insertFunctions();//put functions to the actual code
                    this.NewTemplateR.insertFunctions();
                    this.NewTemplate.checkForLeftovers();
                    this.NewTemplateR.checkForLeftovers();
                    
                    VisualElement templateVisual = new VisualElement();

                    //templateVisual.MouseEnter += new MouseEventHandler(VisualElement_MouseEnter);
                    //this.MouseLeave += new MouseEventHandler(VisualElement_MouseLeave);

                    tempRepo.templates.Add(NewTemplate);
                    tempRepoR.templates.Add(NewTemplateR);

                    //Matches.Add(NewTemplate.TemplateName + " -> " + NewTemplateR.TemplateName);
                    templateVisual.templateVM = NewTemplate;
                    templateVisual.templateVMR = NewTemplateR;

                    string name = (NewTemplate.TemplateName + "2" + NewTemplateR.TemplateName);
                    templateVisual.VEName = name;

                    templateVisual.Content = ruleContent.Content;
                    Matches.Add(templateVisual);

                    NewTemplate = null;
                    NewTemplateR = null;

                    ReportStatusBar.ShowStatus("Transformation rule saved.", ReportIcon.OK);

                    //log event
                    logger.log("Rule \"" + LHS.VEName + "\" <-> \"" + RHS.VEName + "\" created.", ReportIcon.Info);

                    //update suggestions
                    prepareSuggestions();
                }
                else
                    ReportStatusBar.ShowStatus("No template has been defined yet!", ReportIcon.Error);

            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
        }

        private bool generateTransformationCode(XSLTTemplateRepository rep, String AbstractTreeRootElement, string fileName)
        {
            try
            {
                //check start (top most) template exists and find it
                XSLTTemplate start = null;

                foreach (XSLTTemplate x in rep.templates)
                {
                    if (String.Equals(x.TemplateName, AbstractTreeRootElement))
                    {
                        start = x;
                        break;
                    }
                }

                if (start != null)
                {

                    string xslCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
                    xslCode += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n";

                    xslCode += "<xsl:template match=\"/\">\n";

                    xslCode += start.generateXSLTCall(start.TemplateName).OuterXml + "\n";

                    xslCode += "</xsl:template>\n";

                    foreach (XSLTTemplate xt in rep.templates)
                        xslCode += xt.generateXSLTTemplateCode() + "\n";

                    xslCode += "</xsl:stylesheet>\n";

                    if (!fileName.Equals(""))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xslCode);
                        doc.Save(fileName);
                        ReportStatusBar.ShowStatus(fileName + " Transformation saved successfully.", ReportIcon.OK);

                        //log event
                        logger.log("Transformation file \"" + fileName + "\" created.", ReportIcon.Info);

                    }
                    else
                        ReportStatusBar.ShowStatus("Unable to save transformation.", ReportIcon.Error);

                    return true;
                }

                ReportStatusBar.ShowStatus("Mapping Rule for " + AbstractTreeRootElement + " is not yet defined!", ReportIcon.Error);
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
            return false;

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Generate Transformation
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save Forward transformation";
                saveDialog.ShowDialog();
                //string output = "";

                if (!saveDialog.FileName.Equals(""))
                {
                    string xslFileF = saveDialog.FileName;
                    if (!xslFileF.ToLower().EndsWith(".xsl"))
                        xslFileF += ".xsl";

                    xslFileF = xslFileF.ToLower();
                    string xslFileR = xslFileF.Replace(".xsl", "R.xsl");

                    bool alrightF = generateTransformationCode(tempRepo, sourceASTL.Root.Name, xslFileF);
                    bool alrightR = generateTransformationCode(tempRepoR, targetASTL.Root.Name, xslFileR);

                    if ((alrightF) && (alrightR))
                    {
                        //Run it

                        saveDialog = new SaveFileDialog();
                        saveDialog.Title = "Save transformation results";
                        saveDialog.ShowDialog();
                        if (!saveDialog.FileName.Equals(""))
                        {
                            string resultFileF = saveDialog.FileName;
                            if (!resultFileF.ToLower().EndsWith(".xml"))
                                resultFileF += ".xml";

                            bool resultF = RunTransformation(sourceFile, xslFileF, resultFileF);

                            string resultFileR = resultFileF.ToLower().Replace(".xml", "R.xml");

                            if (resultF)//forward is ok
                            {
                                //run reverse transformation on the result (mostly for checking)
                                bool resultR = RunTransformation(resultFileF, xslFileR, resultFileR);

                                if (resultR)
                                {
                                    ReportStatusBar.ShowStatus("Transforamtion Complete!", ReportIcon.OK);
                                    logger.log("Transforamtion Complete!", ReportIcon.OK);
                                }
                                else
                                {
                                    ReportStatusBar.ShowStatus("Reverse transformation returned empty!", ReportIcon.Error);
                                    logger.log("Reverse transformation returned empty!", ReportIcon.Error);
                                }
                            }
                            else
                            {
                                ReportStatusBar.ShowStatus("Could not run reverse transformation as forward transformation had problem!", ReportIcon.Error);
                                logger.log("Could not run reverse transformation as forward transformation had problem!", ReportIcon.Error);
                            }
                        }
                        else
                        {
                            ReportStatusBar.ShowStatus("Problem saving transformation results!", ReportIcon.Error);
                            logger.log("Problem saving transformation results!", ReportIcon.Error);
                        }
                    }
                    else
                    {
                        ReportStatusBar.ShowStatus("Problem saving transformation file!", ReportIcon.Error);
                        logger.log("Problem saving transformation file!", ReportIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
        }

        private bool RunTransformation(string model, string xslf, string resultFile)
        {
            try
            {
                if (!resultFile.Equals(""))
                {
                    XslCompiledTransform myXslTransform;
                    myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(xslf);
                    myXslTransform.Transform(model, resultFile);

                    //clear defined visual templates
                    //customToolbox.Items.Clear();
                    //templateRepo.templates.Clear();

                    //log event
                    logger.log("Transformation \"" + xslf + "\" was applied on \"" + model + "\" and result is saved in \"" + resultFile + "\".", ReportIcon.Info);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ReportStatusBar.ShowStatus(ex.ToString(), ReportIcon.Error);
            }
            return false;
        }

        private void Learn_Click(object sender, RoutedEventArgs s)
        {
            prepareSuggestions();
            //trainSuggester();
        }

        private void ClearRules_Click(object sender, RoutedEventArgs s)
        {
            //Clear listbox
            Matches.Clear();
            
            //clear rule templates
            tempRepo.templates.Clear();
            tempRepoR.templates.Clear();

        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            logger.clearLogs();
        }

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dObj = LogicalTreeHelper.GetParent(this);

            while ((dObj != null) && (!(dObj is MainWindow)))
                dObj = LogicalTreeHelper.GetParent(dObj);

            if (dObj != null)
                (dObj as MainWindow).executeClose();
        }

        #endregion //menu buttons


        #region Suggestion highligthing

        private void SourceCanvas_MouseMove(object sender, MouseEventArgs e)//for highlighting suggestions
        {

            DependencyObject test = Mouse.DirectlyOver as DependencyObject;//get what is under mouse

            if (test != null)
            {
                while (test != null && (test as FrameworkElement) != null && String.IsNullOrEmpty((test as FrameworkElement).Name))//traverse visual tree to get to actual element with name
                    test = VisualTreeHelper.GetParent(test);

                if (test != null && (test as FrameworkElement) != null && !(test as FrameworkElement).Name.Equals("SourceCanvas"))//show adorner for test element (but not for Source Canvas)
                {
                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(test as FrameworkElement);

                    foreach (Adorner a in sourceAdorners)
                        myAdornerLayer.Remove(a);

                    //highlight element
                    VisualElementMouseAdorner visualMouseAdorner = new VisualElementMouseAdorner(test as UIElement);
                    if (visualMouseAdorner.Parent == null && myAdornerLayer != null)
                    {
                        myAdornerLayer.Add(visualMouseAdorner);
                        sourceAdorners.Add(visualMouseAdorner);
                    }

                    DependencyObject test2 = test;//start from the element
                    string localAddr = "";
                    string fullAddr = "";

                    do //traverse logical tree to find the address of this element
                    {
                        if (!String.IsNullOrEmpty((test2 as FrameworkElement).Name))
                        {
                            if (String.IsNullOrEmpty(localAddr))
                                localAddr = (test2 as FrameworkElement).Name;
                            else
                                localAddr = (test2 as FrameworkElement).Name + "/" + localAddr;
                        }
                        test2 = LogicalTreeHelper.GetParent(test2);

                    } while (test2 != null && !(test2 is VisualElement) && !((test2 as FrameworkElement).Name.Equals("SourceCanvas")));

                    if (test2 is VisualElement)
                    {
                        string topPortionOfAddress = "";
                        AbstractTreeLatticeNode test2AbstractNode = sourceASTL.Root.findInChildrenByName((test2 as VisualElement).Data.Name);
                        if (test2AbstractNode != null && test2AbstractNode.parent != null)
                            topPortionOfAddress = test2AbstractNode.parent.Address;

                        if (!string.IsNullOrEmpty(topPortionOfAddress))
                            fullAddr = topPortionOfAddress + "/" + localAddr;
                        else
                            fullAddr = localAddr;
                    }

                    ReportStatusBar.ShowStatus((test as FrameworkElement).Name + " ===> " + fullAddr, ReportIcon.Info);

                    if (TargetCanvas.Children.Count > 0)
                        highlightSuggestedCorrespondeces(QualifiedNameString.Convert(fullAddr));
                }
            }

        }

        public void highlightSuggestedCorrespondeces(string sourceAddress)
        {
            MyLogicalTreeHelper helper = new MyLogicalTreeHelper("TargetCanvas", this);

            FrameworkElement firstChild = (TargetCanvas.Children[0] as VisualElement).Content as FrameworkElement;
            if (firstChild is ScrollViewer)
                firstChild = (firstChild as ScrollViewer).Content as FrameworkElement;//by-pass scrol viewr problem for highlighting elements

            AdornerLayer targetAdornerLayer = AdornerLayer.GetAdornerLayer(firstChild);

            //clear previouse adorners(highlighting)
            if ((targetAdorners.Count > 0) && (targetAdornerLayer != null))
            {
                foreach (Adorner a in targetAdorners)
                    targetAdornerLayer.Remove(a);
            }

            foreach (Suggestion s in suggestions)
            {
                string left = s.LHS;
                string right = s.RHS;

                if (QualifiedNameString.Convert(left).Equals(sourceAddress))
                {
                    //clearReportMessage();

                    //find and highlight elements on the target
                    Collection<object> results = helper.getVisualElementsByAddress(TargetCanvas, QualifiedNameString.Convert(right));

                    if (results.Count > 0)
                    {
                        foreach (var obj in results)
                        {

                            FrameworkElement found = (obj) as FrameworkElement;
                            FrameworkElementAdorner fea = new FrameworkElementAdorner(found);
                            if (targetAdornerLayer != null)
                            {
                                targetAdornerLayer.Add(fea);
                                targetAdorners.Add(fea);
                            }
                        }

                    }
                    else
                        ReportStatusBar.ShowStatus("Could not locate recommended element on the target", ReportIcon.Info);
                }
            }
        }

        #endregion //suggestion highlighting


        #region suggestions


        /// <summary>
        /// This function implements the action to be perfomed when a suggestion is accepted
        /// </summary>
        /// <param name="sg">Suggestion to be accepted</param>
        /// <returns></returns>
        public bool acceptSuggestion(Suggestion sg)
        {

            VisualElement vs = new VisualElement();
            VisualElement vt = new VisualElement();

            if ((sourceVisualElements.Count > 0) && (targetVisualElements.Count > 0))
            {
                
                vs = findVisualElementInListByAddress(sourceVisualElements, sg.LHS);
                vt = findVisualElementInListByAddress(targetVisualElements, sg.RHS);
 
                if ((vs != null)&&(vt != null))// source and target notations exist
                {
                    //update weights in Suggester
                    this.mapperSuggester.retrieveSuggestion(sg.SuggestionString, "ACCEPT");
                    
                    vs = vs.Clone() as VisualElement; //duplicate element not to mess with original one.
                    vt = vt.Clone() as VisualElement; //duplicate
                    
                    vt.processVisual_VisualNotationDrop(vs,this); //like dropping a visual element on another in Mapper
                    
                    return true;
                }
                else if ((vs == null) && (vt == null))// source and target notations do not exist
                {
                    //look for internal elements of the visualisation
                    //when this happens, I have my source and target visualisations in LHS and RHS
                    if ((NewTemplate != null) && (NewTemplateR != null))
                    {
                        bool check = true;

                        string targetMatch = sg.RHS;
                        string sourceMatch = sg.LHS;
                        string targetElementValue = "";
                        string sourceElementValue = "";

                        AbstractTreeLatticeNode lhsNode = LHS.abstractTree.getAbstractNodeAtAddress(NewTemplate.TemplateName + "/" + sg.LHS);
                        if ((lhsNode != null) && (lhsNode.Values.Count > 0))
                        {
                            sourceElementValue = lhsNode.Values[0];
                        }

                        AbstractTreeLatticeNode rhsNode = RHS.abstractTree.getAbstractNodeAtAddress(NewTemplateR.TemplateName + "/" + sg.RHS);
                        if ((rhsNode != null) && (rhsNode.Values.Count > 0))
                        {
                            targetElementValue = rhsNode.Values[0];
                        }

                        if (!String.IsNullOrEmpty(targetElementValue))
                            this.NewTemplate.updateXmlNodeByExactValue(targetMatch, sourceMatch, targetElementValue);
                        else
                        {
                            check = false;
                            ReportStatusBar.ShowStatus("Problem is finding element -> Target element value in Mapper.acceptSuggestion(...)", ReportIcon.Error);
                        }
                        //for reverse
                        if (!String.IsNullOrEmpty(sourceElementValue))
                            this.NewTemplateR.updateXmlNodeByExactValue(sourceMatch, targetMatch, sourceElementValue);
                        else
                        {
                            check = false;
                            ReportStatusBar.ShowStatus("Problem is finding reverse element -> Source element value in Mapper.acceptSuggestion(...)", ReportIcon.Error);
                        }

                        if (check == true)
                        {
                            //update weights in Suggester
                            Suggestion sgTemp = new Suggestion(lhsNode.Address, rhsNode.Address, this);//the previouse suggestion has been alltered and the visual element name is removed, therefore a new one is temporarily generated.
                            this.mapperSuggester.retrieveSuggestion(sgTemp.SuggestionString, "ACCEPT");
                        }
                        return check;
                    }
                }
            } 
            
            //if we are here then the suggestion is for internal elements
            //or there is an error -> mapping notation to internal elements or vice versa
                       
            return false;
        }

        /// <summary>
        /// prepare suggestion for suggesting based on whole input models
        /// A pretty CPU intensive task!
        /// </summary>
        private void prepareSuggestions()
        {
            if (sourceASTL != null && targetASTL != null)
            {
                SuggesterConfig config = new SuggesterConfig();
                config.UseIsoRankSimSuggester = true;
                config.UseNameSimSuggester = true;
                config.UseStructSimSuggester = true;
                config.UseValueSimSuggester = true;
                config.UseTypeSimSuggester = true;

                this.sourceASTL.prepare();
                this.targetASTL.prepare();
                mapperSuggester = new Suggester(sourceASTL, targetASTL, config);

                //this.updateSuggestions(this.mapperSuggester.getOrderedSuggestionsAsStrings(this.mapperSuggester.getSuggestions()));
                //this.updateSuggestions(this.mapperSuggester.getSuggestionsAsStrings(this.mapperSuggester.getRankedSuggestions(1)));
                this.updateSuggestions(this.mapperSuggester.getSuggestionsAsStrings(this.mapperSuggester.imFeelingLucky()), true);

                ReportStatusBar.ShowStatus("Suggestions updated.", ReportIcon.Info);
            }
        }

        /// <summary>
        /// Clears previous suggestions and lists new ones
        /// </summary>
        /// <param name="collection">new suggestions string collection</param>
        internal void updateSuggestions(Collection<string> collection, bool notationsONLY)
        {
            suggestions.Clear();

            foreach (string s in collection)
            {
                string[] splitSt = { " -> " };
                string[] strArray = s.Split(splitSt, StringSplitOptions.RemoveEmptyEntries);
                
                if ((notationsONLY)&&//show suggestions for notations
                    (findVisualElementInListByAddress(sourceVisualElements,strArray[0])!= null)&&
                    (findVisualElementInListByAddress(targetVisualElements,strArray[1])!=null))
                {
                    Suggestion sg = new Suggestion(strArray[0], strArray[1], this);
                    suggestions.Add(sg);
                }
                else if (!(notationsONLY)&&//show suggestions for internal elements, filter out notation to internal element
                    (findVisualElementInListByAddress(sourceVisualElements,strArray[0])== null)&&
                    (findVisualElementInListByAddress(targetVisualElements,strArray[1])== null))
                {
                    if ((NewTemplate != null) && (NewTemplateR != null))
                    {
                        if (strArray[0].StartsWith(NewTemplate.TemplateName))//cut root element out
                            strArray[0] = strArray[0].Substring(strArray[0].IndexOf("/") + 1);

                        if (strArray[1].StartsWith(NewTemplateR.TemplateName))//cut root element out
                            strArray[1] = strArray[1].Substring(strArray[1].IndexOf("/") + 1);

                        if ((!strArray[0].Equals(NewTemplate.TemplateName)) && (!strArray[1].Equals(NewTemplateR.TemplateName)))//remove repeated root suggestion
                        {
                            Suggestion sg = new Suggestion(strArray[0], strArray[1], this);
                            suggestions.Add(sg);
                        }
                    }
                }
            }
            ReportStatusBar.ShowStatus("Suggestions updated.", ReportIcon.OK);
        }

        private VisualElement findVisualElementInListByAddress(Collection<VisualElement> list, String vAddress)
        {
            foreach (VisualElement vs in list)
            {
                if (vs.VAddress != null)
                    if (vAddress.Equals(vs.VAddress))// source notation exists
                        return vs;
            }
            return null;
        }

        #endregion //suggestions


        #region (Commented) INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members


        #region handling values

        private void AddValues_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ValueTextBox.Text))
            {
                string valueToAdd = ValueTextBox.Text;
                if (ISStringCheckBox.IsChecked == true)
                    valueToAdd = "'" + valueToAdd + "'";

                ValuesListBox.Items.Add(valueToAdd);
                ISStringCheckBox.IsChecked = false;
                ValueTextBox.Text = "";
            }
        }

        private void ValueListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            valueListboxDragStartPoint = new Point?(e.GetPosition(this));
        }

        private void ValueListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                valueListboxDragStartPoint = null;
            }

            if (valueListboxDragStartPoint.HasValue)
            {
                Point pos = e.GetPosition(this);

                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(pos.X - valueListboxDragStartPoint.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(pos.Y - valueListboxDragStartPoint.Value.Y))))
                {

                    string valueT = (ValuesListBox.SelectedItem as string);
                    ReportStatusBar.ShowStatus(valueT, ReportIcon.Info);


                    if (!String.IsNullOrEmpty(valueT))
                    {

                        DataObject dataObject = new DataObject("ConstantValue", valueT);

                        if (dataObject != null)
                        {
                            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                        }
                    }
                    else
                        ReportStatusBar.ShowStatus("something went wrong with value listbox dragged value", ReportIcon.Error);
                }

                e.Handled = true;
            }
        }

        #endregion //handling values
                

    }


}
