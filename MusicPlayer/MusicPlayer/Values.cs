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
using System.Windows.Forms;

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
        public static Screen TheWindowsMainScreen(Rectangle Bounds)
        {
            System.Drawing.Point P = new System.Drawing.Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
            for (int i = 0; i < Screen.AllScreens.Length; i++)
                if (Screen.AllScreens[i].Bounds.Contains(P))
                    return Screen.AllScreens[i];
            return null;
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
    public class Approximate
    {
        public static float Sqrt(float z)
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; /* Subtract 2^m. */
            u.tmp >>= 1; /* Divide by 2. */
            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
            return u.f;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }
    }
}
