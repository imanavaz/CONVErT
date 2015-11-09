using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.ObjectModel;
//using weka;
using System.Xml;

namespace CONVErT
{
    public class Suggester //created 1/09/2011
    {
        #region DB properties
        //private SqlConnection dbCon = new SqlConnection();

        //private SqlDataAdapter rulesElementsAdapter;
        //private SqlDataAdapter rulesAdapter;
        //private SqlDataAdapter LElementsAdapter;
        //private SqlDataAdapter RElementsAdapter;

        //private DataSet rulesElementsDS;
        //private DataSet LElementsDS;
        //private DataSet RElementsDS;
        //private DataSet rulesDS;

        //private SqlCommandBuilder cbRule;
        //private SqlCommandBuilder cbLeft;
        //private SqlCommandBuilder cbRight;

        #endregion


        #region Properties

        XmlDocument configDoc; //configuration XML
        
        SuggesterConfig suggesterConfig;
        public SuggesterConfig Configuration 
        {
            get { return suggesterConfig; }
            set { suggesterConfig = value; }
        }

        public double[,] _lastResults;
        public double[,] LastResults 
        {
            get
            {
                return _lastResults;
            }
            set
            {
                _lastResults = value;

                //whenever suggesters value changes, evaluate it and put results in file
                //SuggesterEvaluator eval = new SuggesterEvaluator(this);
                //eval.analyseResults();// no need, if bench,ark exists it does it automatically
            }
        }

        public AbstractLattice sourceASTL;
        public AbstractLattice targetASTL;

        IsoRankSuggester isoSuggester;
        NameSimSuggester nameSimSuggester;
        StructureSimSuggester structSimSuggester;
        ValueSimSuggester valueSimSuggester;
        TypeSimSuggester typeSimSuggester;
        NeighborSimSuggester neighborSimSuggester;
        LevDistNameSimSuggester levDistNameSimSuggester;

        double[,] nameSimMatrix;
        double[,] isoRankSimMatrix;
        double[,] structSimMatrix;
        double[,] valueSimMatrix;
        double[,] typeSimMatrix;
        double[,] neighborSimMatrix;
        double[,] levDistNameSimMatrix;

        //weights are defined as static so that updating them will affect all suggester objects
        public static double nameSimWeight = 1.0;
        public static double isoRankSimWeight = 1.0;
        public static double valueSimWeight = 1.0;
        public static double structSimWeight = 1.0;
        public static double typeSimWeight = 1.0;
        public static double neighborSimWeight = 1.0;
        public static double levDistNameSimWeight = 1.0;

        string startTime;
        string endTime;
        StreamWriter log;

        string wekaInputFile = null;

        string configFilePath = "";

        public SuggesterEvaluator evaluator;

        #endregion //properties


        #region ctor

        public Suggester() 
        {
            isoSuggester = new IsoRankSuggester();
            structSimSuggester = new StructureSimSuggester();
            nameSimSuggester = new NameSimSuggester();
            valueSimSuggester = new ValueSimSuggester();
            typeSimSuggester = new TypeSimSuggester();
            neighborSimSuggester = new NeighborSimSuggester();
            levDistNameSimSuggester = new LevDistNameSimSuggester();

            loadWeights();

            suggesterConfig = new SuggesterConfig();
        }

        public Suggester(string sourceFile, string targetFile) : this(sourceFile, targetFile, null) { }

        public Suggester(string sourceFile, string targetFile, SuggesterConfig config)
        {
            //reading files to abstract trees
            sourceASTL = new AbstractLattice(sourceFile);
            targetASTL = new AbstractLattice(targetFile);

            isoSuggester = new IsoRankSuggester(sourceASTL,targetASTL);
            structSimSuggester = new StructureSimSuggester(sourceASTL,targetASTL);
            nameSimSuggester = new NameSimSuggester(sourceASTL,targetASTL);
            valueSimSuggester = new ValueSimSuggester(sourceASTL, targetASTL);
            typeSimSuggester = new TypeSimSuggester(sourceASTL, targetASTL);
            neighborSimSuggester = new NeighborSimSuggester(sourceASTL, targetASTL);
            levDistNameSimSuggester = new LevDistNameSimSuggester(sourceASTL, targetASTL);

            loadWeights();

            if (config == null)
            {
                suggesterConfig = new SuggesterConfig();//all suggesters will be used by default
            }
            else
                suggesterConfig = config;


            //createBenchmark();// Do not create benchmark for every suggestion (for now)
            updateSimilarityValues();
        }

        public Suggester(AbstractLattice sASTL, AbstractLattice tASTL) : this(sASTL, tASTL, null) { }

        public Suggester(AbstractLattice sASTL, AbstractLattice tASTL, SuggesterConfig config)
        {
            sourceASTL = sASTL;
            targetASTL = tASTL;

            isoSuggester = new IsoRankSuggester(sourceASTL, targetASTL);
            structSimSuggester = new StructureSimSuggester(sourceASTL, targetASTL);
            nameSimSuggester = new NameSimSuggester(sourceASTL, targetASTL);
            valueSimSuggester = new ValueSimSuggester(sourceASTL, targetASTL);
            typeSimSuggester = new TypeSimSuggester(sourceASTL, targetASTL);
            neighborSimSuggester = new NeighborSimSuggester(sourceASTL, targetASTL);
            levDistNameSimSuggester = new LevDistNameSimSuggester(sourceASTL, targetASTL);

            loadWeights();

            if (config == null)
            {
                suggesterConfig = new SuggesterConfig();//all suggesters will be used by default
            }
            else
                suggesterConfig = config;

            createBenchmark();
            updateSimilarityValues();
        }

        #endregion //ctor

        
        #region load/save/update weights

