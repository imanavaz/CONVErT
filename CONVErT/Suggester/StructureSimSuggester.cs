using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace CONVErT
{
    public class StructureSimSuggester : ISuggester
    {

        #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties
        
        #region ctor

        public StructureSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public StructureSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        #endregion //ctor

        #region getSimilarity

        /// <summary>
        ///  Get similarities based on number of neighbours. Use abstract trees defined previously.
        /// </summary>
        /// <returns></returns>
        public double[,] getSimilarity()
        {

            if (sourceASTL != null && targetASTL != null)
                return getSimilarity(sourceASTL.abstractGraphNeighbourMatrix, targetASTL.abstractGraphNeighbourMatrix);
            else
            {
                MessageBox.Show("Abstract Trees are not defined yet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// get structural similarity based on number of neighbours
        /// </summary>
        /// <param name="neighborhoodMatrixSource"></param>
        /// <param name="neighborhoodMatrixTarget"></param>
        /// <returns></returns>
        public double[,] getSimilarity(double[,] neighborhoodMatrixSource, double[,] neighborhoodMatrixTarget)
        {
            double[,] sMat = new double[neighborhoodMatrixSource.GetLength(0), neighborhoodMatrixTarget.GetLength(0)];
            //int count = 0;

            for (int i = 0; i < sMat.GetLength(0); i++)
                for (int j = 0; j < sMat.GetLength(1); j++)
                {
                    //calculate outbound links
                    int sNO = getNeighbours(neighborhoodMatrixSource, i).Count;
                    int tNO = getNeighbours(neighborhoodMatrixTarget, j).Count;

                    double outbound = 0;
                    double inbound = 0;

                    if (i != 0)
                        sNO--;//minus 1 for inbound links (i know there is only one inbound link for nodes other than root)

                    if (j != 0)
                        tNO--;//minus 1 for inbound links 

                    if ((sNO == 0) && (tNO == 0))
                        sMat[i, j] = 1;
                    else if (Math.Max(sNO, tNO) != 0)
                    {
                        outbound = (double)Math.Min(sNO, tNO) / (double)Math.Max(sNO, tNO);
                        inbound = 1;// I know this
                        sMat[i, j] = (outbound + inbound) / 2;//average 
                    }
                    else
                        sMat[i, j] = 0;

                    //if (sMat[i, j] != 0)
                        //count++;
                    //MessageBox.Show("s: " + sNO.ToString() + " t: " + tNO.ToString() + " sMat : " + sMat[i, j].ToString());
                }

            return normaliseResults(sMat);
        }

        /// <summary>
        /// normalise similarity scores
        /// </summary>
        /// <param name="simM">double matrix of similarity scores</param>
        /// <returns>normalised scores</returns>
        private double[,] normaliseResults(double[,] scores)
        {
            double[,] simM = scores;

            double sum = 0;
            for (int i = 0; i < simM.GetLength(0); i++)
                for (int j = 0; j < simM.GetLength(1); j++)
                    if (simM[i, j] > 0)
                    {
                        sum += simM[i, j];//for normalisation
                    }

            //double cons = 1.0 / (count);
            double cons = 1.0 / (sum);
            simM = MatrixLibrary.Matrix.ScalarMultiply(cons, simM);

            return simM;

        }

        #endregion //getSimilarity



        private Collection<int> getNeighbours(double[,] nMat, int index)
        {

            Collection<int> results = new Collection<int>();

            for (int j = 0; j < nMat.GetLength(1); j++)
                if (nMat[index, j] > 0)
                    results.Add(j);


            return results;

        }
    }
}
