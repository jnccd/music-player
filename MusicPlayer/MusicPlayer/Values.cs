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
using System.Runtime.InteropServices;

namespace MusicPlayer
{
    public static class Values
    {
        public static Random RDM = new Random();

        public static Point WindowSize = new Point(500, 300);
        public static Rectangle ScreenRect
        {
            get
            {
                return new Rectangle(0, 0, WindowSize.X, WindowSize.Y);
            }
        }

        public static float OutputVolume = 0;
        public static float TargetVolume
        {
            get
            {
                return MusicPlayerwNAudio.config.Default.Volume;
            }
            set
            {
                MusicPlayerwNAudio.config.Default.Volume = value;
            }
        }

        public static int Timer = 0;
        
        public static float GetAverageVolume(float[] samples)
        {
            if (samples != null)
            {
                float average = 0;

                for (int i = 0; i < samples.Length; i++)
                    average += Math.Abs(samples[i]);

                average /= samples.Length;

                return average;
            }
            else
                return 0;
        }

        public static float DistanceFromLineToPoint(Vector2 Line1, Vector2 Line2, Vector2 Point)
        {
            Vector2 HelpingVector = new Vector2(-(Line1.Y - Line2.Y), Line1.X - Line2.X);
            Vector2 Intersection = IntersectionPoint(Line1, Line2, Point, HelpingVector);
            return (Point - Intersection).Length();
        }
        public static Vector2 IntersectionPoint(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
        {
            // Nutze hier die Cramerschen Regel:
            return new Vector2(((B2.X - B1.X) * (A2.X * A1.Y - A1.X * A2.Y) - (A2.X - A1.X) * (B2.X * B1.Y - B1.X * B2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)),

                                ((A1.Y - A2.Y) * (B2.X * B1.Y - B1.X * B2.Y) - (B1.Y - B2.Y) * (A2.X * A1.Y - A1.X * A2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)));
        }


        // Console Control
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void HideConsole() { ShowWindow(GetConsoleWindow(), 0); }
        public static void MinimizeConsole() { ShowWindow(GetConsoleWindow(), 2); }
        public static void ShowConsole() { ShowWindow(GetConsoleWindow(), 5); }
    }
    public static class StringExtensions
    {
        public static bool Contains(this String str, String substring,
                                    StringComparison comp)
        {
            return str.IndexOf(substring, comp) >= 0;
        }
    }
}
