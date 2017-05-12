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
        RenderTarget2D AudioVisTarget;

        // Visualization
        public static Visualizations VisSetting = (Visualizations)config.Default.Vis;
        public static BackGroundModes BgModes = (BackGroundModes)config.Default.Background;
        public static Color primaryColor = Color.FromNonPremultiplied(25, 75, 255, 255);
        public static Color secondaryColor = Color.FromNonPremultiplied(35, 125, 255, 255);
        GaussianDiagram GD = new GaussianDiagram();

        // Stuff
        System.Drawing.Point MouseClickedPos = new System.Drawing.Point();
        System.Drawing.Point WindowLocation = new System.Drawing.Point();
        SelectedControl selectedControl = SelectedControl.None;
        int SongsFoundMessageAlpha = 555;

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
            AudioVisTarget = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X, Values.WindowSize.Y);

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
                    Console.Write("Play Song: ");
                    string Path = "";
                    while (!Path.Contains(".mp3\""))
                    {
                        ConsoleKeyInfo e = Console.ReadKey();

                        if (Path.Contains("/cls"))
                        {
                            Path = "";
                            Console.Clear();
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
                        else
                            Path += e.KeyChar;
                    }
                    Console.WriteLine();
                    Assets.PlayNewSong(Path);
                }
            });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Values.Timer % 100 == 0)
            {
                InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
            }

            Control.Update();
            FPSCounter.Update(gameTime);
            //gameWindowForm.SendToBack();
            ComputeControls();
            Values.Timer++;
            SongsFoundMessageAlpha--;
            
            // Stuff
            if (Assets.output != null && Assets.output.PlaybackState == PlaybackState.Playing)
            {
                UpdateGD();
                
                Values.OutputVolume = Values.GetAverageVolume(Assets.WaveBuffer);

                if (Values.OutputVolume < 0.0001f)
                    Values.OutputVolume = 0.0001f;

                if (Assets.Channel32 != null && Assets.Channel32.Position > Assets.Channel32.Length)
                    Assets.GetNextSong(false);
                
                Assets.UpdateWaveBufferWithEntireSongWB();
                Assets.UpdateFFTbuffer();

                GD.Smoothen();
            }

            if (Assets.Channel32 != null)
            {
                Assets.Channel32.Volume = Values.TargetVolume - Values.OutputVolume * Values.TargetVolume * 0.6f;
                if (Assets.Channel32.Volume < Values.TargetVolume / 2f)
                    Assets.Channel32.Volume = Values.TargetVolume / 2f;
            }

            base.Update(gameTime);
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
            }

            // Swap Backgrounds [B]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                BgModes++;
                if ((int)BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                    BgModes = 0;
            }

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
        }
        void UpdateGD()
        {
            if (Assets.FFToutput != null)
            {
                float ReadLength = Assets.FFToutput.Length / 3f;
                float[] values = new float[Values.WindowSize.X - 70];
                for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    double lastindex = Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X);
                    double index = Math.Pow(ReadLength, i / (double)Values.WindowSize.X);
                    values[i - 70] = Assets.GetMaxHeight(Assets.FFToutput, (int)lastindex, (int)index);
                }
                GD = new GaussianDiagram(values, new Point(35, (int)(Values.WindowSize.Y / 1.25f)), 175, true, 3);

                GD.ApplyAllOutputData(i => i *= Values.TargetVolume * 2);
                GD.ApplyAllInputData(i => i *= Values.TargetVolume * 2);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
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
                int MostUsedFrequency = Array.IndexOf(Assets.RawFFToutput, Assets.RawFFToutput.Max());

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
            #region FFT Graph
            // FFT Graph
            if (VisSetting == Visualizations.fft && Assets.Channel32 != null && Assets.FFToutput != null)
            {
                spriteBatch.Begin();
                //float Length = Assets.FFToutput.Length;

                #region first try [INACTIVE]
                // Shadow
                /*for (int i = 0; i < Length; i++)
                {
                    double value = Assets.FFToutput[i];

                    if (value > 100)
                        value = 100;

                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / Length + 5,
                                    Values.WindowSize.Y / 2f + (int)value + 5),

                                    new Vector2(i * Values.WindowSize.X / Length + 5,
                                    Values.WindowSize.Y / 2f - (int)value + 5),

                                    2, Color.Black * 0.6f, spriteBatch);
                }

                for (int i = 0; i < Length; i++)
                {
                    double value = Assets.FFToutput[i];

                    if (value > 100)
                        value = 100;

                    Assets.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / Length,
                                    Values.WindowSize.Y / 2f + (int)value),

                                    new Vector2(i * Values.WindowSize.X / Length,
                                    Values.WindowSize.Y / 2f - (int)value),

                                    2, Color.Lerp(primaryColor, secondaryColor, i / Length), spriteBatch);
                }*/
                #endregion
                #region second try

                #region Schatten zur Seite [INACTIVE]
                /*int StartIndex = (int)(Math.Pow(ReadLength, 70 / (double)Values.WindowSize.X));
                for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    int lastindex = (int)(Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X));
                    int index = (int)(Math.Pow(ReadLength, i / (double)Values.WindowSize.X));
                    double value = Assets.GetMaxHeight(Assets.FFToutput, lastindex, index) * 175;

                    if (value > 175)
                        value = 175;

                    if (value < 1)
                        value = 1;

                    Vector2 V1 = new Vector2(i - 35 + (int)(value),
                                    Values.WindowSize.Y / 1.25f - (int)value);

                    Vector2 V2 = new Vector2(i - 35,
                                    Values.WindowSize.Y / 1.25f);

                    Assets.DrawLine(V1 + (V2 - V1) / 2,

                                    V2,

                                    2, Color.Black * 0.3f, spriteBatch);
                }*/
                #endregion

                #region Schlagschatten [INACTIVE]
                /*for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    int lastindex = (int)(Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X));
                    int index = (int)(Math.Pow(ReadLength, i / (double)Values.WindowSize.X));
                    double value = Assets.GetMaxHeight(Assets.FFToutput, lastindex, index) * 175;

                    if (value > 175)
                        value = 175;

                    if (value < 1)
                        value = 1;

                    Assets.DrawLine(new Vector2(i - 35 + 5,
                                    Values.WindowSize.Y / 1.25f - (int)value + 5),

                                    new Vector2(i - 35 + 5,
                                    Values.WindowSize.Y / 1.25f + 15),

                                    1, Color.Black * 0.6f, spriteBatch);
                }*/
                #endregion

                GD.Draw(spriteBatch);

                #region No Gaussian Diagram Save [INACTIVE]
                /*for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    int lastindex = (int)(Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X));
                    int index = (int)(Math.Pow(ReadLength, i / (double)Values.WindowSize.X));
                    double value = Assets.GetMaxHeight(Assets.FFToutput, lastindex, index) * 175;

                    if (value > 175)
                        value = 175;

                    if (value < 1)
                        value = 1;

                    Assets.DrawLine(new Vector2(i - 35,
                                    Values.WindowSize.Y / 1.25f - (int)value),

                                    new Vector2(i - 35,
                                    Values.WindowSize.Y / 1.25f + 10),

                                    1, Color.Lerp(primaryColor, secondaryColor, i / Length), spriteBatch);
                }*/
                #endregion
                #endregion
                #region third try [INACTIVE]
                /*double Max = Assets.FFToutput.Max();
                Debug.WriteLine("Max: " + Max.ToString());
                for (int i = 10; i < Values.WindowSize.X - 10; i++)
                {
                    double value = Assets.FFToutput[i] * 70;
                    //value = value / Max * 175;
                    
                    if (value > 175)
                        value = 175;

                    if (value < 1)
                        value = 1;

                    Assets.DrawLine(new Vector2(i,
                                    Values.WindowSize.Y / 1.25f - (int)value),

                                    new Vector2(i,
                                    Values.WindowSize.Y / 1.25f),

                                    2, Color.Lerp(primaryColor, secondaryColor, i / Length), spriteBatch);
                }*/
                #endregion

                spriteBatch.End();
            }
            #endregion
            #region Raw FFT Graph
            if (VisSetting == Visualizations.rawfft && Assets.Channel32 != null && Assets.FFToutput != null)
            {
                spriteBatch.Begin();
                GD.DrawInputData(spriteBatch);
                spriteBatch.End();
            }
            #endregion
            #region FFT Bars
            // FFT Bars
            if (VisSetting == Visualizations.barchart)
            {
                spriteBatch.Begin();
                GD.DrawAsBars(spriteBatch);
                spriteBatch.End();
            }
            #endregion

            #region HUD
            // HUD
            spriteBatch.Begin();

            // Duration Bar
            spriteBatch.Draw(Assets.White, new Rectangle(30, Values.WindowSize.Y - 25, Values.WindowSize.X - 50, 3), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, Values.WindowSize.X - 50, 3), Color.White);
            if (Assets.Channel32 != null)
                spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, (int)((Values.WindowSize.X - 50) *
                        (Assets.Channel32.Position / (float)Assets.Channel32.WaveFormat.AverageBytesPerSecond /
                        ((float)Assets.Channel32.TotalTime.TotalSeconds))), 3), primaryColor);

            // Song Title Displayer
            string Title;
            if (Assets.currentlyPlayingSongName.Contains(".mp3"))
                Title = Assets.currentlyPlayingSongName.TrimEnd(new char[] { '.', 'm', 'p', '3' });
            else
                Title = Assets.currentlyPlayingSongName;

            try {
                spriteBatch.DrawString(Assets.Title, 
                    (Title.Length <= 37 ? Title : Title.Substring(0, 37)), new Vector2(29, 17), Color.Black * 0.6f);
            } catch { Debug.WriteLine("Cant write: " + Title); }
            try
            {
                spriteBatch.DrawString(Assets.Title,
                    (Title.Length <= 37 ? Title : Title.Substring(0, 37)), new Vector2(24, 12), Color.White);
            } catch { }
            if (SongsFoundMessageAlpha > 0)
            {
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24 + 5, 45 + 5), Color.Black * 0.6f * (SongsFoundMessageAlpha / 255f));
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24, 45), primaryColor * (SongsFoundMessageAlpha / 255f));
            }

            // Volume
            spriteBatch.Draw(Assets.Volume, new Rectangle(Values.WindowSize.X - 25 - 75 - 8 - 24 + 5, 16 + 5, 24, 24), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, new Rectangle(Values.WindowSize.X - 25 - 75 + 5, 24 + 5, 75, 8), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.Volume, new Rectangle(Values.WindowSize.X - 25 - 75 - 8 - 24, 16, 24, 24), Color.White);
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 25 - 75         , 24, 75, 8), Color.White);
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 25 - 75         , 24, (int)(75 * Values.TargetVolume * 2), 8), 
                secondaryColor);
            if (Assets.Channel32 != null)
            {
                spriteBatch.Draw(Assets.White, new Rectangle(Values.WindowSize.X - 25 - 75, 24, (int)(75 * Assets.Channel32.Volume * 2), 8),
                primaryColor);
            }
            //FPSCounter.Draw(spriteBatch);
            spriteBatch.End();
            #endregion
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
