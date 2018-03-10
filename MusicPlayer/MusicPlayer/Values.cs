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
using System.Collections;

namespace MusicPlayer
{
    public static class Values
    {
        public static Random RDM = new Random();

        public static Point WindowSize = new Point(500, 300);
        public static Rectangle WindowRect
        {
            get
            {
                return new Rectangle(0, 0, WindowSize.X, WindowSize.Y);
            }
        }

        public static string CurrentExecutablePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf("\\MusicPlayer.exe"));

        public static float OutputVolume = 0;
        public static float LastOutputVolume = 0;
        public static float OutputVolumeIncrease = 0;
        public static float TargetVolume
        {
            get
            {
                return config.Default.Volume;
            }
            set
            {
                config.Default.Volume = value;
            }
        }

        public static int Timer = 0;

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        public static double JaroWinklerDistance(string aString1, string aString2)
        {
            int lLen1 = aString1.Length;
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;

            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];

            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;

            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                             + lNumCommonD / lLen2
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            if (lWeight <= 0.7) return lWeight;
            int lMax = Math.Min(4, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);

        }
        public static float OwnDistance(string Input, string SongName)
        {
            if (Input == "" || Input == null || SongName == "" || SongName == null)
                return float.MaxValue;

            int Errors = 0;
            int Distances = 0;
            int LastFindingIndex = -5;
            List<float> Scores = new List<float>();
            
            for (int k = 0; k < SongName.Length; k++)
            {
                if (SongName.ElementAt(k) == char.ToUpper(Input.First()) ||
                    SongName.ElementAt(k) == char.ToLower(Input.First()))
                {
                    LastFindingIndex = k - 1;
                    foreach (char c in Input.ToCharArray())
                    {
                        int j = LastFindingIndex;
                        if (j < 0)
                            j = 0;
                        int a = SongName.IndexOf(char.ToLower(c), j);
                        int b = SongName.IndexOf(char.ToUpper(c), j);

                        if (a == -1 && b == -1)
                            Errors++;
                        else
                        {
                            int i;
                            if (b == -1 || a < b && a != -1)
                                i = a;
                            else
                                i = b;

                            if (LastFindingIndex != -5)
                                Distances += i - LastFindingIndex - 1;
                            LastFindingIndex = i;
                        }
                    }
                    Scores.Add(Errors + Distances / 3f);
                    Errors = 0;
                    Distances = 0;
                    LastFindingIndex = -5;
                }
            }

            if (Scores.Count > 0)
                return Scores.Min();
            else
                return 1000;
        }
        public static float OwnDistanceWrapper(string Input, string SongName)
        {
            if (Input == null || Input == "" || SongName == "" || SongName == null)
                return float.MaxValue;

            Input = Input.ToLower();
            SongName = SongName.ToLower();

            if (SongName.Length <= Input.Length)
                return LevenshteinDistance(Input, SongName);

            List<float> Distances = new List<float>();

            if (false)
            {
                // old
                for (int i = 0; i < SongName.Length - Input.Length; i++)
                {
                    string Work = SongName;
                    Work = Work.Remove(0, i);
                    Work = Work.Remove(Input.Length, Work.Length - Input.Length);

                    Distances.Add(LevenshteinDistance(Work, Input));
                }
            }
            else
            {
                string[] InSplit = Input.Split(' ');

                if (InSplit.Length == 1)
                {
                    string[] split = SongName.Split(' ');
                    for (int i = 0; i < split.Length; i++)
                        Distances.Add(LevenshteinDistance(split[i], Input));
                }
                else
                {
                    float count = 0;
                    for (int i = 0; i < InSplit.Length; i++)
                    {
                        count += OwnDistanceWrapper(InSplit[i], SongName);
                    }
                    Distances.Add(count);
                }
            }

            return Distances.Min();
        }

        public static float GetAverageAmplitude(float[] samples)
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
        public static float GetAverageChangeRate(float[] samples)
        {
            if (samples != null)
            {
                float average = 0;

                for (int i = 1; i < samples.Length; i++)
                    average += Math.Abs(samples[i] - samples[i - 1]);

                average /= samples.Length;

                return average;
            }
            else
                return 0;
        }
        public static float GetRootMeanSquare(float[] samples)
        {
            if (samples != null)
            {
                float n = 0;

                for (int i = 0; i < samples.Length; i++)
                    n += samples[i] * samples[i];
                
                n = (float)Math.Sqrt(n / samples.Length);

                return n;
            }
            else
                return 0;
        }
        public static float GetRootMeanSquareApproximation(float[] samples)
        {
            if (samples != null)
            {
                float n = 0;

                for (int i = 0; i < samples.Length; i++)
                    n += samples[i] * samples[i];

                n = (float)Approximate.Sqrt(n / samples.Length);

                return n;
            }
            else
                return 0;
        }

        public static float AnimationFunction(float x)
        {
            return (float)((Math.Pow(2, -1.5 * (x - 1) * (x - 1)) / 10f + 1) * (-Math.Pow(5, -x) + 1));
        }
        public static string AsTime(double seconds)
        {
            int s = (int)(seconds % 60);
            int m = (int)(seconds / 60);
            int h = m / 60;
            int d = h / 24;
            int y = d / 365;

            if (m == 0)
                return s.ToString();
            else if (h == 0)
                return m + ":" + s;
            else if (d == 0)
                return h + ":" + m + ":" + s;
            else if (y == 0)
                return d + ":" + h + ":" + m + ":" + s;
            else
                return y + ":" + d + ":" + h + ":" + m + ":" + s;
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
        public static Screen TheWindowsMainScreen(Rectangle WindowBounds)
        {
            System.Drawing.Point P = new System.Drawing.Point(WindowBounds.X + WindowBounds.Width / 2, 
                WindowBounds.Y + WindowBounds.Height / 2);

            for (int i = 0; i < Screen.AllScreens.Length; i++)
                if (Screen.AllScreens[i].Bounds.Contains(P))
                    return Screen.AllScreens[i];

            int[] d = new int[Screen.AllScreens.Length];
            for (int i = 0; i < d.Length; i++)
                d[i] = P.X - Screen.AllScreens[i].Bounds.X - Screen.AllScreens[i].Bounds.Width / 2 +
                     P.Y - Screen.AllScreens[i].Bounds.Y - Screen.AllScreens[i].Bounds.Height / 2;

            int LowestDindex = 0;
            for (int i = 0; i < d.Length; i++)
                if (d[i] > d[LowestDindex])
                    LowestDindex = i;

            return Screen.AllScreens[LowestDindex];
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        const int MF_BYCOMMAND = 0x00000000;
        const int SC_CLOSE = 0xF060;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_SIZE = 0xF000;

        public static void HideConsole() { ShowWindow(GetConsoleWindow(), 0); }
        public static void MinimizeConsole() { ShowWindow(GetConsoleWindow(), 2); }
        public static void ShowConsole() { ShowWindow(GetConsoleWindow(), 5); }
        public static void DisableConsoleRezise()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }

        public static bool AttachToConsole()
        {
            const uint ParentProcess = 0xFFFFFFFF;
            if (!AttachConsole(ParentProcess))
                return false;

            return true;
        }
    }

    public static class WindowExtension
    {
        [DllImport("user32.dll")]
        static internal extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static void EnableBlur(this Form @this)
        {
            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);
            var Data = new WindowCompositionAttributeData();
            Data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            Data.SizeOfData = accentStructSize;
            Data.Data = accentPtr;
            SetWindowCompositionAttribute(@this.Handle, ref Data);
            Marshal.FreeHGlobal(accentPtr);
        }

    }
    enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }
    struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }
    struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
    enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
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
