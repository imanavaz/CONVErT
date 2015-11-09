using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using DotNetMatrix;
using System.Runtime.Serialization.Formatters.Binary;

namespace CONVErT
{
    public class IsoRankSuggester : ISuggester
    {
        #region properties

        AbstractLattice sourceASTL;
        AbstractLattice targetASTL;

        int sourceCount = 0;
        int targetCount = 0;

        double[][] A;//needed for IsoRank calculation

        //double[,] structSimMatrix;
        double[,] simMatrix;

        // Create a log writer and open the file:
        StreamWriter log;
        string startTime;
        string endTime;

        #endregion properties
        
        #region ctor

        public IsoRankSuggester()
        {
            sourceASTL = null;
            targetASTL = null;
        }
        
        public IsoRankSuggester(AbstractLattice sASTL, AbstractLattice tASTL)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;
        }

        public IsoRankSuggester(String sourceFile, String targetFile)
        {
            //reading files to abstract trees
            sourceASTL = new AbstractLattice(sourceFile);
            targetASTL = new AbstractLattice(targetFile);
        }

        #endregion ctor

        #region IsoRank computation

        public double[,] getSimilarity()
        {
            // record start time
            startTime = ("\n\n================\n Iso Rank started at: " + DateTime.Now+"\n==================\n\n");
            
            NameSimSuggester nsSugg = new NameSimSuggester(sourceASTL, targetASTL);
            //TypeSimSuggester tsSugg = new TypeSimSuggester(sourceASTL, targetASTL);
            simMatrix = nsSugg.getSimilarity();

            //StructureSimSuggester nsSugg = new StructureSimSuggester(sourceASTL, targetASTL);
            //simMatrix = nsSugg.getSimilarity();

            calulateA();

            double[][] temp = calculateR(); //calculate R using principal eigenvalue and eigenvector

            double[,] result = new double[temp.GetLength(0), 1];

            for (int i = 0; i < temp.GetLength(0); i++)
                result[i, 0] = temp[i][0];

            result = vector2Matrix(result, 0);
            
            
            //result = findAndPrepareMatches(result); //ommitted since the new suggester already does the stable marriage is desired
            
            //================ Greedy Matching ==============
            //findAndPrepareMatches(result);
                        
            //record end time
            endTime = "\n===========\nfinished at: "+DateTime.Now+"\n===========\n";
            
            return result;
        }

        private void calulateA()
        {
            sourceCount = sourceASTL.abstractLatticeGraph.VertexCount;
            targetCount = targetASTL.abstractLatticeGraph.VertexCount;
            long size = sourceCount * targetCount;

            A = new double[size][];
            for (int i = 0; i < size; i++)
            {
                A[i] = new double[size];
            }

            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                {
                    int i = row / targetCount;
                    int j = row % targetCount;

                    int u = col / targetCount;
                    int v = col % targetCount;

                    if (((sourceASTL.abstractGraphNeighbourMatrix[i, u] != 0))// || (neighborhoodMatrixSource[u, i] != 0))
                        && ((targetASTL.abstractGraphNeighbourMatrix[j, v] != 0)))//||(neighborhoodMatrixTarget[v,j] != 0)))
                    {
                        double temp = (getNeighbours(sourceASTL.abstractGraphNeighbourMatrix, u).Count * getNeighbours(targetASTL.abstractGraphNeighbourMatrix, v).Count);
                        A[row][col] = 1.0 / temp;
                    }
                    else
                        A[row][col] = 0;
                }
        }

        private Collection<int> getNeighbours(double[,] nMat, int index)
        {

            Collection<int> results = new Collection<int>();

            for (int j = 0; j < nMat.GetLength(1); j++)
                if (nMat[index, j] > 0)
                    results.Add(j);


            return results;

        }

        private double[][] calculateR()
        {
            sourceCount = sourceASTL.abstractLatticeGraph.VertexCount;
            targetCount = targetASTL.abstractLatticeGraph.VertexCount;
            
            GeneralMatrix newR = new GeneralMatrix(sourceCount * targetCount, 1);
            GeneralMatrix myA = new GeneralMatrix(A);

            //initial R
            for (int i = 0; i < newR.RowDimension; i++)
            {
                newR.SetElement(i, 0, 1.0 / newR.RowDimension);
            }

            //printMatrix(vector2Matrix(newR.Array, 0));

            //move similarity matrix to a double index vector
            GeneralMatrix newSim = new GeneralMatrix(sourceCount * targetCount, 1);

            for (int i = 0; i < sourceCount; i++)
                for (int j = 0; j < targetCount; j++)
                {
                    newSim.SetElement(targetCount * i + j, 0, simMatrix[i, j]);
                }


            //move structure similarity matrix to a double index vector
            GeneralMatrix newStructSim = new GeneralMatrix(sourceCount * targetCount, 1);

            //for (int i = 0; i < sourceCount; i++)
            //    for (int j = 0; j < targetCount; j++)
            //    {
           //         newStructSim.SetElement(targetCount * i + j, 0, structSimMatrix[i, j]);
            //    }

            //calculate R using power method (eigen vector)
            //==========================================
            
            int count = 0;
            while (count < 50)
            {
                //R = aAR + (1-2a)E1 + (1-2a)E2 where a = 0.333
                //newR = (((myA.Multiply(newR)).Multiply(0.333)).Add(newStructSim.Multiply(0.333))).Add(newSim.Multiply(0.333));//ommited to have name similarity only 17/4/2012
                newR = (((myA.Multiply(newR)).Multiply(0.5)).Add(newSim.Multiply(0.5)));

                double sum = 0;
                for (int i = 0; i < newR.RowDimension; i++)
                    for (int j = 0; j < newR.ColumnDimension; j++)
                        sum += newR.GetElement(i, j);

                newR = newR.Multiply(1.0 / sum);

                count++;
            }

            return newR.Array;
        }

