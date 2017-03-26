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

        public static Song currentlyPlayingSong
        {
            get
            {
                if (LastPlayedIndex == LastPlayed.Count - 1)
                    return Playlist[PlaylistIndex];
                else
                    return LastPlayed[LastPlayedIndex + 1];
            }
        }
        public static List<Song> Playlist = new List<Song>();
        public static List<Song> LastPlayed = new List<Song>();

        public static int PlaylistIndex = 0;
        public static int InputCooldown = 10;
        public static int LastPlayedIndex = -1;

        public static void Load(ContentManager Content, GraphicsDevice GD)
        {
            Console.WriteLine("Loading Effects...");
            gaussianBlurHorz = Content.Load<Effect>("GaussianBlurHorz");
            gaussianBlurVert = Content.Load<Effect>("GaussianBlurVert");

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

            RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
            {
                MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
            }
            FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
            bg = Texture2D.FromStream(GD, Stream);
            Stream.Dispose();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler((object o, UserPreferenceChangedEventArgs target) => { RefreshBGtex(GD); });
            
            MediaLibrary ML = new MediaLibrary();
            int errors = 0;
            for (int i = 0; i < ML.Songs.Count; i++)
            {
                try
                {
                    Playlist.Add(ML.Songs[i]);
                }
                catch
                {
                    errors++;
                }
            }
            Console.WriteLine("Found " + Playlist.Count.ToString() + " Songs!");

            if (Playlist.Count > 0)
            {
                PlaylistIndex = Values.RDM.Next(Playlist.Count);
                MediaPlayer.Play(Playlist[PlaylistIndex]);

                if (Program.args.Length > 0)
                    PlaySongByPath(Program.args[0]);
            }
            else
                Console.WriteLine("Playlist empty!");
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
        public static void GetNewSong()
        {
            if (InputCooldown < 0)
            {
                LastPlayed.Add(currentlyPlayingSong);
                LastPlayedIndex = LastPlayed.Count - 1;
                int PlaylistIndex_old = PlaylistIndex;
                PlaylistIndex = Values.RDM.Next(Playlist.Count);
                MediaPlayer.Play(Playlist[PlaylistIndex]);
                InputCooldown = 5;
                Values.SongTime = 0;
            }
        }
        public static void GetPreviousSong()
        {
            if (InputCooldown < 0 && LastPlayedIndex >= 0)
            {
                MediaPlayer.Play(LastPlayed[LastPlayedIndex]);
                LastPlayedIndex--;

                if (LastPlayedIndex < 0)
                    LastPlayedIndex = 0;

                InputCooldown = 5;
                Values.SongTime = 0;
            }
        }
        public static void PlaySongByPath(string Path)
        {
            string Filename = Path.Split('\\').Last();

            foreach (Song S in Playlist)
                if (Filename.Contains(S.Name, StringComparison.OrdinalIgnoreCase))
                {
                    PlaylistIndex = Playlist.IndexOf(S);
                    MediaPlayer.Play(Playlist[PlaylistIndex]);
                    Values.SongTime = 0;
                }
        }

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
