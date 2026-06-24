using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataSonificationConfiguration
{
    public class Instrument : DataSonificationDBObject
    {
        public const string InstrumentIdKey = "inst_id";
        public string Type
        {
            get { return TableName; }
        }

        private static Dictionary<string, XElement> instrumentTypes = DBObjectTypes.InstrumentTypes;

        public static Instrument FindInstrument(string tableName, int id, DataSonificationDB db)
        {

            if (!instrumentTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Instrument Type: " + tableName);

            return new Instrument(tableName, id, db);
        }

        public static Instrument CreateInstrument(string tableName, int id, DataSonificationDB db)
        {

            if (!instrumentTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Instrument Type: " + tableName);

            Dictionary<string, object> defaultValues = new Dictionary<string, object>();
            defaultValues[TickerIdKey] = id;
            AddDefaultAttributes(instrumentTypes[tableName], defaultValues);

            string allInstrumentsTable = (string)defaultValues["all_instruments_table"];
            defaultValues.Remove("all_instruments_table");


            defaultValues[InstrumentIdKey] = GetDefaultInstIdFor(allInstrumentsTable, db);


            return new Instrument(tableName, defaultValues, db);

        }

        private static int GetDefaultInstIdFor(string tableName, DataSonificationDB db)
        {
            return db.GetFirstId(tableName, InstrumentIdKey);
        }

        private Instrument(string tableName, Dictionary<string, object> defaultValues, DataSonificationDB db)
            : base(tableName, TickerIdKey, db)
        {
            InitializeProperties(defaultValues);
        }

        private Instrument(string tableName, int id, DataSonificationDB db)
            : base(tableName, TickerIdKey, id, db)
        {
            
        }

    }
}
