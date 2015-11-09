using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CONVErT
{
    public class ValueSimSuggester : ISuggester
    {
        #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties

        #region ctor

        public ValueSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public ValueSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        #endregion //ctor

        #region getSimilarity

        /// <summary>
        ///  Get similarities by checking node actual values. Use abstract trees and defaule threshold value of 0.5 to limit the results.
        /// </summary>
        /// <returns></returns>
        public double[,] getSimilarity()
        {
            return getSimilarity(sourceASTL.abstractLatticeGraph, targetASTL.abstractLatticeGraph, 0.5);
        }

        /// <summary>
        ///  Get similarities by checking node actual values. Take default threshold 0.5 to limit results.
        /// </summary>
        /// <param name="gSource"></param>
        /// <param name="gTarget"></param>
        /// <returns></returns>
        public double[,] getSimilarity(BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gSource, BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gTarget)
        {
            return this.getSimilarity(gSource, gTarget, 0.5);
        }

        /// <summary>
        /// Get similarities by checking node actual values. Take threshold to limit results.
        /// </summary>
        /// <param name="gSource"></param>
        /// <param name="gTarget"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public double[,] getSimilarity(BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gSource, BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gTarget, double threshold)
        {
            double[,] simM = new double[gSource.VertexCount, gTarget.VertexCount];

            //int count = 0;

            for (int i = 0; i < gSource.VertexCount; i++)
                for (int j = 0; j < gTarget.VertexCount; j++)
                {
                    simM[i, j] = checkValues(gSource.Vertices.ElementAt(i) as AbstractTreeLatticeNode, gTarget.Vertices.ElementAt(j) as AbstractTreeLatticeNode);

                    //if (simM[i, j] != 0)
                        //count++;//keep it for normalisation
                }

            
            return normaliseResults(simM);
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

        /// <summary>
        /// Check all node values against each other
        /// </summary>
        /// <param name="s">Source Abstract tree lattice node</param>
        /// <param name="t">Target abstract tree lattice node</param>
        /// <returns></returns>
        private double checkValues(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            double heuTotal = 0;
            int count = 0;//for normalisation

            foreach (String sv in s.Values)
                foreach (String tv in t.Values)
                {
                    int heu = 0;
                    if (sv.Equals(tv))
                        heu++;
                    //heu = 1;

                    if (sv.ToLower().Contains(tv.ToLower()))
                        heu++;
                    //heu = 1;

                    if (tv.ToLower().Contains(sv.ToLower()))
                        heu++;
                    //heu = 1;

                    if (heu > 0)
                    {
                        count++;
                        heuTotal += heu; // / 3; //do not do local normalisation
                    }
                }

            if (count > 0)
                return heuTotal / count;//normalise the values
            else
                return 0;
        }

        #endregion //getSimilarity
    }

}
