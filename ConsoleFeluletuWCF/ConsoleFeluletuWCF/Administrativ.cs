using System.Diagnostics;

namespace ConsoleFeluletuWCF
{
    /// <logolás>
    /// Alternatív osztály a logoláshoz
    /// <logolás>
    class Logger
    {
        TextWriterTraceListener figyelo = new TextWriterTraceListener(@"C:\Users\Adrian\Source\Repos\ConsoleFeluletWCF\ConsoleFeluletuWCF\ConsoleFeluletuWCF\Logs\log.txt");
        TextWriterTraceListener hibafigyelo = new TextWriterTraceListener(@"C:\Users\Adrian\Source\Repos\ConsoleFeluletWCF\ConsoleFeluletuWCF\ConsoleFeluletuWCF\Logs\errorlog.txt");
    ///<summary>
    ///  az első az eredményeket mutatóba ír
    /// </summary>
    /// <param name = "uzenet">A log üzenet szövege!</param>

        public void log(string uzenet)
        {
            Trace.Listeners.Add(figyelo);
            Trace.Write("\n" + uzenet);
            Trace.Flush();
            Trace.Listeners.Remove(figyelo);
        }
        public void error(string err)
        {
            Trace.Listeners.Add(hibafigyelo);
            Trace.Write("\n" + err);
            Trace.Flush();
            Trace.Listeners.Remove(hibafigyelo);
        }
    }
    /// <felhasználók>
    /// Alternatív osztály a felhasználók kezelésére
    /// </felhasználók>
    class Alkalmazottak
    {
        static string alkalmazottNeve;
        private static Alkalmazottak alkalmazott = new Alkalmazottak();
        protected static double munkaora;
        private Alkalmazottak()
        {
            munkaora = 8;
        }
        ///<munkaora>
        /// Property a munkaórák kiolvasásához
        /// </munkaora>
        public static double Munkaora
        {
            get { return munkaora; }
            set { munkaora = munkaora + value; }
        }
        /// <alkalmazott>
        /// Property az alkalmazott nevének kiolvasásához
        /// </alkalmazott>
        public static string AlkalmazottNeve
        {
            get { return alkalmazottNeve; }
            set { alkalmazottNeve = value; }
        }
            
    }
   
}
