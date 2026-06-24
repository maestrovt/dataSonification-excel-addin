using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;


namespace DataSonificationLib
{
    public class SonifyClient
    {
        private System.Net.Sockets.TcpClient socket;
        private Stream socketStream;

        private StreamWriter socketWriter;
        private StreamReader socketReader;

        private System.Diagnostics.Process coreProcess;
        private Thread coreMonitorThread;
        private string deploymentLocation;
        private int port;

        public SonifyClient(int port, string deploymentLocation)
        {
            this.port = port;
            this.deploymentLocation = deploymentLocation;

            ConnectToCore();

            
            StartMessageLoop();

        }

        private void ConnectToCore()
        {
            if (!TryConnect())
            {
                LaunchCore();
                bool connected = false;
                for (int i = 0; i < 5; i++)
                {
                    if (TryConnect())
                    {
                        connected = true;
                        break;
                    }

                    Thread.Sleep(1000);
                }
                if (!connected)
                    throw new InvalidOperationException("Could not connect to core");
            }
        }

        

        private void LaunchCore()
        {
            if (!File.Exists(CorePath))
            {
                throw new InvalidOperationException("Could not find dataSonification.jar at: " + deploymentLocation);
            }

            if (!File.Exists(JavaPath))
            {
                throw new InvalidOperationException("Could not find java.exe at: " + deploymentLocation + "\\jre\\bin");
            }

            coreProcess = new System.Diagnostics.Process();
            coreProcess.StartInfo.FileName = JavaPath;
            coreProcess.StartInfo.Arguments = CoreArguments;

            coreProcess.StartInfo.RedirectStandardOutput = false;
            
            coreProcess.StartInfo.UseShellExecute = false;
            coreProcess.Start();

            int parentid = System.Diagnostics.Process.GetCurrentProcess().Id;
            System.Diagnostics.Process.Start(Path.Combine(deploymentLocation, "DataSonificationMonitor.exe"), parentid + " " + coreProcess.Id);

            //coreProcess.WaitForInputIdle();

          /*  if (coreMonitorThread != null && coreMonitorThread.IsAlive)
            {
                coreMonitorThread.Abort();
            }*/

           // coreMonitorThread = new Thread((ThreadStart)MonitorCore);
           // coreMonitorThread.Start();
           // while (!coreRunning)
            {
               Thread.Sleep(1000);
            }
            

        }

        private bool coreRunning = false;

        private void MonitorCore()
        {
            using (var stdout = coreProcess.StandardOutput)
            {
                string line = stdout.ReadLine();
                while (line != null)
                {
                    Console.WriteLine("CORE: " + line);

                    if (line.Equals("RUNNING on port " + port.ToString()))
                        coreRunning = true;

                    line = stdout.ReadLine();
                }

            }
        }

        private string JavaPath
        {
            get
            {
                return Path.Combine(deploymentLocation, "jre\\bin\\java.exe");
            }
        }

        private string CoreArguments
        {
            get
            {
                return "-cp \"" + CorePath + "\" " + MainClass + " -PORT " + port;
            }
        }

        private string MainClass
        {
            get
            {
                return "com.dataSonification.v2.MainDaemon";
            }
        }

        private string CorePath
        {
            get
            {
                return Path.Combine(deploymentLocation, "dataSonification.jar");
            }

        }

        private bool TryConnect()
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Close();
                }

