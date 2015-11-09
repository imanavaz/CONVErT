using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace CONVErT
{
    public class XMLHelper
    {
        public XMLHelper()
        {

        }

        public string getAdressIncludingSelf(XmlNode xnode)
        {
            string address = findAddress(xnode);

            if (address.StartsWith("#document"))
                address = address.Substring(10);

            return address;
        }

        private string findAddress(XmlNode xnode)
        {
            if (xnode.ParentNode == null)
                return xnode.Name;
            else if (xnode.NodeType == XmlNodeType.Attribute)
            {
                return findAddress(xnode.ParentNode) + @"/@" + xnode.Name;
            }
            else if (xnode.NodeType == XmlNodeType.Element)
            {
                return findAddress(xnode.ParentNode) + "/" + xnode.Name;
            }
            else if (xnode.NodeType == XmlNodeType.Text)
                return findAddress(xnode.ParentNode);
            else
                return null;
        }

    }
}
