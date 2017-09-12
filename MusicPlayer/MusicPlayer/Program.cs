using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace MusicPlayer
{
    public static class ConsoleHelper
    {
        public static void CreateConsole()
        {
            AllocConsole();

            // stdout's handle seems to always be equal to 7
            IntPtr defaultStdout = new IntPtr(7);
            IntPtr currentStdout = GetStdHandle(StdOutputHandle);

            if (currentStdout != defaultStdout)
                // reset stdout
                SetStdHandle(StdOutputHandle, defaultStdout);

            // reopen stdout
            TextWriter writer = new StreamWriter(Console.OpenStandardOutput())
            { AutoFlush = true };
            Console.SetOut(writer);
        }

        // P/Invoke required:
        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();
    }

    public static class Program
    {
        public static string[] args;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Checking for other MusicPlayer instances...");
            foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                if (p.Id != Process.GetCurrentProcess().Id && p.MainModule.FileName == Process.GetCurrentProcess().MainModule.FileName)
                {
                    Console.WriteLine("Found another instance... sending data...");
                    if (args.Length > 0)
                    {
                        RequestedSong.Default.RequestedSongString = args[0];
                        RequestedSong.Default.Save();
                    }
                    return;
                }

            Values.DisableConsoleRezise();

            if (config.Default.SongPaths != null)
            {
                Assets.UpvotedSongNames = config.Default.SongPaths.ToList();
                Assets.UpvotedSongScores = config.Default.SongScores.ToList();
            }
            else
            {
                Assets.UpvotedSongNames = new List<string>();
                Assets.UpvotedSongScores = new List<int>();
            }

            Console.Clear();

            // Testing Ground
            //for (int i = 0; i < 100; i++)
            //    Console.Write(Convert.ToChar(i));

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);

            FileSystemWatcher weightwatchers = new FileSystemWatcher();
            string[] P = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MusicPlayer");
#if DEBUG
            weightwatchers.Path = P[1] + @"\1.0.0.0";
#else
            weightwatchers.Path = P[0] + @"\1.0.0.0";
#endif
            weightwatchers.Changed += ((object source, FileSystemEventArgs e) => {
                XNA.CheckForRequestedSongs();
            });
            weightwatchers.EnableRaisingEvents = true;

            try
            {
                using (XNA game = new XNA())
                    game.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
                string strPath = Environment.CurrentDirectory + @"\Log.txt";
                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine();
                    sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("============Start=============" + DateTime.Now);
                    sw.WriteLine("Error Message: " + ex.Message);
                    sw.WriteLine("Stack Trace: " + ex.StackTrace);
                    sw.WriteLine("=============End=============");
                }
            }
        }
    }
}

