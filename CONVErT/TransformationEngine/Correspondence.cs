using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace CONVErT
{
    public class Correspondence
    {
        //Collection<CorrespondenceLink> _links = new Collection<CorrespondenceLink>();

        public static Collection<CorrespondenceLink> Links = new Collection<CorrespondenceLink> ();
        //{
        //    get { return _links; }
        //}

        private AbstractLattice _ATLHS;        
        private AbstractLattice _ATRHS;

        public AbstractLattice ASTLHS
        {
            get { return _ATLHS; }
        }

        public AbstractLattice ASTRHS
        {
            get { return _ATRHS; }
        }


        public Correspondence()
        {
            _ATLHS = new AbstractLattice();
            _ATRHS = new AbstractLattice();
        }

        /// <summary>
        /// Locates elements in both sides
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public void addCorrespondence(string lhs, string rhs)
        {

            AbstractTreeLatticeNode lnode = _ATLHS.getAbstractNodeAtAddress(lhs);
            AbstractTreeLatticeNode rnode = _ATRHS.getAbstractNodeAtAddress(rhs);

            if (lnode != null && rnode != null)
            {

                //for signature
                lnode.isMatched = true;
                rnode.isMatched = true;

                //CorrespondenceLink corr = new CorrespondenceLink(lhs, rhs);

                //_links.Add(corr);
            }
            else
                MessageBox.Show("Could not find addresses for the specified correspondence!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);


        }
         

        public string printLinks()
        {
            string ls = "";

            foreach (CorrespondenceLink l in Links)
                ls += l.getLinkString() + "\n";

            MessageBox.Show(ls);

            return ls;
        }

        public void optimiseLinks()
        {

            //clear previous processings
            //treeView4.Nodes.Clear();

            //process rules
            foreach (CorrespondenceLink l in Links)
            {
                
                char[] seps2 = { '\\', '/' };
                //String[] valuesL = l.Left.Split(seps2);
                //String[] valuesR = l.Right.Split(seps2);

                //_ATLHS.addConcept(valuesL, l.ID);//which tree view to add to, what is the rules, and what is rule index
                //_ATRHS.addConcept(valuesR, l.ID);
                
            }
            
        }

        public void clearLinks()
        {
            Links.Clear();
            //clear trees as well
        }

    }

    /*public class Link
    {
        public int ID { get; set; }

        public string Left { get; set; }

        public string Right { get; set; }
        
        public Link(string l, int id)
        {
            char[] seps = { '-' };
            String[] v = l.Split(seps);

            Left = v[0];
            Right = v[1];

            ID = id;
        }

        public string getLinkString()
        {
            return "Link " + ID.ToString() + " : " + Left + "-" + Right;
        }
    }*/
}
