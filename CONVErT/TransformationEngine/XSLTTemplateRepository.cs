using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CONVErT
{
    public class XSLTTemplateRepository
    {
        private ObservableCollection<XSLTTemplate> _templates = new ObservableCollection<XSLTTemplate>();
        public ObservableCollection<XSLTTemplate> templates {
            get { return _templates; }
            set { _templates = value; }
        }

        public XSLTTemplate findTemplateByName(string name)
        {
            foreach (XSLTTemplate tem in templates)
                if (tem.TemplateName.Equals(name))
                    return tem;

            return null;
        }

        public XSLTTemplate findTemplateByAddressID(string name)
        {
            foreach (XSLTTemplate tem in templates)
                if (tem.TemplateAddress.Equals(name))
                    return tem;

            return null;
        }
    }
}
