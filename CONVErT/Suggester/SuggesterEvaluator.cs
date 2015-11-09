using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;

namespace CONVErT
{
    public class SuggesterEvaluator
    {
        #region prop

        public Suggester ToEvaluate { get; set; }
        
        //inputs
        public double [,] SuggesterResultMatrix { get; set; }

        public double[,] DesiredResultMatrix { get; set; }

        //Analysis part
        private int RelevantSelected { get; set; }
        private int RelevantNOTSelected { get; set; }
        private int TotalRelevant { get; set; }
        private int IrrelevantSelected { get; set; }
        private int IrrelevantNOTSelected { get; set; }
        private int TotalIrrelevant { get; set; }
        private int TotalSelected { get; set; }
        private int TotalNOTSelected { get; set; }
        private int TotalLinks { get; set; }


        //Metrics

        public double TP 
        { 
            get {return RelevantSelected;}
        }

        public double FN 
        { 
            get {return RelevantNOTSelected;}
        }

        public double TN 
        { 
            get {return IrrelevantNOTSelected;}
        }

        public double FP 
        { 
            get {return IrrelevantSelected;}
        }
        
        /// <summary>
        /// Precision/Confidence/PositivePredictiveValue -> RelevantSelected (TP) / (RelevantSelected (TP) + Irrelevant Selected(FP)); 
        /// </summary>
        public double Precision
        {
            get { return TP / (TP + FP); }
        }

        /// <summary>
        /// Recall/TruePositiveRate/sesitivity/Support -> RelevantSelected (TP) / (RelevantSelected (TP) + RelevantNOTSelected (FN))
        /// </summary>
        public double Recall
        {
            get { return TP / (TP + FN); }
        }

        /// <summary>
        /// FallOut/FalsePositiveRate -> Irrelevant Selected(FP) / (Irrelevant Selected(FP) + Irrelevant NOT Selected (TN))
        /// </summary>
        public double Fall_Out
        {
            get { return FP / (FP + TN); }
        }

        /// <summary>
        /// Specifity/TrueNegativeRate -> Irrelevant NOT Selected (TN) / (Irrelevant NOT Selected (TN) + Irrelevant Selected(FP))
        /// </summary>
        public double Specifity
        { 
            get { return TN / (TN + FP); } 
        }

        /// <summary>
        /// FalseNegativeRate/1-Recall -> RelevantNOTSelected (FN) / (RelevantSelected (TP) + RelevantNOTSelected (FN))
        /// </summary>
        public double FalseNegativeRate
        {
            get { return FN / (TP + FN); }
        }

        /// <summary>
        /// NegativePredicitveValue -> Irrelevant NOT Selected (TN) / (Irrelevant NOT Selected (TN) + RelevantNOTSelected (FN))
        /// </summary>
        public double NegativePredicitveValue
        {
            get { return TN / (TN + FN); }
        }

        /// <summary>
        /// FalsePositiveErrorRate or 1-Precision -> Irrelevant Selected(FP) / (Irrelevant Selected(FP) + RelevantSelected (TP))
        /// </summary>
        public double FalsePositiveErrorRate
        {
            get { return FP / (FP + TP); }
        }

        /// <summary>
        /// Accuracy -> (RelevantSelected (TP) + Irrelevant NOT Selected (TN)) / TotalLinks (TP + TN + FP + FN)
        /// </summary>
        public double Accuracy
        {
            get { return (TP + TN) / (TP + TN + FP + FN); }
        }

        /// <summary>
        /// OddsRatio -> (RelevantSelected (TP) * Irrelevant NOT Selected (TN)) / (Irrelevant Selected(FP) * RelevantNOTSelected (FN))
        /// </summary>
        public double OddsRatio
        {   
            get { return (TP * TN) / (FP * FN); } 
        }

        /// <summary>
        /// (2 * precision * recall) / (precision + recall) and is in [0, 1]. Higher F-measure means better suggester.
        /// </summary>
        public double FMeasure
        {
            get { return (2 * Precision * Recall) / (Precision + Recall); }
        }


