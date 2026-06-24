using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DataSonificationMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Process parentProcess = System.Diagnostics.Process.GetProcessById(int.Parse(args[0]));
            Process childProcess = System.Diagnostics.Process.GetProcessById(int.Parse(args[1]));
            if (parentProcess == null || childProcess == null)
                return;

            while (true)
            {
                if (parentProcess.HasExited && !childProcess.HasExited)
                {
                    childProcess.Kill();
                    return;
                }
                if (childProcess.HasExited)
                {
                    return;
                }
                System.Threading.Thread.Sleep(2000);
            }


        }
    }
}
