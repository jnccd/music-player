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
        public static XNA game;
        public static bool Closing = false;
        public static FileSystemWatcher weightwatchers;
        public static FileSystemWatcher crackopenthebois;

        [STAThread]
        static void Main(string[] args)
        {
            #region Check for other program instances
            Console.WriteLine("Checking for other MusicPlayer instances...");
            try
            {
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
            } catch {
                Console.WriteLine("Please just start one instance of me at a time!");
                Thread.Sleep(1000);
                return;
            }
            #endregion

            // Smol settings
            Console.Title = "MusicPlayer Console";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Values.DisableConsoleRezise();
            Values.RegisterUriScheme();

            #region Song Data List initialization
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
            // SongDate
            if (config.Default.SongDate == null || config.Default.SongDate.Length != Assets.UpvotedSongScores.Count)
            {
                Assets.UpvotedSongAddingDates = new List<long>(Assets.UpvotedSongScores.Count);
                for (int i = 0; i < Assets.UpvotedSongScores.Count; i++)
                    Assets.UpvotedSongAddingDates.Add(0);
            }
            else
                Assets.UpvotedSongAddingDates = config.Default.SongDate.ToList();
            #endregion

            Console.Clear();

            // Actual start
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            DiscordRPCWrapper.Initialize("460490126607384576");

            #region Filewatcher
            // SettingsPath
            weightwatchers = new FileSystemWatcher();
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
                        try
                        {
                            game.CheckForRequestedSongs();
                        } catch { }
                    });
                    weightwatchers.EnableRaisingEvents = true;
                }
                else
                {
                    Console.WriteLine("Couldn't set filewatcher! (WRONG SETTINGSPATH: " + SettingsPath + " )");
                }
            }
            catch { Console.WriteLine("Couldn't set filewatcher! (UNKNOWN ERROR)"); }

            // DownloadPath
            if (config.Default.BrowserDownloadFolderPath != "" && config.Default.BrowserDownloadFolderPath != null)
            {
                crackopenthebois = new FileSystemWatcher();
                try
                {
                    if (Directory.Exists(config.Default.BrowserDownloadFolderPath))
                    {
                        config.Default.BrowserDownloadFolderPath = config.Default.BrowserDownloadFolderPath;
                        config.Default.Save();

                        crackopenthebois.Path = config.Default.BrowserDownloadFolderPath;
                        crackopenthebois.Changed += CrackOpen;
                        crackopenthebois.EnableRaisingEvents = true;
                    }
                    else
                    {
                        MessageBox.Show("Couldn't set filewatcher! (wrong SelectedPath: " + config.Default.BrowserDownloadFolderPath + " )");
                    }
                }
                catch (Exception ex) { MessageBox.Show("Couldn't set filewatcher! (ERROR: " + ex + ")"); }
            }
            #endregion

#if DEBUG
            game = new XNA();
            game.Run();
#else
            try
            {
                game = new XNA();
                game.Run();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Auf das verworfene Objekt kann nicht zugegriffen werden.\nObjektname: \"WindowsGameForm\".\n")
                    MessageBox.Show("I got brutally murdered by another Program. Please restart me.");
                else if (ex.Message == "CouldntFindWallpaperFile")
                    MessageBox.Show("You seem to have moved your Desktop Wallpaper file since you last set it as your Wallpaper.\nPlease set it as your wallpaper again and restart me so I can actually find its file.");
                else
                    MessageBox.Show("Error Message: " + ex.Message + "\n\nStack Trace: \n" + ex.StackTrace + "\n\nInner Error: " + ex.InnerException + "\n\nSource: " + ex.Source);
                
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

        public static void CrackOpen(object source, FileSystemEventArgs ev)
        {
            string[] bois = Directory.GetFiles(config.Default.BrowserDownloadFolderPath);
            for (int i = 0; i < bois.Length; i++)
            {
                string fileName = Path.GetFileName(bois[i]);
                if (fileName == "MusicPlayer.PlayRequest")
                {
                    string boi = config.Default.BrowserDownloadFolderPath + "\\MusicPlayer.PlayRequest";
                    string crackedOpenBoi = File.ReadAllText(boi);
                    File.Delete(boi);
                    game.PauseConsoleInputThread = true;
                    Task.Factory.StartNew(() => {
                        string down = crackedOpenBoi;
                        string[] split = down.Split('±');
                        game.Download(split[0]);
                        if (split.Length > 1)
                        {
                            long secondspassed = Convert.ToInt64(split[1].Split('.')[0]);
                            Assets.Channel32.Position = secondspassed * Assets.Channel32.WaveFormat.AverageBytesPerSecond;
                        }
                    });
                    Thread.Sleep(200);
                    Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                    Values.SetForegroundWindow(Values.GetConsoleWindow());
                    SendKeys.SendWait("odd");
                    break;
                }
            }
        }
    }
}

