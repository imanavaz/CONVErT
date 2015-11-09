using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CONVErT
{
    public class TypeSimSuggester : ISuggester
    {
         #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties

        #region ctor

        public TypeSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public TypeSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        #endregion //ctor

        #region getSimilarity

        /// <summary>
        ///  Get similarities by checking types. Use abstract trees and refactored types.
        /// </summary>
        /// <returns></returns>
        public double[,] getSimilarity()
        {
            return getSimilarity(sourceASTL.abstractLatticeGraph, targetASTL.abstractLatticeGraph);
        }

        /// <summary>
        ///  Get similarities by checking types. Use the graph of abstract trees and refactored types.
        /// </summary>
        /// <param name="gSource">Source graph</param>
        /// <param name="gTarget">Target graph</param>
        /// <returns></returns>
        public double[,] getSimilarity(BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gSource, BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gTarget)
        {
       
            double[,] simM = new double[gSource.VertexCount, gTarget.VertexCount];

            //int count = 0;

            for (int i = 0; i < gSource.VertexCount; i++)
                for (int j = 0; j < gTarget.VertexCount; j++)
                {
                    simM[i, j] = checkTypes(gSource.Vertices.ElementAt(i) as AbstractTreeLatticeNode, gTarget.Vertices.ElementAt(j) as AbstractTreeLatticeNode);

                    //if (simM[i, j] != 0)
                       //count++;//keep it for normalisation
                }

            //normalise similarity ratings
            //double cons = 1.0 / (count);
            //simM = MatrixLibrary.Matrix.ScalarMultiply(cons, simM);

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
        /// Check all node types against each other
        /// </summary>
        /// <param name="s">Source Abstract tree lattice node</param>
        /// <param name="t">Target abstract tree lattice node</param>
        /// <returns></returns>
        private double checkTypes(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            if (string.IsNullOrEmpty(s.ValueType) || string.IsNullOrEmpty(t.ValueType))
                return 0;

            if (s.ValueType.Equals(t.ValueType))
                return 1;

            return 0;
        }

        #endregion //getSimilarity
    }
}
