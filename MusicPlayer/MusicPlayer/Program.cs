using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicPlayerwNAudio;

namespace MusicPlayer
{
    public static class Program
    {
        public static string[] args;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");
            foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                if (p.Id != Process.GetCurrentProcess().Id)
                { p.CloseMainWindow(); Console.WriteLine("Killed another instance of this program!"); Thread.Sleep(500); config.Default.Reload(); }
            
            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            using (XNA game = new XNA())
                game.Run();
        }
    }
}

