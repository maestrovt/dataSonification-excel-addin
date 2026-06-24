using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataSonificationConfiguration
{
    public abstract class DataSonificationDBObject
    {
        

        private Dictionary<string, object> properties = new Dictionary<string,object>();
        public object this[string property] 
        {
            get
            {
                return properties[property]; 
            }
            protected set
            {
                if (property.Equals(idColumn))
                    throw new InvalidOperationException("Cannot set ID of object");

                if (!properties.ContainsKey(property))
                    throw new KeyNotFoundException("Invalid property: " + property);

                if (!properties[property].GetType().Equals(value.GetType()))
                    throw new InvalidOperationException("Invalid Type for " + property + " expecting " + properties[property].GetType());

                properties[property] = value;
            }
        }

        public IEnumerable<string> Properties
        {
            get
            {
                return properties.Keys;
            }
        }

        public void SaveChanges()
        {
            db.SaveDBObject(tableName, idColumn, properties);
        }
        private string idColumn;
        private string tableName;

        private DataSonificationDB db;

        protected string TableName { get { return tableName; } }
        protected DataSonificationDB DB { get { return db; } }

        // Create an empty DB Object (doesn't have storage in database unless properties are set
        // and savechanges is called
        protected DataSonificationDBObject(string tableName, string idColumn, DataSonificationDB db)
        {
            this.tableName = tableName;
            properties = null;
            this.db = db;
            this.idColumn = idColumn;
        }

        protected void InitializeProperties(Dictionary<string, object> defaultValues)
        {
            if (properties != null)
                throw new InvalidOperationException("You can only initialize properties on empty objects");

            // Get next available id
            properties = db.GetDBObject(tableName, idColumn, defaultValues);
            
        }

        protected DataSonificationDBObject(string tableName, string idColumn, int id, DataSonificationDB db)
        {
            this.tableName = tableName;
            this.idColumn = idColumn;
            properties = db.GetDBObject(tableName, idColumn, id);
            this.db = db;
        }


        protected static void AddDefaultAttributes(XElement xElement, Dictionary<string, object> defaultValues)
        {
            foreach (XAttribute attr in xElement.Attributes())
            {
                if (attr.Name.LocalName == "tableName")
                    continue;

                defaultValues[attr.Name.LocalName] = attr.Value;
            }
        }

        public virtual void Delete()
        {
            if(properties != null)
                db.DeleteDBObject(tableName, idColumn, properties[idColumn]);
        }



        public const string TickerIdKey = "s_id";
    }
}
