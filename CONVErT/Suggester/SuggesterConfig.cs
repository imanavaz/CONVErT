using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CONVErT
{
    public class SuggesterConfig
    {
        #region properties

        public bool UseNameSimSuggester { get; set; }
        
        public bool UseValueSimSuggester { get; set; }

        public bool UseIsoRankSimSuggester { get; set; }
        
        public bool UseStructSimSuggester { get; set; }
        
        public bool UseTypeSimSuggester { get; set; }

        public bool UseNeighborSimSuggester { get; set; }

        public bool UseLevDistNameSimSuggester { get; set; }

        #endregion //properties

        #region ctor

        /// <summary>
        /// By default all suggesters are set to be used
        /// </summary>
        public SuggesterConfig()
        {
            UseIsoRankSimSuggester = true;
            UseValueSimSuggester = true;
            UseTypeSimSuggester = true;
            UseStructSimSuggester = true;
            UseNameSimSuggester = true;
            UseNeighborSimSuggester = true;
            UseLevDistNameSimSuggester = true;
        }

        public SuggesterConfig(bool useIsoRankSimSuggester, bool useValueSimSuggester, bool useTypeSimSuggester, bool useStructSimSuggester, bool useNameSimSuggester, 
                                bool useNeighborSimSuggester, bool useLevDistNameSimSuggester)
        {
            UseIsoRankSimSuggester = useIsoRankSimSuggester;
            UseValueSimSuggester = useValueSimSuggester;
            UseTypeSimSuggester = useTypeSimSuggester;
            UseStructSimSuggester = useStructSimSuggester;
            UseNameSimSuggester = useNameSimSuggester;
            UseNeighborSimSuggester = useNeighborSimSuggester;
            UseLevDistNameSimSuggester = useLevDistNameSimSuggester;
        }

        #endregion //ctor

        #region toString

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Suggestion Configuration : \n\n");

            sb.Append("UseIsoRankSimSuggester = " + UseIsoRankSimSuggester);
            sb.Append("\n");
            sb.Append("UseValueSimSuggester = " + UseValueSimSuggester);
            sb.Append("\n");
            sb.Append("UseTypeSimSuggester = " + UseTypeSimSuggester);
            sb.Append("\n");
            sb.Append("UseStructSimSuggester = " + UseStructSimSuggester);
            sb.Append("\n");
            sb.Append("UseNameSimSuggester = " + UseNameSimSuggester);
            sb.AppendLine();
            sb.Append("UseNeighborSimSuggester = " + UseNeighborSimSuggester);
            sb.AppendLine();
            sb.Append("UseLevDistNameSimSuggester = " + UseLevDistNameSimSuggester);
            sb.AppendLine();
            sb.Append("\n");

            
            return sb.ToString();
        }

        public XmlNode toXML()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNode config = xdoc.CreateElement("Configuration");

            XmlNode temp1 = xdoc.CreateElement("UseIsoRankSimSuggester");
            temp1.AppendChild(xdoc.CreateTextNode((UseIsoRankSimSuggester?1:0).ToString()));
            config.AppendChild(temp1);

            XmlNode temp2 = xdoc.CreateElement("UseValueSimSuggester");
            temp2.AppendChild(xdoc.CreateTextNode((UseValueSimSuggester?1:0).ToString()));
            config.AppendChild(temp2);

            XmlNode temp3 = xdoc.CreateElement("UseTypeSimSuggester");
            temp3.AppendChild(xdoc.CreateTextNode((UseTypeSimSuggester?1:0).ToString()));
            config.AppendChild(temp3);

            XmlNode temp4 = xdoc.CreateElement("UseStructSimSuggester");
            temp4.AppendChild(xdoc.CreateTextNode((UseStructSimSuggester?1:0).ToString()));
            config.AppendChild(temp4);

            XmlNode temp5 = xdoc.CreateElement("UseNameSimSuggester");
            temp5.AppendChild(xdoc.CreateTextNode((UseNameSimSuggester?1:0).ToString()));
            config.AppendChild(temp5);

            XmlNode temp6 = xdoc.CreateElement("UseNeighborSimSuggester");
            temp6.AppendChild(xdoc.CreateTextNode((UseNeighborSimSuggester ? 1 : 0).ToString()));
            config.AppendChild(temp6);

            XmlNode temp7 = xdoc.CreateElement("UseLevDistNameSimSuggester");
            temp7.AppendChild(xdoc.CreateTextNode((UseLevDistNameSimSuggester ? 1 : 0).ToString()));
            config.AppendChild(temp7);

            return config;
        }

        public SuggesterConfig Clone ()
        {
            SuggesterConfig temp = new SuggesterConfig();

            temp.UseIsoRankSimSuggester = this.UseIsoRankSimSuggester;
            temp.UseValueSimSuggester = this.UseValueSimSuggester;
            temp.UseTypeSimSuggester = this.UseTypeSimSuggester;
            temp.UseStructSimSuggester = this.UseStructSimSuggester;
            temp.UseNameSimSuggester = this.UseNameSimSuggester;
            temp.UseNeighborSimSuggester = this.UseNeighborSimSuggester;
            temp.UseLevDistNameSimSuggester = this.UseLevDistNameSimSuggester;

            return temp;
        }

        #endregion //tostring
    }
}
