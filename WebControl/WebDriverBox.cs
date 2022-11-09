using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using static AdAutoClick.Program;
using static AdAutoClick.Util.Util;

namespace AdAutoClick.WebControl
{
    class WebDriverBox
    {
        private readonly ControlWebDriver webDriver;
        private readonly ProxyServer proxyServer;
        private volatile string? _requestUrl_block;
        private readonly object lockObj = new();

        public WebDriverBox()
        {
            proxyServer = new ProxyServer(false, false, false);
            var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Loopback, 0, true);
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();
            proxyServer.BeforeResponse += ProxyServer_BeforeResponse;
            proxyServer.BeforeRequest += ProxyServer_BeforeRequest;

            int port = proxyServer.ProxyEndPoints[0].Port;
            var proxy = new Proxy
            {
                HttpProxy = $"http://localhost:{port}",
                SslProxy = $"http://localhost:{port}",
                FtpProxy = $"http://localhost:{port}"
            };

            ChromeOptions chromeOptions = new()
            {
                UnhandledPromptBehavior = UnhandledPromptBehavior.Accept,
                PageLoadStrategy = PageLoadStrategy.None,
                Proxy = proxy,
                AcceptInsecureCertificates = true,
            };
            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddExcludedArgument("disable-popup-blocking");
            chromeOptions.AddExcludedArgument("--disable-popup-blocking");
            chromeOptions.AddAdditionalOption("useAutomationExtension", false);

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            webDriver = new ControlWebDriver(new ChromeDriver(chromeDriverService, chromeOptions));
        }

        public ControlWebDriver GetWebDriver()
        {
            return webDriver;
        }

        private Task ProxyServer_BeforeResponse(object sender, SessionEventArgs e)
        {
            void BeforeResponseThreadProc()
            {
                lock (lockObj)
                {
                    ProgramLog.WriteLog($"_proxyServer_AfterResponse: {_requestUrl_block}");
                    Monitor.PulseAll(lockObj);
                }
            }
            if (e.HttpClient.Request.RequestUri.ToString() == _requestUrl_block)
            {
                new Thread(BeforeResponseThreadProc).Start();
            }

            return null;
        }

        private Task ProxyServer_BeforeRequest(object sender, SessionEventArgs e)
        {
            void BeforeRequestThreadProc()
            {
                lock (lockObj)
                {
                    ProgramLog.WriteLog($"_proxyServer_BeforeRequest: {_requestUrl_block}");
                    Monitor.PulseAll(lockObj);
                }
            }
            if (e.HttpClient.Request.RequestUri.ToString() == _requestUrl_block)
            {
                new Thread(BeforeRequestThreadProc).Start();
            }

            return null;
        }

        public bool GotoPage(string url, CancellationToken token)
        {
            return GotoPage(url, DateTime.Now, 0, token);
        }

        public bool GotoPage(string url, DateTime start, int timeout, CancellationToken token, Action<string>? proc = null)
        {
            try
            {
            retry: if (string.IsNullOrWhiteSpace(url))
                    return true;

                url = URLFix(url);

                webDriver.ExecuteScript("return window.stop;", token);

                int totalMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                if (totalMilliseconds >= timeout && timeout > 0)
                    return false;

                proc?.Invoke($"{totalMilliseconds}/{timeout}");

                lock (lockObj)
                {
                    _requestUrl_block = url;
                    webDriver.Url = url;

                    ProgramLog.WriteLog("request wait loop start");

                    // request Wait
                    bool succ = false;
                    while (!succ)
                    {
                        token.ThrowIfCancellationRequested();
                        totalMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                        if (totalMilliseconds >= timeout && timeout > 0)
                            return false;

                        proc?.Invoke($"{totalMilliseconds}/{timeout}");

                        if (timeout > 0)
                            succ = Monitor.Wait(lockObj, Math.Min(totalMilliseconds, 1000));
                        else
                            succ = Monitor.Wait(lockObj, 1000);
                    }

                    ProgramLog.WriteLog("request wait loop end");

                    // response wait
                    ProgramLog.WriteLog("response wait loop start");

                    succ = false;
                    while (!succ)
                    {
                        token.ThrowIfCancellationRequested();
                        totalMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                        if (totalMilliseconds >= timeout && timeout > 0)
                            return false;

                        proc?.Invoke($"{totalMilliseconds}/{timeout}");
                        string? loadErrChk_state = webDriver.ExecuteScript("return document.readyState;", token) as string;

                        if (!succ && loadErrChk_state == "complete")
                        {
                            ProgramLog.WriteLog("response complete error. retry.");
                            goto retry;
                        }

                        if (timeout > 0)
                            succ = Monitor.Wait(lockObj, Math.Min(timeout - totalMilliseconds, 1000));
                        else
                            succ = Monitor.Wait(lockObj, 1000);
                    }

                    ProgramLog.WriteLog("response wait loop end");

                    return succ;
                }
            }
            catch (ThreadInterruptedException)
            {
                ProgramLog.WriteLog("GotoPage Thread Interrupted");
                return false;
            }
            catch (OperationCanceledException ex)
            {
                ProgramLog.WriteLog($"GotoPage OperationCanceledException\r\n{ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                ProgramLog.WriteLog("GotoPage error", ex);
                return false;
            }
        }

        public void Quit()
        {
            webDriver.Quit();

            proxyServer.Stop();
            proxyServer.Dispose();
        }
    }
}
