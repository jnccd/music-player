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
using System.Configuration;

namespace MusicPlayer
{
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
                    Console.WriteLine("Found another instance. \nSending data...");
                    if (args.Length > 0)
                    {
                        RequestedSong.Default.RequestedSongString = args[0];
                        RequestedSong.Default.Save();
                    }
                    Console.WriteLine("Data sent! Closing...");
                    return;
                }

            // Smol settings
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Values.DisableConsoleRezise();

            // Song Data List initialization
            if (config.Default.SongPaths != null && config.Default.SongScores != null)
            {
                Assets.UpvotedSongNames = config.Default.SongPaths.ToList();
                Assets.UpvotedSongScores = config.Default.SongScores.ToList();
            }
            else
            {
                Assets.UpvotedSongNames = new List<string>();
                Assets.UpvotedSongScores = new List<float>();
            }
            // Streaks
            if (config.Default.SongUpvoteStreak == null || config.Default.SongUpvoteStreak.Length != Assets.UpvotedSongScores.Count)
            {
                Assets.UpvotedSongStreaks = new List<int>(Assets.UpvotedSongScores.Count);
                for (int i = 0; i < Assets.UpvotedSongScores.Count; i++)
                    Assets.UpvotedSongStreaks.Add(0);
            }
            else
                Assets.UpvotedSongStreaks = config.Default.SongUpvoteStreak.ToList();
            // TotalLikes
            if (config.Default.SongTotalLikes == null || config.Default.SongTotalLikes.Length != Assets.UpvotedSongScores.Count)
            {
                Assets.UpvotedSongTotalLikes = new List<int>(Assets.UpvotedSongScores.Count);
                for (int i = 0; i < Assets.UpvotedSongScores.Count; i++)
                    Assets.UpvotedSongTotalLikes.Add(0);
            }
            else
                Assets.UpvotedSongTotalLikes = config.Default.SongTotalLikes.ToList();


            Console.Clear();

            // Actual start
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);

            FileSystemWatcher weightwatchers = new FileSystemWatcher();

            try
            {
                string[] P = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MusicPlayer");
#if DEBUG
                string SettingsPath = P[1] + @"\1.0.0.0";
#else
                string SettingsPath = P[0] + @"\1.0.0.0";
#endif
                if (Directory.Exists(SettingsPath))
                {
                    weightwatchers.Path = SettingsPath;
                    weightwatchers.Changed += ((object source, FileSystemEventArgs e) =>
                    {
                        XNA.CheckForRequestedSongs();
                    });
                    weightwatchers.EnableRaisingEvents = true;
                }
                else
                {
                    Console.WriteLine("Couldn't set filewatcher! (WRONG SETTINGSPATH: " + SettingsPath + " )");
                }
            }
            catch { Console.WriteLine("Couldn't set filewatcher! (ERROR)"); }

#if DEBUG
            using (XNA game = new XNA())
                game.Run();
#else
            try
            {
                using (XNA game = new XNA())
                    game.Run();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Auf das verworfene Objekt kann nicht zugegriffen werden.\nObjektname: \"WindowsGameForm\".")
                    MessageBox.Show("I got brutally murdered by another Program. Please restart me.");
                else
                    MessageBox.Show("Error Message: " + ex.Message + "\n\nStack Trace: \n" + ex.StackTrace + "\n\nInner Error: \n" + ex.InnerException + "\n\nSource: " + ex.Source);
                
                string strPath = Values.CurrentExecutablePath + @"\Log.txt";
                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine();
                    sw.WriteLine("==========================Error Logging========================");
                    sw.WriteLine("============Start=============" + DateTime.Now);
                    sw.WriteLine("Error Message: " + ex.Message);
                    sw.WriteLine("Stack Trace: " + ex.StackTrace);
                    sw.WriteLine("=============End=============");
                }
            }
#endif
        }
    }
}

