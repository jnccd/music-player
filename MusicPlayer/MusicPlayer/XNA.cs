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
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using NAudio.Wave;
using NAudio.Dsp;
using System.IO;
using MusicPlayerwNAudio;

namespace MusicPlayer
{
    public enum Visualizations
    {
        line,
        dynamicline,
        fft,
        rawfft,
        barchart
    }
    public enum BackGroundModes
    {
        None,
        Blur,
        bg1,
        bg2
    }
    public enum SelectedControl
    {
        VolumeSlider,
        DurationBar,
        DragWindow,
        None
    }

    public class XNA : Game
    {
        // Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Form gameWindowForm;
        RenderTarget2D TempBlur;
        RenderTarget2D BlurredTex;
        static RenderTarget2D TitleTarget;

        // Visualization
        public static Visualizations VisSetting = (Visualizations)config.Default.Vis;
        public static BackGroundModes BgModes = (BackGroundModes)config.Default.Background;
        public static Color primaryColor = Color.FromNonPremultiplied(25, 75, 255, 255);
        public static Color secondaryColor = Color.FromNonPremultiplied(35, 125, 255, 255);
        GaussianDiagram GauD = null;

        // Stuff
        System.Drawing.Point MouseClickedPos = new System.Drawing.Point();
        System.Drawing.Point WindowLocation = new System.Drawing.Point();
        SelectedControl selectedControl = SelectedControl.None;
        int SongsFoundMessageAlpha = 555;
        List<string> LastConsoleInput = new List<string>();
        long CurrentDebugTime = 0;

        public XNA()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Values.WindowSize.X;
            graphics.PreferredBackBufferHeight = Values.WindowSize.Y;
            gameWindowForm = (Form)Form.FromHandle(this.Window.Handle);
            gameWindowForm.FormBorderStyle = FormBorderStyle.None;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 120.0f);
            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            IsMouseVisible = true;

            gameWindowForm.FormClosing += (object o, FormClosingEventArgs e) => {
                
            };
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Assets.Load(Content, GraphicsDevice);
            
            gameWindowForm.Location = new System.Drawing.Point(0, 0);
            