                socket = new System.Net.Sockets.TcpClient();
                socket.Connect(IPAddress.Loopback, port);
                socketStream = socket.GetStream();
                socketReader = new StreamReader(socketStream);
                socketWriter = new StreamWriter(socketStream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int dbId = -1;
        const string xmlDataHdr = "<data>";
        const string xmlDataTrlr = "</data>";


        const string dbDriver = "org.sqlite.JDBC";
        const string dbName = "dataSonification.db";

        const string DB_ID = "DB_ID";

        const string INST_DB_NAME = "INST_DB_NAME";
        const string INST_DB_DRIVER = "INST_DB_DRIVER";

        const string DB_NAME = "DB_NAME";
        const string DB_DRIVER = "DB_DRIVER";
        const string SAMPLE_DIR = "SAMPLE_DIR";

        public string SampleDirectory
        {
            get
            {
                return Path.Combine(deploymentLocation, "Samples");
            }
        }
        private string ConfigDatabase
        {

            get
            {
                return "jdbc:sqlite:" + Path.Combine(deploymentLocation, dbName);
            }
        }


        public string ConfigDatabaseFile
        {

            get
            {
                return Path.Combine(deploymentLocation, dbName);
            }
        }


        // Stage 6 instrumentation: serialize all coreState reads/writes and log
        // every transition. The field is read from the Excel UI thread
        // (SendDataMessage), from MessageLoop (KillCore), and from the
        // Send{Init,Start,Stop,Exit} helpers — H3 in the crash investigation.
        private readonly object stateLock = new object();
        private CoreState _coreState = CoreState.Exited;
        private CoreState coreState
        {
            get { lock (stateLock) { return _coreState; } }
            set
            {
                lock (stateLock)
                {
                    CoreState old = _coreState;
                    if (old != value)
                    {
                        _coreState = value;
                        SonifyLog.Write("coreState " + old + " -> " + value + " (db_id=" + dbId + ")");
                    }
                }
            }
        }
        private enum CoreState
        {
            Exited,
            Running,
            Stopped
        }

        public void Start()
        {
            SendInit();
            SendStart();
        }

        private void SendInit()
        {

            if (coreState != CoreState.Exited)
                throw new InvalidOperationException("Core can only be initialized from exited state");

            SonifyControlMessage controlMessage = new SonifyControlMessage(SonifyControlMessage.ControlAction.init);

            controlMessage.AddField(DB_NAME, ConfigDatabase);
            controlMessage.AddField(DB_DRIVER, dbDriver);

            controlMessage.AddField(INST_DB_NAME, ConfigDatabase);
            controlMessage.AddField(INST_DB_DRIVER, dbDriver);
            controlMessage.AddField(SAMPLE_DIR, SampleDirectory);

            SendMessage(controlMessage);
            
            SonifyMessage response = SonifyMessage.ParseMessage(ReadMessage());
            dbId = int.Parse(response.Stamps["UI"].Value);
            SonifyLog.Write("SendInit: assigned db_id=" + dbId);
            if (!response.Stamps["UI"].Status.Equals("SUCCESS"))
                throw new InvalidOperationException("Unexpected Response from Core");

            coreState = CoreState.Stopped;
        }

        public int DbId { get { return dbId; } }

        private string ReadMessage()
        {
            lock(messageQueue)
            {
                while(messageQueue.Count == 0)
                    Monitor.Wait(messageQueue);

                return messageQueue.Dequeue();
            }
        }

        private string ReadMessageInternal()
        {
            StringBuilder message = new StringBuilder();
            try
            {

                string line = socketReader.ReadLine();
                if (line == null)
                {
                    SonifyLog.Write("ReadMessageInternal: socket returned null line (peer closed)");
                    return null;
                }
                while (line != null && !line.Trim().EndsWith("</message>"))
                {
                    message.Append(line + "\n");
                    line = socketReader.ReadLine();
                }

                message.Append(line);
            }
            catch (IOException ex)
            {
                SonifyLog.Write("ReadMessageInternal: IOException: " + ex.GetType().Name + ": " + ex.Message);
                return null;
            }

            return message.ToString();
        }

        public void Close()
        {
            SonifyLog.WriteWithStack("Close entered. coreState=" + coreState + " db_id=" + dbId);

            if (coreState == CoreState.Running)
                SendStop();

            if(coreState == CoreState.Stopped)
                SendExit();

            messageThread.Interrupt();
            messageThread.Abort();


            if (socketReader != null)
                socketReader.Dispose();
            if (socketWriter != null)
                socketWriter.Dispose();

            if (socketStream != null)
                socketStream.Dispose();

            if (socket != null && socket.Connected)
                socket.Close();

            if (!coreProcess.HasExited)
                coreProcess.Kill();


        }

        private void SendStop()
        {
            SonifyLog.WriteWithStack("SendStop entered. coreState=" + coreState + " db_id=" + dbId);

            if (coreState != CoreState.Running)
                throw new InvalidOperationException("Core can only be stopped from running state");

            SonifyControlMessage controlMessage = new SonifyControlMessage(SonifyControlMessage.ControlAction.stop);

            SendMessage(controlMessage);

            ReadMessage();

            coreState = CoreState.Stopped;
        }

        private Thread messageThread;



        private void SendExit()
        {
            SonifyLog.WriteWithStack("SendExit entered. coreState=" + coreState + " db_id=" + dbId);

            if (coreState != CoreState.Stopped)
                throw new InvalidOperationException("Core can only be exited from stopped state");

            SonifyControlMessage controlMessage = new SonifyControlMessage(SonifyControlMessage.ControlAction.exit);

            SendMessage(controlMessage);
            ReadMessage();

            coreState = CoreState.Exited;
        }

        private void SendStart()
        {
            if (coreState != CoreState.Stopped)
                throw new InvalidOperationException("Core can only be started from stopped state");

            SonifyControlMessage controlMessage = new SonifyControlMessage(SonifyControlMessage.ControlAction.start);

            SendMessage(controlMessage);
            ReadMessage();

            StartMessageLoop();
            coreState = CoreState.Running;
        }

        private void StartMessageLoop()
        {
            if(messageThread == null || !messageThread.IsAlive)
                messageThread = new Thread(MessageLoop);

            if(!messageThread.IsAlive)
                messageThread.Start();
        }


        private void MessageLoop()
        {
            while (true)
            {
                string message = ReadMessageInternal();
                if (message == null)
                {
                    SonifyLog.Write("MessageLoop: null message -> KillCore");
                    KillCore();
                    return;
                }

                Debug.WriteLine("RECEIVED:" + message);

                if (CheckForDataErrors(message))
                {
                    SonifyLog.Write("MessageLoop: error stamp in response -> KillCore. preview=" + Preview(message));
                    Debug.WriteLine("DATA ERROR!");
                    KillCore();
                    return;
                }

                lock (messageQueue)
                {
                    messageQueue.Enqueue(message);
                    Monitor.Pulse(messageQueue);
                }
            }
        }

        private static string Preview(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            string flat = s.Replace("\r"," ").Replace("\n"," ");
            return flat.Length > 240 ? flat.Substring(0, 240) + "..." : flat;
        }

        private void KillCore()
        {
            SonifyLog.WriteWithStack("KillCore entered. coreState=" + coreState + " db_id=" + dbId);
            lock (coreLock)
            {
                try
                {
                    // Stage 6 fix: coreProcess is null when the user runs the
                    // Java backend manually (ant run) instead of letting
                    // SonifyClient launch it via LaunchCore(). Previously this
                    // threw NullReferenceException, dropping out of the method
                    // BEFORE the coreState assignment, leaving the client stuck
                    // in Running while the message-reader thread was dead.
                    if (coreProcess != null && !coreProcess.HasExited)
                    {
                        try
                        {
                            coreProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        { }
                    }
                }
                catch (Exception ex)
                {
                    SonifyLog.Write("KillCore: unexpected exception killing process: " + ex.GetType().Name + ": " + ex.Message);
                }
                finally
                {
                    // Always advance to Exited so the next SendDataMessage
                    // enters the reconnect path instead of writing into a
                    // socket whose reader thread has died.
                    coreState = CoreState.Exited;
                }
            }
        }
        Random random = new Random();
        private bool CheckForDataErrors(string message)
        {
            SonifyMessage response = SonifyMessage.ParseMessage(message);
            return (response is SonifyDataMessage && response.Stamps["UI"].Status != "SUCCESS");
        }

        private object coreLock = new object();
        private Queue<string> messageQueue = new Queue<string>();

        public void SendDataMessage(SonifyDataMessage dataMessage)
        {
            // Only reinitialize if not running, or if we launched the process and it exited
            CoreState snap = coreState;
            bool procExited = (coreProcess != null && coreProcess.HasExited);
            if (snap != CoreState.Running || procExited)
            {
                SonifyLog.Write("SendDataMessage: reconnect path. coreState=" + snap + " procExited=" + procExited + " db_id=" + dbId);
                ConnectToCore();
                StartMessageLoop();
                Start();
            }

            SendMessage(dataMessage);
        }

        private void SendMessage(SonifyMessage controlMessage)
        {
            string message = controlMessage.ToString(this);
            string desc = controlMessage is SonifyControlMessage
                ? "CONTROL." + ((SonifyControlMessage)controlMessage).Action.ToString().ToUpper()
                : "data";
            SonifyLog.Write("SendMessage " + desc + " db_id=" + dbId);
            Console.WriteLine(message);
            socketWriter.WriteLine(controlMessage.ToString(this));
            socketWriter.Flush();

        }


    }
}
