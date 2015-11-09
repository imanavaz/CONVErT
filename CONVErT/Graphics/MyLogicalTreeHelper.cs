using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Documents;

namespace CONVErT
{
    public class MyLogicalTreeHelper
    {
        string topElementName;
        DependencyObject parentWindow;


        public MyLogicalTreeHelper(string topEN, DependencyObject ownerWindow)
        {
            topElementName = topEN;
            parentWindow = ownerWindow;
        }

        public Collection<object> getVisualElementsByAddress(object obj, string addr)
        {
            Collection<object> finalResults = new Collection<object>();

            //process right address to get to the addresses
            string[] splitSt = { "/" };
            string[] rightStringArray = addr.Split(splitSt, StringSplitOptions.RemoveEmptyEntries);

            TraverseAndSave(rightStringArray, 0, obj, finalResults);

            return finalResults;
        }

        public void TraverseAndSave(string[] rightStrings, int index, object obj, Collection<object> results)
        {
            if (index >= rightStrings.Length)
                return;

            if (index == rightStrings.Length - 1)//the element we are looking for
            {
                Collection<object> r = getVisualElementsByName(obj, QualifiedNameString.Convert(rightStrings[index]));
                checkAndCombine(results, r,rightStrings);
                return;
            }

            if (index < rightStrings.Length - 1)//iterate 
            {
                Collection<object> r = getVisualElementsByName(obj, QualifiedNameString.Convert(rightStrings[index]));

                while ((r.Count == 0) && (index < rightStrings.Length - 1))
                {
                    index++;
                    r = getVisualElementsByName(obj, QualifiedNameString.Convert(rightStrings[index]));
                }

                if (index == rightStrings.Length - 1)
                {
                    checkAndCombine(results, r, rightStrings);
                    return;
                }

                foreach (var o in r)
                {
                    TraverseAndSave(rightStrings, index + 1, o, results);
                }

                return;
            }

        }

        public Collection<object> getVisualElementsByName(object obj, string name)
        {
            Collection<object> results = new Collection<object>();

            TraverseLogicalTree(results, obj, name);

            return results;
        }

        private void TraverseLogicalTree(Collection<object> results, Object obj, string name)
        {
            if (obj is DependencyObject)
            {
                var childern = LogicalTreeHelper.GetChildren(obj as DependencyObject);

                foreach (var child in childern)
                {
                    var temp = child;

                    //MessageBox.Show(temp.GetType().ToString());

                    if (temp is DependencyObject)
                    {
                        if (temp is System.Windows.Controls.UserControl)//for passing visual elements and going to their content
                        {
                            temp = (temp as System.Windows.Controls.UserControl).Content;
                        }

                        if ((temp as FrameworkElement).Name.Equals(name))
                        {
                            results.Add(temp);
                        }
                        else if ((temp as FrameworkElement) == null || string.IsNullOrEmpty((temp as FrameworkElement).Name))
                        {
                            TraverseLogicalTree(results, temp, name);
                        }
                    }

                }
            }
        }

        public void checkAndCombine(Collection<object> col1, Collection<object> col2, string[] rightStrings)
        {
            string targetAddress = "";
            
            foreach (string s in rightStrings)
                targetAddress = targetAddress + "/" + s;
            
            targetAddress = targetAddress.Substring(1);
            targetAddress = QualifiedNameString.Convert(targetAddress);

            foreach (object item in col2)
            {
                string elementAddress = findElementAddress(item as DependencyObject);
                elementAddress = QualifiedNameString.Convert(elementAddress);
               
                if ((!string.IsNullOrEmpty(elementAddress)) && (elementAddress.Equals(targetAddress)))
                    col1.Add(item);
            }
        }

        public string findElementAddress(DependencyObject obj)
        {
            string localAddr = "";
            string fullAddr = "";

            if (obj != null)
            {
                DependencyObject test2 = obj;//start from the element
                

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

                } while (test2 != null && !(test2 is VisualElement) && !((test2 as FrameworkElement).Name.Equals(topElementName)));

                if (test2 is VisualElement)
                {
                    string topPortionOfAddress = "";
                    AbstractTreeLatticeNode test2AbstractNode = (parentWindow as Mapper).sourceASTL.Root.findInChildrenByName((test2 as VisualElement).Data.Name);
                    if (test2AbstractNode != null && test2AbstractNode.parent != null)
                        topPortionOfAddress = test2AbstractNode.parent.Address;

                    if (!string.IsNullOrEmpty(topPortionOfAddress))
                        fullAddr = topPortionOfAddress + "/" + localAddr;
                    else
                        fullAddr = localAddr;
                }
            }

            return fullAddr;
        }


    }
}
