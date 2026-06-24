using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelDna.Integration;
using System.Xml;
using System.IO;
using DataSonificationLib;
using System.Reflection;

namespace DataSonificationFunctions
{
    public class SonifyFunctions
    {

        public static DataSonificationConfiguration.DataSonificationDB db;
        public static DataSonificationLib.SonifyClient client;
        private static string initError = null;

        static SonifyFunctions()
        {
            try
            {
                string sonifyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                client = new DataSonificationLib.SonifyClient(2011, sonifyPath);
                db = new DataSonificationConfiguration.DataSonificationDB(client.ConfigDatabaseFile);
                DataSonificationLib.SonifyStrategy.LoadStrategies(new XmlTextReader(File.OpenRead(Path.Combine(sonifyPath, "SonifyStrategies.xml"))));
                client.Start();
            }
            catch (Exception ex)
            {
                // Log initialization error
                try
                {
                    string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "sonify_init_error.log");
                    File.WriteAllText(logPath, DateTime.Now + ": INIT ERROR\n" + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace + "\n");
                    if (ex.InnerException != null)
                    {
                        File.AppendAllText(logPath, "Inner: " + ex.InnerException.GetType().Name + ": " + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace + "\n");
                    }
                }
                catch { }
                initError = ex.GetType().Name + ": " + ex.Message;
            }
        }

        [ExcelFunction(Description="Does dataSonification Sonify", Category="Useful functions")]
        public static string Sonify(string name, object arg0, object arg1, object arg2)
        {
            if (initError != null)
            {
                return "#INIT:" + initError;
            }
            try
            {
                int sid = db.GetSID(name);
                string analyzer = db.GetAnalyzer(sid);
                string arranger = db.GetArranger(sid);
                var strategy = SonifyStrategy.ForAnalyzerArranger(analyzer, arranger);


                double[] args = new double[strategy.ParameterCount];

                if (args.Length == 2)
                {
                    args[0] = (double)arg0;
                    args[1] = (double)arg1;
                }
                else if (args.Length == 3)
                {
                    args[0] = (double)arg0;
                    args[1] = (double)arg1;
                    args[2] = (double)arg2;
                }

                var dataMessage = strategy.CreateDataMessage(sid, name, args);
                client.SendDataMessage(dataMessage);

                return "♫" + name + "♫";
            }
            catch (ArgumentException)
            {
                return "#NARG";
            }
            catch (Exception ex)
            {
                // Log to file for debugging
                string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sonify_error.log");
                File.AppendAllText(logPath, DateTime.Now + ": " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace + "\n\n");
                return "#ERR:" + ex.GetType().Name;
            }
        }

    }
}
