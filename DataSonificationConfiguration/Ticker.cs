using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSonificationConfiguration
{
    public class Ticker : DataSonificationDBObject
    {
        public const string TickerKey = "ticker";
        public const string DescriptionKey = "description";
        public const string AmplitudeKey = "amplitude";
        public const string PanKey = "pan";
        public const string EnabledKey = "enabled";
        public const string DataIdKey = "d_id";
        public const string AnalyzerKey = "analyzer";
        public const string ArrangerKey = "arranger";
        public const string InstrumentKey = "instrument";
        public const string TrainerKey = "trainer";


        public static Ticker FindTicker(string ticker, DataSonificationDB db)
        {
            return new Ticker(ticker, db);
        }

        public static Ticker CreateTicker(string ticker, string analyzer, string arranger, string instrument, string trainer, DataSonificationDB db)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values[TickerKey] = ticker;
            values[DescriptionKey] = "No Description";
            values[AmplitudeKey] = 0.9;
            values[PanKey] = 0.5;
            values[EnabledKey] = 1;
            values[DataIdKey] = 1;
            values[AnalyzerKey] = analyzer;
            values[ArrangerKey] = arranger;
            values[InstrumentKey] = instrument;
            values[TrainerKey] = trainer;



            return new Ticker(values, db);
        }

        private DataSonificationDB db;


        public int Id
        {
            get { return (int)(decimal)this[TickerIdKey]; }
        }

        public string Name
        {
            get { return (string)this[TickerKey]; }
            set { this[TickerKey] = value; }
        }

        public string Description
        {
            get { return (string)this[DescriptionKey]; }
            set { this[DescriptionKey] = value; }
        }

        public bool Enabled
        {
            get { return (decimal)this[EnabledKey] == 1; }
            set { this[EnabledKey] = (value ? 1 : 0); }
        }

        public decimal Volume
        {
            get { return (decimal)this[AmplitudeKey]; }
            set { 
                if(value < 0 || value > 1)
                    throw new ArgumentException("Invalid Volume got : " + value + " : expected 0-1");

                this[AmplitudeKey] = value; }
        }

        public decimal Pan
        {
            get { return (decimal)this[PanKey]; }
            set {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Invalid Pan got : " + value + " : expected 0-1");

                this[PanKey] = value;
                
            }
        }

        public Analyzer Analyzer{ get; private set;}
        public Arranger Arranger { get; private set; }
        public Trainer Trainer { get; private set; }
        public Instrument Instrument { get; private set; }


        private Ticker(Dictionary<string, object> values, DataSonificationDB db)
            : base("sonification", TickerIdKey, db)
        {
            InitializeProperties(values);
            Arranger = Arranger.CreateArranger((string)values[ArrangerKey], this.Id, 60, db);
            Analyzer = Analyzer.CreateAnalyzer((string)values[AnalyzerKey], this.Id, db);
            Instrument = Instrument.CreateInstrument((string)values[InstrumentKey], this.Id, db);
            Trainer = Trainer.CreateTrainer((string)values[TrainerKey], this.Id, db);
        }

        private Ticker(string ticker, DataSonificationDB db) : base("sonification", TickerIdKey, db.GetSID(ticker), db)
        { 
            // Get Analyzer and Arranger and Instrument
            Trainer = Trainer.FindTrainer((string)this[TrainerKey], Id, DB);
            Analyzer = Analyzer.FindAnalyzer((string)this[AnalyzerKey], Id, DB);
            Arranger = Arranger.FindArranger((string)this[ArrangerKey], Id, DB);
            Instrument = Instrument.FindInstrument((string)this[InstrumentKey], Id, DB);
        }

        public override void Delete()
        {
            base.Delete();
            Analyzer.Delete();
            Arranger.Delete();
            Instrument.Delete();
            Trainer.Delete();
        }

    }
}