        private double[,] vector2Matrix(double[,] m, int index)
        {
            double[,] temp = new double[sourceCount, targetCount];

            for (int i = 0; i < m.GetLength(0); i++)
            {
                //if (m[i, index] >= dynamicThreshold)
                if (Math.Abs(m[i, index]) > 0)
                {
                    //process index i
                    int iIndex = i / targetCount;
                    int jIndex = i % targetCount;

                    temp[iIndex, jIndex] = m[i, index];
                }
            }

            return temp;
        }

        public double[,] findAndPrepareMatches(double [,] result)
        {
            double[,] matchMatrix = new double[result.GetLength(0), result.GetLength(1)];

            Queue<int> sourceNodes = new Queue<int>();//push source elements into a queue
            
            for (int i = 0; i < result.GetLength(0); i++)
                sourceNodes.Enqueue(i);

            while (sourceNodes.Count > 0)//repeat till you have found a match for every node
            {
                int i = sourceNodes.Dequeue(); //get a graph vertex index from source elements

                double[,] result2 = (double[,])GetCopy(result);//copy by value

                int match = -1;//match for i
                bool matchExists = true;

                while ((match == -1) && (matchExists))// a match has not been found and it exists
                {
                    double max = -10000;
                    //find match with maximum value
                    for (int j = 0; j < result2.GetLength(1); j++)
                    {
                        if ((result2[i, j] > max) && (Math.Abs(result2[i, j]) > 0.0001))//
                        {
                            max = result2[i, j];
                            match = j;
                        }
                    }
                    if (match == -1)//there exists no match for this element!
                    {
                        matchExists = false;
                        break;//breake the while and move on to next node
                    }
                    else
                        matchExists = true;
                    
                    //by now we have found a "match" and it has maximum similarity value

                    //check if match has not been matched before
                    bool check = false;
                    int c = -1;
                    
                    for (c = 0; c < matchMatrix.GetLength(0); c++)
                        if (matchMatrix[c, match] == 1)
                        {
                            check = true;
                            break;
                        }

                    if (check != true)//match has not been used before 
                    {
                        matchMatrix[i, match] = 1; //make a match with i -> match
                    }
                    else if ((check == true) && (result2[i, match] > result2[c, match]))//match has been used in another mapping but the similarity value here is higher
                    {
                        sourceNodes.Enqueue(c);
                        matchMatrix[c, match] = 0;
                        matchMatrix[i, match] = 1;
                    }
                    else
                    {
                        result2[i, match] = -100000; //not to be used in the process again -> go for second max similarity value
                        //MessageBox.Show(result[i, match].ToString());
                        match = -1;

                    }
                }


            }
            
            
            //for logging and testing
            //******************
            if (!File.Exists("matchfile.txt"))
            {
                log = new StreamWriter("matchfile.txt");
            }
            else
            {
                log = File.AppendText("matchfile.txt");
            }
            log.WriteLine(startTime);

            //canvasMatches.Children.Clear();
            //print matches
            int count = 0;
            for (int i = 0; i < matchMatrix.GetLength(0); i++)
                for (int j = 0; j < matchMatrix.GetLength(1); j++)
                    if (matchMatrix[i, j] == 1)
                    {
            //            //MessageBox.Show(i.ToString() + "    " + j.ToString());
            //            prepareLine(gSource.Vertices.ElementAt(i), gTarget.Vertices.ElementAt(j));
                        log.WriteLine(sourceASTL.abstractLatticeGraph.Vertices. ElementAt(i).Address + " -> " + targetASTL.abstractLatticeGraph.Vertices.ElementAt(j).Address);
                        log.WriteLine();
                        count++;
                    }

            MessageBox.Show(count.ToString() + " matches have been presented using abstract representation!");
            
            log.WriteLine(endTime);
            //Close the stream:
            log.Close();
            
            return matchMatrix;
        }

        ///get a copy of the objects (by value)
        private static object GetCopy(object input)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }

        #endregion
    }
}
