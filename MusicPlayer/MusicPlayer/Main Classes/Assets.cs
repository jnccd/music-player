using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public static class HammingWindowValues
    {
        public static float GetHammingWindow(int i)
        {
            if (i >= 0 && i < HammingWindow.Length)
                return HammingWindow[i];
            else
                return 0;
        }
        public static void CreateIfNotFilled(int Length)
        {
            if (HammingWindow == null || HammingWindow.Length != Length)
            {
                HammingWindow = new float[Length];
                HammingWindowValues.Length = Length;

                for (int i = 0; i < HammingWindow.Length; i++)
                    HammingWindow[i] = (float)FastFourierTransform.HammingWindow(i, Length);
            }
        }

        private static float[] HammingWindow;
        public static int Length;
    }
    public struct DistancePerSong
    {
        public int SongIndex;
        public float SongDifference;
    }
    public struct SongActionStruct
    {
        public SongActionStruct(string SongName, bool hasUpvoted)
        {
            this.SongName = SongName;
            this.hasUpvoted = hasUpvoted;
        }

        public string SongName;
        public bool hasUpvoted;
    }

    public static class Assets
    {
        public static SpriteFont Font;
        public static SpriteFont Title;

        public static Texture2D White;
        public static Texture2D bg;
        public static Texture2D Volume;
        public static Texture2D Volume2;
        public static Texture2D Volume3;
        public static Texture2D Volume4;
        public static Texture2D ColorFade;
        public static Texture2D Play;
        public static Texture2D Pause;
        public static Texture2D Upvote;
        public static Texture2D Close;
        public static Texture2D Options;
        public static Texture2D TrumpetBoy;
        public static Texture2D TrumpetBoyBackground;
        public static Texture2D TrumpetBoyTrumpet;
        public static Texture2D CoverPicture;

        public static Color SystemDefaultColor;

        public static Effect gaussianBlurHorz;
        public static Effect gaussianBlurVert;
        public static Effect PixelBlur;
        public static Effect TitleFadeout;
        public static Effect Vignette;
        public static BasicEffect basicEffect;

        // Music Player Manager Values
        public static string currentlyPlayingSongName
        {
            get
            {
                return PlayerHistory[PlayerHistoryIndex].Split('\\').Last();
            }
        }
        public static string currentlyPlayingSongPath
        {
            get
            {
                return PlayerHistory[PlayerHistoryIndex];
            }
        }
        public static string previouslyPlayingSongName
        {
            get
            {
                return PlayerHistory[PlayerHistoryIndex - 1].Split('\\').Last();
            }
        }
        public static string previouslyPlayingSongPath
        {
            get
            {
                return PlayerHistory[PlayerHistoryIndex - 1];
            }
        }
        public static List<string> Playlist = new List<string>();
        public static List<string> PlayerHistory = new List<string>();
        public static LinkedList<SongActionStruct> PlayerUpvoteHistory = new LinkedList<SongActionStruct>();
        public static int PlayerHistoryIndex = 0;
        public static int SongChangedTickTime = -100000;
        public static int SongStartTime;
        public static bool IsCurrentSongUpvoted;
        public static int LastUpvotedSongStreak;
        static List<string> SongChoosingList = new List<string>();
        public static float LastScoreChange = 0;

        // Song Data
        public struct UpvotedSong
        {
            string Name;
            float Score;
            int Streak;
            int TotalLikes;
            long AddingDates;
        }
        public static List<UpvotedSong> UpvotedSongData = new List<UpvotedSong>();
        public static List<string> UpvotedSongNames;
        public static List<float> UpvotedSongScores;
        public static List<int> UpvotedSongStreaks;
        public static List<int> UpvotedSongTotalLikes;
        public static List<long> UpvotedSongAddingDates;

        // MultiThreading
        public static Task T = null;
        public static bool AbortAbort = false;

        // NAudio
        public static WaveChannel32 Channel32;
        public static WaveChannel32 Channel32Reader;
        public static DirectSoundOut output;
        public static Mp3FileReader mp3;
        public static Mp3FileReader mp3Reader;
        public static MMDevice device;
        public static MMDeviceEnumerator enumerator;
        //public const int bufferLength = 8192;
        //public const int bufferLength = 16384;
        //public const int bufferLength = 32768;
        public const int bufferLength = 65536;
        //public const int bufferLength = 131072; 
        //public const int bufferLength = 262144;
        public static GigaFloatList EntireSongWaveBuffer;
        public static byte[] buffer = new byte[bufferLength];
        public static float[] WaveBuffer = new float[bufferLength / 4];
        public static float[] FFToutput;
        public static float[] RawFFToutput;
        public static Complex[] tempbuffer = null;
        static int TempBufferLengthLog2;
        public static bool SongBufferThreadWasAborted = false;
        public static Exception LastSongBufferThreadException = new Exception();

        // Debug
        public static long CurrentDebugTime = 0;

        // Loading / Disposing Data
        public static void LoadLoadingScreen(ContentManager Content, GraphicsDevice GD)
        {
            White = new Texture2D(GD, 1, 1);
            Color[] Col = new Color[1];
            Col[0] = Color.White;
            White.SetData(Col);

            gaussianBlurHorz = Content.Load<Effect>("GaussianBlurHorz");
            gaussianBlurVert = Content.Load<Effect>("GaussianBlurVert");
        }
        public static void Load(ContentManager Content, GraphicsDevice GD)
        {
            Console.WriteLine("Loading Effects...");
            PixelBlur = Content.Load<Effect>("PixelBlur");
            TitleFadeout = Content.Load<Effect>("TitleFadeout");
            Vignette = Content.Load<Effect>("Vignette");
            basicEffect = new BasicEffect(GD);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GD.Viewport.Width, GD.Viewport.Height, 0, 1.0f, 1000.0f);
            basicEffect.VertexColorEnabled = true;
            
            Console.WriteLine("Loading Textures...");
            Color[] Col = new Color[1];
            int res = 8;
            ColorFade = new Texture2D(GD, 1, res);
            Col = new Color[res];
            for (int i = 0; i < Col.Length; i++)
                Col[i] = Color.FromNonPremultiplied(255, 255, 255, (int)(i / (float)res * 255));
            ColorFade.SetData(Col);

            Volume = Content.Load<Texture2D>("volume");
            Volume2 = Content.Load<Texture2D>("volume2");
            Volume3 = Content.Load<Texture2D>("volume3");
            Volume4 = Content.Load<Texture2D>("volume4");
            Play = Content.Load<Texture2D>("play");
            Pause = Content.Load<Texture2D>("pause");
            Upvote = Content.Load<Texture2D>("Upvote");
            Close = Content.Load<Texture2D>("Close");
            Options = Content.Load<Texture2D>("Options");
            TrumpetBoy = Content.Load<Texture2D>("trumpetboy");
            TrumpetBoyBackground = Content.Load<Texture2D>("trumpetboybackground");
            TrumpetBoyTrumpet = Content.Load<Texture2D>("trumpetboytrumpet");

            Console.WriteLine("Loading Fonts...");
            Font = Content.Load<SpriteFont>("Font");
            Title = Content.Load<SpriteFont>("Title");


            Console.WriteLine("Loading Background...");
            RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
            {
                MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
            }
            try
            {
                FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
                bg = Texture2D.FromStream(GD, Stream);
                Stream.Dispose();
            }
            catch
            {
                throw new Exception("CouldntFindWallpaperFile");
            }
            Program.game.TaskbarHidden = new Taskbar().AutoHide;
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler((object o, UserPreferenceChangedEventArgs target) =>
            {
                RefreshBGtex(GD);
                // System Default Color
                int argbColorRefresh = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                System.Drawing.Color tempRefresh = System.Drawing.Color.FromArgb(argbColorRefresh);
                SystemDefaultColor = Color.FromNonPremultiplied(tempRefresh.R, tempRefresh.G, tempRefresh.B, tempRefresh.A);

                var tb = new Taskbar();

                if (Program.game.TaskbarHidden == true && tb.AutoHide == false)
                {
                    Program.game.TaskbarHidden = tb.AutoHide;
                    Program.game.KeepWindowInScreen();
                }
                else if (Program.game.TaskbarHidden == false && tb.AutoHide == true)
                {
                    config.Default.WindowPos = new System.Drawing.Point(Program.game.gameWindowForm.Location.X, Program.game.gameWindowForm.Location.Y + 38);
                    Program.game.gameWindowForm.Location = new System.Drawing.Point(Program.game.gameWindowForm.Location.X, Program.game.gameWindowForm.Location.Y + 38);
                    Program.game.TaskbarHidden = tb.AutoHide;
                    Program.game.KeepWindowInScreen();
                }
                else
                    Program.game.TaskbarHidden = tb.AutoHide;
            });
            // System Default Color
            try
            {
                int argbColor = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                System.Drawing.Color temp = System.Drawing.Color.FromArgb(argbColor);
                SystemDefaultColor = Color.FromNonPremultiplied(temp.R, temp.G, temp.B, temp.A);
            }
            catch
            {
                Console.WriteLine("Couldn't find System Default Color!");
                SystemDefaultColor = Color.White;
            }

            Console.WriteLine("Searching for Songs...");
            if (Directory.Exists(config.Default.MusicPath) && DirOrSubDirsContainMp3(config.Default.MusicPath))
                FindAllMp3FilesInDir(config.Default.MusicPath, true);
            else
            {
                FolderBrowserDialog open = new FolderBrowserDialog();
                open.Description = "Select your music folder";
                if (open.ShowDialog() != DialogResult.OK) Process.GetCurrentProcess().Kill();
                config.Default.MusicPath = open.SelectedPath;
                config.Default.Save();
                FindAllMp3FilesInDir(open.SelectedPath, true);
            }
            Console.WriteLine();


            if (config.Default.Col != System.Drawing.Color.Transparent)
            {
                Program.game.primaryColor = Color.FromNonPremultiplied(config.Default.Col.R, config.Default.Col.G, config.Default.Col.B, config.Default.Col.A);
                Program.game.secondaryColor = Color.Lerp(Program.game.primaryColor, Color.White, 0.4f);
            }
            else
            {
                Program.game.primaryColor = SystemDefaultColor;
                if (Program.game.primaryColor.A != 255) Program.game.primaryColor.A = 255;
                Program.game.secondaryColor = Color.Lerp(Program.game.primaryColor, Color.White, 0.4f);
            }

            Console.WriteLine("Starting first Song...");
            UpdateSongChoosingList();
            if (Playlist.Count > 0)
            {
                if (Program.args.Length > 0)
                    PlayNewSong(Program.args[0]);
                else if (false && File.Exists(Values.CurrentExecutablePath + "\\History.txt"))
                {
                    try
                    {
                        string songName = File.ReadLines(Values.CurrentExecutablePath + "\\History.txt").Last();

                        if (!songName.EndsWith(".mp3"))
                            songName += ".mp3";

                        PlayPlaylistSong(songName);
                    }
                    catch
                    {
                        int PlaylistIndex = Values.RDM.Next(Playlist.Count);
                        GetNextSong(true, false);
                        PlayerHistory.Add(Playlist[PlaylistIndex]);
                    }
                }
                else
                {
                    int PlaylistIndex = Values.RDM.Next(Playlist.Count);
                    GetNextSong(true, false);
                    PlayerHistory.Add(Playlist[PlaylistIndex]);
                }
            }
            else
                Console.WriteLine("Playlist empty!");

            Console.WriteLine("Loading GUI...");
            Values.MinimizeConsole();
        }
        public static void FindAllMp3FilesInDir(string StartDir, bool ConsoleOutput)
        {
            foreach (string s in Directory.GetFiles(StartDir))
                if (s.EndsWith(".mp3"))
                {
                    Playlist.Add(s);
                    AddSongToListIfNotDoneSoFar(s);
                    UpdateSongDate(s);
                    if (ConsoleOutput)
                    {
                        Console.CursorLeft = 0;
                        Console.Write("Found " + Playlist.Count.ToString() + " Songs!");
                    }
                }

            foreach (string D in Directory.GetDirectories(StartDir))
                FindAllMp3FilesInDir(D, ConsoleOutput);
        }
        public static bool DirOrSubDirsContainMp3(string StartDir)
        {
            foreach (string s in Directory.GetFiles(StartDir))
                if (s.EndsWith(".mp3"))
                    return true;

            foreach (string D in Directory.GetDirectories(StartDir))
                if (DirOrSubDirsContainMp3(D))
                    return true;
            return false;
        }
        public static void RefreshBGtex(GraphicsDevice GD)
        {
            Task.Factory.StartNew(() =>
            {
                lock (bg)
                {
                    try
                    {
                        //Thread.Sleep(400);
                        RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                        if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
                        {
                            MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
                        }
                        FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
                        bg = Texture2D.FromStream(GD, Stream);
                        Stream.Dispose();

                        Program.game.ForceBackgroundRedraw();
                    }
                    catch { }
                }
            });
        }
        public static void DisposeNAudioData()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (Channel32 != null)
            {
                try
                {
                    Channel32.Dispose();
                    Channel32 = null;
                }
                catch { }
            }
            if (Channel32Reader != null)
            {
                try
                {
                    Channel32Reader.Dispose();
                }
                catch { Debug.WriteLine("Couldn't dispose the reader"); }
                Channel32Reader = null;
            }
            if (mp3 != null)
            {
                mp3.Dispose();
                mp3 = null;
            }
        }

        // Visualization
        public static void UpdateWaveBuffer()
        {
            //buffer = new byte[bufferLength];
            //WaveBuffer = new float[bufferLength / 4];

            if (Channel32 != null && Channel32Reader != null && Channel32Reader.CanRead)
            {
                Channel32Reader.Position = Channel32.Position;

                while (true)
                {
                    try
                    {
                        int Read = Channel32Reader.Read(buffer, 0, bufferLength);
                        break;
                    }
                    catch { Debug.WriteLine("AHAHHAHAHAHA.... ich kann nicht lesen"); }
                }

                // Converting the byte buffer in readable data
                for (int i = 0; i < bufferLength / 4; i++)
                    WaveBuffer[i] = BitConverter.ToSingle(buffer, i * 4);
            }
        }
        public static void UpdateFFTbuffer()
        {
            lock (Channel32)
            {
                //CurrentDebugTime = Stopwatch.GetTimestamp();
                if (tempbuffer == null)
                {
                    tempbuffer = new Complex[WaveBuffer.Length];
                    TempBufferLengthLog2 = (int)Math.Log(tempbuffer.Length, 2.0);
                }
                //Debug.WriteLine("UpdateFFTbuffer 1 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                HammingWindowValues.CreateIfNotFilled(tempbuffer.Length);
                for (int i = 0; i < tempbuffer.Length; i++)
                {
                    tempbuffer[i].X = WaveBuffer[i] * HammingWindowValues.GetHammingWindow(i);
                    tempbuffer[i].Y = 0;
                }
                //Debug.WriteLine("UpdateFFTbuffer 2 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                FastFourierTransform.FFT(true, TempBufferLengthLog2, tempbuffer);
                //Debug.WriteLine("UpdateFFTbuffer 3 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                FFToutput = new float[tempbuffer.Length / 2 - 1];
                RawFFToutput = new float[tempbuffer.Length / 2 - 1];
                for (int i = 0; i < FFToutput.Length; i++)
                {
                    RawFFToutput[i] = Approximate.Sqrt((tempbuffer[i].X * tempbuffer[i].X) + (tempbuffer[i].Y * tempbuffer[i].Y)) * 7;
                    FFToutput[i] = (RawFFToutput[i] * Approximate.Sqrt(i + 1));
                }
                //Debug.WriteLine("UpdateFFTbuffer 4 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
            }
        }
        public static void UpdateEntireSongBuffers()
        {
            try
            {
                lock (Channel32Reader)
                {
                    byte[] buffer = new byte[16384];

                    EntireSongWaveBuffer = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    Channel32Reader.Position = 0;
                    EntireSongWaveBuffer = new GigaFloatList();
                    
                    while (Channel32Reader.Position < Channel32Reader.Length)
                    {
                        if (AbortAbort)
                            break;

                        int read = Channel32Reader.Read(buffer, 0, 16384);

                        if (AbortAbort)
                            break;

                        for (int i = 0; i < read / 4; i++)
                        {
                            EntireSongWaveBuffer.Add(BitConverter.ToSingle(buffer, i * 4));

                            if (AbortAbort)
                                break;
                        }

                        while (Channel32 != null && Channel32.Position < Channel32Reader.Position - config.Default.WavePreload * Channel32Reader.Length / 100f)
                        {
                            if (AbortAbort)
                                break;
                            Thread.Sleep(20);
                        }
                    }

                    SongBufferThreadWasAborted = AbortAbort;

                    Debug.WriteLine("SongBuffer Length: " + EntireSongWaveBuffer.Count + " Memory: " + GC.GetTotalMemory(true));
                    Debug.WriteLine("Memory per SongBuffer Length: " + (GC.GetTotalMemory(true) / (double)EntireSongWaveBuffer.Count));
                    AbortAbort = false;
                }
            }
            catch (Exception e)
            {
                LastSongBufferThreadException = e;

                Debug.WriteLine("Couldn't load " + currentlyPlayingSongPath);
                Debug.WriteLine("SongBuffer Length: " + EntireSongWaveBuffer.Count + " Memory: " + GC.GetTotalMemory(true));
                Debug.WriteLine("Memory per SongBuffer Length: " + (GC.GetTotalMemory(true) / (double)EntireSongWaveBuffer.Count));
                Debug.WriteLine("Exception: " + e);
            }
        }
        public static void UpdateWaveBufferWithEntireSongWB()
        {
            try
            {
                lock (EntireSongWaveBuffer)
                {
                    WaveBuffer = new float[bufferLength / 4];
                    if (Channel32 != null && Channel32.CanRead && EntireSongWaveBuffer.Count > Channel32.Position / 4 && Channel32.Position > bufferLength)
                        WaveBuffer = EntireSongWaveBuffer.GetRange((int)((Channel32.Position - bufferLength / 2) / 4), bufferLength / 4).ToArray();
                    else
                        for (int i = 0; i < bufferLength / 4; i++)
                            WaveBuffer[i] = 0;
                }
            }
            catch { }
        }
        public static float GetAverageHeight(float[] array, int from, int to)
        {
            float temp = 0;

            if (from < 0)
                from = 0;

            if (to > array.Length)
                to = array.Length;

            for (int i = from; i < to; i++)
                temp += array[i];

            return temp / array.Length;
        }
        public static float GetMaxHeight(float[] array, int from, int to)
        {
            if (from < 0)
                from = 0;

            if (to > array.Length)
                to = array.Length;

            if (from >= to)
                to = from + 1;

            float max = 0;
            for (int i = from; i < to; i++)
                if (array[i] > max)
                    max = array[i];

            return max;
        }

        // Music Player Managment
        public static void PlayPause()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                {
                    output.Pause();
                    Program.game.UpdateDiscordRPC(false, true);
                }
                else if (output.PlaybackState == PlaybackState.Paused || output.PlaybackState == PlaybackState.Stopped)
                {
                    output.Play();
                    Program.game.UpdateDiscordRPC(true, true);
                }
            }
        }
        public static bool IsPlaying()
        {
            if (output == null) return false;
            else if (output.PlaybackState == PlaybackState.Playing) return true;
            return false;
        }
        public static bool PlayNewSong(string sPath)
        {
            if (Values.Timer > SongChangedTickTime + 10 && !config.Default.MultiThreading ||
                config.Default.MultiThreading)
            {
                SaveUserSettings();

                sPath = sPath.Trim('"');

                if (!File.Exists(sPath))
                {
                    List<string> Choosing = Playlist.OrderBy(x => Values.RDM.NextDouble()).ToList();
                    DistancePerSong[] LDistances = new DistancePerSong[Choosing.Count];
                    for (int i = 0; i < LDistances.Length; i++)
                    {
                        LDistances[i].SongDifference = Values.OwnDistanceWrapper(sPath, Path.GetFileNameWithoutExtension(Choosing[i]));
                        LDistances[i].SongIndex = i;
                    }

                    LDistances = LDistances.OrderBy(x => x.SongDifference).ToArray();
                    int NonWorkingIndexes = 0;
                    sPath = Choosing[LDistances[NonWorkingIndexes].SongIndex];
                    while (!File.Exists(sPath))
                    {
                        NonWorkingIndexes++;
                        sPath = Choosing[LDistances[NonWorkingIndexes].SongIndex];
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">Found one matching song: \"" + Path.GetFileNameWithoutExtension(sPath) + "\" with a difference of " +
                        Math.Round(LDistances[NonWorkingIndexes].SongDifference, 2));

                    for (int i = 1; i <= 5; i++)
                    {
                        if (LDistances[NonWorkingIndexes + i].SongDifference > 2)
                            break;
                        if (i == 1)
                            Console.WriteLine("Other well fitting songs were:");
                        Console.WriteLine(i + ". \"" + Path.GetFileNameWithoutExtension(Choosing[LDistances[NonWorkingIndexes + i].SongIndex]) + "\" with a difference of " +
                            Math.Round(LDistances[NonWorkingIndexes + i].SongDifference, 2));
                    }
                }

                PlayerHistory.Add(sPath);
                PlayerHistoryIndex = PlayerHistory.Count - 1;

                if (!Playlist.Contains(sPath))
                    Playlist.Add(sPath);

                try
                {
                    PlaySongByPath(sPath);
                }
                catch
                {
                    MessageBox.Show("That song is not readable!");
                    PlayerHistory.Remove(sPath);
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    GetNextSong(true, false);
                }

                SongChangedTickTime = Values.Timer;
                return true;
            }
            return false;
        }
        public static bool PlayPlaylistSong(string SongNameWithFileEnd)
        {
            for (int i = 0; i < Playlist.Count; i++)
            {
                if (Playlist[i].Split('\\').Last() == SongNameWithFileEnd)
                {
                    SaveUserSettings();
                    PlayerHistory.Add(Playlist[i]);
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    PlaySongByPath(Playlist[i]);
                    return true;
                }
            }
            return false;
        }
        public static void GetNextSong(bool forced, bool DownVoteCurrentSongForUserSkip)
        {
            if (config.Default.MultiThreading || forced ||
                Values.Timer > SongChangedTickTime + 5 && !config.Default.MultiThreading)
            {
                DownvoteCurrentSongIfNeccesary(DownVoteCurrentSongForUserSkip);

                SaveUserSettings();

                PlayerHistoryIndex++;
                if (PlayerHistoryIndex > PlayerHistory.Count - 1)
                    GetNewPlaylistSong();
                else
                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);

                SongChangedTickTime = Values.Timer;
            }
        }
        public static void GetPreviousSong()
        {
            if (Values.Timer > SongChangedTickTime + 5 && !config.Default.MultiThreading ||
                config.Default.MultiThreading)
            {
                SaveUserSettings();

                if (PlayerHistoryIndex > 0)
                {
                    PlayerHistoryIndex--;

                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
                }

                SongChangedTickTime = Values.Timer;
            }
        }
        private static void GetNewPlaylistSong()
        {
            CurrentDebugTime = Stopwatch.GetTimestamp();

            int SongChoosingListIndex = 0;

            do
                SongChoosingListIndex = Values.RDM.Next(SongChoosingList.Count);
            while (PlayerHistory.Count != 0 && SongChoosingList[SongChoosingListIndex] == PlayerHistory[PlayerHistoryIndex - 1] && Playlist.Count > 1);

            PlayerHistory.Add(SongChoosingList[SongChoosingListIndex]);
            PlayerHistoryIndex = PlayerHistory.Count - 1;
            PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
            Debug.WriteLine("New Song calc time: " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
        }
        private static void PlaySongByPath(string PathString)
        {
            if (!File.Exists(PathString))
            {
                Playlist.Remove(PathString);
                GetNextSong(true, false);
                return;
            }

            config.Default.Preload = Program.game.Preload;
            Program.game.ReHookGlobalKeyHooks();
            if (T != null && T.Status == TaskStatus.Running)
            {
                if (AbortAbort == false)
                    AbortAbort = true;
                T.Wait();
            }

            Program.game.SongTimeSkipped = 0;
            Program.game.ForcedCoverBackgroundRedraw = true;
            
            DisposeNAudioData();
            Program.game.ForceTitleRedraw();
            if (Program.game.DG != null)
                Program.game.DG.Clear();

            if (PathString.Contains("\""))
                PathString = PathString.Trim(new char[] { '"', ' ' });
            
            mp3 = new Mp3FileReader(PathString);
            mp3Reader = new Mp3FileReader(PathString);
            Channel32 = new WaveChannel32(mp3);
            Channel32Reader = new WaveChannel32(mp3Reader);

            output = new DirectSoundOut();
            output.Init(Channel32);

            if (config.Default.Preload)
            {
                if (config.Default.MultiThreading)
                    T = Task.Factory.StartNew(UpdateEntireSongBuffers);
                else
                    UpdateEntireSongBuffers();
            }
            
            output.Play();
            Channel32.Volume = 0;
            SongStartTime = Values.Timer;
            Channel32.Position = bufferLength / 2;

            Program.game.UpdateDiscordRPC(true, true);
        }
        public static void SaveUserSettings()
        {
            UpvoteCurrentSongIfNeccesary();
            SaveCurrentSongToHistoryFile(LastScoreChange);
            LastScoreChange = 0;

            // Sorting
            float Swapi;
            int Swapi2;
            int Swapi3;
            long Swapi4;
            string SwapS;
            for (int i = 1; i < UpvotedSongNames.Count; i++)
            {
                if (UpvotedSongScores[i - 1] < UpvotedSongScores[i])
                {
                    Swapi = UpvotedSongScores[i];
                    Swapi2 = UpvotedSongStreaks[i];
                    Swapi3 = UpvotedSongTotalLikes[i];
                    Swapi4 = UpvotedSongAddingDates[i];
                    SwapS = UpvotedSongNames[i];

                    UpvotedSongScores[i] = UpvotedSongScores[i - 1];
                    UpvotedSongStreaks[i] = UpvotedSongStreaks[i - 1];
                    UpvotedSongTotalLikes[i] = UpvotedSongTotalLikes[i - 1];
                    UpvotedSongNames[i] = UpvotedSongNames[i - 1];
                    UpvotedSongAddingDates[i] = UpvotedSongAddingDates[i - 1];

                    UpvotedSongScores[i - 1] = Swapi;
                    UpvotedSongStreaks[i - 1] = Swapi2;
                    UpvotedSongTotalLikes[i - 1] = Swapi3;
                    UpvotedSongAddingDates[i - 1] = Swapi4;
                    UpvotedSongNames[i - 1] = SwapS;

                    i = 1;
                }
            }

            config.Default.SongPaths = UpvotedSongNames.ToArray();
            config.Default.SongScores = UpvotedSongScores.ToArray();
            config.Default.SongUpvoteStreak = UpvotedSongStreaks.ToArray();
            config.Default.SongTotalLikes = UpvotedSongTotalLikes.ToArray();
            config.Default.SongDate = UpvotedSongAddingDates.ToArray();

            config.Default.Background = (int)Program.game.BgModes;
            config.Default.Vis = (int)Program.game.VisSetting;

            config.Default.Col = System.Drawing.Color.FromArgb(Program.game.primaryColor.R, Program.game.primaryColor.G, Program.game.primaryColor.B);

            config.Default.Save();
        }
        private static float GetUpvoteWeight(float SongScore)
        {
            return (float)Math.Pow(2, -SongScore / 20);
        }
        private static float GetDownvoteWeight(float SongScore)
        {
            return (float)Math.Pow(2, (SongScore - 100) / 20);
        }
        private static void DownvoteCurrentSongIfNeccesary(bool DownVoteCurrentSongForUserSkip)
        {
            if (PlayerHistoryIndex > 0)
            {
                int index = UpvotedSongNames.IndexOf(currentlyPlayingSongName);

                if (index > -1 && DownVoteCurrentSongForUserSkip && PlayerHistoryIndex == PlayerHistory.Count - 1 && !IsCurrentSongUpvoted)
                {
                    float percentage = (Channel32.Position - Program.game.SongTimeSkipped) / (float)Channel32.Length;

                    if (UpvotedSongScores[index] > 120)
                        UpvotedSongScores[index] = 120;
                    if (UpvotedSongScores[index] < -1)
                        UpvotedSongScores[index] = -1;

                    if (UpvotedSongStreaks[index] > -1)
                        UpvotedSongStreaks[index] = -1;
                    else
                        UpvotedSongStreaks[index] -= 1;

                    LastScoreChange = UpvotedSongStreaks[index] * GetDownvoteWeight(UpvotedSongScores[index]) * 32 * (1 - percentage);
                    UpvotedSongScores[index] += UpvotedSongStreaks[index] * GetDownvoteWeight(UpvotedSongScores[index]) * 32 * (1 - percentage);

                    PlayerUpvoteHistory.AddFirst(new SongActionStruct(currentlyPlayingSongName, false));
                    Program.game.ShowSecondRowMessage("Downvoted  previous  song!", 1.2f);

                    UpdateSongChoosingList();
                }
            }
        }
        private static void UpvoteCurrentSongIfNeccesary()
        {
            if (IsCurrentSongUpvoted)
            {
                Program.game.UpvoteSavedAlpha = 1.4f;
                PlayerUpvoteHistory.AddFirst(new SongActionStruct(currentlyPlayingSongName, true));

                AddSongToListIfNotDoneSoFar(currentlyPlayingSongPath);

                int index = UpvotedSongNames.IndexOf(currentlyPlayingSongName);
                double percentage;
                if (Channel32 == null)
                    percentage = 1;
                else
                    percentage = (Channel32.Position - Program.game.SongTimeSkipped) / (double)Channel32.Length;
                
                if (UpvotedSongScores[index] > 120)
                    UpvotedSongScores[index] = 120;
                if (UpvotedSongScores[index] < -1)
                    UpvotedSongScores[index] = -1;

                if (UpvotedSongStreaks[index] < 1)
                    UpvotedSongStreaks[index] = 1;
                else if (Channel32 != null && Channel32.Position > Channel32.Length - bufferLength / 2)
                    UpvotedSongStreaks[index]++;
                if (UpvotedSongScores[index] < 0)
                    UpvotedSongScores[index] = 0;

                LastScoreChange = UpvotedSongStreaks[index] * GetUpvoteWeight(UpvotedSongScores[index]) * (float)percentage * 8;
                UpvotedSongScores[index] += UpvotedSongStreaks[index] * GetUpvoteWeight(UpvotedSongScores[index]) * (float)percentage * 8;
                LastUpvotedSongStreak = UpvotedSongStreaks[index];
                UpvotedSongTotalLikes[index]++;

                UpdateSongChoosingList();
            }
            IsCurrentSongUpvoted = false;
        }
        public static void AddSongToListIfNotDoneSoFar(string Song)
        {
            if (!UpvotedSongNames.Contains(Song.Split('\\').Last()))
            {
                UpvotedSongNames.Add(Song.Split('\\').Last());
                UpvotedSongScores.Add(0);
                UpvotedSongStreaks.Add(0);
                UpvotedSongTotalLikes.Add(0);
                UpvotedSongAddingDates.Add(GetSongFileCreationDate(Song));
            }
        }
        public static void QueueNewSong(string Song, bool ConsoleOutput)
        {
            if (!File.Exists(Song))
            {
                DistancePerSong[] LDistances = new DistancePerSong[Playlist.Count];
                for (int i = 0; i < LDistances.Length; i++)
                {
                    LDistances[i].SongDifference = Values.OwnDistanceWrapper(Song, Path.GetFileNameWithoutExtension(Playlist[i]));
                    LDistances[i].SongIndex = i;
                }

                LDistances = LDistances.OrderBy(x => x.SongDifference).ToArray();
                int NonWorkingIndexes = 0;
                Song = Playlist[LDistances[NonWorkingIndexes].SongIndex];
                while (!File.Exists(Song))
                {
                    NonWorkingIndexes++;
                    Song = Playlist[LDistances[NonWorkingIndexes].SongIndex];
                }
                
                if (ConsoleOutput)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">Queued one matching song: \"" + Path.GetFileNameWithoutExtension(Song) + "\" with a difference of " +
                        Math.Round(LDistances[NonWorkingIndexes].SongDifference, 2));


                    for (int i = 1; i <= 5; i++)
                    {
                        if (LDistances[NonWorkingIndexes + i].SongDifference > 2)
                            break;
                        if (i == 1)
                            Console.WriteLine("Other well fitting songs were:");
                        Console.WriteLine(i + ". \"" + Path.GetFileNameWithoutExtension(Playlist[LDistances[NonWorkingIndexes + i].SongIndex]) + "\" with a difference of " +
                            Math.Round(LDistances[NonWorkingIndexes + i].SongDifference, 2));
                    }
                }
            }

            QueueSong(Song);
        }
        public static void QueueSong(string Song)
        {
            Program.game.ShowSecondRowMessage("Added  a  song  to  the  queue!", 1f);
            PlayerHistory.Add(Song);
        }
        // For Statistics
        public static long GetSongFileCreationDate(string SongPath)
        {
            if (File.Exists(SongPath))
                return File.GetCreationTime(SongPath).ToBinary();
            else
                return 0;
        }
        public static void UpdateSongDate(string SongPath)
        {
            int index = UpvotedSongNames.IndexOf(SongPath.Split('\\').Last());
            long OriginalSongBinary = UpvotedSongAddingDates[index];
            DateTime OriginalSongCreationDate = DateTime.FromBinary(OriginalSongBinary);
            if (OriginalSongBinary == 0 || File.Exists(SongPath) && DateTime.Compare(OriginalSongCreationDate, File.GetCreationTime(SongPath)) > 0)
                UpvotedSongAddingDates[index] = File.GetCreationTime(SongPath).ToBinary();
        }
        private static float SongAge(int indexInUpvotedSongNames)
        {
            return (float)Math.Round(DateTime.Today.Subtract(DateTime.FromBinary(UpvotedSongAddingDates[indexInUpvotedSongNames])).TotalHours / 24.0, 4) + 1f;
        }
        public static float SongAge(string SongPath)
        {
            if (File.Exists(SongPath))
                return SongAge(UpvotedSongNames.IndexOf(SongPath.Split('\\').Last()));
            else
                return float.NaN;
        }
        public static object[,] GetSongInformationList()
        {
            object[,] SongInformationArray = new object[UpvotedSongNames.Count, 6];
            
            for (int i = 0; i < UpvotedSongNames.Count; i++)
            {
                SongInformationArray[i, 0] = Path.GetFileNameWithoutExtension(UpvotedSongNames[i]);
                SongInformationArray[i, 1] = UpvotedSongScores[i];
                SongInformationArray[i, 2] = UpvotedSongStreaks[i];
                SongInformationArray[i, 3] = UpvotedSongTotalLikes[i];
                SongInformationArray[i, 4] = SongAge(i);
            }
            string lastSong = "";
            int lastIndex = 0;
            float singleTicketWorth = 1f / SongChoosingList.Count * 100;
            for (int i = 0; i < SongChoosingList.Count; i++)
            {
                if (lastSong == SongChoosingList[i])
                {
                    SongInformationArray[lastIndex, 5] = (float)(SongInformationArray[lastIndex, 5]) + singleTicketWorth;
                }
                else
                {
                    int index = UpvotedSongNames.IndexOf(Path.GetFileName(SongChoosingList[i]));
                    string song = SongChoosingList[i];
                    string path = Path.GetFileName(SongChoosingList[i]);
                    if (index != -1)
                    {
                        if (SongInformationArray[index, 5] == null)
                            SongInformationArray[index, 5] = singleTicketWorth;
                        else
                            SongInformationArray[index, 5] = (float)(SongInformationArray[index, 5]) + singleTicketWorth;

                        lastIndex = index;
                        lastSong = SongChoosingList[i];
                    }
                }
            }

            return SongInformationArray;
        }
        public static string GetSongPathFromSongName(string SongName)
        {
            if (!SongName.Contains(".mp3"))
                SongName += ".mp3";

            foreach (string s in Playlist)
                if (s.Split('\\').Last() == SongName)
                    return s;
            return "";
        }
        public static void UpdateSongChoosingList() // This determines the song chances
        {
            CurrentDebugTime = Stopwatch.GetTimestamp();

            SongChoosingList.Clear();
            List<SongActionStruct> PlayerUpvoteHistoryList = PlayerUpvoteHistory.ToList();
            float ChanceIncreasePerUpvote = Playlist.Count / 1000;
            for (int i = 0; i < Playlist.Count; i++)
            {
                SongChoosingList.Add(Playlist[i]);

                int amount = 0;

                int index = UpvotedSongNames.IndexOf(Playlist[i].Split('\\').Last());
                if (index >= 0)
                {
                    if (UpvotedSongScores[index] > 0)
                        amount += (int)(Math.Ceiling(UpvotedSongScores[index] * UpvotedSongScores[index] * ChanceIncreasePerUpvote));

                    float age = SongAge(index);
                    if (age < 7)
                        amount += (int)((30 - age) * ChanceIncreasePerUpvote * 60f / 30f);

                    if (UpvotedSongScores[index] < 50)
                    {
                        int hisindex = PlayerUpvoteHistoryList.FindIndex(x => x.SongName == UpvotedSongNames[index]);
                        if (hisindex != -1 && PlayerUpvoteHistoryList[hisindex].hasUpvoted && hisindex > 2 && hisindex < 8)
                            amount += (int)((100 - UpvotedSongScores[index]) * (100 - UpvotedSongScores[index]) * 4);
                    }

                    if (UpvotedSongStreaks[index] == 0)
                        amount += (int)(130 * 130 * ChanceIncreasePerUpvote);
                }

                amount /= 4;

                for (int k = 0; k < amount; k++)
                    SongChoosingList.Add(Playlist[i]);
            }

            Debug.WriteLine("SongChoosing List update time: " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
        }
        private static void SaveCurrentSongToHistoryFile(float ScoreChange)
        {
            try { string s = currentlyPlayingSongName; } catch { return; }

            string path = Values.CurrentExecutablePath + "\\History.txt";
            string write = currentlyPlayingSongName + ":" + DateTime.Now.ToBinary() + ":" + ScoreChange;
            
            // This text is added only once to the file.
            if (!File.Exists(path))
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                    sw.WriteLine(write);

            if (write == File.ReadLines(Values.CurrentExecutablePath + "\\History.txt").Last())
                return;

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(path))
                sw.WriteLine(write);
        }

        // Draw Methods
        public static void DrawLine(Vector2 End1, Vector2 End2, int Thickness, Color Col, SpriteBatch SB)
        {
            Vector2 Line = End1 - End2;
            SB.Draw(White, End1, null, Col, -(float)Math.Atan2(Line.X, Line.Y) - (float)Math.PI / 2, new Vector2(0, 0.5f), new Vector2(Line.Length(), Thickness), SpriteEffects.None, 0f);
        }
        public static void DrawCircle(Vector2 Pos, float Radius, Color Col, SpriteBatch SB)
        {
            if (Radius < 0)
                Radius *= -1;

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Approximate.Sqrt(Radius * Radius - i * i);
                SB.Draw(White, new Rectangle((int)Pos.X + i, (int)Pos.Y - HalfHeight, 1, HalfHeight * 2), Col);
            }
        }
        public static void DrawCircle(Vector2 Pos, float Radius, float HeightMultiplikator, Color Col, SpriteBatch SB)
        {
            if (Radius < 0)
                Radius *= -1;

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Math.Sqrt(Radius * Radius - i * i);
                SB.Draw(White, new Rectangle((int)Pos.X + i, (int)Pos.Y, 1, (int)(HalfHeight * HeightMultiplikator)), Col);
            }

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Math.Sqrt(Radius * Radius - i * i);
                SB.Draw(White, new Rectangle((int)Pos.X + i + 1, (int)Pos.Y, -1, (int)(-HalfHeight * HeightMultiplikator)), Col);
            }
        }
        public static void DrawRoundedRectangle(Rectangle Rect, float PercentageOfRounding, Color Col, SpriteBatch SB)
        {
            float Rounding = PercentageOfRounding / 100;
            Rectangle RHorz = new Rectangle(Rect.X, (int)(Rect.Y + Rect.Height * (Rounding / 2)), Rect.Width, (int)(Rect.Height * (1 - Rounding)));
            Rectangle RVert = new Rectangle((int)(Rect.X + Rect.Width * (Rounding / 2)), Rect.Y, (int)(Rect.Width * (1 - Rounding)), (int)(Rect.Height * 0.999f));

            int RadiusHorz = (int)(Rect.Width * (Rounding / 2));
            int RadiusVert = (int)(Rect.Height * (Rounding / 2));

            if (RadiusHorz != 0)
            {
                // Top-Left
                DrawCircle(new Vector2(Rect.X + RadiusHorz, Rect.Y + RadiusVert), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Top-Right
                DrawCircle(new Vector2(Rect.X + Rect.Width - RadiusHorz - 1, Rect.Y + RadiusVert), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Bottom-Left
                DrawCircle(new Vector2(Rect.X + RadiusHorz, Rect.Y + RadiusVert + (int)(Rect.Height * (1 - Rounding))), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Bottom-Right
                DrawCircle(new Vector2(Rect.X + Rect.Width - RadiusHorz - 1, Rect.Y + RadiusVert + (int)(Rect.Height * (1 - Rounding))), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);
            }

            SB.Draw(White, RHorz, Col);
            SB.Draw(White, RVert, Col);
        }
    }
}
