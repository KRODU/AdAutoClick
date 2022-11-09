using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using static AdAutoClick.Program;

namespace AdAutoClick.WebControl
{
    class ControlWebDriver
    {
        private readonly IWebDriver webDriver;
        private readonly IJavaScriptExecutor executor;
        private const int RETRY_MAX = 5;

        public ControlWebDriver(IWebDriver webDriver)
        {
            this.webDriver = webDriver;
            executor = (IJavaScriptExecutor)webDriver;
        }

        public string Url
        {
            get => webDriver.Url;
            set
            {
                for (int retry = 1; retry <= RETRY_MAX; retry++)
                {
                    try
                    {
                        webDriver.Url = value;
                        break;
                    }
                    catch (UnhandledAlertException) when (retry < RETRY_MAX)
                    {
                        UnhandledAlertExceptionProc();
                        continue;
                    }
                    catch (Exception ex) when (retry < RETRY_MAX)
                    {
                        ProgramLog.WriteLog($"ControlWebDriver.unknown exception", ex);
                        Thread.Sleep(2000);
                        continue;
                    }
                }
            }
        }

        public string Title => webDriver.Title;

        public string PageSource => webDriver.PageSource;

        public ReadOnlyCollection<string> WindowHandles => webDriver.WindowHandles;

        public string CurrentWindowHandle => webDriver.CurrentWindowHandle;

        public void Close() => webDriver.Close();

        public void Dispose() => webDriver.Dispose();

        public IWebElement? FindElement(By by, CancellationToken token)
        {
            for (int retry = 1; retry <= RETRY_MAX && !token.IsCancellationRequested; retry++)
            {
                try
                {
                    return webDriver.FindElement(by);
                }
                catch (UnhandledAlertException) when (retry < RETRY_MAX)
                {
                    UnhandledAlertExceptionProc();
                    continue;
                }
                catch (Exception ex) when (retry < RETRY_MAX)
                {
                    ProgramLog.WriteLog($"ControlWebDriver.unknown exception", ex);
                    Thread.Sleep(2000);
                    continue;
                }
            }

            return null;
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by, CancellationToken token)
        {
            for (int retry = 1; retry <= RETRY_MAX && !token.IsCancellationRequested; retry++)
            {
                try
                {
                    return webDriver.FindElements(by);
                }
                catch (UnhandledAlertException) when (retry < RETRY_MAX)
                {
                    UnhandledAlertExceptionProc();
                    continue;
                }
                catch (Exception ex) when (retry < RETRY_MAX)
                {
                    ProgramLog.WriteLog($"ControlWebDriver.unknown exception", ex);
                    Thread.Sleep(2000);
                    continue;
                }
            }

            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }

        public IOptions Manage() => webDriver.Manage();

        public INavigation Navigate() => throw new NotImplementedException();

        public void Quit() => webDriver.Quit();

        public ITargetLocator SwitchTo() => webDriver.SwitchTo();

        public object? ExecuteScript(string script, CancellationToken token, params object[] args)
        {
            for (int retry = 1; retry <= RETRY_MAX && !token.IsCancellationRequested; retry++)
            {
                try
                {
                    return executor.ExecuteScript(script, args);
                }
                catch (UnhandledAlertException) when (retry < RETRY_MAX)
                {
                    UnhandledAlertExceptionProc();
                    continue;
                }
                catch (Exception ex) when (retry < RETRY_MAX)
                {
                    ProgramLog.WriteLog($"ControlWebDriver.unknown exception", ex);
                    Thread.Sleep(2000);
                    continue;
                }
            }

            return null;
        }

        private void UnhandledAlertExceptionProc()
        {
            ProgramLog.WriteLog("UnhandledAlertException Prcessing");
            try
            {
                webDriver.SwitchTo().Alert().Accept();
            }
            catch (NoAlertPresentException) { }
        }
    }
}
