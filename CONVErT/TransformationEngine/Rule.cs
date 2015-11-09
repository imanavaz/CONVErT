using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CONVErT
{
    public class Rule
    {
        #region properties

        private string _ruleID;
        public string ID { 
            get { return _ruleID; } 
            set { _ruleID = value; } 
        }

        private XmlNode _precond;
        public XmlNode PreCondition
        {
            get { return _precond;}
            set { _precond = value; }
        }

        private XmlNode _lhs;
        public XmlNode LHS 
        {
            get { return _lhs; }
            set { _lhs = value; } 
        }

        private XmlNode _targetData;
        public XmlNode TargetData
        {
            get { return _targetData; }
            set { _targetData = value; }
        }


        #endregion //properties

        #region ctor

        public Rule()
        {
            _ruleID = "";
            _precond = null;
            _lhs = null;
            _targetData = null;
        }

        #endregion //ctor
    }

    public class RuleSide 
    { 
        
    }
}
