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

namespace MusicPlayer
{
    public static class Assets
    {
        public static SpriteFont Font;
        public static SpriteFont Title;

        public static Texture2D White;
        public static Texture2D bg;
        public static Texture2D bg1;
        public static Texture2D bg2;
        public static Texture2D bg3;
        public static Texture2D bg4;
        public static Texture2D bg5;
        public static Texture2D Volume;

        public static Effect gaussianBlurHorz;
        public static Effect gaussianBlurVert;
        public static Effect PixelBlur;
        
        // Music Player Manager Values
        public static string currentlyPlayingSongName
        {
            get
            {
                return PlayerHistory[PlayerHistoryIndex].Split('\\').Last();
            }
        }
        public static List<string> Playlist = new List<string>();
        public static List<string> PlayerHistory = new List<string>();
        public static int PlayerHistoryIndex = 0;

        // NAudio
        public static WaveChannel32 Channel32;
        public static WaveChannel32 Channel32Reader;
        public static DirectSoundOut output;
        public static Mp3FileReader mp3;
        public static MMDevice device;
        public static MMDeviceEnumerator enumerator;
        //public const int bufferLength = 8192;
        //public const int bufferLength = 16384;
        //public const int bufferLength = 32768;
        public const int bufferLength = 65536;
        //public const int bufferLength = 131072; 
        //public const int bufferLength = 262144;
        public static byte[] buffer;
        public static float[] WaveBuffer;
        public static float[] FFToutput;

        // Data Management
        public static void Load(ContentManager Content, GraphicsDevice GD)
        {
            Console.WriteLine("Loading Effects...");
            gaussianBlurHorz = Content.Load<Effect>("GaussianBlurHorz");
            gaussianBlurVert = Content.Load<Effect>("GaussianBlurVert");
            PixelBlur = Content.Load<Effect>("PixelBlur");


            Console.WriteLine("Loading Textures...");
            White = new Texture2D(GD, 1, 1);
            Color[] Col = new Color[1];
            Col[0] = Color.White;
            White.SetData(Col);


            Volume = Content.Load<Texture2D>("volume");
            bg1 = Content.Load<Texture2D>("bg1");
            bg2 = Content.Load<Texture2D>("bg2");
            bg3 = Content.Load<Texture2D>("bg3");
            bg4 = Content.Load<Texture2D>("bg4");
            bg5 = Content.Load<Texture2D>("bg5");


            Console.WriteLine("Loading Fonts...");
            Font = Content.Load<SpriteFont>("Font");
            Title = Content.Load<SpriteFont>("Title");


            Console.WriteLine("Loading Background...");
            RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
            {
                MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
            }
            FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
            bg = Texture2D.FromStream(GD, Stream);
            Stream.Dispose();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler((object o, UserPreferenceChangedEventArgs target) => { RefreshBGtex(GD); });


            /*FolderBrowserDialog open = new FolderBrowserDialog();
            open.Description = "Select your music folder";
            if (open.ShowDialog() != DialogResult.OK) Process.GetCurrentProcess().Kill();*/
            Console.WriteLine("Loading Songs...");
            //FindAllMp3FilesInDir(open.SelectedPath);
            FindAllMp3FilesInDir(Values.MusicPath);
            Console.WriteLine("Found " + Playlist.Count.ToString() + " Songs!");


            Console.WriteLine("Monitoring System Audio output...");
            enumerator = new MMDeviceEnumerator();
            device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);


