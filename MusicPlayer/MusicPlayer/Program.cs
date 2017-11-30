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
                    Console.WriteLine("Found another instance... sending data...");
                    if (args.Length > 0)
                    {
                        RequestedSong.Default.RequestedSongString = args[0];
                        RequestedSong.Default.Save();
                    }
                    return;
                }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
            if (config.Default.SongUpvoteStreak == null || config.Default.SongUpvoteStreak.Length != config.Default.SongScores.Length)
            {
                Assets.UpvotedSongStreaks = new List<int>(Assets.UpvotedSongScores.Count);
                for (int i = 0; i < Assets.UpvotedSongScores.Count; i++)
                    Assets.UpvotedSongStreaks.Add(0);
            }
            else
                Assets.UpvotedSongStreaks = config.Default.SongUpvoteStreak.ToList();

            Console.Clear();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);

            FileSystemWatcher weightwatchers = new FileSystemWatcher();
            /*
            string[] P = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MusicPlayer");
#if DEBUG
            weightwatchers.Path = P[1] + @"\1.0.0.0";
#else
            weightwatchers.Path = P[0] + @"\1.0.0.0";
#endif
*/
            string SettingsPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            SettingsPath = SettingsPath.Remove(SettingsPath.Length - "\\user.config".Length);
            weightwatchers.Path = SettingsPath;
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

