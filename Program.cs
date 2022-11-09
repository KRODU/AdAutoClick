using AdAutoClick.Util;

namespace AdAutoClick
{
    internal static class Program
    {
        public static Logger ProgramLog = new(true);
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            ADClickProc? adClickFrm;
            try
            {
                adClickFrm = new ADClickProc();
            }
            catch (Exception)
            {
                return;
            }

            Application.Run(adClickFrm);
        }
    }
}