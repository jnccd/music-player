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
        PlayPauseButton,
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
        public static Color secondaryColor = Color.FromNonPremultiplied((int)(primaryColor.R * 1.5f), (int)(primaryColor.G * 1.5f), (int)(primaryColor.B * 1.5f), 255);
        public static GaussianDiagram GauD = null;
        static ColorDialog LeDialog;
        static bool ShowingColorDialog = false;

        // Stuff
        System.Drawing.Point MouseClickedPos = new System.Drawing.Point();
        System.Drawing.Point WindowLocation = new System.Drawing.Point();
        SelectedControl selectedControl = SelectedControl.None;
        int SongsFoundMessageAlpha = 555;
        float UpvoteIconAlpha = 0;
        List<string> LastConsoleInput = new List<string>();
        int LastConsoleInputIndex = -1;
        //long CurrentDebugTime = 0;
        long CurrentDebugTime2 = 0;
        OptionsMenu optionsMenu = new OptionsMenu();
        public static bool FocusWindow = false;

        // Draw Rectangles
        static Rectangle DurationBar = new Rectangle(50, Values.WindowSize.Y - 28, Values.WindowSize.X - 77, 3);
        static Rectangle VolumeIcon = new Rectangle(Values.WindowSize.X - 132, 16, 24, 24);
        static Rectangle VolumeBar = new Rectangle(Values.WindowSize.X - 100, 24, 75, 8);
        static Rectangle PlayPauseButton = new Rectangle(24, Values.WindowSize.Y - 35, 16, 16);
        static Rectangle Upvote = new Rectangle(24, 43, 20, 20);
        static Rectangle TargetVolumeBar = new Rectangle(); // needs Updates
        static Rectangle ActualVolumeBar = new Rectangle(); // needs Updates

        static Rectangle DurationBarShadow = new Rectangle(DurationBar.X + 5, DurationBar.Y + 5, DurationBar.Width, DurationBar.Height);
        static Rectangle VolumeIconShadow = new Rectangle(VolumeIcon.X + 5, VolumeIcon.Y + 5, VolumeIcon.Width, VolumeIcon.Height);
        static Rectangle VolumeBarShadow = new Rectangle(VolumeBar.X + 5, VolumeBar.Y + 5, VolumeBar.Width, VolumeBar.Height);
        static Rectangle PlayPauseButtonShadow = new Rectangle(PlayPauseButton.X + 5, PlayPauseButton.Y + 5, PlayPauseButton.Width, PlayPauseButton.Height);
        static Rectangle UpvoteShadow = new Rectangle(Upvote.X + 5, Upvote.Y + 5, Upvote.Width, Upvote.Height);

        // Hitbox Rectangles
        Rectangle DurationBarHitbox = new Rectangle(DurationBar.X, DurationBar.Y - 10, DurationBar.Width, 23);
        Rectangle VolumeBarHitbox = new Rectangle(Values.WindowSize.X - 100, 20, 110, 16);
        Rectangle PlayPauseButtonHitbox = new Rectangle(14, Values.WindowSize.Y - 39, 26, 26);

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
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameWindowForm.FormClosing += (object sender, FormClosingEventArgs e) => {
                Assets.SaveSongUpvote();

                InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                Assets.DisposeNAudioData();

                config.Default.Background = (int)BgModes;
                config.Default.Vis = (int)VisSetting;
                config.Default.Save();
                //MessageBox.Show("ApplicationExit EVENT");
            };

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
                    string Path = "";
                    int originY = Console.CursorTop;
                    while (!Path.Contains(".mp3\""))
                    {
                        Thread.Sleep(7);
                        Console.SetCursorPosition(0, originY);
                        for (int i = 0; i < 5; i++)
                            Console.Write("                                                                    ");
                        Console.SetCursorPosition(0, originY);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Play Song: ");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(Path);

                        ConsoleKeyInfo e = Console.ReadKey();

                        if (Path.Contains("/cls"))
                        {
                            Path = "";
                            Console.Clear();
                        }

                        if (e.Key == ConsoleKey.UpArrow)
                        {
                            LastConsoleInputIndex++;
                            if (LastConsoleInputIndex > LastConsoleInput.Count - 1)
                                LastConsoleInputIndex = LastConsoleInput.Count - 1;
                            if (LastConsoleInput.Count > 0)
                                Path = LastConsoleInput[LastConsoleInput.Count - 1 - LastConsoleInputIndex];
                        }

                        if (e.Key == ConsoleKey.DownArrow)
                        {
                            LastConsoleInputIndex--;
                            if (LastConsoleInputIndex < -1)
                                LastConsoleInputIndex = -1;
                            if (LastConsoleInputIndex > -1)
                                Path = LastConsoleInput[LastConsoleInput.Count - 1 - LastConsoleInputIndex];
                            else
                                Path = "";
                        }

                        if (e.Key == ConsoleKey.Enter)
                            break;

                        if (e.Key == ConsoleKey.Backspace)
                        {
                            if (Path.Length > 0)
                                Path = Path.Remove(Path.Length - 1);
                        }
                        else if (e.Key >= (ConsoleKey)48 && e.Key <= (ConsoleKey)90 || 
                                 e.Key >= (ConsoleKey)186 && e.Key <= (ConsoleKey)226 ||
                                 e.Key == ConsoleKey.Spacebar)
                            Path += e.KeyChar;
                    }
                    Console.WriteLine();
                    if (Assets.PlayNewSong(Path))
                        LastConsoleInput.Add(Path);
                }
            });
        }

        protected override void Update(GameTime gameTime)
        {
            Control.Update();
            if (gameWindowForm.Focused)
                ComputeControls();

            // Next / Previous Song [MouseWheel] ((On Win10 mouseWheel input is send to the process even if its not focused))
            if (Control.ScrollWheelWentDown())
                Assets.GetPreviousSong();
            if (Control.ScrollWheelWentUp())
                Assets.GetNextSong(false);

            if (FocusWindow) { gameWindowForm.Focus(); FocusWindow = false; }

            if (Assets.IsCurrentSongUpvoted)
            {
                if (UpvoteIconAlpha < 1)
                    UpvoteIconAlpha += 0.05f;
            }
            else if (UpvoteIconAlpha > 0)
                UpvoteIconAlpha -= 0.05f;

            Values.Timer++;
            SongsFoundMessageAlpha--;
            
            // Stuff
            if (Assets.output != null && Assets.output.PlaybackState == PlaybackState.Playing)
            {
                if (Assets.WaveBuffer != null)
                    Values.OutputVolume = Values.GetAverageVolume(Assets.WaveBuffer) * 1.2f;

                if (Values.OutputVolume < 0.0001f)
                    Values.OutputVolume = 0.0001f;

                if (Assets.Channel32 != null && Assets.Channel32.Position > Assets.Channel32.Length)
                    Assets.GetNextSong(false);

                if (Assets.EntireSongWaveBuffer != null)
                    Assets.UpdateWaveBufferWithEntireSongWB();

                if (VisSetting != Visualizations.line && Assets.Channel32 != null)
                    Assets.UpdateFFTbuffer();
                
                UpdateGD();
            }
            
            if (Assets.Channel32 != null)
                Assets.Channel32.Volume = Values.TargetVolume - Values.OutputVolume * Values.TargetVolume;
            
            UpdateRectangles();

            base.Update(gameTime);
        }
        public static void CheckForRequestedSongs()
        {
            bool Worked = false;
            while (!Worked)
            {
                try
                {
                    RequestedSong.Default.Reload();
                    if (RequestedSong.Default.RequestedSongString != "")
                    {
                        Assets.PlayNewSong(RequestedSong.Default.RequestedSongString);
                        RequestedSong.Default.RequestedSongString = "";
                        RequestedSong.Default.Save();
                    }
                    Worked = true;
                }
                catch { }
            }
        }
        void ComputeControls()
        {
            // Mouse Controls
            if (gameWindowForm.Focused && Control.WasLMBJustPressed())
            {
                MouseClickedPos = new System.Drawing.Point(Control.CurMS.X, Control.CurMS.Y);
                WindowLocation = gameWindowForm.Location;

                if (Control.GetMouseRect().Intersects(DurationBarHitbox))
                    selectedControl = SelectedControl.DurationBar;
                else if (Control.GetMouseRect().Intersects(VolumeBarHitbox))
                    selectedControl = SelectedControl.VolumeSlider;
                else if (Control.GetMouseRect().Intersects(PlayPauseButtonHitbox))
                    selectedControl = SelectedControl.PlayPauseButton;
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
                    float value = (Control.GetMouseVector().X - VolumeBar.X) / (VolumeBar.Width * 2f);
                    if (value < 0) value = 0;
                    if (value > 0.5f) value = 0.5f;
                    Values.TargetVolume = value;
                    break;

                case SelectedControl.PlayPauseButton:
                    if (Control.WasLMBJustPressed())
                        Assets.PlayPause();
                    break;

                case SelectedControl.DurationBar:
                    Assets.Channel32.Position =
                           (long)(((Control.GetMouseVector().X - DurationBar.X) / DurationBar.Width) *
                           Assets.Channel32.TotalTime.TotalSeconds * Assets.Channel32.WaveFormat.AverageBytesPerSecond);

                    if (Control.CurMS.X == Control.LastMS.X)
                        Assets.output.Pause();
                    else
                        Assets.output.Play();
                    break;

                case SelectedControl.DragWindow:
                    config.Default.WindowPos = new System.Drawing.Point(gameWindowForm.Location.X + Control.CurMS.X - MouseClickedPos.X,
                                                                        gameWindowForm.Location.Y + Control.CurMS.Y - MouseClickedPos.Y);
                    KeepWindowInScreen();
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

            // Open OptionsMenu [O / F1]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.O) ||
                Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F1))
            {
                if (optionsMenu == null || optionsMenu.IsDisposed)
                    optionsMenu = new OptionsMenu();

                optionsMenu.Show();
            }

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

            // New Color [C]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.C))
                ShowColorDialog();

            // Toggle Anti-Alising [A]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
                GauD.AntiAlising = !GauD.AntiAlising;

            // Upvote/Like Current Song [L]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.L))
                Assets.IsCurrentSongUpvoted = !Assets.IsCurrentSongUpvoted;

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
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Left))
                Assets.GetPreviousSong();
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Right))
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
                    Process.Start("explorer.exe", "/select, \"" + Assets.currentlyPlayingSongPath + "\"");
            }

            // Reset Music Source Folder [S]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
            {
                ResetMusicSourcePath();
            }
        }
        public static void ShowColorDialog()
        {
            Task.Factory.StartNew(() =>
            {
                if (LeDialog == null)
                {
                    LeDialog = new ColorDialog();
                    LeDialog.AllowFullOpen = true;
                    LeDialog.AnyColor = true;
                    LeDialog.Color = System.Drawing.Color.FromArgb(primaryColor.A, primaryColor.R, primaryColor.G, primaryColor.B);

                    Color[] Background = new Color[Assets.bg.Bounds.Width * Assets.bg.Bounds.Height];
                    Vector3 AvgColor = new Vector3();
                    Assets.bg.GetData(Background);

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.X += Background[i].R;
                    AvgColor.X /= Background.Length;

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.Y += Background[i].G;
                    AvgColor.Y /= Background.Length;

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.Z += Background[i].B;
                    AvgColor.Z /= Background.Length;

                    System.Drawing.Color C = System.Drawing.Color.FromArgb(255, (int)AvgColor.X, (int)AvgColor.Y, (int)AvgColor.Z);

                    LeDialog.CustomColors = new int[]{C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(),
                            C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb(),
                            C.ToArgb(), C.ToArgb(), C.ToArgb(), C.ToArgb()};
                }
                if (!ShowingColorDialog)
                {
                    ShowingColorDialog = true;
                    DialogResult DR = LeDialog.ShowDialog();
                    if (DR == DialogResult.OK)
                    {
                        primaryColor = Color.FromNonPremultiplied(LeDialog.Color.R, LeDialog.Color.G, LeDialog.Color.B, LeDialog.Color.A);
                        secondaryColor = Color.FromNonPremultiplied((int)(primaryColor.R * 1.5f), (int)(primaryColor.G * 1.5f), (int)(primaryColor.B * 1.5f), 255);
                    }
                    ShowingColorDialog = false;
                }
            });
        }
        public static void ResetMusicSourcePath()
        {
            DialogResult DR = MessageBox.Show("Are you sure you want to reset the music source path?", "Source Path Reset", MessageBoxButtons.YesNo);

            if (DR != DialogResult.Yes)
                return;
            
            config.Default.MusicPath = "";
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C ping 127.0.0.1 -n 2 && cls && \"" + Application.ExecutablePath + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
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
        void UpdateRectangles()
        {
            TargetVolumeBar = new Rectangle(Values.WindowSize.X - 100, 24, (int)(75 * Values.TargetVolume * 2), 8);
            if (Assets.Channel32 != null)
                ActualVolumeBar = new Rectangle(Values.WindowSize.X - 25 - 75, 24, (int)(75 * Assets.Channel32.Volume * 2), 8);
        }
        void KeepWindowInScreen()
        {
            Rectangle VirtualWindow = new Rectangle(config.Default.WindowPos.X, config.Default.WindowPos.Y, 
                gameWindowForm.Bounds.Width, gameWindowForm.Bounds.Height);
            Rectangle[] ScreenBoxes = new Rectangle[Screen.AllScreens.Length];

            for (int i = 0; i < ScreenBoxes.Length; i++)
                ScreenBoxes[i] = new Rectangle(Screen.AllScreens[i].Bounds.X, Screen.AllScreens[i].Bounds.Y, 
                    Screen.AllScreens[i].Bounds.Width, Screen.AllScreens[i].Bounds.Height - 56);

            Point[] WindowPoints = new Point[4];
            WindowPoints[0] = new Point(VirtualWindow.X, VirtualWindow.Y);
            WindowPoints[1] = new Point(VirtualWindow.X + VirtualWindow.Width, VirtualWindow.Y);
            WindowPoints[2] = new Point(VirtualWindow.X, VirtualWindow.Y + VirtualWindow.Height);
            WindowPoints[3] = new Point(VirtualWindow.X + VirtualWindow.Width, VirtualWindow.Y + VirtualWindow.Height);
            
            for (int i = 0; i < WindowPoints.Length; i++)
                if (!RectanglesContainPoint(WindowPoints[i], ScreenBoxes))
                {
                    Screen Main = Values.TheWindowsMainScreen(VirtualWindow);
                    if (Main == null) Main = Screen.PrimaryScreen;
                    Point Diff = PointRectDiff(WindowPoints[i], new Rectangle(Main.Bounds.X, Main.Bounds.Y, Main.Bounds.Width, Main.Bounds.Height - 40));
                    if (Diff != new Point(0, 0))
                    {
                        VirtualWindow = new Rectangle(VirtualWindow.X + Diff.X, VirtualWindow.Y + Diff.Y, VirtualWindow.Width, VirtualWindow.Height);
                        MouseClickedPos = new System.Drawing.Point(MouseClickedPos.X - Diff.X, MouseClickedPos.Y - Diff.Y);

                        WindowPoints[0] = new Point(VirtualWindow.X, VirtualWindow.Y);
                        WindowPoints[1] = new Point(VirtualWindow.X + VirtualWindow.Width, VirtualWindow.Y);
                        WindowPoints[2] = new Point(VirtualWindow.X, VirtualWindow.Y + VirtualWindow.Height);
                        WindowPoints[3] = new Point(VirtualWindow.X + VirtualWindow.Width, VirtualWindow.Y + VirtualWindow.Height);
                    }
                }

            config.Default.WindowPos = new System.Drawing.Point(VirtualWindow.X, VirtualWindow.Y);
        }
        bool RectanglesContainPoint(Point P, Rectangle[] R)
        {
            for (int i = 0; i < R.Length; i++)
                if (R[i].Contains(P))
                    return true;
            return false;
        }
        Point PointRectDiff(Point P, Rectangle R)
        {
            if (P.X > R.X && P.X < R.X + R.Width &&
                P.Y > R.Y && P.Y < R.Y + R.Height)
                return new Point(0, 0);
            else
            {
                Point r = new Point(0, 0);
                if (P.X < R.X)
                    r.X = R.X - P.X;
                if (P.X > R.X + R.Width)
                    r.X = R.X + R.Width - P.X;
                if (P.Y < R.Y)
                    r.Y = R.Y - P.Y;
                if (P.Y > R.Y + R.Height)
                    r.Y = R.Y + R.Height - P.Y;
                return r;
            }
        }
        public static void ReHookGlobalKeys()
        {
            InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
            InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);
        }

        protected override void Draw(GameTime gameTime)
        {
            CurrentDebugTime2 = Stopwatch.GetTimestamp();
            // RenderTargets
            #region RT
            if (TitleTarget == null || TitleTarget.IsContentLost || TitleTarget.IsDisposed)
            {
                string Title;
                if (Assets.currentlyPlayingSongName.Contains(".mp3"))
                {
                    Title = Assets.currentlyPlayingSongName.TrimEnd(new char[] { '3' });
                    Title = Title.TrimEnd(new char[] { 'p' });
                    Title = Title.TrimEnd(new char[] { 'm' });
                    Title = Title.TrimEnd(new char[] { '.' });
                }
                else
                    Title = Assets.currentlyPlayingSongName;

                TitleTarget = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X - 166, (int)Assets.Title.MeasureString("III()()()III").Y);
                GraphicsDevice.SetRenderTarget(TitleTarget);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);

                try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(5), Color.Black * 0.6f); } catch { }
                try { spriteBatch.DrawString(Assets.Title, Title, Vector2.Zero, Color.White); } catch { }

                spriteBatch.End();
            }

            if (VisSetting == Visualizations.fft && GauD != null && Assets.Channel32 != null && Assets.FFToutput != null)
                GauD.DrawToRenderTarget(spriteBatch, GraphicsDevice);
            #endregion
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
            spriteBatch.Draw(Assets.White, DurationBarShadow, Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, DurationBar, Color.White);
            if (Assets.Channel32 != null)
            {
                lock (Assets.Channel32)
                {
                    spriteBatch.Draw(Assets.White, new Rectangle(DurationBar.X, DurationBar.Y, (int)(DurationBar.Width *
                        (Assets.Channel32.Position / (float)Assets.Channel32.WaveFormat.AverageBytesPerSecond /
                        ((float)Assets.Channel32.TotalTime.TotalSeconds))), 3), primaryColor);
                    double Percetage = (double)Assets.EntireSongWaveBuffer.Count / Assets.Channel32.Length * 4.0;
                    if (Percetage < 1)
                        spriteBatch.Draw(Assets.White, new Rectangle(DurationBar.X + (int)(DurationBar.Width * Percetage), DurationBar.Y, DurationBar.Width - (int)(DurationBar.Width * Percetage), 3),
                            Color.Red * 0.5f);
                }
            }

            // Song Title Displayer
            spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default,
                RasterizerState.CullNone, Assets.TitleFadeout);
            if (TitleTarget != null)
                spriteBatch.Draw(TitleTarget, new Vector2(24, 12), Color.White);
            spriteBatch.End();

            spriteBatch.Begin();

            if (Assets.IsCurrentSongUpvoted)
            {
                spriteBatch.Draw(Assets.Upvote, UpvoteShadow, Color.Black * 0.6f * UpvoteIconAlpha);
                spriteBatch.Draw(Assets.Upvote, Upvote, Color.White * UpvoteIconAlpha);
                spriteBatch.DrawString(Assets.Font, "Upvoted!", new Vector2(Upvote.X + Upvote.Width + 8, Upvote.Y + Upvote.Height / 2 - 3), Color.Black * 0.6f * UpvoteIconAlpha);
                spriteBatch.DrawString(Assets.Font, "Upvoted!", new Vector2(Upvote.X + Upvote.Width + 3, Upvote.Y + Upvote.Height / 2 - 8), Color.White * UpvoteIconAlpha);
            }
            else if (SongsFoundMessageAlpha > 0)
            {
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24 + 5, 45 + 5), Color.Black * 0.6f * (SongsFoundMessageAlpha / 255f));
                spriteBatch.DrawString(Assets.Title, "Found " + Assets.Playlist.Count + " Songs!", new Vector2(24, 45), primaryColor * (SongsFoundMessageAlpha / 255f));
            }
            else
            {
                spriteBatch.Draw(Assets.Upvote, UpvoteShadow, Color.Black * 0.6f * UpvoteIconAlpha);
                spriteBatch.Draw(Assets.Upvote, Upvote, Color.White * UpvoteIconAlpha);
                spriteBatch.DrawString(Assets.Font, "Upvoted!", new Vector2(Upvote.X + Upvote.Width + 8, Upvote.Y + Upvote.Height / 2 - 3), Color.Black * 0.6f * UpvoteIconAlpha);
                spriteBatch.DrawString(Assets.Font, "Upvoted!", new Vector2(Upvote.X + Upvote.Width + 3, Upvote.Y + Upvote.Height / 2 - 8), Color.White * UpvoteIconAlpha);
            }

            // PlayPause Button
            if (Assets.IsPlaying())
            {
                spriteBatch.Draw(Assets.Pause, PlayPauseButtonShadow, Color.Black * 0.6f);
                spriteBatch.Draw(Assets.Pause, PlayPauseButton, Color.White);
            }
            else
            {
                spriteBatch.Draw(Assets.Play, PlayPauseButtonShadow, Color.Black * 0.6f);
                spriteBatch.Draw(Assets.Play, PlayPauseButton, Color.White);
            }

            // Volume
            spriteBatch.Draw(Assets.Volume, VolumeIconShadow, Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, VolumeBarShadow, Color.Black * 0.6f);

            spriteBatch.Draw(Assets.Volume, VolumeIcon, Color.White);
            spriteBatch.Draw(Assets.White, VolumeBar, Color.White);
            spriteBatch.Draw(Assets.White, TargetVolumeBar, secondaryColor);
            spriteBatch.Draw(Assets.White, ActualVolumeBar, primaryColor);

            spriteBatch.End();
            #endregion
            Debug.WriteLine("Draw " + (Stopwatch.GetTimestamp() - CurrentDebugTime2));
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
