using AdAutoClick.WebControl;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading;
using static AdAutoClick.Program;

namespace AdAutoClick.Util
{
    static class Util
    {
        private static readonly Random rnd = new();
        public static string URLFix(string url)
        {
            // http 또는 https가 없는 경우 붙임
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            // 도메인 이름 뒤에 최소한 1번은 /가 나와야 함
            if (url.IndexOf('/', 8) == -1)
                url += "/";

            return url;
        }

        public static void WaitUntilLoadComplete(ControlWebDriver webDriver, CancellationToken token)
        {
            WaitUntilLoadComplete(webDriver, DateTime.Now, 0, token);
        }

        public static bool WaitUntilLoadComplete(ControlWebDriver webDriver, DateTime start, int timeout, CancellationToken token, Action<string>? proc = null)
        {
            try
            {
                while (webDriver.ExecuteScript("return document.readyState;", token) as string != "complete")
                {
                    int totalMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                    if (totalMilliseconds >= timeout && timeout > 0)
                    {
                        ProgramLog.WriteLog($"WaitUntilLoadComplete.timeout");
                        return false;
                    }

                    proc?.Invoke($"{totalMilliseconds}/{timeout}");

                    if (timeout > 0)
                        Thread.Sleep(Math.Min(timeout - totalMilliseconds, 300));
                    else
                        Thread.Sleep(300);
                }

                return true;
            }
            catch (ThreadInterruptedException)
            {
                ProgramLog.WriteLog("WaitUntilLoadComplete Thread Interrupted");
                return false;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null || list.Count <= 1)
                return;

            for (int n = list.Count - 1; n > 0; n--)
            {
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
