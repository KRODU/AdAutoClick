using AdAutoClick.Model;
using AdAutoClick.Util;
using AdAutoClick.WebControl;
using OpenQA.Selenium;
using System.Text.RegularExpressions;
using static AdAutoClick.Program;
using static AdAutoClick.Util.Util;

namespace AdAutoClick
{
    public partial class ADClickProc : Form
    {
        private Thread runningThread;
        private readonly ConcurrentHashSet<string> clickedSet = new();
        private readonly ControlWebDriver webDriver;
        private bool _cur_closing = false;
        private const string CAPTION_INIT_TEXT = "��Ŭ �����\r\n�α��� �� ���� ��ư�� �����ּ���.";
        private string? _timeout_caption_str;
        private readonly WebDriverBox box = new();
        private readonly XmlConfig config;
        private CancellationTokenSource cts = new();

        public ADClickProc()
        {
            InitializeComponent();
            lblCaption.Text = CAPTION_INIT_TEXT;
            btnStart.Enabled = false;
            btnStart.Text = "�ε���";
            lblTimeout.Text = "";
            logFlushTimer.Enabled = true;

            try
            {
                config = new XmlConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"config.xml�� �д� �� ������ �߻��߽��ϴ�.\r\n{ex.Message}\r\n{ex.StackTrace}", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _cur_closing = true;
                throw;
            }

            try
            {
                webDriver = box.GetWebDriver();
            }
            catch (DriverServiceNotFoundException)
            {
                MessageBox.Show("chromedriver.exe ������ ã�� �� �����ϴ�.\r\n\r\n���α׷��� �����մϴ�.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _cur_closing = true;
                throw;
            }

            runningThread = new Thread(() =>
            {
                CancellationToken token = cts.Token;
                box.GotoPage(config.StartUpPage, token);
                WaitUntilLoadComplete(webDriver, token);

                Invoke(new Action(delegate
                {
                    btnStart.Enabled = true;
                    btnStart.Text = "����";
                    ProgramLog.WriteLog("INIT COMPLETE");
                    runningThread = new Thread(ThreadAdClick);
                }));
            });
            runningThread.Start();
        }

        private void ThreadAdClick()
        {
            try
            {
                CancellationToken token = cts.Token;
                int clickedCount = 0;
                DateTime start;

                // rotate Ƚ���� ���� ���� seed�� �������� maxRotate ����
                int maxRotate = 0;
                List<AdSeed> seedList = config.SeedList.ToList();
                for (int i = 0; i < seedList.Count; i++)
                {
                    if (maxRotate < seedList[i].Rotation)
                        maxRotate = seedList[i].Rotation;

                    seedList[i].CurBreakPoint = 0;
                }

                ProgramLog.WriteLog($"maxRotate: {maxRotate}");

                ProgramLog.WriteLog("Rotation Loop Start");
                for (int rotateCnt = 1; rotateCnt <= maxRotate && seedList.Count > 0; rotateCnt++)
                {
                    token.ThrowIfCancellationRequested();
                    seedList.Shuffle();
                    ProgramLog.WriteLog("Seed Loop Start");
                    for (int i = 0; i < seedList.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        Invoke(new Action(delegate
                        {
                            lblCaption.Text = $"�����̼�: {rotateCnt}/{maxRotate}\r\n" +
                                $"������: {i + 1}/{seedList.Count}\r\n" +
                                $"������ �ҷ����� ��\r\n" +
                                $"���ݱ��� �� {clickedCount}���� ���� Ŭ�� ��";
                        }));

                        start = DateTime.Now;
                        AdSeed adSeed = seedList[i];

                        ProgramLog.WriteLog($"goto seed page: {adSeed.SeedUrl}");
                        box.GotoPage(adSeed.SeedUrl, start, adSeed.Timeout, token, TimeoutCaptionUpdate);
                        WaitUntilLoadComplete(webDriver, start, adSeed.Timeout, token, TimeoutCaptionUpdate);

                        if (adSeed.Wait > 0)
                        {
                            Thread.Sleep(adSeed.Wait);
                        }

                        List<string> adSet = GetAdUrl(token);
                        adSet.Shuffle();
                        for (int j = 0; j < adSet.Count; j++)
                        {
                            token.ThrowIfCancellationRequested();
                            Invoke(new Action(delegate
                            {
                                lblCaption.Text = $"�����̼�: {rotateCnt}/{maxRotate}\r\n" +
                                    $"������: {i + 1}/{seedList.Count}\r\n" +
                                    $"�����Ͽ��� Ŭ������ ���� {j + 1}/{adSet.Count}\r\n" +
                                    $"���ݱ��� �� {clickedCount}���� ���� Ŭ�� ��";
                            }));

                            string eachAdUrl = adSet[j];

                            ProgramLog.WriteLog($"goto adUrl: {eachAdUrl}");
                            if (box.GotoPage(eachAdUrl, DateTime.Now, config.ClickedPageTimeout, token, TimeoutCaptionUpdate))
                            {
                                ProgramLog.WriteLog($"adClick complete. adView Start: {eachAdUrl}");
                                WaitUntilLoadComplete(webDriver, DateTime.Now, config.ClickedPageViewTimeout, token, TimeoutCaptionUpdate);
                                clickedCount++;

                                if (config.DupClickChk)
                                    clickedSet.Add(eachAdUrl);
                            }
                            else
                            {
                                ProgramLog.WriteLog($"adClick timeout: {eachAdUrl}");
                            }

                            Invoke(new Action(delegate
                            {
                                lblCaption.Text = $"�����̼�: {rotateCnt}/{maxRotate}\r\n" +
                                    $"������: {i + 1}/{seedList.Count}\r\n" +
                                    $"�����Ͽ��� Ŭ������ ���� {j + 1}/{adSet.Count}\r\n" +
                                    $"���ݱ��� �� {clickedCount}���� ���� Ŭ�� ��";
                            }));
                        }

                        // breakPoint üũ
                        if (adSet.Count > 0)
                            adSeed.CurBreakPoint = 0;
                        else
                            adSeed.CurBreakPoint++;


                    }
                    ProgramLog.WriteLog("Seed Loop End");

                    // rotation ����� �õ�, breakPoint ��� �õ� ����
                    for (int i = seedList.Count - 1; i >= 0; i--)
                    {
                        if (rotateCnt >= seedList[i].Rotation)
                        {
                            ProgramLog.WriteLog($"Rotation count out: {seedList[i].SeedUrl}");
                            seedList.RemoveAt(i);
                            continue;
                        }

                        if (seedList[i].CurBreakPoint >= seedList[i].BreakPoint)
                        {
                            ProgramLog.WriteLog($"breakPoint out: {seedList[i].SeedUrl}");
                            seedList.RemoveAt(i);
                            continue;
                        }
                    }
                }

                ProgramLog.WriteLog("Rotation Loop End");

                Invoke(new Action(delegate
                {
                    btnStart.Text = "����";
                    timerTimeoutCaption.Enabled = false;
                    lblCaption.Text = CAPTION_INIT_TEXT;
                    lblTimeout.Text = "";
                }));

                box.GotoPage(config.EndingPage, token);
            }
            catch (OperationCanceledException ex)
            {
                ProgramLog.WriteLog($"ThreadAdClick OperationCanceledException\r\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                ProgramLog.WriteLog($"ThreadAdClick ERROR: {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        private List<string> GetAdUrl(CancellationToken token)
        {
            HashSet<string> retValue = new();
            GetAdUrl_Recursive(retValue, 0, token);
            return retValue.ToList();
        }

        private void GetAdUrl_Recursive(HashSet<string> adUrlList, int deepCnt, CancellationToken token)
        {
            if (deepCnt > 50)
            {
                ProgramLog.WriteLog("GetAdUrl_Recursive deepCnt > 50");
                return;
            }

            foreach (var frameElement in webDriver.FindElements(By.TagName("iframe"), token))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    webDriver.SwitchTo().Frame(frameElement);
                }
                catch (Exception ex)
                {
                    ProgramLog.WriteLog("FrameMove error", ex);
                    continue;
                }
                GetAdUrl_Recursive(adUrlList, deepCnt + 1, token);
                webDriver.SwitchTo().ParentFrame();
            }

            if (webDriver.ExecuteScript("return document.documentElement.outerHTML;", token) is not string pageSource)
                return;

            foreach (Regex regex in config.ADPatterns)
            {
                token.ThrowIfCancellationRequested();
                foreach (Match eachMatch in regex.Matches(pageSource))
                {
                    for (int i = eachMatch.Groups.Count - 1; i >= 0; i--)
                    {
                        string adUrl = eachMatch.Groups[i].Value.Trim();

                        if (adUrl.Length == 0)
                            continue;

                        // ��� URL�� ���� URL�� ��ȯ��
                        adUrl = new Uri(new Uri(webDriver.Url), adUrl).AbsoluteUri;

                        if (clickedSet.Contains(adUrl))
                        {
                            ProgramLog.WriteLog($"already clicked adUrl(skip): {adUrl}");
                        }
                        else
                        {
                            ProgramLog.WriteLog($"add adUrl: {adUrl}");
                            adUrlList.Add(adUrl);
                        }
                    }
                }
            }
        }

        private void TimeoutCaptionUpdate(string caption)
        {
            _timeout_caption_str = caption;
        }

        private void ADClickProc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cur_closing)
                return;

            e.Cancel = true;
            StopThread(() =>
            {
                box.Quit();
                ProgramLog.Close();
                _cur_closing = true;
                Application.Exit();
            });
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (runningThread.IsAlive)
            {
                StopThread(() =>
                {
                    ProgramLog.WriteLog("adClickThread ABORT");
                    btnStart.Enabled = true;
                    btnStart.Text = "����";
                    timerTimeoutCaption.Enabled = false;
                    lblCaption.Text = CAPTION_INIT_TEXT;
                    lblTimeout.Text = "";
                    runningThread = new Thread(ThreadAdClick);
                });
            }
            else
            {
                ProgramLog.WriteLog("adClickThread START");
                cts = new CancellationTokenSource();
                _timeout_caption_str = "";
                timerTimeoutCaption.Enabled = true;
                btnStart.Text = "����";
                lblCaption.Text = "����Ŭ�� ������";
                runningThread.Start();
            }
        }

        private void StopThread(Action callBack)
        {
            void InvokeCallBack()
            {
                Invoke(new Action(delegate
                {
                    callBack.Invoke();
                }));
            }

            if (!runningThread.IsAlive)
            {
                InvokeCallBack();
                return;
            }

            btnStart.Enabled = false;
            btnStart.Text = "�����ϴ� ��";

            new Thread(() =>
            {
                cts.Cancel();
                SpinWait.SpinUntil(() =>
                {
                    runningThread.Interrupt();
                    return !runningThread.IsAlive;
                });
                cts.Dispose();
                InvokeCallBack();
            }).Start();
        }

        private void TimerTimeoutCaption_Tick(object sender, EventArgs e)
        {
            lblTimeout.Text = $"Timeout: {_timeout_caption_str}";
        }

        private async void LogFlushTimer_Tick(object sender, EventArgs e)
        {
            await ProgramLog.FlushAsync();
        }

        private void BtnInit_Click(object sender, EventArgs e)
        {
            clickedSet.Clear();
        }
    }
}
