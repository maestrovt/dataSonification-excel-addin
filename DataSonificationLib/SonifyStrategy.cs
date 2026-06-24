using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DataSonificationLib
{
    public class SonifyStrategy
    {
        private string name;
        private string analyzer;
        private string arranger;

        private List<string> parameters;

        public string Name { get { return name;} private set { name = value;}}
        public string Analyzer { get { return analyzer; } private set { analyzer = value;}}
        public string Arranger { get { return arranger; } private set { arranger = value;}}
        public IEnumerable<string> Parameters { get { return parameters; } private set { parameters = new List<string>(value); } }

        private static Dictionary<string, SonifyStrategy> StrategyByAnalyzerArranger = new Dictionary<string, SonifyStrategy>();
        private static Dictionary<string, SonifyStrategy> StrategyByName = new Dictionary<string, SonifyStrategy>();
        private string StrategyKey { get { return analyzer + ":" + arranger; }}

        public SonifyStrategy(string name, string analyzer, string arranger, IEnumerable<string> parameters)
        {
            Name = name;
            Analyzer = analyzer;
            Arranger = arranger;
            
            if (StrategyByAnalyzerArranger.ContainsKey(StrategyKey))
                throw new ArgumentException("Strategy (" + StrategyByAnalyzerArranger[StrategyKey].Name + ") Already Exists for: " + analyzer + " and " + arranger);

            if (StrategyByName.ContainsKey(Name))
                throw new ArgumentException("Strategy (" + name + ") already exists");

            StrategyByAnalyzerArranger[StrategyKey] = this;
            StrategyByName[name] = this;

            Parameters = parameters;
        }

        public int ParameterCount { get { return parameters.Count; } }

        public static void LoadStrategies(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            XmlNode root = doc.ChildNodes[1];
            if (!root.Name.Equals("strategies"))
                throw new InvalidOperationException("Unexpected Node: " + root.Name);

            foreach (XmlNode strategyNode in root.ChildNodes)
            {
                string name = strategyNode.Attributes["name"].Value;
                string analyzer = strategyNode.Attributes["analyzer"].Value;
                string arranger = strategyNode.Attributes["arranger"].Value;

                string[] parameters = strategyNode.Attributes["parameters"].Value.Split(',', ' ');

                new SonifyStrategy(name, analyzer, arranger, parameters);
            }
        }

        public static SonifyStrategy ForAnalyzerArranger(string analyzer, string arranger)
        {
            return StrategyByAnalyzerArranger[analyzer + ":" + arranger];

        }

        public static SonifyStrategy ForName(string name)
        {
            return StrategyByName[name];
        }

        public SonifyDataMessage CreateDataMessage(int id, string ticker, params double[] args)
        {
            if(args.Length != parameters.Count)
                throw new ArgumentException("Invalid # of arguments expecting: " + string.Join(", ", parameters.ToArray()));

            SonifyDataMessage message = new SonifyDataMessage(id, ticker);
            for (int i = 0; i < parameters.Count; i++)
            {
                message.AddField(parameters[i], args[i].ToString());
            }

            return message;
        }

    }
}