        public void loadWeights ()
        {
            configDoc = new XmlDocument();
            configFilePath = getDirectory("Resources\\SuggesterConfig.xml");
            configDoc.Load(configFilePath);

            nameSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("nameSugg/weight").InnerText);
            isoRankSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("isoRankSugg/weight").InnerText);
            valueSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("valueSugg/weight").InnerText);
            structSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("structSugg/weight").InnerText);
            typeSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("typeSugg/weight").InnerText);
            neighborSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("neighborSugg/weight").InnerText);
            levDistNameSimWeight = Double.Parse(configDoc.DocumentElement.SelectSingleNode("levDistNameSim/weight").InnerText);

            /*MessageBox.Show("loaded Weights are : \n" +
                                "\nName Similarity Weight : " + nameSimWeight.ToString() +
                                "\nValue Similarity Weight : " + valueSimWeight.ToString() +
                                "\nIsoRank Similarity Weight : " + isoRankSimWeight.ToString() +
                                "\nStructural Similarity weight : " + structSimWeight.ToString()+
                                "\nlevenshtein Disttance Name Similarity weight : " + levDistNameSimWeight.ToString());
             */
        }

        public void saveWeights()
        {
            XmlNode nameX = configDoc.DocumentElement.SelectSingleNode("nameSugg/weight");
            nameX.RemoveAll();
            nameX.AppendChild(configDoc.CreateTextNode(nameSimWeight.ToString()));

            XmlNode valueX = configDoc.DocumentElement.SelectSingleNode("valueSugg/weight");
            valueX.RemoveAll();
            valueX.AppendChild(configDoc.CreateTextNode(valueSimWeight.ToString()));

            XmlNode isoRankX = configDoc.DocumentElement.SelectSingleNode("isoRankSugg/weight");
            isoRankX.RemoveAll();
            isoRankX.AppendChild(configDoc.CreateTextNode(isoRankSimWeight.ToString()));

            XmlNode structX = configDoc.DocumentElement.SelectSingleNode("structSugg/weight");
            structX.RemoveAll();
            structX.AppendChild(configDoc.CreateTextNode(structSimWeight.ToString()));

            XmlNode typeX = configDoc.DocumentElement.SelectSingleNode("typeSugg/weight");
            typeX.RemoveAll();
            typeX.AppendChild(configDoc.CreateTextNode(typeSimWeight.ToString()));

            XmlNode neighborX = configDoc.DocumentElement.SelectSingleNode("neighborSugg/weight");
            neighborX.RemoveAll();
            neighborX.AppendChild(configDoc.CreateTextNode(neighborSimWeight.ToString()));

            XmlNode levDistX = configDoc.DocumentElement.SelectSingleNode("levDistNameSim/weight");
            levDistX.RemoveAll();
            levDistX.AppendChild(configDoc.CreateTextNode(levDistNameSimWeight.ToString()));
            //write final weights to config file

            //configDoc.Save(System.IO.Path.GetFullPath(getDirectory("Resources\\SuggesterConfig.xml")));
            //configDoc.Save("../../Resources/SuggesterConfig.xml");
        }

        public void updateSuggestionWeights(int sourceIndex, int targetIndex, string action)
        {
            if (sourceIndex == -1 || targetIndex == -1)
                MessageBox.Show("indexes are out of bound! \n\n source index : " + sourceIndex.ToString() + "\n target index : " + targetIndex.ToString());
            else
            {
                //MessageBox.Show("final suggestion retrived :\n\n" + sourceASTL.abstractLatticeGraph.Vertices.ElementAt(sourceIndex).Address + " -> " + targetASTL.abstractLatticeGraph.Vertices.ElementAt(targetIndex).Address);

                //process based on action
                if (action.ToLower().Equals("reject"))
                {
                    //check suggesters
                    if (nameSimMatrix != null)
                    {
                        if (nameSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            nameSimWeight -= 0.01; //decrease weights
                        else if (nameSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            nameSimWeight += 0.01; //increase wight
                    }

                    if (valueSimMatrix != null)
                    {
                        if (valueSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            valueSimWeight -= 0.01; //decrease weights
                        else if (valueSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            valueSimWeight += 0.01; //increase wight
                    }

                    if (isoRankSimMatrix != null)
                    {
                        if (isoRankSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            isoRankSimWeight -= 0.01; //decrease weights
                        else if (isoRankSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            isoRankSimWeight += 0.01; //increase wight
                    }

                    if (structSimMatrix != null)
                    {
                        if (structSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            structSimWeight -= 0.01; //decrease weights
                        else if (structSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            structSimWeight += 0.01; //increase wight
                    }

                    if (levDistNameSimMatrix != null)//////////////////////////////////////////////////////
                    {
                        if (levDistNameSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            levDistNameSimWeight -= 0.01; //decrease weights
                        else if (levDistNameSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            levDistNameSimWeight += 0.01; //increase wight
                    }

                    if (neighborSimMatrix != null)
                    {
                        if (neighborSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            neighborSimWeight -= 0.01; //decrease weights
                        else if (neighborSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            neighborSimWeight += 0.01; //increase wight
                    }
                }
                else if (action.ToLower().Equals("accept"))
                {
                    //check suggesters
                    if (nameSimMatrix != null)
                    {
                        if (nameSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            nameSimWeight += 0.01; //increase weights
                        else if (nameSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            nameSimWeight -= 0.01; //decrease wight
                    }

                    if (valueSimMatrix != null)
                    {
                        if (valueSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            valueSimWeight += 0.01; //increase weights
                        else if (valueSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            valueSimWeight -= 0.01; //decrease wight
                    }

                    if (isoRankSimMatrix != null)
                    {
                        if (isoRankSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            isoRankSimWeight += 0.01; //increase weights
                        else if (isoRankSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            isoRankSimWeight -= 0.01; //decrease wight
                    }

                    if (structSimMatrix != null)
                    {
                        if (structSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            structSimWeight += 0.01; //increase weights
                        else if (structSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            structSimWeight -= 0.01; //decrease wight
                    }

                    if (neighborSimMatrix != null)
                    {
                        if (neighborSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            neighborSimWeight += 0.01; //increase weights
                        else if (neighborSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            neighborSimWeight -= 0.01; //decrease wight
                    }

                    if (levDistNameSimMatrix != null)//////////////////////////////////////////////////////
                    {
                        if (levDistNameSimMatrix[sourceIndex, targetIndex] > 0) //is a + suggestion
                            levDistNameSimWeight += 0.01; //decrease weights
                        else if (levDistNameSimMatrix[sourceIndex, targetIndex] <= 0) //is a - suggestion
                            levDistNameSimWeight -= 0.01; //increase wight
                    }
                }

                //MessageBox.Show("Weights after updating are : \n" +
                //                 "\nName Similarity Weight : " + nameSimWeight.ToString() +
                //                 "\nValue Similarity Weight : " + valueSimWeight.ToString() +
                //                 "\nIsoRank Similarity Weight : " + isoRankSimWeight.ToString() +
                //                 "\nStructural Similarity weight : " + structSimWeight.ToString() +
                //                 "\nlevenshtein Disttance Name Similarity weight : " + levDistNameSimWeight.ToString());
            }
        }

        #endregion //load/save weights


        #region destructor

        ~Suggester()
        {
            saveWeights();
        }

        #endregion


        #region get Suggestions

        /// <summary>
        /// Use stable marriage to get the highest scoring suggestions for each pair of links
        /// Returns a matchMatrix in which values of 1 indicate a match and -1 otherwise
        /// </summary>
        /// <returns></returns>
        public double[,] imFeelingLucky()
        {
            double[,] result = getSuggestions();
            
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

            //printSuggestionsToFile(matchMatrix);


            LastResults = matchMatrix;

            //evaluate your work
            //evaluator = new SuggesterEvaluator(this);

            return matchMatrix;
        }

        /// <summary>
        /// Get suggestion matrix and prints suggestion pairs to matchfile.txt
        /// </summary>
        /// <param name="matchMatrix"></param>
        public void printSuggestionsToFile(double [,] matchMatrix)
        {
            //for logging and testing
            //******************
            if (!File.Exists(DirectoryHelper.getFilePathExecutingAssembly("matchfile.txt")))
            {
                log = new StreamWriter(DirectoryHelper.getFilePathExecutingAssembly("matchfile.txt"));
            }
            else
            {
                log = File.AppendText(DirectoryHelper.getFilePathExecutingAssembly("matchfile.txt"));
            }
            log.WriteLine(startTime);

            int count = 0;
            for (int i = 0; i < matchMatrix.GetLength(0); i++)
                for (int j = 0; j < matchMatrix.GetLength(1); j++)
                    if (matchMatrix[i, j] == 1)
                    {
                        //            //MessageBox.Show(i.ToString() + "    " + j.ToString());
                        //            prepareLine(gSource.Vertices.ElementAt(i), gTarget.Vertices.ElementAt(j));
                        log.WriteLine(sourceASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address + " -> " + targetASTL.abstractLatticeGraph.Vertices.ElementAt(j).Address);
                        count++;
                    }

            //MessageBox.Show(count.ToString() + " matches have been presented by Suggester (feeling lucky)!");

            log.WriteLine(endTime);

            //Close the stream:
            log.Close();
            //******************
        }
        
        public List<suggestionVector> prepareSuggestionVector(double[,] matchMatrix)
        {
            List<suggestionVector> vector = new List<suggestionVector>();

            for(int i = 0; i< matchMatrix.GetLength(0); i++)
                for (int j = 0; j < matchMatrix.GetLength(1); j++)
                {
                    suggestionVector node = new suggestionVector();
                    node.i = i;
                    node.j = j;
                    node.score = matchMatrix[i,j];
                    node.LHS = sourceASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address;
                    node.RHS = targetASTL.abstractLatticeGraph.Vertices.ElementAt(j).Address;
                    vector.Add(node);
                }


            return vector;
        }

        /// <summary>
        /// Get suggestions on a matrix consisting of scoring values
        /// </summary>
        /// <returns></returns>
        public double[,] getSuggestions()
        {
            //log event
            startTime = "\n\n**********************\n * Suggester started at: " + DateTime.Now + "*\n*************************\n\n";

            //double [,] suggestionMatrix = new double[nameSimMatrix.GetLength(0), nameSimMatrix.GetLength(1)];
            double [,] suggestionMatrix = new double[sourceASTL.abstractLatticeGraph.Vertices.Count(), targetASTL.abstractLatticeGraph.Vertices.Count()];

            if(suggesterConfig.UseNameSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, nameSimMatrix, nameSimWeight);

            if(suggesterConfig.UseValueSimSuggester) 
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, valueSimMatrix, valueSimWeight);

            if(suggesterConfig.UseStructSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, structSimMatrix, structSimWeight);

            if(suggesterConfig.UseIsoRankSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, isoRankSimMatrix, isoRankSimWeight);

            if(suggesterConfig.UseTypeSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, typeSimMatrix, typeSimWeight);

            if (suggesterConfig.UseNeighborSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, neighborSimMatrix, neighborSimWeight);

            if (suggesterConfig.UseLevDistNameSimSuggester)
                suggestionMatrix = analyseAndUpdate(suggestionMatrix, levDistNameSimMatrix, levDistNameSimWeight);

            //log event
            endTime = "\n***************\nfinished at: " + DateTime.Now + "\n**************\n";

            LastResults = suggestionMatrix;

            //evaluate your work
            //SuggesterEvaluator eval = new SuggesterEvaluator(this);

            return suggestionMatrix;
        }

        /// <summary>
        /// Reinitiate getSimilarity of each of suggester (in suggester config) to get simialrity scores
        /// </summary>
        public void updateSimilarityValues()
        {
            if (suggesterConfig.UseIsoRankSimSuggester)
                isoRankSimMatrix = isoSuggester.getSimilarity();

            if (suggesterConfig.UseStructSimSuggester)
                structSimMatrix = structSimSuggester.getSimilarity();

            if (suggesterConfig.UseNameSimSuggester)
                nameSimMatrix = nameSimSuggester.getSimilarity();

            if (suggesterConfig.UseValueSimSuggester)
                valueSimMatrix = valueSimSuggester.getSimilarity();

            if (suggesterConfig.UseTypeSimSuggester)
                typeSimMatrix = typeSimSuggester.getSimilarity();

            if (suggesterConfig.UseNeighborSimSuggester)
                neighborSimMatrix = neighborSimSuggester.getSimilarity();

            if (suggesterConfig.UseLevDistNameSimSuggester)
                levDistNameSimMatrix = levDistNameSimSuggester.getSimilarity();

        }

        /// <summary>
        /// Update suggestion matrix using values of the new similarity heuristic
        /// </summary>
        /// <param name="suggestionMatrix"></param>
        /// <param name="simMatrix"></param>
        /// <returns></returns>
        private double[,] analyseAndUpdate(double[,] suggestionMatrix, double[,] simMatrix, double weight)
        {
            //check if matrixes are talking with the same language
            if (suggestionMatrix.GetLength(0) == simMatrix.GetLength(0) && suggestionMatrix.GetLength(1) == simMatrix.GetLength(1))
            {
                //the strategi: check each score against the threshold (average of that similarity matrix) and if higher set as suggestion
                //if lower, divide the suggestion score by half to reduce score

                double thresh = getAverage(simMatrix);//threshold

                for (int i = 0; i < simMatrix.GetLength(0); i++)
                    for (int j = 0; j < simMatrix.GetLength(1); j++)
                        if (simMatrix[i, j]*weight >= thresh)
                        {
                            suggestionMatrix[i, j] += simMatrix[i, j]*weight;
                        }
                        //else //no need to change here as the weights will do the job now, 14/05/2012
                        //{ 
                        //    suggestionMatrix[i,j] = suggestionMatrix[i,j] - (suggestionMatrix[i,j]/2);
                        //}
                
                LastResults = suggestionMatrix;
                
                return suggestionMatrix;

            }
            else
            {
                MessageBox.Show("Matrix dimentions do not match!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

        }

        private double getAverage(double[,] simMatrix)
        {
            double sum = 0;

            for (int i = 0; i < simMatrix.GetLength(0); i++)
                for (int j=0; j < simMatrix.GetLength(1); j++)
                    sum += simMatrix[i, j];

            return sum / simMatrix.Length;
        }

        #endregion //getSuggestions


        #region ranking process

        /// <summary>
        /// Get the similarity matrix and checks similarity scores.
        /// </summary>
        /// <param name="matchMatrix"></param>
        /// <returns>Matrix of ranks in the suggesters, higher scores are ranks lower, Highest score -> 1, lowest -> i*j</returns>
        private double[,] rankSuggestions(double[,] matchMatrix)
        {
            double[,] output = new double[matchMatrix.GetLength(0), matchMatrix.GetLength(1)];

            List<suggestionVector> vector = prepareSuggestionVector(matchMatrix);

            //IEnumerable<suggestionVector> sortedlist = from v in vector
            //                                           orderby v.score descending
            //                                           select v;

            vector.Sort();

            uint rank = 1;

            output[vector.ElementAt(vector.Count - 1).i, vector.ElementAt(vector.Count - 1).j] = rank;
            double previouseScore = vector.ElementAt(vector.Count - 1).score;
            for (int i = vector.Count - 2; i >= 0; i--)
            {
                if (vector.ElementAt(i).score == previouseScore)
                    output[vector.ElementAt(i).i, vector.ElementAt(i).j] = rank;
                else
                {
                    rank++;
                    output[vector.ElementAt(i).i, vector.ElementAt(i).j] = rank;
                    previouseScore = vector.ElementAt(i).score;
                }
                //MessageBox.Show(vector.ElementAt(i).score.ToString() + " : " + vector.ElementAt(i).suggestion + " Rank : " + rank.ToString());  //testing
            }

            return output;
        }


        /// <summary>
        /// Checks the ranked suggestions and returns the highest ranked suggestions per element (forward and backward separately)
        /// </summary>
        /// <param name="suggestionPerElement"></param>
        /// <returns></returns>
        public double[,] getRankedSuggestions(int suggestionPerElement)
        {
            double[,] result = rankSuggestions(calculateRankedSuggestions());
            result = getSuggestionByBetterRanks(result, suggestionPerElement);

            LastResults = result;

            return result;
        }


        public double[,] getSuggestionByBetterRanks(double[,] matchMatrix, int suggestionPerRank)
        {
            double[,] output = rankSuggestions(matchMatrix);
            double[,] result = new double[output.GetLength(0), output.GetLength(1)];


            for (int i = 0; i < output.GetLength(0); i++)
            {
                //find lowest rank(best)
                double minRank = double.MaxValue;
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    if (output[i, j] < minRank)
                        minRank = output[i, j];
                }

                //find elements with same rank (lowest rank)
                List<suggestionVector> sameRanks = new List<suggestionVector>();
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    if (output[i, j] == minRank)
                    {
                        suggestionVector temp = new suggestionVector();
                        temp.i = i;
                        temp.j = j;
                        temp.score = matchMatrix[i, j];//not important

                        sameRanks.Add(temp);
                    }
                }

                //get desired elements in output
                int c = 0;
                for (int t = 0; c < suggestionPerRank && t < sameRanks.Count; t++)
                {
                    result[sameRanks.ElementAt(t).i, sameRanks.ElementAt(t).j] = 1;
                    c++;
                }

            }

            return result;
        }

        /// <summary>
        /// Get suggestions on a matrix consisting of scoring values
        /// </summary>
        /// <returns></returns>
        public double[,] calculateRankedSuggestions()
        {
            //log event
            startTime = "\n\n**********************\n * Suggester with Ranks started at: " + DateTime.Now + "*\n*************************\n\n";

            //double [,] suggestionMatrix = new double[nameSimMatrix.GetLength(0), nameSimMatrix.GetLength(1)];
            double[,] suggestionRankMatrix = new double[sourceASTL.abstractLatticeGraph.Vertices.Count(), targetASTL.abstractLatticeGraph.Vertices.Count()];

            if (suggesterConfig.UseNameSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(nameSimMatrix), nameSimWeight);

            if (suggesterConfig.UseValueSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(valueSimMatrix), valueSimWeight);

            if (suggesterConfig.UseStructSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(structSimMatrix), structSimWeight);

            if (suggesterConfig.UseIsoRankSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(isoRankSimMatrix), isoRankSimWeight);

            if (suggesterConfig.UseTypeSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(typeSimMatrix), typeSimWeight);

            if (suggesterConfig.UseNeighborSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(neighborSimMatrix), neighborSimWeight);

            if (suggesterConfig.UseLevDistNameSimSuggester)
                suggestionRankMatrix = analyseRankingAndUpdate(suggestionRankMatrix, rankSuggestions(levDistNameSimMatrix), levDistNameSimWeight);

            //log event
            endTime = "\n***************\nfinished at: " + DateTime.Now + "\n**************\n";

            //LastResults = suggestionMatrix;

            //evaluate your work
            //SuggesterEvaluator eval = new SuggesterEvaluator(this);

            return suggestionRankMatrix;
        }

        private double[,] analyseRankingAndUpdate(double[,] suggestionRankMatrix, double[,] simMatrix, double nameSimWeight)//is weight important here?
        {
            //for now just add them
            return MatrixLibrary.Matrix.Add(suggestionRankMatrix, simMatrix);
        }

        #endregion


        #region Suggestion Strings

        public Collection<string> getSuggestionStringsFor(string ElementAddress)
        {
            Collection<string> results = new Collection<string>();
            Collection<string> suggestions = getSuggestionsAsStrings(LastResults);

            foreach (string sugg in suggestions)
                if (sugg.IndexOf(ElementAddress) != -1)
                    results.Add(sugg);

            return results;
        }

        /// <summary>
        /// outputs suggestions as a colelction of correspondence strings
        /// </summary>
        /// <param name="matchMatrix">Suggester results</param>
        /// <returns>Collection of correspondence strings</returns>
        public Collection<string> getSuggestionsAsStrings(double[,] matchMatrix)
        {
            Collection<string> suggs = new Collection<string>();

            for (int i = 0; i < matchMatrix.GetLength(0); i++)
                for (int j = 0; j < matchMatrix.GetLength(1); j++)
                    if (matchMatrix[i, j] == 1)
                    {
                        //            //MessageBox.Show(i.ToString() + "    " + j.ToString());
                        //            prepareLine(gSource.Vertices.ElementAt(i), gTarget.Vertices.ElementAt(j));
                        suggs.Add(sourceASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address + " -> " + targetASTL.abstractLatticeGraph.Vertices.ElementAt(j).Address);

                    }

            /*suggs.Add("DONE!");
            
            for (int i = 0; i < matchMatrix.GetLength(0); i++)
                for (int j = 0; j < matchMatrix.GetLength(1); j++)
            {
                suggs.Add(sourceASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address + " -> " + targetASTL.abstractLatticeGraph.Vertices.ElementAt(j).Address
                    + "\n Score " + LastResults[i, j].ToString());
            }*/

            return suggs;
        }

        /// <summary>
        /// prepares a list of suggestion based on their scores, higher scores appear higher in the list
        /// </summary>
        /// <param name="matchMatrix"></param>
        /// <returns></returns>
        public Collection<string> getOrderedSuggestionsAsStrings(double[,] matchMatrix)
        {
            Collection<string> output = new Collection<string>();
            List<suggestionVector> vector = prepareSuggestionVector(matchMatrix);

            //IEnumerable<suggestionVector> sortedlist = from v in vector
            //                                           orderby v.score descending
            //                                           select v;
            vector.Sort();
            foreach (suggestionVector s in vector)
            {
                output.Add(s.suggestion);
                //MessageBox.Show(s.score.ToString() + " : " + s.suggestion);  //testing
            }

            return output;
        }

        #endregion //Suggestion strings


        #region Analyse user interaction (ACCEPT/REJECT)

        /// <summary>
        /// retrieve a suggestion by its string and based on action accept or reject it
        /// </summary>
        /// <param name="suggestionString">Suggestion String</param>
        /// <param name="action">ACCEPT/REJECT</param>
        public void retrieveSuggestion(string suggestionString, string action)
        {
            string [] splitSt = {" -> "};
            string [] strArray = suggestionString.Split(splitSt, StringSplitOptions.RemoveEmptyEntries);

            string left = strArray[0];
            string right = strArray[1];

            //MessageBox.Show(left + "\n" + right);
            int sourceIndex = -1;
            int targetIndex = -1;

            //find elements in abstract tree graph
            for (int i = 0; i < sourceASTL.abstractLatticeGraph.Vertices.Count(); i++)
                if (sourceASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address.Equals(left))
                {
                    sourceIndex = i;
                    break;
                }

            for (int i = 0; i < targetASTL.abstractLatticeGraph.Vertices.Count(); i++)
                if (targetASTL.abstractLatticeGraph.Vertices.ElementAt(i).Address.Equals(right))
                {
                    targetIndex = i;
                    break;
                }

            updateSuggestionWeights(sourceIndex, targetIndex, action);

        }

        
        #endregion


        #region Weka


        /// <summary>
        /// Generate ARFF file to be used by Weka, from scores derieved from suggesters 
        /// and run weka to calcualte initial weights
        /// </summary>
        /// <param name="arffFileName">Name of the output file</param>
        public void prepareWekaInput(string arffFileName)
        {
            //string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            wekaInputFile = arffFileName;

            double[,] suggestionMatrix = getSuggestions();
            double thresh = getAverage(suggestionMatrix);//threshold

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("@RELATION recommender");
            sb.AppendLine("@ATTRIBUTE row NUMERIC");
            sb.AppendLine("@ATTRIBUTE col NUMERIC");
            sb.AppendLine("@ATTRIBUTE isorank NUMERIC");
            sb.AppendLine("@ATTRIBUTE namesim NUMERIC");
            sb.AppendLine("@ATTRIBUTE valuesim NUMERIC");
            sb.AppendLine("@ATTRIBUTE structsim NUMERIC");
            sb.AppendLine("@ATTRIBUTE typesim NUMERIC");
            sb.AppendLine("@ATTRIBUTE neighborsim NUMERIC");
            sb.AppendLine("@ATTRIBUTE class {False,True}");
            sb.AppendLine("@DATA");

            for (int i = 0; i < suggestionMatrix.GetLength(0); i++)
                for (int j = 0; j < suggestionMatrix.GetLength(1); j++)
                {
                    string str = i.ToString()+","+j.ToString()+","+isoRankSimMatrix[i,j].ToString()+","+nameSimMatrix[i,j]+","+valueSimMatrix[i,j]
                                + "," + structSimMatrix[i, j] + "," + typeSimMatrix[i, j] + "," + neighborSimMatrix[i, j];
                    
                    if (suggestionMatrix[i, j] >= thresh)
                        str += ",True";
                    else
                        str += ",False";

                    sb.AppendLine(str);
                }

                        
            using (StreamWriter outfile = new StreamWriter(arffFileName))
            {
                outfile.Write(sb.ToString());
            }

            //run weka
            //here weka should be initialised with the file and run to get weights

        }

        /*public void classifyTest()
        {
            
            try
            {
                int percentSplit = 59;
                weka.core.Instances insts = new weka.core.Instances(new java.io.FileReader("iris.arff"));
                insts.setClassIndex(insts.numAttributes() - 1);

                weka.classifiers.Classifier cl = new weka.classifiers.trees.J48();
                Console.WriteLine("Performing " + percentSplit + "% split evaluation.");

                //randomize the order of the instances in the dataset.
                weka.filters.Filter myRandom = new weka.filters.unsupervised.instance.Randomize();
                myRandom.setInputFormat(insts);
                insts = weka.filters.Filter.useFilter(insts, myRandom);

                int trainSize = insts.numInstances() * percentSplit / 100;
                int testSize = insts.numInstances() - trainSize;
                weka.core.Instances train = new weka.core.Instances(insts, 0, trainSize);

                cl.buildClassifier(train);
                int numCorrect = 0;
                for (int i = trainSize; i < insts.numInstances(); i++)
                {
                    weka.core.Instance currentInst = insts.instance(i);
                    double predictedClass = cl.classifyInstance(currentInst);
                    if (predictedClass == insts.instance(i).classValue())
                        numCorrect++;
                }

                Console.WriteLine(numCorrect + " out of " + testSize + " correct (" +
                           (double)((double)numCorrect / (double)testSize * 100.0) + "%)");

                System.Threading.Thread.Sleep(5000);
            }
            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }
        }*/


        /*/// <summary>
        /// aske weka to calculate weights for the neural network.
        /// </summary>
        public void calculateWeightsWeka()
        {

            if (wekaInputFile != null)
            {

                //log event
                startTime = "\n\n**********************\n * Suggester started at: " + DateTime.Now + "*\n*************************\n\n";

                //initiate suggestions by name similarity
                double[,] suggestionMatrix = new double[nameSimMatrix.GetLength(0), nameSimMatrix.GetLength(1)];

                suggestionMatrix = analyseAndUpdate(suggestionMatrix, nameSimMatrix);

                suggestionMatrix = analyseAndUpdate(suggestionMatrix, valueSimMatrix);

                suggestionMatrix = analyseAndUpdate(suggestionMatrix, structSimMatrix);

                suggestionMatrix = analyseAndUpdate(suggestionMatrix, isoRankSimMatrix);

                //log event
                endTime = "\n***************\nfinished at: " + DateTime.Now + "\n**************\n";

                //return suggestionMatrix;
            }
            else
                MessageBox.Show("Prepare Weka Input file first!","Error",MessageBoxButton.OK,MessageBoxImage.Error);
        }*/

        #endregion


        #region tools

        /// <summary>
        /// get a copy of the objects (by value)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
        
        private string getDirectory(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            return System.IO.Path.Combine(pbase, c);
        }
        
        #endregion //tools


        #region previous suggester
        /*
        public Suggester()
        {
            //28/10/2011 dbCon.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\TestProg\\Resources\\RulesDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
            dbCon.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Documents and Settings\iavazpour\My Documents\Visual Studio 2010\Projects\PZeroXaml\WithMerge\RulesDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";

            try
            {
                dbCon.Open();

                //define adapters for rules and tables.
                rulesElementsAdapter = new SqlDataAdapter("SELECT Rate,LeftElements.EName,RightElements.EName FROM Rules,LeftElements,RightElements where Rules.RNumber = LeftElements.RNumber and Rules.RNumber = RightElements.RNumber", dbCon);
                rulesAdapter = new SqlDataAdapter("SELECT * FROM Rules", dbCon);
                RElementsAdapter = new SqlDataAdapter("SELECT * FROM RightElements", dbCon);
                LElementsAdapter = new SqlDataAdapter("SELECT * FROM LeftElements", dbCon);

                //define DataSets
                rulesElementsDS = new DataSet();
                rulesDS = new DataSet();
                RElementsDS = new DataSet();
                LElementsDS = new DataSet();

                //create command builders
                cbRule = new SqlCommandBuilder(rulesAdapter);
                cbLeft = new SqlCommandBuilder(LElementsAdapter);
                cbRight = new SqlCommandBuilder(RElementsAdapter);

                //fill data sets with tables data
                rulesElementsAdapter.Fill(rulesElementsDS, "myRules");
                rulesAdapter.Fill(rulesDS, "Rules");
                RElementsAdapter.Fill(RElementsDS, "RightElements");
                LElementsAdapter.Fill(LElementsDS, "LeftElements");

                //dbCon.Close();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error preparing data base for Suggester",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        public ObservableCollection<Rule> getSuggestions(PocGraph g, PocGraph gr)//7/09/2011
        {
            ObservableCollection<Rule> result = new ObservableCollection<Rule>();

            result = getSuggestionFromDB(g, gr);
            result = mergeCollections(result, suggestOnNameSimilarity(g, gr));

            return result;
        }

        public ObservableCollection<Rule> getSuggestionFromDB(PocGraph g, PocGraph gr)//Added 5/09/2011
        {
            ObservableCollection<Rule> result = new ObservableCollection<Rule>();

            //it was here before//foreach (PocVertex p in g.Vertices)
            //{
            //    foreach (PocVertex ps in gr.Vertices)
            //    {
            //        foreach (DataRow dr in rulesElementsDS.Tables["myRules"].Rows)
            //            if (((p.Name.ToLower().Equals(dr.ItemArray.GetValue(1).ToString().ToLower())) && (ps.Name.ToLower().Equals(dr.ItemArray.GetValue(2).ToString().ToLower())))||
            //                ((ps.Name.ToLower().Equals(dr.ItemArray.GetValue(1).ToString().ToLower())) && (p.Name.ToLower().Equals(dr.ItemArray.GetValue(2).ToString().ToLower()))))
            //            //if a rule with same LName and RName has existed before
            //            {
            //                Rule newRule = new Rule();
            //                newRule.Left.Add(p);
            //                newRule.Right.Add(ps);
            //                result.Add(newRule);
            //            }
            //    }
            //}

            //changed 7/11/2011 suggesting multiple element rules

            
            foreach (DataRow dr in rulesDS.Tables["Rules"].Rows)
            {
                bool check = true;
                Rule newRule = new Rule();

                //check left elements
                String query = "Select EName FROM LeftElements WHERE RNumber = " + dr.ItemArray.GetValue(0).ToString();

                SqlDataAdapter temp = new SqlDataAdapter(query, dbCon);
                DataSet dt = new DataSet();
                temp.Fill(dt);

                foreach (DataRow d in dt.Tables[0].Rows)
                {

                    if (check == true)
                    {
                        if (g.HasVertex(d.ItemArray.GetValue(0).ToString()))
                        {
                            newRule.Left.Add(g.getVertex(d.ItemArray.GetValue(0).ToString()));
                            //MessageBox.Show(d.ItemArray.GetValue(0).ToString());
                        }
                        else
                            check = false;
                    }
                }

                if (check == true)//left elements exist, then go for right elements
                {
                    query = "Select EName FROM RightElements WHERE RNumber = " + dr.ItemArray.GetValue(0).ToString();

                    temp = new SqlDataAdapter(query, dbCon);
                    dt = new DataSet();
                    temp.Fill(dt);

                    foreach (DataRow d in dt.Tables[0].Rows)
                    {
                        if (check == true)
                        {
                            if (gr.HasVertex(d.ItemArray.GetValue(0).ToString()))
                                newRule.Right.Add(gr.getVertex(d.ItemArray.GetValue(0).ToString()));
                            else
                                check = false;
                        }
                    }
                }

                if (check == true)
                {
                    result.Add(newRule);
                    //MessageBox.Show(newRule.RuleString);
                }

            }
            
            return result;
            //return null;
        }

        public ObservableCollection<Rule> suggestOnUserSelectionNameSimilarity(PocGraph g, PocGraph gr, Rule r)
        {
            ObservableCollection<Rule> result = new ObservableCollection<Rule>();

            foreach (PocVertex p in g.Vertices)
            {
                foreach (PocVertex ps in gr.Vertices)
                {
                    if (checkNames(r.Left[0], p) > 0 && (checkNames(r.Right[0], ps) > 0))//just for one to one rules at the moment
                    {
                        Rule newRule = new Rule();
                        newRule.Left.Add(p);
                        newRule.Right.Add(ps);
                        result.Add(newRule);
                    }
                }
            }

            return result;
        }

        public ObservableCollection<Rule> suggestOnNameSimilarity(PocGraph g, PocGraph gr)
        {
            ObservableCollection<Rule> result = new ObservableCollection<Rule>();

            foreach (PocVertex p in g.Vertices)
            {
                foreach (PocVertex ps in gr.Vertices)
                {
                    if (checkNames(ps, p) > 0)
                    {
                        Rule newRule = new Rule();
                        newRule.Left.Add(p);
                        newRule.Right.Add(ps);
                        result.Add(newRule);
                    }
                }
            }

            return result;
        }

        private int checkNames(Element ps, Element p)
        {
            int heu = 0;

            if (ps.Name.ToLower().Contains(p.Name.ToLower()))
                heu++;
            if (p.Name.ToLower().Contains(ps.Name.ToLower()))
                heu++;

            return heu;
        }

        public string printRulesInDB()
        {
            string test = "";
            foreach (DataRow dr in rulesElementsDS.Tables["myRules"].Rows)
                //foreach(Object o in dr.ItemArray)
                test = test + "Rate : " + dr.ItemArray.GetValue(0).ToString().ToLower() + " , LName : " + dr.ItemArray.GetValue(1).ToString().ToLower()
                                + " , RName : " + dr.ItemArray.GetValue(2).ToString().ToLower() + "\n\n";
            MessageBox.Show(test);

            return test;
        }

        private ObservableCollection<Rule> mergeCollections(ObservableCollection<Rule> left, ObservableCollection<Rule> right)
        {
            ObservableCollection<Rule> result = left;

            foreach (Rule r in right)
            {
                bool check = false;
                //add rule to left if it is not already in there
                foreach (Rule r2 in left)
                    if (r2.isEqual(r)) //rule already exists
                        check = true;

                if (check == false)
                {
                    left.Add(r);

                }
            }

            return result;
        }
        */
        #endregion


        #region DataBase insertion
        /*

        public void inserRuleToDB(Rule r) //completed 6/09/2011
        {
            if (r != null)
            {
                if (findRuleInDB(r) == null)//does not exist so you can add
                {

                    DataRow ruleRow = rulesDS.Tables["Rules"].NewRow();

                    int ruleNumber = getMaxQuery("Rules", "RNumber") + 1;
                    ruleRow[0] = ruleNumber;
                    ruleRow[1] = 1;//rule rating

                    rulesDS.Tables["Rules"].Rows.Add(ruleRow);
                    rulesAdapter.Update(rulesDS, "Rules");

                    //put left elements in table
                    int leftElementNumber = getMaxQuery("LeftElements", "ENumber") + 1;

                    foreach (PocVertex p in r.Left)
                    {
                        DataRow leftRow = LElementsDS.Tables["LeftElements"].NewRow();
                        leftRow[0] = p.Name;//element name
                        leftRow[1] = leftElementNumber;//element number
                        leftRow[2] = ruleNumber;//rule number

                        LElementsDS.Tables["LeftElements"].Rows.Add(leftRow);

                        leftElementNumber = leftElementNumber + 1;
                    }

                    LElementsAdapter.Update(LElementsDS, "LeftElements");

                    //put right elements in table
                    int rightElementNumber = getMaxQuery("RightElements", "ENumber") + 1;

                    foreach (PocVertex p in r.Right)
                    {
                        DataRow rightRow = RElementsDS.Tables["RightElements"].NewRow();
                        rightRow[0] = p.Name;//element name
                        rightRow[1] = rightElementNumber;//element number
                        rightRow[2] = ruleNumber;//rule number

                        RElementsDS.Tables["RightElements"].Rows.Add(rightRow);

                        rightElementNumber = rightElementNumber + 1;
                    }

                    RElementsAdapter.Update(RElementsDS, "RightElements");

                    MessageBox.Show("Rule inserted into data base");
                }
                else//exists so update the row
                    updateRuleRating(findRuleInDB(r));
                //printRulesInDB();
            }
        }

        private DataRow findRuleInDB(Rule r)
        {
            
            //check existance of the rule 7/09/2011
            foreach (DataRow dr in rulesDS.Tables["Rules"].Rows)
            {
                //check left elements
                bool check = true; //existance

                String query = "Select EName FROM LeftElements WHERE RNumber = " + dr.ItemArray.GetValue(0).ToString();

                SqlDataAdapter temp = new SqlDataAdapter(query, dbCon);
                DataSet dt = new DataSet();
                temp.Fill(dt);

                foreach (PocVertex p in r.Left)
                    if (checkExistanceInDataSet(dt.Tables[0].Rows, p.Name) == false)
                    {
                        check = false;
                        break;
                    }

                if (check == true)//left elements exist, then go for right elements
                {
                    query = "Select EName FROM RightElements WHERE RNumber = " + dr.ItemArray.GetValue(0).ToString();

                    temp = new SqlDataAdapter(query, dbCon);
                    dt = new DataSet();
                    temp.Fill(dt);

                    foreach (PocVertex p in r.Right)
                        if (checkExistanceInDataSet(dt.Tables[0].Rows, p.Name) == false)
                        {
                            check = false;
                            break;
                        }
                    if (check == true)
                    {
                        //MessageBox.Show("Rule Exists in DB");
                        return dr;
                    }
                }
            }
            return null;
        }

        private int getMaxQuery(string table, string field)
        {
            int result = 0;
            
            String query = "Select Max(" + field + ") FROM " + table;

            SqlDataAdapter temp = new SqlDataAdapter(query, dbCon);
            DataSet dt = new DataSet();
            temp.Fill(dt);

            if (dt.Tables[0].Rows[0].ItemArray.GetValue(0).ToString().Equals(""))
                result = 0;
            else
                result = (int)(dt.Tables[0].Rows[0].ItemArray.GetValue(0));

            return result;
        }

        private bool checkExistanceInDataSet(DataRowCollection drc, string value)
        {
           
            foreach (DataRow dr in drc)
                if (dr.ItemArray.GetValue(0).ToString().Equals(value))
                {
                    return true;
                }
            return false;
        }
        */
        #endregion


        #region benchmarking

        public void createBenchmark()
        {
            try
            {
                //check if benchmark exists
                XmlDocument doc = new XmlDocument();
                doc.Load(getDirectory(@"Suggester\Evaluation Files\EvaluationBenchmark.xml"));

                //check if a benchmark exists
                XmlNode check = doc.SelectSingleNode("Benchmarks/Benchmark[@LHS='" + sourceASTL.Root.Name + "' and @RHS='" + targetASTL.Root.Name + "']");//' and @tag='OK']");

                if (check == null)
                {
                    XmlElement test = doc.CreateElement("Benchmark");

                    XmlAttribute lhs = doc.CreateAttribute("LHS");
                    lhs.AppendChild(doc.CreateTextNode(sourceASTL.Root.Name));
                    test.Attributes.Append(lhs);

                    XmlAttribute rhs = doc.CreateAttribute("RHS");
                    rhs.AppendChild(doc.CreateTextNode(targetASTL.Root.Name));
                    test.Attributes.Append(rhs);

                    XmlAttribute tag = doc.CreateAttribute("tag");
                    tag.AppendChild(doc.CreateTextNode("NOTOK"));
                    test.Attributes.Append(tag);

                    foreach (AbstractTreeLatticeNode node in sourceASTL.abstractLatticeGraph.Vertices)
                    {
                        XmlElement corr = doc.CreateElement("Corr");
                        XmlElement clhs = doc.CreateElement("LHS");
                        clhs.AppendChild(doc.CreateTextNode(node.Address));

                        corr.AppendChild(clhs);
                        test.AppendChild(corr);
                    }

                    foreach (AbstractTreeLatticeNode node in targetASTL.abstractLatticeGraph.Vertices)
                    {
                        XmlElement crhs = doc.CreateElement("RHS");
                        crhs.AppendChild(doc.CreateTextNode(node.Address));

                        test.AppendChild(crhs);
                    }

                    XmlNode benchmarks = doc.SelectSingleNode("Benchmarks");
                    benchmarks.AppendChild(test);

                    //save updated benchmark
                    doc.Save((getDirectory(@"Suggester\Evaluation Files\EvaluationBenchmark.xml")).Replace(@"file:\", ""));
                    //MessageBox.Show("Done");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion //benchmarking

    }

    public class suggestionVector : IComparable<suggestionVector>

    {
        public int i;
        public int j;
        public double score;
        public string suggestion {
            get { return LHS + " -> " + RHS; }
        }
        public string LHS;
        public string RHS;

        public int CompareTo(suggestionVector other)
        {
            return this.score.CompareTo(other.score);
        }


        public override string ToString()
        {
            return "LHS: "+ LHS+ " RHS: "+ RHS +  "\ni: "+i.ToString() + " j: "+ j.ToString()+ " score: "+score.ToString();
        }
    }
}