            BlurredTex = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);
            TempBlur = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);

            InactiveSleepTime = new TimeSpan(0);
            Console.WriteLine("Finished Loading!");
            StartSongInputLoop();
        }
        void StartSongInputLoop()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Play Song: ");
                    string Path = "";
                    while (!Path.Contains(".mp3\""))
                    {
                        ConsoleKeyInfo e = Console.ReadKey();

                        if (Path.Contains("/cls"))
                        {
                            Path = "";
                            Console.Clear();
                            Console.Write("Play Song: ");
                        }

                        if (e.Key == ConsoleKey.Enter)
                            break;

                        if (e.Key == ConsoleKey.Backspace)
                        { 
                            if (Path.Length > 0)
                                Path = Path.Remove(Path.Length - 1);
                            Console.CursorLeft = 0;
                            Console.Write("Play Song: " + Path + " ");
                            if (Path.Length > 1)
                                Console.CursorLeft = Path.Length + 11;
                            else
                                Console.CursorLeft = 11;
                        }
                        else if (e.Key >= (ConsoleKey)48 && e.Key <= (ConsoleKey)90 || 
                                 e.Key >= (ConsoleKey)186 && e.Key <= (ConsoleKey)226 ||
                                 e.Key == ConsoleKey.Spacebar)
                            Path += e.KeyChar;

                        Console.CursorLeft = 0;
                        Console.Write("                                                                    ");
                        Console.CursorLeft = 0;
                        Console.Write("Play Song: " + Path);
                    }
                    Console.WriteLine();
                    Assets.PlayNewSong(Path);
                }
            });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Values.Timer % 10000 == 0)
            {
                InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            }

            //Debug.WriteLine("New Update ----------------------------------------------------------------- ");

            Control.Update();
            ComputeControls();
            Values.Timer++;
            SongsFoundMessageAlpha--;

            //CurrentDebugTime = Stopwatch.GetTimestamp();
            if (Values.Timer % 100 == 0)
                CheckForRequestedSongs();
            //Debug.WriteLine("CheckForRequestedSongs " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

            // Stuff
            if (Assets.output != null && Assets.output.PlaybackState == PlaybackState.Playing)
            {
                //CurrentDebugTime = Stopwatch.GetTimestamp();
                UpdateGD();
                //Debug.WriteLine("GD Update " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                if (Assets.WaveBuffer != null)
                    Values.OutputVolume = Values.GetAverageVolume(Assets.WaveBuffer) * 1.2f;

                if (Values.OutputVolume < 0.0001f)
                    Values.OutputVolume = 0.0001f;

                if (Assets.Channel32 != null && Assets.Channel32.Position > Assets.Channel32.Length)
                    Assets.GetNextSong(false);
                //Debug.WriteLine("ifs " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                Assets.UpdateWaveBufferWithEntireSongWB();
                //Debug.WriteLine("UpdateWaveBufferWithEntireSongWB " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                if (VisSetting != Visualizations.line && Assets.Channel32 != null)
                    Assets.UpdateFFTbuffer();
                //Debug.WriteLine("UpdateFFTbuffer " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
            }

            //CurrentDebugTime = Stopwatch.GetTimestamp();
            if (Assets.Channel32 != null)
                Assets.Channel32.Volume = Values.TargetVolume - Values.OutputVolume * Values.TargetVolume;
            //Debug.WriteLine("Volume Update " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

            base.Update(gameTime);
        }
        void CheckForRequestedSongs()
        {
            RequestedSong.Default.Reload();
            if (RequestedSong.Default.RequestedSongString != "")
            {
                Assets.PlayNewSong(RequestedSong.Default.RequestedSongString);
                RequestedSong.Default.RequestedSongString = "";
                RequestedSong.Default.Save();
            }
        }
        void ComputeControls()
        {
            // Mouse Controls
            if (gameWindowForm.Focused && Control.WasLMBJustPressed())
            {
                MouseClickedPos = new System.Drawing.Point(Control.CurMS.X, Control.CurMS.Y);
                WindowLocation = gameWindowForm.Location;

                if (Control.GetMouseRect().Intersects(new Rectangle(25, Values.WindowSize.Y - 40, Values.WindowSize.X - 50, 23)))
                    selectedControl = SelectedControl.DurationBar;
                else if (Control.GetMouseRect().Intersects(new Rectangle(Values.WindowSize.X - 25 - 75, 20, 110, 16)))
                    selectedControl = SelectedControl.VolumeSlider;
                else
                    selectedControl = SelectedControl.DragWindow;
            }
            if (Control.WasLMBJustReleased())
            {
                if (selectedControl == SelectedControl.DurationBar)
                    Assets.output.Play();
                selectedControl = SelectedControl.None;
            }

            switch (selectedControl)
            {
                case SelectedControl.VolumeSlider:
                    float value = (Control.GetMouseVector().X - (Values.WindowSize.X - 100)) / 150f;
                    if (value < 0) value = 0;
                    if (value > 0.5f) value = 0.5f;
                    Values.TargetVolume = value;
                    break;

                case SelectedControl.DurationBar:
                    Assets.Channel32.Position =
                           (long)(((Control.GetMouseVector().X - 25) / (Values.WindowSize.X - 50)) *
                           Assets.Channel32.TotalTime.TotalSeconds *
                           Assets.Channel32.WaveFormat.AverageBytesPerSecond);

                    if (Control.CurMS.X == Control.LastMS.X)
                        Assets.output.Pause();
                    else
                        Assets.output.Play();
                    break;

                case SelectedControl.DragWindow:
                    config.Default.WindowPos = new System.Drawing.Point(gameWindowForm.Location.X + Control.CurMS.X - MouseClickedPos.X,
                                                                        gameWindowForm.Location.Y + Control.CurMS.Y - MouseClickedPos.Y);
                    break;
            }
            gameWindowForm.Location = config.Default.WindowPos;

            // Pause [Space]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
                Assets.PlayPause();

            // Set Location to (0, 0) [0]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D0) ||
                Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad0))
                config.Default.WindowPos = new System.Drawing.Point(0, 0);

            // Swap Visualisations [V]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                VisSetting++;
                if ((int)VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                    VisSetting = 0;

                if (VisSetting == Visualizations.dynamicline)
                    VisSetting = Visualizations.fft;
            }

            // Swap Backgrounds [B]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                BgModes++;
                if ((int)BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                    BgModes = 0;
            }

            // Toggle Anti-Alising [A]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
                GauD.AntiAlising = !GauD.AntiAlising;

            // Higher / Lower Volume [Up/Down]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                Values.TargetVolume += 0.005f;
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                Values.TargetVolume -= 0.005f;
            if (Values.TargetVolume > 0.5f)
                Values.TargetVolume = 0.5f;
            if (Values.TargetVolume < 0)
                Values.TargetVolume = 0;

            // Next / Previous Song [Left/Right]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) || Control.ScrollWheelWentDown())
                Assets.GetPreviousSong();
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) || Control.ScrollWheelWentUp())
                Assets.GetNextSong(false);

            // Close [Esc]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && base.IsActive)
                Exit();

            // Show Music File in Explorer [E]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                if (!File.Exists(Assets.currentlyPlayingSongPath))
                    return;
                else
                {
                    Process.Start("explorer.exe", "/select, \"" + Assets.currentlyPlayingSongPath + "\"");
                }
            }

            // Reset Music Source Folder [S]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
            {
                config.Default.MusicPath = "";
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C ping 127.0.0.1 -n 2 && cls && \"" + Application.ExecutablePath + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Application.Exit();
            }
        }
        void UpdateGD()
        {
            if (Assets.FFToutput != null && VisSetting != Visualizations.line)
            {
                float ReadLength = Assets.FFToutput.Length / 3f;
                float[] values = new float[Values.WindowSize.X - 70];
                for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    double lastindex = Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X);
                    double index = Math.Pow(ReadLength, i / (double)Values.WindowSize.X);
                    values[i - 70] = Assets.GetMaxHeight(Assets.FFToutput, (int)lastindex, (int)index);
                }
                //Debug.WriteLine("GD Update 1 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
                if (GauD == null)
                    GauD = new GaussianDiagram(values, new Point(35, (int)(Values.WindowSize.Y / 1.25f)), 175, true, 3);
                else
                {
                    GauD.Update(values);
                    GauD.Smoothen();
                }

                //Debug.WriteLine("GD Update 2 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // RenderTargets
            if (TitleTarget == null)
            {
                string Title;
                if (Assets.currentlyPlayingSongName.Contains(".mp3"))
                    Title = Assets.currentlyPlayingSongName.TrimEnd(new char[] { '.', 'm', 'p', '3' });
                else
                    Title = Assets.currentlyPlayingSongName;

                TitleTarget = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X - 166, (int)Assets.Title.MeasureString(Title).Y); // DRAW TO (24, 12)
                GraphicsDevice.SetRenderTarget(TitleTarget);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                
                spriteBatch.DrawString(Assets.Title, Title, Vector2.Zero, Color.White);

                spriteBatch.End();
            }

            if (VisSetting == Visualizations.fft && Assets.Channel32 != null && Assets.FFToutput != null)
                GauD.DrawToRenderTarget(spriteBatch, GraphicsDevice);

            #region Blur
            BeginBlur();
            if (BgModes == BackGroundModes.Blur)
            {
                spriteBatch.Begin();
                // Blurred Background
                foreach (Screen S in Screen.AllScreens)
                    spriteBatch.Draw(Assets.bg, new Rectangle(S.Bounds.X - gameWindowForm.Location.X + 50, S.Bounds.Y - gameWindowForm.Location.Y + 50,
                        S.Bounds.Width, S.Bounds.Height), Color.White);
                spriteBatch.End();
            }
            EndBlur();
            #endregion

            base.Draw(gameTime);
            
            // Background
            #region Background
            // Background
            spriteBatch.Begin();
            if (BgModes == BackGroundModes.None)
            {
                foreach (Screen S in Screen.AllScreens)
                    spriteBatch.Draw(Assets.bg, new Rectangle(S.Bounds.X - gameWindowForm.Location.X, S.Bounds.Y - gameWindowForm.Location.Y, 
                        S.Bounds.Width, S.Bounds.Height), Color.White);
            }

            if (BgModes == BackGroundModes.bg1)
                spriteBatch.Draw(Assets.bg1, Vector2.Zero, Color.White);
            if (BgModes == BackGroundModes.bg2)
                spriteBatch.Draw(Assets.bg2, Vector2.Zero, Color.White);
            spriteBatch.End();
            #endregion
            DrawBlurredTex();

            // Visualizations
            #region Line graph
            // Line Graph
            if (VisSetting == Visualizations.line && Assets.Channel32 != null)
            {
                spriteBatch.Begin();

                float Height = Values.WindowSize.Y / 1.96f;
                int StepLength = Assets.WaveBuffer.Length / 512;

                // Shadow
                for (int i = 1; i < 512; i++)
                {
                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (Assets.WaveBuffer.Length / StepLength) + 5,
                                    Height + (int)(Assets.WaveBuffer[(i - 1) * StepLength] * 100) + 5),

                                    new Vector2(i * Values.WindowSize.X / (Assets.WaveBuffer.Length / StepLength) + 5,
                                    Height + (int)(Assets.WaveBuffer[i * StepLength] * 100) + 5),

                                    2, Color.Black * 0.6f, spriteBatch);
                }

                for (int i = 1; i < 512; i++)
                {
                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (Assets.WaveBuffer.Length / StepLength),
                                    Height + (int)(Assets.WaveBuffer[(i - 1) * StepLength] * 100)),

                                    new Vector2(i * Values.WindowSize.X / (Assets.WaveBuffer.Length / StepLength),
                                    Height + (int)(Assets.WaveBuffer[i * StepLength] * 100)),

                                    2, Color.Lerp(primaryColor, secondaryColor, i / 512), spriteBatch);
                }
                spriteBatch.End();
            }
            #endregion
            #region Dynamic Line graph
            // Line Graph
            if (VisSetting == Visualizations.dynamicline && Assets.Channel32 != null)
            {
                spriteBatch.Begin();

                float Height = Values.WindowSize.Y / 1.96f;
                int StepLength = Assets.WaveBuffer.Length / 512;
                float MostUsedFrequency = Array.IndexOf(Assets.RawFFToutput, Assets.RawFFToutput.Max());
                float MostUsedWaveLength = 10000;
                if (MostUsedFrequency != 0)
                    MostUsedWaveLength = 1 / MostUsedFrequency;
                float[] MostUsedFrequencyMultiplications = new float[100];
                for (int i = 1; i <= 100; i++)
                    MostUsedFrequencyMultiplications[i - 1] = MostUsedFrequency * i;
                Debug.WriteLine((MostUsedFrequency / Assets.Channel32.WaveFormat.SampleRate * Assets.RawFFToutput.Length) + " ::: " + MostUsedFrequency);

                // Shadow
                for (int i = 1; i < 512; i++)
                {
                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (512) + 5,
                                    Height + (int)(Assets.WaveBuffer[(i - 1) * StepLength] * 100) + 5),

                                    new Vector2(i * Values.WindowSize.X / (512) + 5,
                                    Height + (int)(Assets.WaveBuffer[i * StepLength] * 100) + 5),

                                    2, Color.Black * 0.6f, spriteBatch);
                }

                for (int i = 1; i < 512; i++)
                {
                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (512),
                                    Height + (int)(Assets.WaveBuffer[(i - 1) * StepLength] * 100)),

                                    new Vector2(i * Values.WindowSize.X / (512),
                                    Height + (int)(Assets.WaveBuffer[i * StepLength] * 100)),

                                    2, Color.Lerp(primaryColor, secondaryColor, i / 512), spriteBatch);
                }
                spriteBatch.End();
            }
            #endregion
            #region FFT Graph
            // FFT Graph
            if (VisSetting == Visualizations.fft && Assets.Channel32 != null && Assets.FFToutput != null)
                GauD.DrawRenderTarget(spriteBatch);
            #endregion
            #region Raw FFT Graph
            if (VisSetting == Visualizations.rawfft && Assets.Channel32 != null && Assets.FFToutput != null)
            {
                spriteBatch.Begin();
                GauD.DrawInputData(spriteBatch);
                spriteBatch.End();
            }
            #endregion
            #region FFT Bars
            // FFT Bars
            if (VisSetting == Visualizations.barchart)
            {
                spriteBatch.Begin();
                GauD.DrawAsBars(spriteBatch);
                spriteBatch.End();
            }
            #endregion
            
            // HUD
            #region HUD
            spriteBatch.Begin();
            // Duration Bar
            spriteBatch.Draw(Assets.White, new Rectangle(30, Values.WindowSize.Y - 25, Values.WindowSize.X - 50, 3), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, Values.WindowSize.X - 50, 3), Color.White);
            if (Assets.Channel32 != null)
            {
                lock (Assets.Channel32)
                {
                    spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, (int)((Values.WindowSize.X - 50) *
                        (Assets.Channel32.Position / (float)Assets.Channel32.WaveFormat.AverageBytesPerSecond /
                        ((float)Assets.Channel32.TotalTime.TotalSeconds))), 3), primaryColor);
                    double Percetage = (double)Assets.EntireSongWaveBuffer.Count / Assets.Channel32.Length * 4.0;
                    if (Percetage < 1)
                        spriteBatch.Draw(Assets.White, new Rectangle(25 + (int)((Values.WindowSize.X - 50) * Percetage), Values.WindowSize.Y - 30, Values.WindowSize.X - 50 - (int)((Values.WindowSize.X - 50) * Percetage), 3),
                            Color.Red * 0.5f);
                }
            }

            // Song Title Displayer
            spriteBatch.Draw(TitleTarget, new Vector2(29, 17), Color.Black * 0.6f);
            spriteBatch.Draw(TitleTarget, new Vector2(24, 12), Color.White);

            if (SongsFoundMessageAlpha > 0)
            {
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24 + 5, 45 + 5), Color.Black * 0.6f * (SongsFoundMessageAlpha / 255f));
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24, 45), primaryColor * (SongsFoundMessageAlpha / 255f));
            }

            // Volume
            spriteBatch.Draw(Assets.Volume, new Rectangle(Values.WindowSize.X - 127, 21, 24, 24), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 95,  29, 75, 8),  Color.Black * 0.6f);

            spriteBatch.Draw(Assets.Volume, new Rectangle(Values.WindowSize.X - 132, 16, 24, 24), Color.White);
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 100         , 24, 75, 8), Color.White);
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 100         , 24, (int)(75 * Values.TargetVolume * 2), 8), secondaryColor);

            if (Assets.Channel32 != null)
                spriteBatch.Draw(Assets.White, new Rectangle(Values.WindowSize.X - 25 - 75, 24, (int)(75 * Assets.Channel32.Volume * 2), 8), primaryColor);

            spriteBatch.End();
            #endregion
        }
        public static void ForceTitleRedraw()
        {
            TitleTarget = null;
        }
        void BeginBlur()
        {
            GraphicsDevice.SetRenderTarget(TempBlur);
            GraphicsDevice.Clear(Color.Transparent);
        }
        void EndBlur()
        {
            GraphicsDevice.SetRenderTarget(BlurredTex);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, Assets.gaussianBlurVert);
            spriteBatch.Draw(TempBlur, Vector2.Zero, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }
        void DrawBlurredTex()
        {
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, Assets.gaussianBlurHorz);
            spriteBatch.Draw(BlurredTex, new Vector2(-50), Color.White);
            spriteBatch.End();
        }
    }
}
