using AdAutoClick.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using static AdAutoClick.Util.Util;

namespace AdAutoClick.Util
{
    class XmlConfig
    {
        public IReadOnlyList<AdSeed> SeedList { get; private set; }
        public IReadOnlyList<Regex> ADPatterns { get; private set; }
        public string StartUpPage { get; private set; }
        public string EndingPage { get; private set; }
        public bool DupClickChk { get; private set; }
        public int ClickedPageTimeout { get; private set; }
        public int ClickedPageViewTimeout { get; private set; }

#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
        public XmlConfig()
        {
            XmlDocument configXml = new();
            configXml.Load("config.xml");

            StartUpPage = configXml.SelectSingleNode("/AdAutoClick/startupPage").InnerText;
            EndingPage = configXml.SelectSingleNode("/AdAutoClick/endingPage").InnerText;
            DupClickChk = configXml.SelectSingleNode("/AdAutoClick/dupClickChk").Attributes["value"].Value.ToLower() == "true";
            ClickedPageTimeout = int.Parse(configXml.SelectSingleNode("/AdAutoClick/clickedPageTimeout").Attributes["value"].Value);
            ClickedPageViewTimeout = int.Parse(configXml.SelectSingleNode("/AdAutoClick/clickedPageViewTimeout").Attributes["value"].Value);

            List<AdSeed> seedIdList = new();
            foreach (XmlNode seedNode in configXml.SelectNodes("/AdAutoClick/seed_id/url"))
            {
                seedIdList.Add(new AdSeed(URLFix(seedNode.InnerText))
                {
                    Timeout = int.Parse(seedNode.Attributes["timeout"].Value),
                    Wait = int.Parse(seedNode.Attributes["wait"].Value),
                    Rotation = int.Parse(seedNode.Attributes["rotation"].Value),
                    BreakPoint = int.Parse(seedNode.Attributes["breakPoint"].Value)
                });
            }
            SeedList = seedIdList.AsReadOnly();

            List<Regex> adPatternsList = new();
            foreach (XmlNode adNode in configXml.SelectNodes("/AdAutoClick/ad_pattern/pattern"))
            {
                adPatternsList.Add(new Regex(adNode.InnerText, RegexOptions.Compiled));
            }
            ADPatterns = adPatternsList.AsReadOnly();
        }
    }
}
