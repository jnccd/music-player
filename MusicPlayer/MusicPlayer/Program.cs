using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayer
{
    public static class Program
    {
        public static string[] args;

        static void Main(string[] args)
        {
            foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                if (p.Id != Process.GetCurrentProcess().Id)
                { p.Kill(); Console.WriteLine("Killed another instance of this program!"); }

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            using (Game1 game = new Game1())
                game.Run();
            InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
        }
    }
}

