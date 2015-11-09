using System;
using System.Xml;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CONVErT
{
    //this function does not have reverse
    public class VisualCondition : VisualBase
    {

        #region properties

        //static int ID = 1;//for identifying variables

        private XmlNode functionXml;

        public TreeNodeViewModel sourceRootNode;

        public string conditionString = "";

        public string reverseOperation = "";

        string imageName = "";

        Point? visualConditionPosition;

        public AbstractLattice sourceASTL;
        public AbstractLattice targetASTL;

        XmlPrettyPrinter printer = new XmlPrettyPrinter();

        public bool isMultiCondition;

        XmlDocument xdoc = new XmlDocument();

        #endregion //properties


        #region ctor

        static VisualCondition()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VisualCondition), new FrameworkPropertyMetadata(typeof(VisualCondition)));
        }

        public VisualCondition(XmlNode xnode)
        {
            //keep a record for future use (and self clone)
            functionXml = xnode.Clone();

            //interaction handlers
            this.MouseDown += new MouseButtonEventHandler(VisualCondition_MouseDown);
            this.MouseMove += new MouseEventHandler(VisualCondition_MouseMove);

            //method data (arguments: inputs and outputs)
            //XmlNode xdata = xnode.SelectSingleNode("args");
            this.Data = xnode.Clone();

            //method reverse data (arguments: args -> reverse operation on data.output, and inputs -> placeholder of data outputs)
            //XmlNode xdataR = xnode.SelectSingleNode("argsR");
            //if (xdataR != null)
            //{
            //    this.DataR = xdataR.Clone();
            //    hasReverse = true;
            //}
            //else
            //{
            //    hasReverse = false;
            //    this.DataR = null;
            //}

            //get condition string
            //conditions = xnode.SelectSingleNode("conditions");
            
            //if (conditions.ChildNodes.Count > 1)
             //   isMultiCondition = true;
            //else
           //     isMultiCondition = false;

            //create the picture content
            imageName = Data.SelectSingleNode("image").InnerXml;
            
            if(!String.IsNullOrEmpty(imageName))
                this.Content = getImage();

        }

        public Image getImage()
        {
            if (!String.IsNullOrEmpty(imageName))
            {
                Image simpleImage = new Image();
                simpleImage.Width = 50;
                simpleImage.Height = 50;
                // Create source.
                BitmapImage bi = new BitmapImage();

                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"/Images/" + imageName, UriKind.RelativeOrAbsolute);
                bi.EndInit();

                // Set the image source.
                simpleImage.Source = bi;

                return simpleImage;
            }
            return null;
        }

        #endregion //ctor


        #region interaction

        private void VisualCondition_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseDown(e);
            visualConditionPosition = new Point?(e.GetPosition(this));
            e.Handled = true;
        }

        private void VisualCondition_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                visualConditionPosition = null;
            }

            if (visualConditionPosition.HasValue)
            {
                Point pos = e.GetPosition(this);

                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(pos.X - visualConditionPosition.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(pos.Y - visualConditionPosition.Value.Y))))
                {

                    DataObject dataObject = new DataObject("VisualCondition", this.Clone());

                    if (dataObject != null)
                    {
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                    }
                }

                e.Handled = true;
            }
        }

        #endregion //interaction


        #region listbox element interaction

        public override void ItemElement_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TreeNodeViewModel"))//for visualiser
            {
                TreeNodeViewModel treeNode = e.Data.GetData("TreeNodeViewModel") as TreeNodeViewModel;

                //drop item
                Element dropelement = (sender as ListBoxItem).DataContext as Element;
                string t = dropelement.Address;
                string fph = t.Substring(t.IndexOf('/') + 1);

                if (t.Contains("arg"))
                {
                    if (treeNode.Type == ElementType.InstanceValue)//is value
                    {
                        string valueStr;
                        double Num;
                        bool isNum = double.TryParse(treeNode.EValue, out Num);

                        if (!isNum)//comming value is not a number (is a string)
                            valueStr = "'" + treeNode.EValue + "'";
                        else
                            valueStr = treeNode.EValue;

                        updateData(fph, valueStr);
                    }

                    else //is node or attribute
                    {
                        string sourceRelativeAddress = treeNode.getRelativeAddress(sourceRootNode);
                        updateData(fph, sourceRelativeAddress);
                    }
                }
                else if ((t.Contains("condition")) || (t.Contains("otherwise")))
                {
                    if (!t.Contains("otherwise"))
                        fph = fph.Replace("expression", "operation"); //for condition, update operation instead of expression

                    if (treeNode.Type == ElementType.InstanceValue)//is value
                    {
                        string valueStr = treeNode.EValue;
                        updateData(fph, valueStr);
                    }

                    else //is node or attribute
                    {
                        string sourceRelativeAddress = treeNode.getRelativeAddress(sourceRootNode);
                        XmlNode temp = xdoc.CreateElement("xsl", "value-of", "http://www.w3.org/1999/XSL/Transform");
                        XmlAttribute xattr = xdoc.CreateAttribute("select");
                        xattr.Value = sourceRelativeAddress;
                        temp.Attributes.Append(xattr);

                        updateData(fph, temp);
                    }
                }
                else
                    ShowStatus("Invalid drop object -> VisualCondition.ItemElement_Drop, drop is: " + t, ReportIcon.Error);

                e.Handled = true;
            }
            else if ((e.Data.GetDataPresent("Element")) && (this.ParentWindow is Mapper))//for mapper rule designer
            {
                Element dropelement = (sender as ListBoxItem).DataContext as Element;
                string t = dropelement.Address;
                string fph = t.Substring(t.IndexOf('/') + 1);

                Element dragElement = e.Data.GetData("Element") as Element;

                if (t.Contains("arg"))
                {
                    String valueStr = dragElement.Address.Substring(dragElement.Address.IndexOf('/') + 1);
                    updateData(fph, valueStr);
                }

                else if ((t.Contains("condition")) || (t.Contains("otherwise")))
                {
                    if (!t.Contains("otherwise"))
                        fph = fph.Replace("expression", "operation"); //for condition, update operation instead of expression

                    string sourceRelativeAddress = dragElement.Address.Substring(dragElement.Address.IndexOf('/') + 1);
                    XmlNode temp = xdoc.CreateElement("xsl", "value-of", "http://www.w3.org/1999/XSL/Transform");
                    XmlAttribute xattr = xdoc.CreateAttribute("select");
                    xattr.Value = sourceRelativeAddress;
                    temp.Attributes.Append(xattr);

                    //reportMessage("function Placeholder: " + fph + "\n updating value: " + temp.OuterXml,ReportIcon.Info);

                    updateData(fph, temp);
                }
                else
                    ShowStatus("Invalid drop object -> VisualCondition.ItemElement_Drop, drop is: " + t, ReportIcon.Error);

                e.Handled = true;
            }
            else if (e.Data.GetDataPresent("ConstantValue"))//for mapper rule designer
            {
                Element dropelement = (sender as ListBoxItem).DataContext as Element;
                string t = dropelement.Address;
                string fph = t.Substring(t.IndexOf('/') + 1);

                if (t.Contains("arg"))
                {
                    string valueStr = e.Data.GetData("ConstantValue") as string;
                    updateData(fph, valueStr);
                }
                else if ((t.Contains("condition")) || (t.Contains("otherwise")))
                {
                    if (!t.Contains("otherwise"))
                        fph = fph.Replace("expression", "operation"); //for condition, update operation instead of expression

                    string valueStr = e.Data.GetData("ConstantValue") as string;
                    updateData(fph, valueStr);
                }

                e.Handled = true;
            }
            else
            {
                ShowStatus("Drag and drop for the Condition is undefined!", ReportIcon.Error);
                e.Handled = true;
            }
        }

        /*public override void ItemElement_MouseMove(object sender, MouseEventArgs e)
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

                        if (ele.Address.ToLower().Contains("arg"))
                        {
                            MessageBox.Show("Only conditions can be dragged!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            e.Handled = true;
                        }
                        else
                        {
                            //MessageBox.Show("Name: "+ele.Name + "  Value: "+ele.Value);

                            VisualConditionDragObject eleDrag = new VisualConditionDragObject();
                            eleDrag.Condition = this;
                            eleDrag.element = ele;

                            XmlNode condXml = getConditionCode();

                            DataObject dataObject = new DataObject("VisualConditionDragObject", eleDrag);

                            if (dataObject != null)
                            {
                                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                            }

                            e.Handled = true;
                        }
                    }
                }
            }
        }*/

        //ToDo -> what?
        public override void VisualBase_RightClick(object sender, MouseButtonEventArgs e)
        {

            //clear previous popUp
            if (showList == true)
            {
                showList = false;
                OnPropertyChanged("showList");
            }
            else
            {
                elementList.Clear();

                if (Data != null)
                {
                    XmlNode args = Data.SelectSingleNode("args");

                    foreach (XmlNode arg in args.ChildNodes)
                    {
                        Element el = new Element();
                        el.Value = arg.Name;
                        el.Name = arg.InnerText; //visual Condition                
                        el.Address = getFullAddress(arg.FirstChild);//bizzare way, the full address works with parents, so for now temporarily work like this -> arg1, etc. need to have value for now

                        elementList.Add(el);
                    }

                    XmlNode conditions = Data.SelectSingleNode("conditions");

                    foreach (XmlNode xcond in conditions.ChildNodes)
                    {
                        XmlNode temp = xcond;
                        Element el = new Element();

                        if (xcond.Name.Contains("condition"))
                        {
                            temp = xcond.SelectSingleNode("expression");
                            el.Name = temp.InnerText; //visual Condition  
                        }
                        else
                            el.Name = "Otherwise";
                        
                        el.Value = temp.Name;
                        //el.Name = temp.InnerText; //visual Condition                
                        el.Address = getFullAddress(temp.FirstChild);
                        elementList.Add(el);
                    }
                }
                else
                    ShowStatus("Data is null -> VisualConditions.VisualBase_RightClick(...)",ReportIcon.Error);

                OnPropertyChanged("elementList");
                showList = true;
                OnPropertyChanged("showList");
            }
            e.Handled = true;
        }


        #endregion //listbox element interaction


        #region code generation

        private void updateData(string toBeUpdated, Object updatingValue)
        {
            XmlNode updatee = this.Data.SelectSingleNode(toBeUpdated);//not working with attributes, and not expected to! as functions will not have any
            
            if (updatee != null)
            {
                if (updatingValue.GetType().Equals(typeof(string)))
                {
                    updatee.RemoveAll();
                    updatee.AppendChild(updatee.OwnerDocument.CreateTextNode(updatingValue as string));
                }
                else if (updatingValue is XmlNode)
                {
                    updatee.RemoveAll();
                    updatee.AppendChild(updatee.OwnerDocument.ImportNode((updatingValue as XmlNode),true));
                }

            }
            else
            {
                ShowStatus("Could not find updating node: " + toBeUpdated + " in -> VisualCondition.updateData(..)" + "\n Visual Condition: " + this.Name,ReportIcon.Error);
            }
        }

        /// <summary>
        /// get condition code
        /// </summary>
        /// <returns></returns>
        public XmlNode getConditionCode()
        {
            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";

            XmlElement code = xdoc.CreateElement(prefix, "choose", testNamespace);

            processConditionCode(); //put arguments in their designated places

            XmlNode conditions = Data.SelectSingleNode("conditions");

            foreach (XmlNode xcond in conditions.ChildNodes)
            {
                if (xcond.Name.Contains("condition"))//only for conditions not "otherwise"
                {
                    //create condition code
                    XmlNode conCode = xdoc.CreateElement(prefix, "when", testNamespace);
                    XmlAttribute testAttr = xdoc.CreateAttribute("test");
                    
                    string expression = xcond.SelectSingleNode("expression").InnerText;
                    testAttr.AppendChild(xdoc.CreateTextNode(expression));

                    conCode.Attributes.Append(testAttr);

                    XmlNode operation = xcond.SelectSingleNode("operation").FirstChild;
                    conCode.AppendChild(conCode.OwnerDocument.ImportNode(operation, true));

                    code.AppendChild(conCode);
                }
            }

            //make code for otherwise
            XmlNode otherwise = conditions.SelectSingleNode("otherwise");
            if (!String.IsNullOrEmpty(otherwise.InnerText))
                if (!otherwise.InnerText.ToLower().Equals("otherwise"))//something has been droped on otherwise
                {
                    //create code fro otherwise
                    XmlNode conCode = xdoc.CreateElement(prefix, "otherwise", testNamespace);
                    XmlNode operation = otherwise.FirstChild;
                    conCode.AppendChild(conCode.OwnerDocument.ImportNode(operation, true));

                    code.AppendChild(conCode);
                }

            return code;
        }

        /// <summary>
        /// Updates Arguments of the condition to their assigned values by drag and drop
        /// </summary>
        private void processConditionCode()
        {
            XmlNode args = this.Data.SelectSingleNode("args");
            XmlNode conditions = Data.SelectSingleNode("conditions");

            foreach (XmlNode arg in args.ChildNodes)
            {
                String argName = arg.Name;
                String argValue = arg.InnerText;

                foreach (XmlNode xcond in conditions.ChildNodes)
                {
                    if (xcond.Name.Contains("condition"))//only for conditions not "otherwise"
                    {
                        XmlNode temp = xcond.SelectSingleNode("expression");//get their expression

                        if (temp.InnerText.Contains(argName))//update expression if argument exists
                        {
                            String str = temp.InnerText;
                            str = str.Replace(argName, argValue);
                            temp.RemoveAll();
                            temp.AppendChild(temp.OwnerDocument.CreateTextNode(str));
                        }
                    }
                }
            }
        }

        ///// <summary>
        ///// get reverse function code
        ///// </summary>
        ///// <returns></returns>
        //public XmlNode getReverseFunctionCode()
        //{
        //    if (DataR != null)
        //        return getCode(DataR);

        //    return null;
        //}

        ///// <summary>
        ///// For reverse operation, the operation must know where the forward template puts the result for the 
        ///// reverse can fetch that data later on
        ///// </summary>
        ///// <param name="outputString">the output element of function (drag data)</param>
        ///// <param name="target">Place in the target model (drop position)</param>
        //public void updateOutput(string outputString, string target)
        //{
        //    string inputString = "inputs/arg" + outputString.Substring(6); //cut "output" from its begining and add the rest to input
        //    //MessageBox.Show("input: " + inputString + " target: " + target);

        //    XmlNode inputNode = DataR.SelectSingleNode(inputString);
        //    inputNode.RemoveAll();

        //    inputNode.AppendChild(inputNode.OwnerDocument.CreateTextNode(target));

        //    //MessageBox.Show(DataR.OuterXml);
        //}

        #endregion //code generation


        #region clone

        public VisualCondition Clone()
        {
            VisualCondition vf = new VisualCondition(this.functionXml.Clone() as XmlNode);
            vf.Data = this.Data.Clone();
            
            return vf;
        }

        #endregion

    }
        
}
