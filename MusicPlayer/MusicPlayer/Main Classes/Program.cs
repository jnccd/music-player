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
            Console.Title = "MusicPlayer Console";

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
            // Also check for cheeky curious changes to the settings
            if (config.Default.MultiThreading == false)
            {
                MessageBox.Show("Dont mess with the settings file!\nLook this is an old option and it wont do much but possibly break the program so just activate it again.");
                return;
            }
            #endregion

            // Smol settings
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Values.DisableConsoleRezise();
            Values.RegisterUriScheme();

            // Support for older SongUpvote Lists
            if (config.Default.SongTotalDislikes == null && config.Default.SongPaths != null)
                config.Default.SongTotalDislikes = new int[config.Default.SongPaths.Length];
            if (config.Default.SongVolume == null && config.Default.SongPaths != null)
            {
                config.Default.SongVolume = new float[config.Default.SongPaths.Length];
                for (int i = 0; i < config.Default.SongPaths.Length; i++)
                    config.Default.SongVolume[i] = -1;
            }

            #region Song Data List initialization
            Assets.UpvotedSongData = new List<UpvotedSong>();
            if (config.Default.SongPaths != null && config.Default.SongScores != null && config.Default.SongUpvoteStreak != null && config.Default.SongTotalLikes != null &&
                config.Default.SongTotalDislikes != null && config.Default.SongDate != null && config.Default.SongVolume != null &&
                config.Default.SongScores.Length == config.Default.SongPaths.Length && config.Default.SongUpvoteStreak.Length == config.Default.SongPaths.Length &&
                config.Default.SongTotalLikes.Length == config.Default.SongPaths.Length && config.Default.SongTotalDislikes.Length == config.Default.SongPaths.Length &&
                config.Default.SongDate.Length == config.Default.SongPaths.Length && config.Default.SongVolume.Length == config.Default.SongPaths.Length)
            {
                for (int i = 0; i < config.Default.SongPaths.Length; i++)
                    Assets.UpvotedSongData.Add(new UpvotedSong(config.Default.SongPaths[i], config.Default.SongScores[i], config.Default.SongUpvoteStreak[i],
                            config.Default.SongTotalLikes[i], config.Default.SongTotalDislikes[i], config.Default.SongDate[i], config.Default.SongVolume[i]));
            }
            else if (!config.Default.FirstStart)
                MessageBox.Show("Song statistics corrupted!\nResetting...");

            Assets.HistorySongData = new List<HistorySong>();
            string path = Values.CurrentExecutablePath + "\\History.txt";
            if (File.Exists(path))
            {
                string[] Songs = File.ReadLines(path).ToArray();

                for (int i = 0; i < Songs.Length; i++)
                {
                    string[] Split = Songs[Songs.Length - i - 1].Split(':');
                    string Title = "";
                    long Time = 0;
                    string ScoreChange = "";
                    if (Split.Length == 1)
                    {
                        Title = Path.GetFileNameWithoutExtension(Songs[Songs.Length - i - 1]);
                    }
                    else if (Split.Length == 2)
                    {
                        Title = Path.GetFileNameWithoutExtension(Split[0]);
                        Time = Convert.ToInt64(Split[1]);
                    }
                    else if (Split.Length == 3)
                    {
                        Title = Path.GetFileNameWithoutExtension(Split[0]);
                        Time = Convert.ToInt64(Split[1]);
                        ScoreChange = Split[2];
                    }

                    Assets.HistorySongData.Add(new HistorySong(Title, Convert.ToSingle(ScoreChange), Time));
                }

                File.Delete(path);
            }
            if (config.Default.HistorySong != null && config.Default.HistoryDate != null && config.Default.HistoryDate.Length == config.Default.HistorySong.Length &&
                config.Default.HistoryChange != null && config.Default.HistoryChange.Length == config.Default.HistorySong.Length)
            {
                for (int i = 0; i < config.Default.HistorySong.Length; i++)
                    Assets.HistorySongData.Add(new HistorySong(config.Default.HistorySong[i], config.Default.HistoryChange[i], config.Default.HistoryDate[i]));
            }
            else if (!config.Default.FirstStart)
                MessageBox.Show("Song history corrupted!");

            Assets.HistorySongData.Sort(delegate (HistorySong x, HistorySong y) {
                return -DateTime.FromBinary(x.Date).CompareTo(DateTime.FromBinary(y.Date));
            });
            #endregion

            Console.Clear();

            // Actual start
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"Microsoft XNA Game Studio 4.0\" and \"NAudio\"");
            Console.WriteLine("Build from: " + File.ReadAllText(Values.CurrentExecutablePath + "\\BuildDate.txt").TrimEnd('\n'));

            Program.args = args;

            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            if (config.Default.DiscordRPCActive)
                DiscordRPCWrapper.Initialize("460490126607384576");

            #region clear old browser requests
            if (config.Default.BrowserDownloadFolderPath != "" && config.Default.BrowserDownloadFolderPath != null)
            {
                string[] bois = Directory.GetFiles(config.Default.BrowserDownloadFolderPath);
                for (int i = 0; i < bois.Length; i++)
                {
                    string fileExtension = Path.GetExtension(bois[i]);
                    if (fileExtension == ".PlayRequest")
                        File.Delete(bois[i]);
                    if (fileExtension == ".VideoDownloadRequest")
                        File.Delete(bois[i]);
                }
            }
            #endregion

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

            #region Game Started Stop Discord RPC Watcher
            m_hhook = Values.SetWinEventHook(Values.EVENT_SYSTEM_FOREGROUND,
                    Values.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                    _WinEvent, 0, 0, Values.WINEVENT_OUTOFCONTEXT);
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

                DiscordRPCWrapper.Shutdown();
                InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                Assets.DisposeNAudioData();
                if (game.optionsMenu != null)
                    game.optionsMenu.InvokeIfRequired(game.optionsMenu.Close);
                if (game.statistics != null)
                    game.statistics.InvokeIfRequired(game.statistics.Close);
                Closing = true;

                DialogResult D;
                if (ex.Message.Contains("WindowsGameForm"))
                    D = MessageBox.Show("I got brutally murdered by another Program. Please restart me.", "Slaughtered by another program", 
                        MessageBoxButtons.RetryCancel);
                else if (ex.Message == "CouldntFindWallpaperFile")
                    D = MessageBox.Show("You seem to have moved your Desktop Wallpaper file since you last set it as your Wallpaper.\n" +
                        "Please set it as your wallpaper again and restart me so I can actually find its file.", 
                        "Couldn't find your wallpaper", MessageBoxButtons.RetryCancel);
                else
                    D = MessageBox.Show("Error Message: " + ex.Message + "\n\nStack Trace: \n" + ex.StackTrace + 
                        "\n\nInner Error: " + ex.InnerException + "\n\nSource: " + ex.Source, "Error", MessageBoxButtons.RetryCancel);

                if (D == DialogResult.Retry)
                    Restart();
            }
