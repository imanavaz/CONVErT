using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CONVErT
{
    public class NameSimSuggester : ISuggester
    {
        #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties

        #region ctor

        public NameSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public NameSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
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

            for (int i = 0; i < gSource.VertexCount; i++)
                for (int j = 0; j < gTarget.VertexCount; j++)
                {
                    simM[i, j] = checkNames(gSource.Vertices.ElementAt(i) as AbstractTreeLatticeNode, gTarget.Vertices.ElementAt(j) as AbstractTreeLatticeNode);
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
        private double checkNames(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            
            int heu = 0;

            if (s.Name.Equals(t.Name))
                heu++;
            //heu = 1;

            if (s.Name.ToLower().Contains(t.Name.ToLower()))
                heu++;
            //heu = 1;

            if (t.Name.ToLower().Contains(s.Name.ToLower()))
                heu++;
            //heu = 1;
            
            return heu;
        }

        #endregion //getSimilarity
    }

}
