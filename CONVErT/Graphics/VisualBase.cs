using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using System.Windows;
using System.ComponentModel;

namespace CONVErT
{
    public class VisualBase : UserControl, INotifyPropertyChanged
    {
        #region Properties

        public ObservableCollection<Element> elementList { get; set; }

        public bool showList { get; set; }

        ListBox MyItemsHost;

        //private XmlNode _data;
        public XmlNode Data { get; set; }//toDo

        public XmlNode DataR { get; set; }

        public Point? elementDragStartPoint;

        public Style elementStyle = new Style();

        public AbstractLattice abstractTree;

        private ParentWindowFinder pwFinder = new ParentWindowFinder();
        public DependencyObject ParentWindow
        {
            get
            {
                return pwFinder.getParentWindow(this);
            }
        }

        #endregion //Properties


        #region Ctor

        static VisualBase()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VisualBase), new FrameworkPropertyMetadata(typeof(VisualBase)));
        }

        public VisualBase()
        {            
            this.MouseRightButtonDown += new MouseButtonEventHandler(VisualBase_RightClick);

            elementList = new ObservableCollection<Element>();
        }

        #endregion //Ctor


        #region onApplyTemplate

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MyItemsHost = (ListBox)this.Template.FindName("ElementsListBox", this);

            EventSetter esDrop = new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ItemElement_Drop));// to be overridden in VisualElements
            EventSetter esMouseDown = new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ItemElement_PreviewMouseDown));
            EventSetter esMouseMove = new EventSetter(ListBoxItem.MouseMoveEvent, new MouseEventHandler(ItemElement_MouseMove));

            elementStyle.Setters.Add(esDrop);
            elementStyle.Setters.Add(esMouseDown);
            elementStyle.Setters.Add(esMouseMove);

            MyItemsHost.ItemContainerStyle = elementStyle;
        }

        #endregion //onApplyTemplate


        #region Click

        public virtual void VisualBase_RightClick(object sender, MouseButtonEventArgs e)
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
                    //MessageBox.Show(Data.OuterXml);
                    getElementListFromNotation(Data);
                }
                else
                    MessageBox.Show("Data is null");


                OnPropertyChanged("elementList");
                showList = true;
                OnPropertyChanged("showList");

            }

            e.Handled = true;

        }

        private void getElementListFromNotation(XmlNode notation)
        {

            if ((!notation.HasChildNodes) && (!String.IsNullOrEmpty(notation.Value)))
            {
                Element e = new Element();
                //e.Name = notation.ParentNode.Name;
                //e.Value = notation.InnerText;
                e.Value = notation.ParentNode.Name;
                
                ///this part is different in visualfucntion and visual element
                //e.Name = notation.InnerText; //visual element
                e.Name = notation.ParentNode.Name; //visual function                
                
                //e.Address = getRelativeAddress(Data,notation.Name);
                e.Address = getFullAddress(notation);
                elementList.Add(e);
            }
            else
                foreach (XmlNode x in notation.ChildNodes)
                    getElementListFromNotation(x);

        }

        #endregion


        #region Xml Addressing

        public string getFullAddress(XmlNode x)
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

        #endregion //Xml Addressing
        

        #region ListboxItem drag and drop

        public virtual void ItemElement_Drop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Drop performed on VisualBase base list item element");
        }

        private void ItemElement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            elementDragStartPoint = new Point?(e.GetPosition(this));
        }

        public virtual void ItemElement_MouseMove(object sender, MouseEventArgs e)
        {

            /*if (e.LeftButton != MouseButtonState.Pressed)
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

                        DataObject dataObject = new DataObject("VisualBaseListElement", ele);

                        if (dataObject != null)
                        {
                            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
                        }
                        e.Handled = true;
                    }
                }
            }*/
        }

        #endregion //listboxItem drag and drop


        #region utils
        
        public void ShowStatus(string p, ReportIcon reportIcon)
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
