using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CONVErT
{
    public class CorrespondenceLink
    {

        public int ID { get; set; }

        public CorrespondenceNode LHS { get; set; }

        public CorrespondenceNode RHS { get; set; }

        public CorrespondenceLink(string rhs, string lhs, int id)
        {
            LHS = new CorrespondenceNode(lhs, id);
            RHS = new CorrespondenceNode (rhs, id);

            ID = id;
        }

        public CorrespondenceLink()
        {
            LHS = new CorrespondenceNode ();
            RHS = new CorrespondenceNode ();

            ID = -1;
        }

        public string getLinkString()
        {
            return "Link " + ID.ToString() + " : " + LHS.Address + " < - > " + RHS.Address;
        }
    }
}
