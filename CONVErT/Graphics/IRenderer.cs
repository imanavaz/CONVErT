using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CONVErT
{
    interface IRenderer
    {
        UIElement render(string modelFile);

        UIElement render(XmlNode xNode);

        UIElement createVisualisation(string fileName);

        UIElement createStillVisualisation(string fileName);

    }


    public enum VisualisationType
    {
        XAML,//1-to-1 relation to VisTypeCOmboBox in Skin 
        SVG
    }
}
