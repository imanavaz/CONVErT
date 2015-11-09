using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CONVErT
{
    interface ISuggester
    {
        double[,] getSimilarity();

        //double[,] getSimilarity(double[,] neighborhoodMatrixSource, double[,] neighborhoodMatrixTarget);
    }
}
