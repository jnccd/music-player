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
    public static class Program
    {
        public static string[] args;

        [STAThread]
        static void Main(string[] args)
        {
            foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                if (p.Id != Process.GetCurrentProcess().Id && p.MainModule.FileName == Process.GetCurrentProcess().MainModule.FileName)
                {
                    if (args.Length > 0)
                    {
                        RequestedSong.Default.RequestedSongString = args[0];
                        RequestedSong.Default.Save();
                    }
                    return;
                }

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

            /*AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => {
                Assets.SaveSongUpvote();

                InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                Assets.DisposeNAudioData();

                config.Default.Background = (int)XNA.BgModes;
                config.Default.Vis = (int)XNA.VisSetting;
                config.Default.Save();
                MessageBox.Show("ApplicationExit EVENT");
            };*/

            using (XNA game = new XNA())
                game.Run();
        }
    }
}

