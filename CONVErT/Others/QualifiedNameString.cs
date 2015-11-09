using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CONVErT
{
    public static class QualifiedNameString
    {
        public static string Convert(string input)
        {
            string str = input.Clone() as string;

            str = str.Replace('-', '_');
            str = str.Replace(' ', '_');

            return str;
        }

    }
}
