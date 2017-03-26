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

namespace MusicPlayer
{
    enum Visualizations
    {
        line,
        barchart
    }
    enum BackGroundModes
    {
        None,
        Blur,
        bg1,
        bg2,
        bg3,
        bg4,
        bg5
    }

    public class Game1 : Game
    {
        // Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Form gameWindowForm;
        RenderTarget2D TempBlur;
        RenderTarget2D BlurredTex;

        // Visualization
        float barWidth = Values.WindowSize.X / 260f;
        float barHeigth = 10;
        VisualizationData VisData = new VisualizationData();
        Visualizations VisSetting = Visualizations.barchart;
        BackGroundModes BgModes = BackGroundModes.None;
        Color primaryColor = Color.FromNonPremultiplied(25, 75, 255, 255);
        Color secondaryColor = Color.FromNonPremultiplied(35, 125, 255, 255);

        // Stuff
        System.Drawing.Point MouseClickedPos = new System.Drawing.Point();
        System.Drawing.Point WindowLocation = new System.Drawing.Point();
        int SongsFoundMessageAlpha = 555;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Values.WindowSize.X;
            graphics.PreferredBackBufferHeight = Values.WindowSize.Y;
            gameWindowForm = (Form)Form.FromHandle(this.Window.Handle);
            gameWindowForm.FormBorderStyle = FormBorderStyle.None;
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Assets.Load(Content, GraphicsDevice);
            MediaPlayer.IsVisualizationEnabled = true;
            //gameWindowForm.Location = new System.Drawing.Point(-1365, 0);
            BlurredTex = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);
            TempBlur = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);
            IsFixedTimeStep = false;
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

                        if (e.Key == ConsoleKey.Enter)
                            break;

                        Path += e.KeyChar;
                    }
                    Console.WriteLine();
                    Assets.PlaySongByPath(Path);
                }
            });
        }

        protected override void Update(GameTime gameTime)
        {
            Control.Update();
            FPSCounter.Update(gameTime);
            //gameWindowForm.SendToBack();
            ComputeControls();
            Values.Timer++;
            Assets.InputCooldown--;
            SongsFoundMessageAlpha--;

            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.GetVisualizationData(VisData);

            // Stuff
            if (MediaPlayer.State == MediaState.Playing)
            {
                Values.OutputVolume += (Values.GetAverageFrequency(0, 255, VisData) - Values.OutputVolume) / 1;
                MediaPlayer.Volume = Values.TargetVolume - Values.OutputVolume * Values.TargetVolume * 1.2f;
                Values.SongTime++;

                if (Values.SongTime / 60f > ((float)Assets.currentlyPlayingSong.Duration.TotalSeconds))
                    Assets.GetNewSong();
            }
            Values.currentTime = gameTime;
            
            base.Update(gameTime);
        }
        void ComputeControls()
        {
            // Mouse Controls
            if (Control.WasLMBJustPressed())
            {
                MouseClickedPos = new System.Drawing.Point(Control.CurMS.X, Control.CurMS.Y);
                WindowLocation = gameWindowForm.Location;
            }
            if (Control.CurMS.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && gameWindowForm.Focused && Control.GetMouseRect().Intersects(new Rectangle(0, 0, Values.WindowSize.X, Values.WindowSize.Y)))
            {
                if (Control.GetMouseRect().Intersects(new Rectangle(Values.WindowSize.X - 25 - 75, 20, 110, 16)))
                    Values.TargetVolume = (Control.GetMouseVector().X - (Values.WindowSize.X - 100)) / 150f;
                else
                    gameWindowForm.Location = new System.Drawing.Point(gameWindowForm.Location.X + Control.CurMS.X - MouseClickedPos.X,
                                                                           gameWindowForm.Location.Y + Control.CurMS.Y - MouseClickedPos.Y);
            }

            // Pause [Space]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Space) || Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.MediaPlayPause))
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                    Values.isMusicPlaying = false;
                }
                else
                {
                    MediaPlayer.Resume();
                    Values.isMusicPlaying = true;
                }

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

            // Show Music File in Explorer [E] {WIP}
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                Console.WriteLine(Assets.currentlyPlayingSong.Name);
                MediaLibrary ML = new MediaLibrary();
            }

            // Close [Esc]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && base.IsActive)
                Exit();
        }

        protected override void Draw(GameTime gameTime)
        {
            BeginBlur();
            spriteBatch.Begin();
            if (BgModes == BackGroundModes.Blur)
            {
                // Blurred Background
                spriteBatch.Draw(Assets.bg, new Vector2(-gameWindowForm.Location.X - 1360 + 50, -gameWindowForm.Location.Y + 50),
                    Assets.bg.Bounds, Color.White, 0, Vector2.Zero, new Vector2((float)GraphicsDevice.DisplayMode.Width / Assets.bg.Width,
                    (float)GraphicsDevice.DisplayMode.Height / Assets.bg.Height), SpriteEffects.None, 0);
                spriteBatch.Draw(Assets.bg, new Vector2(-gameWindowForm.Location.X + 50, -gameWindowForm.Location.Y + 50),
                    Assets.bg.Bounds, Color.White, 0, Vector2.Zero, new Vector2((float)GraphicsDevice.DisplayMode.Width / Assets.bg.Width,
                    (float)GraphicsDevice.DisplayMode.Height / Assets.bg.Height), SpriteEffects.None, 0);
            }

            // Line Graph
            if (VisSetting == Visualizations.line)
            {
                for (int i = 1; i < 256; i++)
                {
                    Assets.DrawLine(new Vector2((i - 1) * barWidth + (Values.WindowSize.X - barWidth * 256) / 2 + 50, Values.WindowSize.Y / 1.01f - (int)(VisData.Frequencies[i - 1] * 200 + 50)),
                                    new Vector2(i * barWidth + (Values.WindowSize.X - barWidth * 256) / 2 + 50, Values.WindowSize.Y / 1.01f - (int)(VisData.Frequencies[i] * 200 + 50)),
                                    (int)barHeigth, Color.Lerp(primaryColor, secondaryColor, i / 256f), spriteBatch);
                }
            }
            spriteBatch.End();
            EndBlur();

            base.Draw(gameTime);

            // Background
            spriteBatch.Begin();
            if (BgModes == BackGroundModes.None)
            {
                
                spriteBatch.Draw(Assets.bg, new Vector2(-gameWindowForm.Location.X - 1360, -gameWindowForm.Location.Y),
                    Assets.bg.Bounds, Color.White, 0, Vector2.Zero, new Vector2((float)GraphicsDevice.DisplayMode.Width / Assets.bg.Width,
                    (float)GraphicsDevice.DisplayMode.Height / Assets.bg.Height), SpriteEffects.None, 0);
                spriteBatch.Draw(Assets.bg, new Vector2(-gameWindowForm.Location.X, -gameWindowForm.Location.Y),
                    Assets.bg.Bounds, Color.White, 0, Vector2.Zero, new Vector2((float)GraphicsDevice.DisplayMode.Width / Assets.bg.Width,
                    (float)GraphicsDevice.DisplayMode.Height / Assets.bg.Height), SpriteEffects.None, 0);
            }

            if (BgModes == BackGroundModes.bg1)
                spriteBatch.Draw(Assets.bg1, Vector2.Zero, Color.White);
            if (BgModes == BackGroundModes.bg2)
                spriteBatch.Draw(Assets.bg2, Vector2.Zero, Color.White);
            if (BgModes == BackGroundModes.bg3)
                spriteBatch.Draw(Assets.bg3, Vector2.Zero, Color.White);
            if (BgModes == BackGroundModes.bg4)
                spriteBatch.Draw(Assets.bg4, Vector2.Zero, Color.White);
            if (BgModes == BackGroundModes.bg5)
                spriteBatch.Draw(Assets.bg5, Vector2.Zero, Color.White);
            spriteBatch.End();

            DrawBlurredTex();

            if (VisSetting == Visualizations.barchart)
            {
                spriteBatch.Begin();
                
                for (int i = 0; i < 16; i++)
                {
                    float Heigth = Math.Abs(Values.GetAverageFrequency(i * 15, (i + 1) * 15, VisData));
                    float Width = (int)(barWidth * 16);
                    spriteBatch.Draw(Assets.White, new Rectangle((int)(i * Width * 0.9f + Width * 1.7f) + (int)(Values.WindowSize.X - barWidth * 256) / 2 + 5,
                                                                 (int)(Values.WindowSize.Y / 1.2f) + 5,
                                                                 -(int)(Width * 0.6f),
                                                                 -(int)(Heigth * 255) - 8),
                                     Color.Black * 0.6f);

                    spriteBatch.Draw(Assets.White, new Rectangle((int)(i * Width * 0.9f + Width * 1.7f) + (int)(Values.WindowSize.X - barWidth * 256) / 2,
                                                                 (int)(Values.WindowSize.Y / 1.2f),
                                                                 -(int)(Width * 0.6f),
                                                                 -(int)(Heigth * 255) - 8),
                                     Color.Lerp(primaryColor, secondaryColor, i / 16f));
                }
                spriteBatch.End();
            }

            // HUD
            spriteBatch.Begin();

            // Duration Bar
            spriteBatch.Draw(Assets.White, new Rectangle(30, Values.WindowSize.Y - 25, Values.WindowSize.X - 50, 3), Color.Black * 0.6f);
            spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, Values.WindowSize.X - 50, 3), Color.White);
            spriteBatch.Draw(Assets.White, new Rectangle(25, Values.WindowSize.Y - 30, (int)((Values.WindowSize.X - 50) * 
                ((Values.SongTime / 60f) / ((float)Assets.currentlyPlayingSong.Duration.TotalSeconds))), 3), primaryColor);

            // Song Title Displayer
            try {
                spriteBatch.DrawString(Assets.Title, 
                    (Assets.currentlyPlayingSong.Name.Length <= 37
                    ? Assets.currentlyPlayingSong.Name
                    : Assets.currentlyPlayingSong.Name.Substring(0, 37)), new Vector2(29, 17), Color.Black * 0.6f);
                spriteBatch.DrawString(Assets.Title,
                    (Assets.currentlyPlayingSong.Name.Length <= 37
                    ? Assets.currentlyPlayingSong.Name
                    : Assets.currentlyPlayingSong.Name.Substring(0, 37)), new Vector2(24, 12), Color.White);
            }
            catch { }
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
            spriteBatch.Draw(Assets.White,  new Rectangle(Values.WindowSize.X - 25 - 75         , 24, (int)(75 * MediaPlayer.Volume * 2), 8), primaryColor);
            //spriteBatch.DrawString(Assets.Font, "Estimated output volume: " + Values.OutputVolume.ToString(), new Vector2(12, 24), Color.White);
            //spriteBatch.DrawString(Assets.Font, "Playlist length: " + Assets.Playlist.Count.ToString(), new Vector2(12, 36), Color.White);
            //FPSCounter.Draw(spriteBatch);
            spriteBatch.End();
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
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, null, null, Assets.gaussianBlurVert);
            spriteBatch.Draw(TempBlur, Vector2.Zero, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }
        void DrawBlurredTex()
        {
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, null, null, Assets.gaussianBlurHorz);
            spriteBatch.Draw(BlurredTex, new Vector2(-50), Color.White);
            spriteBatch.End();
        }
    }
}