        #endregion //prop


        #region ctor

        public SuggesterEvaluator(Suggester sugg)
        {
            ToEvaluate = sugg;

            if (readDesiredResultMatrixForSuggester())//if benchmark exists
                analyseResults();
        }

        public SuggesterEvaluator(double [,] suggesterResultMatrix, double [,] desireResultMatrix)
        {
            SuggesterResultMatrix = suggesterResultMatrix;
            DesiredResultMatrix = desireResultMatrix;

            if ((SuggesterResultMatrix != null) && (DesiredResultMatrix != null))
                analyseResults();
        }

        #endregion //ctor


        #region read desired results

        public bool readDesiredResultMatrixForSuggester()
        {
            if (ToEvaluate != null)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(getDirectory(@"Suggester\Evaluation Files\EvaluationBenchmark.xml"));

                //if (ToEvaluate.LastResults == null)//suggester has not worked at all
                //    ToEvaluate.imFeelingLucky();

                DesiredResultMatrix = new double[ToEvaluate.LastResults.GetLongLength(0), ToEvaluate.LastResults.GetLongLength(1)];

                //check if an OK benchmark exists
                XmlNode check = xdoc.SelectSingleNode("Benchmarks/Benchmark[@LHS='" + ToEvaluate.sourceASTL.Root.Name + "' and @RHS='" + ToEvaluate.targetASTL.Root.Name + "' and @tag='OK']");
                
                if (check != null)
                {
                    XmlNodeList correspondences = check.SelectNodes("Corr");

                    
                    foreach (XmlNode node in correspondences)
                    {
                        XmlNode lhs = node.SelectSingleNode("LHS");
                        XmlNode rhs = node.SelectSingleNode("RHS");

                        string lhsAddress = lhs.InnerText;
                        string rhsAddress = rhs.InnerText;
                        
                        int i = -1;
                        int j = -1;

                        for (int I = 0; I < ToEvaluate.sourceASTL.abstractLatticeGraph.Vertices.Count(); I++)
                            if (ToEvaluate.sourceASTL.abstractLatticeGraph.Vertices.ElementAt(I).Address.Equals(lhsAddress))
                            {
                                i = I;
                                break;
                            }

                        for (int J = 0; J < ToEvaluate.targetASTL.abstractLatticeGraph.Vertices.Count(); J++)
                            if (ToEvaluate.targetASTL.abstractLatticeGraph.Vertices.ElementAt(J).Address.Equals(rhsAddress))
                            {
                                j = J;
                                break;
                            }

                        //MessageBox.Show("i: " + i.ToString() + " -> " + lhsAddress + "\nj: " + j.ToString() + " -> " + rhsAddress);//fir testing

                        if ((i != -1) && (j != -1))
                            DesiredResultMatrix[i, j] = 1;
                        else
                        {
                            string notFound = (i == -1 ? lhsAddress : rhsAddress);

                            MessageBox.Show("Could not find " + notFound + " in Suggester elements", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }

                    return true;
                    //MessageBox.Show("Desired results matrix loaded", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                }//if (check != null)
                else
                {
                    MessageBox.Show("OK benchmark does not exists for: "+ToEvaluate.sourceASTL.Root.Name + " -> " + ToEvaluate.targetASTL.Root.Name, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }//if (ToEvaluate != null)
            else
            {
                MessageBox.Show("Suggester to evaluate is not specified!", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

        }

        //degraded
        private void readDesiredResultMatrixFromFile(string fileName)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(fileName);

            if (ToEvaluate != null)
            {
                if (ToEvaluate.LastResults == null)//suggester has not worked at all
                    ToEvaluate.imFeelingLucky();

                DesiredResultMatrix = new double[ToEvaluate.LastResults.GetLongLength(0), ToEvaluate.LastResults.GetLongLength(1)];

                XmlNodeList correspondences = xdoc.SelectNodes("Corrs/Corr");

                //Collection<suggestionVector> vector = ToEvaluate.prepareSuggestionVector(ToEvaluate.LastResults);


                foreach (XmlNode node in correspondences)
                {
                    XmlNode lhs = node.SelectSingleNode("LHS");
                    XmlNode rhs = node.SelectSingleNode("RHS");

                    string lhsAddress = lhs.InnerText;
                    string rhsAddress = rhs.InnerText;

                    int i = -1;
                    int j = -1;

                    for (int I = 0; I < ToEvaluate.sourceASTL.abstractLatticeGraph.Vertices.Count(); I++)
                        if (ToEvaluate.sourceASTL.abstractLatticeGraph.Vertices.ElementAt(I).Address.Equals(lhsAddress))
                        {
                            i = I;
                            break;
                        }

                    for (int J = 0; J < ToEvaluate.targetASTL.abstractLatticeGraph.Vertices.Count(); J++)
                        if (ToEvaluate.targetASTL.abstractLatticeGraph.Vertices.ElementAt(J).Address.Equals(rhsAddress))
                        {
                            j = J;
                            break;
                        }

                    //MessageBox.Show("i: " + i.ToString() + " -> " + lhsAddress + "\nj: " + j.ToString() + " -> " + rhsAddress);//fir testing

                    if ((i != -1) && (j != -1))
                        DesiredResultMatrix[i, j] = 1;
                    else
                    {
                        string notFound = (i == -1 ? lhsAddress : rhsAddress);

                        MessageBox.Show("Could not find "+notFound+ "in Suggester elements", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                //MessageBox.Show("Desired results matrix loaded", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Suggester to evaluate is not specified!", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion //read desired results


        #region evaluator's analysis

        public void analyseResults(){

            if (ToEvaluate == null || DesiredResultMatrix == null)
                throw new NullReferenceException("Suggester to evaluate or DesiredResultMatrix is null -> SuggesterEvaluator.analyseResults()");
            else
            {
                if (SuggesterResultMatrix == null)
                {
                    if (ToEvaluate.LastResults != null)
                        SuggesterResultMatrix = ToEvaluate.LastResults;
                    else
                        SuggesterResultMatrix = ToEvaluate.imFeelingLucky();
                }

                RelevantSelected = 0;
                RelevantNOTSelected = 0;
                TotalRelevant = 0;
                IrrelevantSelected = 0; 
                IrrelevantNOTSelected = 0;
                TotalIrrelevant = 0;
                TotalSelected = 0;
                TotalNOTSelected = 0;
                TotalLinks =0;
                
                //MessageBox.Show(MatrixLibrary.Matrix.PrintMat(DesiredResultMatrix));
                
                for (int i = 0; i < DesiredResultMatrix.GetLength(0); i++)
                    for (int j = 0; j < DesiredResultMatrix.GetLength(1); j++){
                        
                        TotalLinks++;

                        if ((DesiredResultMatrix[i, j] > 0) && (SuggesterResultMatrix[i, j] > 0)) //(TP)
                            RelevantSelected++;

                        if ((DesiredResultMatrix[i, j] > 0) && (SuggesterResultMatrix[i, j] <= 0))//(FN)
                            RelevantNOTSelected++;

                        if (DesiredResultMatrix[i, j] > 0)//(T)
                            TotalRelevant++;

                        if ((DesiredResultMatrix[i, j] <= 0) && (SuggesterResultMatrix[i, j] <= 0))//(TN)
                            IrrelevantNOTSelected++;

                        if ((DesiredResultMatrix[i, j] <= 0) && (SuggesterResultMatrix[i, j] > 0))//(FP)
                            IrrelevantSelected++;

                        if (DesiredResultMatrix[i, j] <= 0)//(F)
                            TotalIrrelevant++;

                        if (SuggesterResultMatrix[i, j] > 0)//(P)
                            TotalSelected++;

                        if (SuggesterResultMatrix[i, j] <= 0)//(N)
                            TotalNOTSelected++;                        
                        
                    }

                
                //check evaluations
                if (RelevantSelected + RelevantNOTSelected != TotalRelevant)
                    MessageBox.Show("Threats to internal valididty -> TP(Relevant selected) + FP (Relevant NOT selected) != Total Positive (Relevant)", "Internal Validity", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                if (IrrelevantSelected + IrrelevantNOTSelected != TotalIrrelevant)
                    MessageBox.Show("Threats to internal valididty -> TN(Irrelevant selected) + FN (Irelevant NOT selected) != Total Negative (Irrelevant)", "Internal Validity", MessageBoxButton.OK, MessageBoxImage.Warning);

                //show results
                //MessageBox.Show(printAnalysisResults(),"Evaluation Results");
                //saveAnalysisResultToXML();
            }
        }

        #endregion //evaluator's analysis


        #region analysis results

        public string printAnalysisResults()
        {
            StringBuilder sb = new StringBuilder("Evaluation Results:");
            sb.AppendLine();
            sb.Append(ToEvaluate.Configuration.ToString());
            sb.AppendLine();
            sb.AppendFormat("{0,-35}{1:0.###} {2,-35}{3:0.###}", "Recall: ", Recall, "Precision: ", Precision);
            sb.AppendLine();
            sb.AppendFormat("{0,-35}{1:0.###} {2,-35}{3:0.###}", "Fall_Out: ", Fall_Out, "Specifity: ", Specifity);
            sb.AppendLine();
            sb.AppendFormat("{0,-35}{1:0.###} {2,-35}{3:0.###}", "FalseNegativeRate: ", FalseNegativeRate, "NegativePredicitveValue: ", NegativePredicitveValue);
            sb.AppendLine();
            sb.AppendFormat("{0,-35}{1:0.###} {2,-35}{3:0.###}", "FalsePositiveErrorRate: ", FalsePositiveErrorRate, "Accuracy: ", Accuracy);
            sb.AppendLine();
            sb.AppendFormat("{0,-35}{1:0.###} {2,-35}{3:0.###}", "OddsRatio: ", OddsRatio, "FMeasure: ", FMeasure);
            sb.AppendLine();

            return sb.ToString();
        }

        private void saveAnalysisResultToXML()
        {
            XmlDocument xdoc = new XmlDocument();

            XmlNode results = xdoc.CreateElement("Evaluation");

            //write when
            XmlAttribute date = xdoc.CreateAttribute("date");
            date.AppendChild(xdoc.CreateTextNode(System.DateTime.Now.ToString()));
            results.Attributes.Append(date);

            //write what's going to be evaluated
            XmlAttribute lhs = xdoc.CreateAttribute("LHS");

            string smodelFile;
            //if (!string.IsNullOrEmpty(ToEvaluate.sourceASTL.ModelFile))
            //    smodelFile = ToEvaluate.sourceASTL.ModelFile.Substring(ToEvaluate.sourceASTL.ModelFile.LastIndexOf('\\')+1);
            //else
                smodelFile = ToEvaluate.sourceASTL.Root.Name;
            lhs.AppendChild(xdoc.CreateTextNode(smodelFile));
            
            results.Attributes.Append(lhs);

            XmlAttribute rhs = xdoc.CreateAttribute("RHS");

            string tmodelFile;
            //if (!string.IsNullOrEmpty(ToEvaluate.targetASTL.ModelFile))
            //    tmodelFile = ToEvaluate.targetASTL.ModelFile.Substring(ToEvaluate.targetASTL.ModelFile.LastIndexOf('\\') + 1);
            //else
                tmodelFile = ToEvaluate.targetASTL.Root.Name;
            rhs.AppendChild(xdoc.CreateTextNode(tmodelFile));
            
            results.Attributes.Append(rhs);

            //write suggester configuration
            XmlNode config = ToEvaluate.Configuration.toXML();

            results.AppendChild(results.OwnerDocument.ImportNode(config, true));

            //write results
            XmlNode tempPrecision = xdoc.CreateElement("Precision");
            tempPrecision.AppendChild(xdoc.CreateTextNode(Precision.ToString()));
            results.AppendChild(tempPrecision);

            XmlNode tempRecall = xdoc.CreateElement("Recall");
            tempRecall.AppendChild(xdoc.CreateTextNode(Recall.ToString()));
            results.AppendChild(tempRecall);

            XmlNode tempFall_Out = xdoc.CreateElement("Fall_Out");
            tempFall_Out.AppendChild(xdoc.CreateTextNode(Fall_Out.ToString()));
            results.AppendChild(tempFall_Out);

            XmlNode tempSpecifity = xdoc.CreateElement("Specifity");
            tempSpecifity.AppendChild(xdoc.CreateTextNode(Specifity.ToString()));
            results.AppendChild(tempSpecifity);

            XmlNode tempFalseNegativeRate = xdoc.CreateElement("FalseNegativeRate");
            tempFalseNegativeRate.AppendChild(xdoc.CreateTextNode(FalseNegativeRate.ToString()));
            results.AppendChild(tempFalseNegativeRate);

            XmlNode tempNegativePredicitveValue = xdoc.CreateElement("NegativePredicitveValue");
            tempNegativePredicitveValue.AppendChild(xdoc.CreateTextNode(NegativePredicitveValue.ToString()));
            results.AppendChild(tempNegativePredicitveValue);

            XmlNode tempFalsePositiveErrorRate = xdoc.CreateElement("FalsePositiveErrorRate");
            tempFalsePositiveErrorRate.AppendChild(xdoc.CreateTextNode(FalsePositiveErrorRate.ToString()));
            results.AppendChild(tempFalsePositiveErrorRate);

            XmlNode tempAccuracy = xdoc.CreateElement("Accuracy");
            tempAccuracy.AppendChild(xdoc.CreateTextNode(Accuracy.ToString()));
            results.AppendChild(tempAccuracy);

            XmlNode tempOddsRatio = xdoc.CreateElement("OddsRatio");
            tempOddsRatio.AppendChild(xdoc.CreateTextNode(OddsRatio.ToString()));
            results.AppendChild(tempOddsRatio);

            XmlNode tempFMeasure = xdoc.CreateElement("FMeasure");
            tempFMeasure.AppendChild(xdoc.CreateTextNode(FMeasure.ToString()));
            results.AppendChild(tempFMeasure);

            //save them
            //if (File.Exists((getDirectory(@"Suggester\Evaluation Files\EvaluationResults.xml")).Replace(@"file:\", "")))
            //{
            //    xdoc.Load((getDirectory(@"Suggester\Evaluation Files\EvaluationResults.xml")).Replace(@"file:\", ""));
            //    XmlNode docElement = xdoc.SelectSingleNode("Evaluations");
            //    docElement.AppendChild(results);

            //    xdoc.Save((getDirectory(@"Suggester\Evaluation Files\EvaluationResults.xml")).Replace(@"file:\",""));
            //}
            //else
            //{

            //always Save to a new file
                XmlNode docElement = xdoc.CreateElement("Evaluations");
                docElement.AppendChild(results);

                xdoc.AppendChild(docElement);

                string fileStr = System.DateTime.Now.ToString();
                fileStr = fileStr.Replace(" ", "");
                fileStr = fileStr.Replace(":","");
                fileStr = fileStr.Replace("/","");
                fileStr = getDirectory(@"Suggester\EvaluationResults\Test" + fileStr + ".xml");
                fileStr = fileStr.Replace(@"file:\", "");
                //MessageBox.Show(fileStr);
                xdoc.Save(fileStr);
            //}
        }

        #endregion //analysis results

        private string getDirectory(string c)
        {
            string p = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string pbase = System.IO.Path.GetDirectoryName((System.IO.Path.GetDirectoryName(p)));

            return System.IO.Path.Combine(pbase, c);
        }
    }


}