#endif
        }

        public static void Restart()
        {
            string RestartLocation = Values.CurrentExecutablePath + "\\Restart.bat";

            if (File.Exists(RestartLocation))
                File.Delete(RestartLocation);

            using (StreamWriter sw = File.CreateText(RestartLocation))
                sw.WriteLine(@"@echo off
echo     ____               __                __   _                     
echo    / __ \ ___   _____ / /_ ____ _ _____ / /_ (_)____   ____ _       
echo   / /_/ // _ \ / ___// __// __ `// ___// __// // __ \ / __ `/       
echo  / _, _//  __/(__  )/ /_ / /_/ // /   / /_ / // / / // /_/ /_  _  _ 
echo /_/ ^|_^| ^\___//____/ \__/ \__,_//_/    \__//_//_/ /_/ \__, /(_)(_)(_)
echo                                                     /____/          
ping 127.0.0.1 > nul
start MusicPlayer.exe");

            Process.Start(RestartLocation);
        }
        #region Event Handlers
        static IntPtr m_hhook;
        static IntPtr hwnd;
        static Values.WinEventDelegate _WinEvent = new Values.WinEventDelegate(WinEventProc);
        static void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == Values.EVENT_SYSTEM_FOREGROUND && config.Default.AutoStopDiscordRPConGameDetection)
            {
                Program.hwnd = hwnd;
                Task T = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1500);
                    //try
                    //{
                    //    StringBuilder sb = new StringBuilder(500);
                    //    Values.GetWindowText(Program.hwnd, sb, sb.Capacity);
                    //    string text = sb.ToString();
                    //    uint id = 0;
                    //    Values.GetWindowThreadProcessId(Program.hwnd, out id);
                    //    Process P = Process.GetProcessById((int)id);
                    //    P.GetHashCode();
                    //} catch { }

                    bool gameActive = Values.IsForegroundFullScreen();
                    //Debug.WriteLine("Switched to: " + sb.ToString() + "\t\t| IsFullscreen: " + gameActive);
                    if (config.Default.DiscordRPCActive && gameActive)
                    {
                        if (game.optionsMenu != null)
                            game.optionsMenu.InvokeIfRequired(game.optionsMenu.DiscordToggleWrapper);
                    }
                    if (!config.Default.DiscordRPCActive && !gameActive)
                    {
                        if (game.optionsMenu != null)
                            game.optionsMenu.InvokeIfRequired(game.optionsMenu.DiscordToggleWrapper);
                    }
                });
            }
        }
        public static void CrackOpen(object source, FileSystemEventArgs ev)
        {
            Thread.Sleep(50);
            string[] bois = Directory.GetFiles(config.Default.BrowserDownloadFolderPath);
            for (int i = 0; i < bois.Length; i++)
            {
                string fileName = Path.GetFileName(bois[i]);
                if (fileName == "MusicPlayer.PlayRequest")
                {
                    string boi = config.Default.BrowserDownloadFolderPath + "\\MusicPlayer.PlayRequest";
                    string crackedOpenBoi = File.ReadAllText(boi);
                    game.PauseConsoleInputThread = true;
                    Task.Factory.StartNew(() => {
                        string down = crackedOpenBoi;
                        string[] split = down.Split('±');
                        if (game.Download(split[0]) && split.Length > 1)
                        {
                            long secondspassed = Convert.ToInt64(split[1].Split('.')[0]);
                            Assets.Channel32.Position = secondspassed * Assets.Channel32.WaveFormat.AverageBytesPerSecond;
                        }
                    });
                    Thread.Sleep(200);
                    File.Delete(boi);
                    Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                    Values.SetForegroundWindow(Values.GetConsoleWindow());
                    SendKeys.SendWait("SUCCCCC");
                    break;
                }
                if (fileName == "MusicPlayer.VideoDownloadRequest")
                {
                    string boi = config.Default.BrowserDownloadFolderPath + "\\MusicPlayer.VideoDownloadRequest";
                    string crackedOpenBoi = File.ReadAllText(boi);
                    game.PauseConsoleInputThread = true;
                    Task.Factory.StartNew(() => {
                        string down = crackedOpenBoi;
                        game.DownloadAsVideo(down);
                    });
                    Thread.Sleep(200);
                    File.Delete(boi);
                    Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                    Values.SetForegroundWindow(Values.GetConsoleWindow());
                    SendKeys.SendWait("SUCCCCC");
                    break;
                }
            }
        }
        #endregion
    }
}

