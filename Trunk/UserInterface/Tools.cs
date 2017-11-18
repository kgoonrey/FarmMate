using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface
{
    public class Tools
    {
        public static void RunProgram(string exe, bool waitForExit)
        {
            Process process = Process.Start(exe);
            int id = process.Id;
            Process tempProc = Process.GetProcessById(id);
            if(waitForExit)
                tempProc.WaitForExit();
        }
    }
}
