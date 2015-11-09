using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Windows;

namespace CONVErT
{
    public class CSV2XMLConverter
    {
        #region props


        #endregion //props


        #region Ctor

        public CSV2XMLConverter()
        {

        }

        #endregion //Ctor


        #region converter

        public void ConvertToXML(string CSVFile, string XMLoutputFile)
        {

            //Read CSV file
            using (CsvReader csvReader = new CsvReader(new StreamReader(CSVFile), true))
            {
                int fieldCount = csvReader.FieldCount;

                //MessageBox.Show("Field count : " + fieldCount.ToString());

                //read headers
                string[] headers = csvReader.GetFieldHeaders();

                XmlWriter writer = XmlWriter.Create(XMLoutputFile);

                try
                {
                    using (writer)
                    {

                        writer.WriteStartDocument();

                        writer.WriteStartElement("Root");

                        //read the rest of the file
                        while (csvReader.ReadNextRecord())
                        {
                            //create data element
                            writer.WriteStartElement("Data");

                            //reade each row
                            for (int i = 0; i < fieldCount; i++)
                            {
                                //check the value for illigal XML characters
                                StringBuilder buffer = new StringBuilder(csvReader[i].Length);
                                foreach (char c in csvReader[i])
                                {
                                    if (IsLegalXmlChar(c))
                                    {
                                        buffer.Append(c);
                                    }
                                }

                                writer.WriteElementString(headers[i], buffer.ToString());

                            }

                            //write end data element
                            writer.WriteEndElement();
                        }

                        //write end root element
                        writer.WriteEndElement();

                        //write end document
                        writer.WriteEndDocument();
                    }
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        XmlPrettyPrinter printer = new XmlPrettyPrinter();
                        printer.PrintXmlFile(XMLoutputFile);
                    }
                }
            }

            //return csvDoc;
        }

        private string[] separateRowFields(string csvRowString, string[] separatorField)
        {
            //split the row
            string[] rowFields = csvRowString.Split(separatorField, StringSplitOptions.RemoveEmptyEntries);

            if (rowFields != null)
                return rowFields;
            else
                throw new ArgumentNullException();
        }


        private bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }


        #endregion //converter
    }
}
