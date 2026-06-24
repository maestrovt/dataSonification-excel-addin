using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataSonificationConfiguration
{
    public class Trainer : DataSonificationDBObject
    {
        private static string defaultTrainerType;
        public static string DefaultTrainerType
        {
            get
            {
                if (defaultTrainerType == null)
                {
                    foreach (var trainerType in trainerTypes)
                    {
                        if ("true".Equals(trainerType.Value.Attribute("defaultType").Value))
                        {
                            defaultTrainerType = trainerType.Key;
                            break;
                        }
                    }
                    if (defaultTrainerType == null)
                        throw new InvalidOperationException("Could not find default trainer, make sure to add defaultType='true' to types.xml");

                }

                
                return defaultTrainerType;
            }

        }

        public string Type
        {
            get { return TableName; }
        }

        private static Dictionary<string,XElement> trainerTypes = DBObjectTypes.TrainerTypes;
        public static Trainer FindTrainer(string tableName, int id, DataSonificationDB db)
        {
            if (!trainerTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Trainer Type: " + tableName);
            if ("true".Equals(trainerTypes[tableName].Attribute("defaultType").Value))
                return new Trainer(tableName, db);

            return new Trainer(tableName, id, db);
        }

        public static Trainer CreateTrainer(string tableName, int id, DataSonificationDB db)
        {
            if (!trainerTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Trainer Type: " + tableName);

            if ("true".Equals(trainerTypes[tableName].Attribute("defaultType").Value))
                return new Trainer(tableName, db);

            Dictionary<string, object> defaultValues = new Dictionary<string, object>();
            defaultValues[TickerIdKey] = id;
            AddDefaultAttributes(trainerTypes[tableName], defaultValues);

            return new Trainer(tableName, defaultValues, db);

        }

        private Trainer(string tableName, Dictionary<string, object> defaultValues, DataSonificationDB db)
            : base(tableName, TickerIdKey, db)
        {
            InitializeProperties(defaultValues);
        }

        private Trainer(string tableName, DataSonificationDB db)
            : base(tableName, TickerIdKey, db)
        { }

        private Trainer(string tableName, int id, DataSonificationDB db)
            : base(tableName, TickerIdKey, id, db)
        {
            
        }

    }
}
