using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataSonificationLib;
using System.Threading;
using System.Xml;
using System.IO;

namespace DataSonificationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string coreLocation = args[0];
            int port = int.Parse(args[1]);

            
            DataSonificationLib.SonifyClient client = new DataSonificationLib.SonifyClient(port, coreLocation);
            SonifyStrategy.LoadStrategies(new XmlTextReader(File.OpenRead("SonifyStrategies.xml")));
            DataSonificationConfiguration.DataSonificationDB configDb = new DataSonificationConfiguration.DataSonificationDB(client.ConfigDatabaseFile);

            SonifyStrategy movementAnalyzer = SonifyStrategy.ForName("MovementAnalyzer");
           


            client.Start();

            for (int i = 0; i < 10; i++)
            {
                int s_id = configDb.GetSID("Bassoon-MA");
                SonifyDataMessage dataMessage = movementAnalyzer.CreateDataMessage(s_id, "Bassoon-MA", i, 0);
                dataMessage.AddField("CURRENT_FIELD", i.ToString());
                dataMessage.AddField("REF1_FIELD", 0.ToString());
                client.SendDataMessage(dataMessage);
                Thread.Sleep(2000);
            }

            System.Console.In.ReadLine();
            client.Close();



        }
    }
}
