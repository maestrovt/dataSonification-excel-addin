using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace DataSonificationLib
{
    public abstract class SonifyMessage
    {
        const string xmlMsgVersion = "0.2";
        const string xmlMsgHdr = "<message version=\"" + xmlMsgVersion + "\">";
        const string xmlMsgTrlr = "</message>";


        protected static Dictionary<SonifyClient, int> sequenceNumbers = new Dictionary<SonifyClient, int>();
        protected abstract void AppendMessageContent(StringBuilder messageBuilder);
        private int sequenceNumber;
        protected int db_id;
        public string ToString(SonifyClient client)
        {
            lock (sequenceNumbers)
            {
                if (!sequenceNumbers.ContainsKey(client))
                    sequenceNumbers[client] = 0;

               
                sequenceNumber = sequenceNumbers[client];
                sequenceNumbers[client]++;
            }

            db_id = client.DbId;
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.Append(xmlMsgHdr);
            AppendMessageContent(messageBuilder);
            messageBuilder.Append(xmlMsgTrlr);

            return messageBuilder.ToString();
        }

        public static SonifyMessage ParseMessage(string xmlMessage)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(xmlMessage));
            reader.WhitespaceHandling = WhitespaceHandling.None;
            reader.ReadStartElement("message");

            switch (reader.Name)
            {
                case "control":
                    return new SonifyControlMessage(reader);
                case "data":
                    return new SonifyDataMessage(reader);
            }

            return null;
        }


        private Dictionary<string, string> fields = new Dictionary<string,string>();

        const string fieldFormat = "<field key=\"{0}\">{1}</field>";

        const string xmlFieldsHdr = "<fields>";
        const string xmlFieldsTrlr = "</fields>";
        protected void AppendFields(StringBuilder messageBuilder)
        {
            messageBuilder.Append(xmlFieldsHdr);
            foreach(var field in fields)
            {
                messageBuilder.AppendFormat(fieldFormat, field.Key, field.Value);
            }
            messageBuilder.Append(xmlFieldsTrlr);
        }
        const string sequenceFormat = "<sequence>{0}</sequence>";

        protected void AppendSequence(StringBuilder messageBuilder)
        {
            messageBuilder.AppendFormat(sequenceFormat, sequenceNumber);
        }

        protected void ReadSequence(XmlTextReader reader)
        {
            reader.ReadStartElement("sequence");
            sequenceNumber = int.Parse(reader.Value);
            reader.Read();
            reader.ReadEndElement();
        }


        protected void ReadFields(XmlTextReader reader)
        {
            reader.ReadStartElement("fields");
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                string name = reader.GetAttribute("key");
                reader.ReadStartElement("field");
                string value = reader.Value;
                reader.Read();
                reader.ReadEndElement();
                fields[name] = value;
                
            }
            reader.ReadEndElement();
        }

        public IDictionary<string, Stamp> Stamps { get { return stamps; } }

        protected void ReadStamps(XmlTextReader reader)
        {
            stamps = new Dictionary<string, Stamp>();

            reader.ReadStartElement("stamps");
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                string name = reader.GetAttribute("key");
                string status = reader.GetAttribute("status");
                string _return = reader.GetAttribute("return");
                reader.ReadStartElement("stamp");
                string value = reader.Value;
                reader.Read();
                reader.ReadEndElement();
                stamps[name] = new Stamp() { Return = _return, Status = status, Value = value };

            }

            reader.ReadEndElement();
        }

        private Dictionary<string, Stamp> stamps;

        public struct Stamp
        {
            public string Status { get; set; }
            public string Return { get; set; }
            public string Value { get; set; }
        }


        public void AddField(string name, string value){
            fields[name]=value;
        }

    }



    public class SonifyControlMessage : SonifyMessage
    {
        const string xmlControlHdr = "<control>";
        const string xmlControlTrlr = "</control>";
        const string xmlActionInitExitFormat = "<action>{0}</action><object><core/></object>";
        const string xmlActionStartStopFormat = "<action>{0}</action><object><db_id>{1}</db_id></object>"; 

        public enum ControlAction
        {
            init,
            start,
            stop,
            exit,
        }

        private ControlAction action;

        public ControlAction Action { get { return action; } }

        public SonifyControlMessage(ControlAction action)
        {
            this.action = action;
        }

        public SonifyControlMessage(XmlTextReader reader)
        {
            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                switch (reader.Name)
                {
                    case "sequence":
                        ReadSequence(reader);
                        break;
                    case "action":
                        reader.ReadStartElement();
                        action =(ControlAction)Enum.Parse(typeof(ControlAction), reader.Value.ToLower());
                        reader.Read();
                        reader.ReadEndElement();
                        break;
                    
                    case "fields":
                        ReadFields(reader);
                        break;
                    case "stamps":
                        ReadStamps(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        protected override void AppendMessageContent(StringBuilder messageBuilder)
        {
            messageBuilder.Append(xmlControlHdr);
            AppendSequence(messageBuilder);
            if(action == ControlAction.init || action == ControlAction.exit)
                messageBuilder.AppendFormat(xmlActionInitExitFormat, action.ToString());
            else
                messageBuilder.AppendFormat(xmlActionStartStopFormat, action.ToString(), db_id);

            AppendFields(messageBuilder);
            messageBuilder.AppendFormat(xmlControlTrlr);
            
        }
    }

    public class SonifyDataMessage : SonifyMessage
    {
        int s_id;
        string ticker;


        const string xmlDataHdr = "<data>";
        const string xmlDataTrlr = "</data>";
        
        const string xmlSidFormat = "<object><db_id>{2}</db_id><s_id name=\"{0}\">{1}</s_id></object>"; 

        public SonifyDataMessage(int s_id, string ticker)
        {
            this.s_id = s_id;
            this.ticker = ticker;
        }

        public SonifyDataMessage(XmlTextReader reader)
        {
            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                switch (reader.Name)
                {
                    case "sequence":
                        ReadSequence(reader);
                        break;
                    case "stamps":
                        ReadStamps(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        protected override void AppendMessageContent(StringBuilder messageBuilder)
        {
            messageBuilder.Append(xmlDataHdr);
            AppendSequence(messageBuilder);
           
            messageBuilder.AppendFormat(xmlSidFormat, ticker, s_id, db_id);
            AppendFields(messageBuilder);
            messageBuilder.AppendFormat(xmlDataTrlr);
        }
    }
}
