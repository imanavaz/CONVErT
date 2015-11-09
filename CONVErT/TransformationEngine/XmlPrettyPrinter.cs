using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace CONVErT
{
    public class XmlPrettyPrinter
    {
        public XmlPrettyPrinter()
        {
                
        }

        public string PrintToString(XmlNode myXmlNode)
        {
            // write the step node to a stream so that we can get the nice indented formatting for free
            XmlNodeReader xnr = new XmlNodeReader(myXmlNode);
            MemoryStream myStream = new MemoryStream();
            XmlTextWriter xtw = new XmlTextWriter(myStream, System.Text.Encoding.UTF8);
            xtw.Formatting = Formatting.Indented;
            xtw.WriteNode(xnr, true);
            xtw.Flush();
            myStream.Position = 0;
            StreamReader sr = new StreamReader(myStream, System.Text.Encoding.UTF8);

            // copy the new xml to the other box
            string myTabbedXmlString = sr.ReadToEnd();

            // clean up
            sr.Close();
            xtw = null;
            myStream.Close();
            xnr.Close();

            return myTabbedXmlString;
        }

        public string PrintToString(string XmlString)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(XmlString);

            return PrintToString(xDoc.DocumentElement);
        }

        public string PrintXmlFileToString(string XmlFileName)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(XmlFileName);

            return PrintToString(xDoc.DocumentElement);
        }

        //public XmlNode PrintToXmlNode(string XmlString)
        //{
        //    XmlDocument xDoc = new XmlDocument();
        //    xDoc.LoadXml(PrintToString(XmlString));

        //    return xDoc.DocumentElement;
        //}

        /// <summary>
        /// Pretty prints an XMl file and saves it in itself
        /// </summary>
        /// <param name="XmlFileName"></param>
        /// <returns></returns>
        public void PrintXmlFile(string XmlFileName)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(XmlFileName);

                xDoc.LoadXml(PrintToString(xDoc.DocumentElement));
                xDoc.Save(XmlFileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ExceptionHappend: " + ex.ToString());
            }
        }
    }
}
