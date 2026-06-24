import sys
import clr

# To build DataSonificationConfiguration.dll you need: Visual C# 2008 Express SP1 http://www.microsoft.com/express/download/
# Load DataSonificationAddin.sln (at repo root)

# To run this script use Iron Python: http://ironpython.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=12481
# You need to add Iron Python to your path
# run: ipy TestConfiguration.py [Debug|Release]


buildType = sys.argv[1] # Debug or Release

# The location of the database to load
dbName = r"..\data\dataSonification.db"

# The location of the DataSonificationConfiguration.dll library
# Override this to whatever you want
clr.AddReferenceToFileAndPath("bin\\x86\\" + buildType + "\\DataSonificationConfiguration.dll")
clr.AddReference("DataSonificationConfiguration")

from DataSonificationConfiguration import *

print "Loading DB:", dbName, "..."
db = DataSonificationDB(dbName)

# Test getting a ticker
ticker = Ticker.FindTicker("Bassoon-MA", db)
print ticker.Name, ticker.Description, ticker.Volume, ticker.Pan, ticker.Enabled, ticker.Trainer.Type, ticker.Analyzer.Type, ticker.Arranger.Type, ticker.Instrument.Type

# Test Creating a ticker
ticker = Ticker.CreateTicker("TestTicker", "MovementAnalyzer", "TwoNoteArranger", "SampleInstrument", "DefaultTrainer", db)
ticker.Delete()

# To modify any object use:

#   obj["XXXX"] = value (where xxxx is a column name in the database)
#   obj.SaveChanges() (actually writes it to the database)
#
#   You can retrieve tickers with Ticker.FindTicker()
#   You can create tickers with Ticker.CreateTicker(name, analyzer, arranger, instrument, trainer, db)
#  
#   If you want to change the analyzer / arranger / instrument / trainer you should first delete the original ticker
#   then create a new one w/ the new types
#   


#inst = Instrument.CreateInstrument("SampleInstrument", 3, db)
#print "Created Test Instrument:", inst["inst_id"], inst["remapper"]
#inst.Delete()

#inst = Instrument.CreateInstrument("JavaMidiInstrument", 3, db)
#print "Created Test Midi Instrument:", inst["inst_id"], inst["channel"]
#inst.Delete()

#analyzers = ["MovementAnalyzer", "SizeAnalyzer", "TrillAnalyzer", "TargetAnalyzer", "SliderAnalyzer"]
#for analyzerType in analyzers:
#	analyzer = Analyzer.CreateAnalyzer(analyzerType, 3, db)
#	print "Created "+analyzerType+":", analyzer["significant_move"]
#	analyzer.Delete()

#arrangers = ["TwoNoteArranger","ThreeNoteArranger","FourNoteArranger","TrillArranger","UnboundedSliderArranger","BoundedSliderArranger"]
#for arrangerType in arrangers:
#	arranger = Arranger.CreateArranger(arrangerType, 55, 3, db)
#	print "Created "+arrangerType+":", arranger["tempo"]
#	arranger.Delete()
	

print "Press Enter to Quit..."
sys.stdin.readline()