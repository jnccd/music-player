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
        public static b Foldl<a, b>(this IEnumerable<a> xs, Func<b, a, b> f)
        {
            return xs.Foldl(default, f);
        }
        public static a MaxElement<a>(this IEnumerable<a> xs, Func<a, double> f) { return xs.MaxElement(f, out double max); }
        public static a MaxElement<a>(this IEnumerable<a> xs, Func<a, double> f, out double max)
        {
            max = 0; a maxE = default;
            foreach (a x in xs)
            {
                double res = f(x);
                if (res > max)
                {
                    max = res;
                    maxE = x;
                }
            }
            return maxE;
        }
        public static a MinElement<a>(this IEnumerable<a> xs, Func<a, double> f) { return xs.MinElement(f, out double min); }
        public static a MinElement<a>(this IEnumerable<a> xs, Func<a, double> f, out double min)
        {
            min = 0; a minE = default;
            foreach (a x in xs)
            {
                double res = f(x);
                if (res < min)
                {
                    min = res;
                    minE = x;
                }
            }
            return minE;
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
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.Start();

            Task.Factory.StartNew(() => { RunEvent?.Invoke(compiler.StandardInput); });

            DateTime start = DateTime.Now;

            Task.Factory.StartNew(() => {
                Thread.CurrentThread.Name = $"RunAsConsoleCommand Thread {RunAsConsoleCommandThreadIndex++}";
                compiler.WaitForExit();

                string o = compiler.StandardOutput.ReadToEnd();
                string e = compiler.StandardError.ReadToEnd();

                exited = true;
                ExecutedEvent(o, e);
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
    }
}
