using System;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;

namespace CONVErT
{
    public class VisualFunction : VisualBase
    {
        #region properties

        static int ID = 1;//for identifying variables in chaining

        private XmlNode functionXml;

        public TreeNodeViewModel sourceRootNode;

        public string operation = "";

        public string reverseOperation = "";

        string imageName = "";

        private Point? visualFunctionPosition;

        public AbstractLattice sourceASTL;
        public AbstractLattice targetASTL;

        XmlPrettyPrinter printer = new XmlPrettyPrinter();

        #endregion //properties


        #region ctor

        static VisualFunction()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VisualFunction), new FrameworkPropertyMetadata(typeof(VisualFunction)));
        }

        public VisualFunction(XmlNode xnode)
        {
            //keep a record for future use (and self clone)
            functionXml = xnode.Clone();
            XmlNode tempFunctionXml = updateInputIDs(functionXml);
            //MessageBox.Show(printer.PrintToString(xnode) + "\n\n" + printer.PrintToString(tempFunctionXml));
            
            //interaction handlers
            this.MouseDown += new MouseButtonEventHandler(VisualFunction_MouseDown);
            this.MouseMove += new MouseEventHandler(VisualFunction_MouseMove);

            //method data (arguments: inputs and outputs)
            XmlNode xdata = tempFunctionXml.SelectSingleNode("args");
            //XmlNode xdataTemp = updateInputIDs(xdata);
            this.Data = xdata.Clone();

            //method reverse data (arguments: args -> reverse operation on data.output, and inputs -> placeholder of data outputs)
            XmlNode xdataR = tempFunctionXml.SelectSingleNode("argsR");
            //this.DataR = xdataR.Clone();
            this.DataR = xdataR.Clone();

            //create the picture content
            imageName = tempFunctionXml.SelectSingleNode("image").InnerXml;
            //MessageBox.Show(imageName);
            this.Content = getImage();

        }

        private XmlNode updateInputIDs(XmlNode data)
        {
            //copy the whole thing
            XmlNode xdataTemp = data.Clone();

            //get forward and reverse
            XmlNode args = xdataTemp.SelectSingleNode("args");
            XmlNode argsR = xdataTemp.SelectSingleNode("argsR");

            //get forward arguments
            XmlNode inputs = args.SelectSingleNode("inputs");
            XmlNode outputs = args.SelectSingleNode("outputs");
            XmlNode newInputs = inputs.OwnerDocument.CreateElement("inputs");
            XmlNode newOutputs = outputs.OwnerDocument.CreateElement("outputs");

            //getreverse reverse arguments
            XmlNode inputsR = argsR.SelectSingleNode("inputs");
            XmlNode outputsR = argsR.SelectSingleNode("outputs");
            XmlNode newInputsR = inputsR.OwnerDocument.CreateElement("inputs");
            XmlNode newOutputsR = outputsR.OwnerDocument.CreateElement("outputs");

            foreach (XmlNode i in inputs.ChildNodes)
            {
                XmlNode ida = i.Attributes.GetNamedItem("ID");
                if (ida != null)
                {
                    //forward area
                    int iid = int.Parse(ida.Value);
                    int newID = ID++;
                    string previousArgName = "arg" + iid.ToString();
                    string newArgName = "arg" + newID;

                    XmlNode newChild = i.OwnerDocument.CreateElement(newArgName);
                    newChild.AppendChild(newChild.OwnerDocument.CreateTextNode(i.InnerText));//assuming the child has only values as text
                    
                    newInputs.AppendChild(newChild);

                    //reverse (inputs of forward become outputs of reverse)
                    XmlNode reverseUpdatee = argsR.SelectSingleNode("outputs/output[@ID='"+iid+"']");
                    if (reverseUpdatee != null)
                    {
                        XmlNode newReverseOutput = reverseUpdatee.OwnerDocument.CreateElement("output" + newID);
                        newReverseOutput.AppendChild(newReverseOutput.OwnerDocument.CreateTextNode(reverseUpdatee.InnerText));
                        newOutputsR.AppendChild(newReverseOutput);
                    }
                 
                    //update output templates
                    foreach (XmlNode o in outputs.ChildNodes)
                    {
                        string oldValue = o.InnerText;
                        if (!String.IsNullOrEmpty(oldValue))
                        {
                            if (oldValue.IndexOf(previousArgName) != -1)
                            {
                                string newValue = oldValue.Replace(previousArgName+" ", newArgName+ " ");

                                o.ReplaceChild(o.OwnerDocument.CreateTextNode(newValue),o.ChildNodes[0]);
                            }
                        }
                    }
                }
            }
            //replace old and new inputs
            args.ReplaceChild(newInputs, inputs);

            foreach (XmlNode o in outputs.ChildNodes)
            {
                XmlNode ida = o.Attributes.GetNamedItem("ID");
                if (ida != null)
                {
                    int iid = int.Parse(ida.InnerText);
                    int newID = ID++;
                    string previousOutputName = "output" + iid.ToString();
                    string newOutputName = "output" + newID;

                    XmlNode newChild = o.OwnerDocument.CreateElement(newOutputName);
                    newChild.AppendChild(newChild.OwnerDocument.CreateTextNode(o.InnerText));//assuming the child has only values as text

                    newOutputs.AppendChild(newChild);

                    //outputs of forward become inputs of reverse
                    XmlNode reverseInputUpdatee = argsR.SelectSingleNode("inputs/arg[@ID='" + iid + "']");
                    if (reverseInputUpdatee != null)
                    {
                        XmlNode newReverseInput = reverseInputUpdatee.OwnerDocument.CreateElement("arg" + newID);
                        newReverseInput.AppendChild(newReverseInput.OwnerDocument.CreateTextNode(reverseInputUpdatee.InnerText));
                        newInputsR.AppendChild(newReverseInput);

                        foreach (XmlNode oR in newOutputsR.ChildNodes)
                        {
                            string oldValue = oR.InnerText;
                            string previousArgName = "arg" + iid.ToString();
                            string newArgName = "arg" + newID;
                            if (!String.IsNullOrEmpty(oldValue))
                            {
                                if (oldValue.IndexOf(previousArgName) != -1)
                                {
                                    string newValue = oldValue.Replace(previousArgName+" ", newArgName+" ");

                                    oR.ReplaceChild(oR.OwnerDocument.CreateTextNode(newValue), oR.ChildNodes[0]);
                                }
                            }
                        }
                    }
                    
                }
            }
            //replace old and new outputs
            args.ReplaceChild(newOutputs, outputs);

            argsR.ReplaceChild(newOutputsR, outputsR);
            argsR.ReplaceChild(newInputsR, inputsR);

            return xdataTemp;
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

        private void VisualFunction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseDown(e);
            visualFunctionPosition = new Point?(e.GetPosition(this));
            e.Handled = true;
        }

        private void VisualFunction_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                visualFunctionPosition = null;
            }

            if (visualFunctionPosition.HasValue)
            {
                Point pos = e.GetPosition(this);

                if ((SystemParameters.MinimumHorizontalDragDistance <=
                    Math.Abs((double)(pos.X - visualFunctionPosition.Value.X))) ||
                    (SystemParameters.MinimumVerticalDragDistance <=
                    Math.Abs((double)(pos.Y - visualFunctionPosition.Value.Y))))
                {

                    DataObject dataObject = new DataObject("VisualFunction", this.Clone());

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
            if ((e.Data.GetDataPresent("TreeNodeViewModel")) && (this.ParentWindow is Visualiser))//for visualiser
            {
                TreeNodeViewModel treeNode = e.Data.GetData("TreeNodeViewModel") as TreeNodeViewModel;

                string t = ((sender as ListBoxItem).DataContext as Element).Address;

                if (!t.Contains("output"))
                {
                    string fph = t.Substring(t.IndexOf('/') + 1);

                    if (treeNode.Type == ElementType.InstanceValue)//is value
                    {
                        string valueStr;
                        double Num;
                        bool isNum = double.TryParse(treeNode.EValue, out Num);

                        if (!isNum)//comming value is not a number (is a string)
                        {
                            valueStr = "'" + treeNode.EValue + "'";
                        }else
                            valueStr = treeNode.EValue;

                        updateData(fph, valueStr);
                    }
                    else //is node ro attribute
                    {
                        string sourceRelativeAddress = treeNode.getRelativeAddress(sourceRootNode);
                        updateData(fph, sourceRelativeAddress);
                    }
                }
                else
                    ShowStatus("Output cannot be dropped on!", ReportIcon.Error);

                e.Handled = true;
            }
            else if ((e.Data.GetDataPresent("Element")) && (this.ParentWindow is Mapper))//for mapper rule designer
            {
                string t = ((sender as ListBoxItem).DataContext as Element).Address;
                Element dragElement = e.Data.GetData("Element") as Element;

                if (!t.Contains("output"))
                {
                    string fph = t.Substring(t.IndexOf('/') + 1);

                    String valueStr = dragElement.Address.Substring(dragElement.Address.IndexOf('/') + 1);
                    updateData(fph, valueStr);

                }
                else
                    ShowStatus("Output cannot be dropped on!", ReportIcon.Error);

                e.Handled = true;

            }
            else if (e.Data.GetDataPresent("ConstantValue"))//for mapper rule designer
            {
                string t = ((sender as ListBoxItem).DataContext as Element).Address;

                if (!t.Contains("output"))
                {
                    string fph = t.Substring(t.IndexOf('/') + 1);
                    string valueStr = e.Data.GetData("ConstantValue") as string;//which here is a numerical value or a string
                    
                    updateData(fph, valueStr);
                }
                else
                    ShowStatus("Output cannot be dropped on!", ReportIcon.Error);

                e.Handled = true;

            }
            else if (e.Data.GetDataPresent("VisualFunctionElementDragObject"))//for function chaining with another function
            {
                //go up and see if the source is a function
                ShowStatus(e.Source.GetType().ToString() + "\n\n" + sender.GetType().ToString(), ReportIcon.Warning);

                //ToDo and check
            }
            else
            e.Handled = false;
        }

        public override void ItemElement_MouseMove(object sender, MouseEventArgs e)
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

                        if (!ele.Address.ToLower().Contains("output"))
                            ShowStatus("Only outputs can be dragged!", ReportIcon.Error);
                        else
                        {
                            VisualFunctionElementDragObject eleDrag = new VisualFunctionElementDragObject();
                            eleDrag.function = this;
                            eleDrag.element = ele;

                            DataObject dataObject = new DataObject("VisualFunctionElementDragObject", eleDrag);

                            if (dataObject != null)
                            {
                                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                            }

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        #endregion //listbox element interaction


        #region code generation

        private void updateData(string toBeUpdated, string updatingValue)
        {
            XmlNode updatee = this.Data.SelectSingleNode(toBeUpdated);//not working with attributes, and not expected to! as functions will not have any
            if (updatee != null)
            {
                updatee.RemoveAll();
                updatee.AppendChild(updatee.OwnerDocument.CreateTextNode(updatingValue));
            }
            else
            {
                ShowStatus("Could not find updating node: " + toBeUpdated + " -> VisualFunction.updateData(..)" + "\n Visual Function: " + this.Name,ReportIcon.Error);
            }
        }

        /// <summary>
        /// get forward function code
        /// </summary>
        /// <returns></returns>
        public XmlNode getFunctionCode()
        {
            return getCode(Data);
        }

        /// <summary>
        /// get reverse function code
        /// </summary>
        /// <returns></returns>
        public XmlNode getReverseFunctionCode()
        {
            return getCode(DataR);
        }

        public XmlNode getCode(XmlNode functionData)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNode functionCodeXml = xdoc.CreateElement("FunctionXml");

            String prefix = "xsl";
            String testNamespace = "http://www.w3.org/1999/XSL/Transform";


            XmlNode inputs = functionData.SelectSingleNode("inputs");
            XmlNode outputs = functionData.SelectSingleNode("outputs");

            foreach (XmlNode x in inputs.ChildNodes)
            {
                XmlElement arg = xdoc.CreateElement(prefix, "variable", testNamespace);
                XmlAttribute name = xdoc.CreateAttribute("name");
                name.AppendChild(xdoc.CreateTextNode(x.Name));
                arg.Attributes.Append(name);

                XmlNode valueof = xdoc.CreateElement(prefix, "value-of", testNamespace);
                XmlAttribute select = xdoc.CreateAttribute("select");
                select.AppendChild(xdoc.CreateTextNode(x.InnerText));
                valueof.Attributes.Append(select);
                arg.AppendChild(valueof);

                functionCodeXml.AppendChild(arg);

                //update outputs by new variable name
                foreach (XmlNode o in outputs.ChildNodes)
                {
                    //replace the name of new varibale
                    if (o.InnerText.Contains(x.Name))
                    {
                        string outputOperation = o.InnerText;
                        outputOperation = outputOperation.Replace(x.Name, "$" + x.Name);

                        o.RemoveAll();
                        o.AppendChild(o.OwnerDocument.CreateTextNode(outputOperation));
                    }
                }

            }

            foreach (XmlNode x in outputs.ChildNodes)
            {
                XmlElement output = xdoc.CreateElement(prefix, "variable", testNamespace);
                XmlAttribute name = xdoc.CreateAttribute("name");
                name.AppendChild(xdoc.CreateTextNode(x.Name));
                output.Attributes.Append(name);

                XmlNode valueof = xdoc.CreateElement(prefix, "value-of", testNamespace);
                XmlAttribute select = xdoc.CreateAttribute("select");
                select.AppendChild(xdoc.CreateTextNode(x.InnerText));
                valueof.Attributes.Append(select);

                output.AppendChild(valueof);

                functionCodeXml.AppendChild(output);
            }

            //MessageBox.Show(printer.PrintToString(functionCodeXml));

            return functionCodeXml;
        }

        /// <summary>
        /// For reverse operation, the operation must know where the forward template puts the result for the 
        /// reverse can fetch that data later on
        /// </summary>
        /// <param name="outputString">the output element of function (drag data)</param>
        /// <param name="target">Place in the target model (drop position)</param>
        public void updateOutput(string outputString, string target)
        {
            string inputString = "inputs/arg" + outputString.Substring(6); //cut "output" from its begining and add the rest to input
            //MessageBox.Show("input: " + inputString + " target: " + target);

            XmlNode inputNode = DataR.SelectSingleNode(inputString);
            inputNode.RemoveAll();

            inputNode.AppendChild(inputNode.OwnerDocument.CreateTextNode(target));

            //MessageBox.Show(DataR.OuterXml);
        }

        #endregion //code generation


        #region clone

        public VisualFunction Clone()
        {
            VisualFunction vf = new VisualFunction(this.functionXml.Clone() as XmlNode);

            return vf;
        }

        #endregion

    }

    public class VisualFunctionElementDragObject
    {
        public VisualFunction function;
        public Element element;
    }
}
