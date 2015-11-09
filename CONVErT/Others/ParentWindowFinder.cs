using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CONVErT
{
    public class ParentWindowFinder
    {
        #region ctor

        public ParentWindowFinder()
        {

        }

        #endregion //ctor

        public DependencyObject getParentWindow(DependencyObject myObject)
        {
            DependencyObject PF = LogicalTreeHelper.GetParent(myObject);

            while ((PF != null) && (!(PF is Visualiser)) && (!(PF is Mapper)))
            {
                PF = LogicalTreeHelper.GetParent(PF);
            }

            return PF;
        }

    }
}
