namespace AdAutoClick.Model
{
    class AdSeed
    {
        public int Timeout { get; set; }
        public int Wait { get; set; }
        public string SeedUrl { get; set; }
        public int Rotation { get; set; }
        public int BreakPoint { get; set; }
        public int CurBreakPoint { get; set; }

        public AdSeed(string seedUrl)
        {
            this.SeedUrl = seedUrl;
        }
    }
}
