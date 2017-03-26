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

namespace MusicPlayer
{
    public static class Values
    {
        public static Random RDM = new Random();
        public static Point WindowSize = new Point(500, 300);
        public static string MusicPath = @"D:\Eigene Dateien\Medien\Musik";
        public static string ContentFoulderPath = @"C:\Users\One\Documents\visual studio 2015\Projects\MusicPlayer\MusicPlayer\MusicPlayerContent";
        public static float OutputVolume = 0;
        public static float TargetVolume = 0.215f;
        public static int SongTime;
        public static GameTime currentTime;
        public static bool isMusicPlaying = true;
        public static int Timer = 0;

        public static float GetAverageFrequency(int x, int y, VisualizationData visData)
        {
            float average = 0;
            for (int i = x; i < y; i++)
            {
                average += visData.Frequencies[i];
            }
            average /= y - x + 1;
            return average;
        }
        public static float GetAverageVolume(VisualizationData visData)
        {
            float average = 0;
            for (int i = 0; i < 255; i++)
            {
                average += Math.Abs(visData.Samples[i]);
            }
            average /= 256;
            return average;
        }
        public static string GetFileName(string FilePath)
        {
            string NameWithDataExtension = FilePath.Split('\\').Last();
            string[] SplittedNameWithDataExtension = NameWithDataExtension.Split('.');
            string NameWithoutDataExtensions = "";
            for (int i = 0; i < SplittedNameWithDataExtension.Length - 1; i++)
                NameWithoutDataExtensions.Insert(NameWithoutDataExtensions.Length, SplittedNameWithDataExtension[i]);
            return NameWithoutDataExtensions;
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
