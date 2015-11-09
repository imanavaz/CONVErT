using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CONVErT
{
    public static class DirectoryHelper
    {
        
        public static string getFilePathExecutingAssembly(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            string path = System.IO.Path.Combine(pbase, c).Replace("file:\\", "");
            return path;
        }

        public static string getFilePathCallingAssembly(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            string path = System.IO.Path.Combine(pbase, c).Replace("file:\\", "");
            return path;
        }

        public static string getFilePathAssembly(string c, Type t)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(t).GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            string path = System.IO.Path.Combine(pbase, c).Replace("file:\\", "");
            return path;
        }

    }
}
