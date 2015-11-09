using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CONVErT
{
    [Serializable]
    public class Element 
    {
        #region propetries

        private String name;
        private String evalue;
        private ElementType etype;

        public Element()
        {
             
        }

        public ElementType Type {
            get
            {
                return etype;
            }
            set
            {
                etype = value;
            }
        }

        public string Value
        {
            get
            {
                return evalue;
            }
            set
            {
                evalue = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }


        public string Address {get; set;}


        public bool IsPlaceHolder { get; set; }


        private List <Element> _children = new List <Element>();
        public List<Element> Children
        {
            get { return _children; }
        }

        #endregion

        /// <summary>
        /// Converts this element to an XML representation. Does not include children.
        /// </summary>
        /// <returns></returns>
        public XmlNode toXML()
        {
            XmlDocument xdoc = new XmlDocument ();

            XmlNode elementXML = xdoc.CreateElement("Element");

            XmlNode nameXML = xdoc.CreateElement("Name");
            nameXML.AppendChild(xdoc.CreateTextNode(Name));

            XmlNode addressXML = xdoc.CreateElement("Address");
            addressXML.AppendChild(xdoc.CreateTextNode(Address));

            XmlNode valueXML = xdoc.CreateElement("Value");
            addressXML.AppendChild(xdoc.CreateTextNode(Value));

            XmlNode typeXML = xdoc.CreateElement("Type");
            typeXML.AppendChild(xdoc.CreateTextNode(Type.ToString()));


            //forget children
            
            elementXML.AppendChild(nameXML);
            elementXML.AppendChild(addressXML);
            elementXML.AppendChild(valueXML); 
            elementXML.AppendChild(typeXML);
            
            return elementXML;
        }

        
    }
        

    [Serializable]
    public enum ElementType
    {
        ModelElement,
        ModelAttribute,
        InstanceValue
    }
    
}
