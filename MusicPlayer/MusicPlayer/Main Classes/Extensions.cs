using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public static class Extensions
    {
        static int RunAsConsoleCommandThreadIndex = 0;
        static Random RDM = new Random();

        // Linq Extensions
        public static b Foldl<a, b>(this IEnumerable<a> xs, b y, Func<b, a, b> f)
        {
            foreach (a x in xs)
                y = f(y, x);
            return y;
        }
        public static bool ContainsAny<a>(this IEnumerable<a> xs, IEnumerable<a> ys)
        {
            foreach (a y in ys)
                if (xs.Contains(y))
                    return true;
            return false;
        }
        public static a GetRandomValue<a>(this IEnumerable<a> xs)
        {
            a[] arr = xs.ToArray();
            return arr[RDM.Next(arr.Length)];
        }
        public static string RemoveLastGroup(this string s, char seperator)
        {
            string[] split = s.Split(seperator);
            return split.Take(split.Length - 1).Foldl("", (a, b) => a + seperator + b).Remove(0, 1);
        }

        public static void RunAsConsoleCommand(this string command, int TimeLimitInSeconds, Action TimeoutEvent, Action<string, string> ExecutedEvent,
            Action<StreamWriter> RunEvent = null)
        {
            bool exited = false;
            string[] split = command.Split(' ');

            if (split.Length == 0)
                return;

            Process compiler = new Process();
            compiler.StartInfo.FileName = split.First();
            compiler.StartInfo.Arguments = split.Skip(1).Foldl("", (x, y) => x + " " + y);
            compiler.StartInfo.CreateNoWindow = true;
            compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.Start();

            Task.Factory.StartNew(() => { RunEvent?.Invoke(compiler.StandardInput); });

            DateTime start = DateTime.Now;

            Task.Factory.StartNew(() => {
                Thread.CurrentThread.Name = $"RunAsConsoleCommand Thread {RunAsConsoleCommandThreadIndex++}";
                compiler.WaitForExit();

                string o = "";
                string e = "";

                try { o = compiler.StandardOutput.ReadToEnd(); } catch { }
                try { e = compiler.StandardError.ReadToEnd(); } catch { }
                
                ExecutedEvent(o, e);
                exited = true;
            });

            while (!exited && (DateTime.Now - start).TotalSeconds < TimeLimitInSeconds)
                Thread.Sleep(100);
            if (!exited)
            {
                exited = true;
                try
                {
                    compiler.Close();
                }
                catch { }
                TimeoutEvent();
            }
        }
        public static string getProgramOutput(this string command)
        {
            string[] split = command.Split(' ');
            if (split.Length == 0)
                return "";

            Process compiler = new Process();
            compiler.StartInfo.FileName = split.First();
            compiler.StartInfo.Arguments = split.Skip(1).Foldl("", (x, y) => x + " " + y);
            compiler.StartInfo.CreateNoWindow = true;
            compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.Start();
            compiler.WaitForExit();

            return compiler.StandardOutput.ReadToEnd();
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this System.Drawing.Color c)
        {
            return Microsoft.Xna.Framework.Color.FromNonPremultiplied(c.R, c.G, c.B, c.A);
        }
    }
}