            if (Playlist.Count > 0)
            {
                if (Program.args.Length > 0)
                    PlayNewSong(Program.args[0]);
                else
                {
                    int PlaylistIndex = Values.RDM.Next(Playlist.Count);
                    GetNextSong();
                    PlayerHistory.Add(Playlist[PlaylistIndex]);
                }
            }
            else
                Console.WriteLine("Playlist empty!");
        }
        public static void FindAllMp3FilesInDir(string StartDir)
        {
            foreach (string s in Directory.GetFiles(StartDir))
                if (s.EndsWith(".mp3"))
                    Playlist.Add(s);

            foreach (string D in Directory.GetDirectories(StartDir))
                FindAllMp3FilesInDir(D);
        }
        public static void RefreshBGtex(GraphicsDevice GD)
        {
            lock (bg)
            {
                RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
                bg = Texture2D.FromStream(GD, Stream);
                Stream.Dispose();
            }
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
                Channel32Reader.Dispose();
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
            buffer = new byte[bufferLength];
            WaveBuffer = new float[bufferLength / 4];

            if (Channel32 != null && Channel32Reader != null && Channel32Reader.CanRead)
            {
                // I have two WaveChannel32 objects, one to play the song and another 
                // that reads the data from the current position in realtime
                // It's not the most efficient method but Im gonna improve that later
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
            Complex[] tempbuffer = new Complex[WaveBuffer.Length];

            for (int i = 0; i < tempbuffer.Length; i++)
            {
                tempbuffer[i].X = (float)(WaveBuffer[i] * FastFourierTransform.HammingWindow(i, tempbuffer.Length));
                tempbuffer[i].Y = 0;
            }

            FastFourierTransform.FFT(true, (int)Math.Log(tempbuffer.Length, 2.0), tempbuffer);

            // EVERYTHING
            FFToutput = new float[tempbuffer.Length / 4 - 1];
            for (int i = 0; i < FFToutput.Length; i++)
            {
                //FFToutput[i] = (float)(tempbuffer[i].X * tempbuffer[i].X) + (tempbuffer[i].Y * tempbuffer[i].Y) * 100000;
                FFToutput[i] = (float)(Math.Log10(1 + Math.Sqrt((tempbuffer[i].X * tempbuffer[i].X) + (tempbuffer[i].Y * tempbuffer[i].Y))) * 250);
                //FFToutput[i] = -(float)Math.Log10((tempbuffer[i].X * tempbuffer[i].X) + (tempbuffer[i].Y * tempbuffer[i].Y)) * 10;
            }
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
                if (output.PlaybackState == PlaybackState.Playing) output.Pause();
                else if (output.PlaybackState == PlaybackState.Paused) output.Play();
            }
        }
        public static void GetNewPlaylistSong()
        {
            int PlaylistIndex = Values.RDM.Next(Playlist.Count);
            PlayerHistory.Add(Playlist[PlaylistIndex]);
            PlayerHistoryIndex = PlayerHistory.Count - 1;
            PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
        }
        public static void PlayNewSong(string Path)
        {
            Path = Path.Trim('"');
            PlayerHistory.Add(Path);
            PlayerHistoryIndex = PlayerHistory.Count - 1;

            if (!Playlist.Contains(Path))
                Playlist.Add(Path);

            PlaySongByPath(Path);
        }
        public static void GetNextSong()
        {
            PlayerHistoryIndex++;
            if (PlayerHistoryIndex > PlayerHistory.Count - 1)
                GetNewPlaylistSong();
            else
                PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
        }
        public static void GetPreviousSong()
        {
            if (PlayerHistoryIndex > 0)
            {
                PlayerHistoryIndex--;

                PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
            }
        }
        private static void PlaySongByPath(string PathString)
        {
            DisposeNAudioData();

            if (PathString.Contains("\""))
                PathString = PathString.Trim(new char[] { '"', ' '});

            mp3 = new Mp3FileReader(PathString);
            Channel32 = new WaveChannel32(mp3);
            Channel32Reader = new WaveChannel32(mp3);

            var meter = new MeteringSampleProvider(mp3.ToSampleProvider());
            meter.StreamVolume += (s, e) => Console.WriteLine("{0} - {1}", e.MaxSampleValues[0], e.MaxSampleValues[1]);

            output = new DirectSoundOut();
            output.Init(Channel32);
            output.Play();
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
                int HalfHeight = (int)Math.Sqrt(Radius * Radius - i * i);
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
            Rectangle RHorz = new Rectangle(Rect.X, (int)(Rect.Y + Rect.Height * (Rounding / 2)), Rect.Width, (int)(Rect.Height * (1-Rounding)));
            Rectangle RVert = new Rectangle((int)(Rect.X + Rect.Width * (Rounding / 2)), Rect.Y, (int)(Rect.Width * (1-Rounding)), (int)(Rect.Height * 0.999f));

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
                DrawCircle(new Vector2(Rect.X + Rect.Width - RadiusHorz -1, Rect.Y + RadiusVert + (int)(Rect.Height * (1 - Rounding))), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);
            }

            SB.Draw(White, RHorz, Col);
            SB.Draw(White, RVert, Col);
        }
    }
}
