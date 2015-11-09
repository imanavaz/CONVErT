using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace CONVErT
{
    public class Logger : ListBox
    {
        #region props

        ObservableCollection<String> logs;

        StreamWriter logFile;

        private string name;

        #endregion //props

        #region ctor

        public Logger(String LoggerName)
        {
            logs = new ObservableCollection<String>();
            this.DataContext = logs;
            
            var converter = new System.Windows.Media.BrushConverter();
            this.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#ecf0f1");
            name = LoggerName;

        }

        #endregion //ctor

        #region destructor

        ~Logger()
        {
            saveLogsToFile();
        }

        private void saveLogsToFile()
        {
            string path = DirectoryHelper.getFilePathExecutingAssembly(name + ".txt");

            if (logs.Count > 0)// if there exists any logs
            {

                if (!File.Exists(path))
                {
                    logFile = new StreamWriter(path);
                }
                else
                {
                    logFile = File.AppendText(path);
                }

                logFile.WriteLine("");
                logFile.WriteLine("*************** New logging session ***************");
                
                foreach (String s in logs)
                    logFile.WriteLine(s);

                //Close the stream:
                logFile.Flush();
                logFile.Close();
            }
        }

        #endregion //destructor

        public void log(string s)
        {
            log(s, ReportIcon.Info);
        }

        public void log(string s,ReportIcon ri)
        {
            //get time
            string timestr = DateTime.Now.ToString("h:mm tt - ");
            //create a log element to be shown
            LogEntity le = new LogEntity(timestr+s,ri);
            //this.AddChild(le);
            this.Items.Insert(0, le);

            //create log event to be saved to file
            logs.Add(System.DateTime.Now.ToString() + " -> " + s);
        }

        public void clearLogs()
        {
            this.Items.Clear();
        }



    }
}
