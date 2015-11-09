using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using QuickGraph;

namespace CONVErT
{
    public class LevDistNameSimSuggester : ISuggester
    {
        #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        #endregion //properties

        #region ctor

        public LevDistNameSimSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }

        public LevDistNameSimSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        #endregion //ctor

        #region getSimilarity

        /// <summary>
        ///  Get similarities by checking node tags using levenshtein Distance. Use abstract trees to limit the results.
        ///  NEEDS TO REORDER SCORES, CURRENTLY DIGITISING WHICH IS NOT A  GOOD WAY, AS SCORES HAVE FASSY ACCEPTANCE RATES AND NOT 1-0
        /// </summary>
        /// <returns></returns>
        public double[,] getSimilarity()
        {
            return getSimilarity(sourceASTL.abstractLatticeGraph, targetASTL.abstractLatticeGraph);
        }

        /// <summary>
        ///  Get similarities by checking node tags using levenshtein Distance. 
        /// </summary>
        /// <param name="gSource"></param>
        /// <param name="gTarget"></param>
        /// <returns></returns>
        public double[,] getSimilarity(BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gSource, BidirectionalGraph<AbstractTreeLatticeNode, Edge<AbstractTreeLatticeNode>> gTarget)
        {
            double[,] simM = new double[gSource.VertexCount, gTarget.VertexCount];

            for (int i = 0; i < gSource.VertexCount; i++)
                for (int j = 0; j < gTarget.VertexCount; j++)
                {
                    simM[i, j] = checkNames(gSource.Vertices.ElementAt(i) as AbstractTreeLatticeNode, gTarget.Vertices.ElementAt(j) as AbstractTreeLatticeNode);
                }

            simM = reverseResults(simM);

            return normaliseResults(simM);
        }

        /// <summary>
        /// digitise similarity scores by calculating average scores and selecting less than average scores (assigning 1 to them).
        /// </summary>
        /// <param name="simM">double matrix of similarity scores</param>
        /// <returns>digitised scores</returns>
        private double[,] digitiseResults(double[,] scores)
        {
            double[,] simM = new double[scores.GetLength(0), scores.GetLength(1)];

            double count = scores.GetLength(0) * scores.GetLength(1);

            double sum = 0;
            for (int i = 0; i < scores.GetLength(0); i++)
                for (int j = 0; j < scores.GetLength(1); j++)
                    sum += scores[i, j];//for average calculation


            double avg = sum / (count);

            //if values less than average mark as suggestion
            for (int i = 0; i < simM.GetLength(0); i++)
                for (int j = 0; j < simM.GetLength(1); j++)
                    if ((scores[i, j] < avg) && (scores[i, j] >= 0))
                    {
                        simM[i, j] = 1;
                    }
            
            return simM;
            
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
        /// Reverse order of scores. Higest levenshtein distance is the lowest score.
        /// </summary>
        /// <param name="simM">double matrix of similarity scores</param>
        /// <returns>reversed scores</returns>
        private double[,] reverseResults(double[,] scores)
        {
            double[,] simM = new double[scores.GetLength(0), scores.GetLength(1)];

            double maxScore = -1;

            //find max distance
            for (int i = 0; i < scores.GetLength(0); i++)
                for (int j = 0; j < scores.GetLength(1); j++)
                    if (scores[i, j] > maxScore)
                    {
                        maxScore = scores[i, j];
                    }

            maxScore += 1; //incerement for best score (distance of 0);

            //reverse scores
            for (int i = 0; i < simM.GetLength(0); i++)
                for (int j = 0; j < simM.GetLength(1); j++)
                    simM[i, j] = maxScore - scores[i, j];

            return simM;

        }

        /// <summary>
        /// Check two nodes against each other. 
        /// </summary>
        /// <param name="s">Source Abstract tree lattice node</param>
        /// <param name="t">Target abstract tree lattice node</param>
        /// <returns></returns>
        private double checkNames(AbstractTreeLatticeNode s, AbstractTreeLatticeNode t)
        {
            return computeLevenshteinDistance1(s.Name.ToLower(), t.Name.ToLower());
            
            //watch = Stopwatch.StartNew();
            //double d2 = computeLevenshteinDistance2(s.Name.ToLower(), t.Name.ToLower());
            //watch.Stop();
            //var elapsedMs2 = watch.ElapsedMilliseconds;
            
            //System.Windows.MessageBox.Show("S: "+s.Name.ToLower() + " T: "+t.Name.ToLower() + 
            //                                "\nD1: " + d1.ToString() + " D2: " + d2.ToString()
            //                                +"Time1: "+elapsedMs1+" Time2: "+elapsedMs2);
           
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        private double computeLevenshteinDistance1(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) 
                    return 0;
                
                return target.Length;
            }

            if (String.IsNullOrEmpty(target)) 
                return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                                distance[previousRow, j] + 1,
                                distance[currentRow, j - 1] + 1),
                                distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        private double computeLevenshteinDistance2(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        #endregion //getSimilarity
    }

}
