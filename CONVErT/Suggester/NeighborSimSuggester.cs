using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CONVErT
{
    public class NeighborSimSuggester : ISuggester
    {
         #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties


        #region ctor

        public NeighborSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public NeighborSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        #endregion //ctor


        #region get similarity

        public double[,] getSimilarity()
        {
            if (sourceASTL == null || targetASTL == null)
                return null;

            double [,] result = new double[sourceASTL.abstractLatticeGraph.VertexCount, targetASTL.abstractLatticeGraph.VertexCount];

            for (int i = 0; i < result.GetLength(0); i++)
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = checkNeighbors(i, j);
                }

            return normaliseResults(result);
        }

        private double checkNeighbors(int sourceElementIndex, int targetElementIndex)
        {
            Collection<int> sourceNeighbors = getNeighbours(sourceASTL.abstractGraphNeighbourMatrix, sourceElementIndex);
            Collection<int> targetNeighbors = getNeighbours(targetASTL.abstractGraphNeighbourMatrix, targetElementIndex);
            
            //sourceNeighbors.Add(sourceElementIndex);//add themselves as well to be compared //commented
            //targetNeighbors.Add(targetElementIndex);

            double score = 0;

            foreach (int s in sourceNeighbors)
                foreach(int t in targetNeighbors)
                    score += checkNames(sourceASTL.abstractLatticeGraph.Vertices.ElementAt(s) as AbstractTreeLatticeNode, targetASTL.abstractLatticeGraph.Vertices.ElementAt(t) as AbstractTreeLatticeNode);
                    //score += checkTypes(sourceASTL.abstractLatticeGraph.Vertices.ElementAt(s) as AbstractTreeLatticeNode, targetASTL.abstractLatticeGraph.Vertices.ElementAt(t) as AbstractTreeLatticeNode);
            
            return score;
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

        private double checkNames(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            //return computeLevenshteinDistance(s.Name.ToLower(), t.Name.ToLower());

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

        private double checkTypes(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            if (string.IsNullOrEmpty(s.ValueType) || string.IsNullOrEmpty(t.ValueType))
                return 0;

            if (s.ValueType.Equals(t.ValueType))
                return 1;

            return 0;
        }

        private Collection<int> getNeighbours(double[,] nMat, int index)
        {

            Collection<int> results = new Collection<int>();

            for (int j = 0; j < nMat.GetLength(1); j++)
                if (nMat[index, j] > 0)
                    results.Add(j);


            return results;

        }

        #endregion //get similarity
    }
}
