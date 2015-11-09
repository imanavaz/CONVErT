using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CONVErT
{
    public class CorrespondenceNode
    {
        public int ID { get; set; }

        private string _address;
        public string Address { 
            get {return _address; }
        }

        public CorrespondenceNode()
        {
            _address = "";
            ID = -1;
        }

        public CorrespondenceNode(String add, int id)
        {
            _address = add;
            ID = id;
        }

        public void updateAddressAddParent(string parentAddress)
        {
            if (!(parentAddress.EndsWith("/")))
                parentAddress = parentAddress + "/";
           
            _address = parentAddress + _address;
        }

        /// <summary>
        /// Breaks an addressing string to its correspondng elements and returns the array of strings
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string [] getAddressAsStringArray (){

            string str = this.Address;

            char[] seps = { '/' , '\\'};
            String[] v = str.Split(seps);

            return v;
        }
    }
}
