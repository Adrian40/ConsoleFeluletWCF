using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.Serialization;

namespace ConsoleFeluletuWCF
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IMunkaidok
    {
        [OperationContract(IsInitiating = true)]
        int Bejelentkezes(string felh, string jelsz);
        [OperationContract]
        int Munkaora();
        [OperationContract]
        string AlkalmazottakNeve();
        [OperationContract]
        int MunkaidokFelvitele(string munkaidonev);
        [OperationContract]
        bool SzerepelazAdatBazisban();
        [OperationContract]
        bool VanMunkaideje();
        [OperationContract]
        DataTable Adattabla();
        [OperationContract]
        bool Teljesit(string munkaidonev);
        [OperationContract]
        DataTable MunkaidokGirdViewba();
        [OperationContract]
        DataTable NemletoltottmunkaidoDataGirdViewba();
        [OperationContract]
        bool MunkaidoLedolgozas(int ora, string munkaidonev);
        [OperationContract]
        double MunkaidoStatusz(string munkaidonev);
        [OperationContract]
        DataTable Bonusz(string munkaidonev);
        [OperationContract(IsTerminating = true)]
        void MunkaidoketLenullaz();
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Munkaidok : IMunkaidok
    {
        Adatbazis adatbaz = Adatbazis.AdatBazis;
        Ellenorzes ell = new Ellenorzes();

        /// <summary>
        /// Hogy hívják a usert?
        /// </summary>
        /// <returns>STRING: A user neve!</returns>
        public string AlkalmazottakNeve()
        {
            return Alkalmazottak.AlkalmazottNeve;
        }
        /// <summary>
        /// Munkaidőket visz fel az adatbázisba!
        /// </summary>
        /// <param name="munkaidonev">A munkaidő neve!</param>
        /// <returns>INT; 1: siker, -1: nem siker, más: nem várt hiba!</returns>
        public int MunkaidokFelvitele(string munkaidonev)
        {
            Random r = new Random();
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = @"INSERT INTO Munkaidok (munkaidonev,n_erteke,k_erteke) VALUES ('" + munkaidonev + "'," + r.Next(1000, 4001) + "," + 1 + ")";

            if (ell.MunkaidoEllenorzo(munkaidonev) == false)
            {
                int siker = adatbaz.Insert(parancs);
                if (ell.VanMunkaido() == false)
                {
                    int munkaidoID = 0;
                    SqlCommand munkaidoi = new SqlCommand();
                    munkaidoi.CommandType = CommandType.Text;
                    munkaidoi.CommandText = "SELECT * FROM Munkaidok WHERE Munkaidok.munkaidonev = '" + munkaidonev + "'";
                    munkaidoID = adatbaz.SelectID(munkaidoi);

                    string alk = Alkalmazottak.AlkalmazottNeve;
                    int alkalmazottID = 0;
                    SqlCommand alkalmazotti = new SqlCommand();
                    alkalmazotti.CommandType = CommandType.Text;
                    alkalmazotti.CommandText = "SELECT * FROM Munkaidok WHERE Munkaidok.munkaidonev='" + alk + "'";
                    alkalmazottID = adatbaz.SelectID(alkalmazotti);

                    SqlCommand parancs2 = new SqlCommand();
                    parancs2.CommandType = CommandType.Text;
                    parancs2.CommandText = "INSERT INTO TulajokVarak (Tulajok_id,Varak_id) VALUES (" + alkalmazottID + "," + munkaidoID + ")";
                    adatbaz.Insert(parancs2);
                }
                return siker;
            }
            else
            {
                return -2;
            }
        }
        /// <summary>
        /// Bejelentkezteti a felhasználót a rendszerbe!
        /// </summary>
        /// <param name="felh">Felhasználónév!</param>
        /// <param name="jelsz">Jelszó!</param>
        /// <returns>INT; 1: sikeres bejelentkezés, -2: helytelen jelszó, -1: parancs végrehajtása miatt sikertelen, egyéb: nem várt hiba!</returns>
        public int Bejelentkezes(string felh, string jelsz)
        {
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = @"INSERT INTO Alkalmazottak (nev, jelszo) VALUES ('" + felh + "','" + jelsz + "')";
            Alkalmazottak.AlkalmazottNeve = felh;
            if (ell.AlkalmazottEllenorzo(felh) == false)
            {
                parancs.Parameters.Clear();
                int siker = adatbaz.Insert(parancs);
                return siker;
            }
            else if (ell.JelszoEllenorzo(felh, jelsz) == false)
            {
                return -2;
            }
            return 1;
        }
        /// <summary>
        /// A user aktuális munkapontjait mutatja!
        /// </summary>
        /// <returns>INT: A munkapontok száma egészre kerekítve!</returns>
        public int Munkaora()
        {
            int munkao = (int)Alkalmazottak.Munkaora;
            return munkao;
        }

        /// <summary>
        /// Meghív egy "Ellenőrző" osztálybéli függvényt!
        /// </summary>
        /// <returns>BOOL érték; true: van vár az adatbázisban, false: nincs vár az adatbázisban!</returns>
        public bool SzerepelazAdatBazisban()
        {
            return ell.VanMunkaido();
        }

        /// <summary>
        /// Meghív egy "Ellenőrző" osztálybéli függvényt!
        /// </summary>
        /// <returns>BOOL érték; true: van a usernek munkaideje, false: nincs a usernek munkaideje!</returns>
        public bool VanMunkaideje()
        {
            return ell.VanMunkaideje();
        }
        /// <summary>
        /// Kilistázza a rendszerben lévő összes munkaidőt!
        /// </summary>
        /// <returns>DataTable: A munkaidők listája egy adattáblában, név szerint rendezve!</returns>

        public DataTable Adattabla()
        {
            DataTable dt = new DataTable();
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT * FROM Munkaidok ORDER BY munkaidonev";
            dt = adatbaz.Select(parancs);
            return ell.Szerializalo(dt, "adattabla");
        }
        /// <summary>
        /// Összerendeli a usert egy munkaidővel!
        /// </summary>
        /// <param name="munkaidonev">string: a kiválasztott tulaj neve!</param>
        /// <returns>BOOL érték; true: sikerült a munkaidő teljesítése, false: nem sikerült!</returns>
        public bool Teljesit(string munkaidonev)
        {
            int munkaidoID = 0;
            SqlCommand munkai = new SqlCommand();
            munkai.CommandType = CommandType.Text;
            munkai.CommandText = "SELECT * FROM Munkaidok WHERE Munkaidok.munkaidonev= '" + munkaidonev + "'";
            munkaidoID = adatbaz.SelectID(munkai);

            string alk = Alkalmazottak.AlkalmazottNeve;
            int alkalmazottID = 0;
            SqlCommand alkalmazotti = new SqlCommand();
            alkalmazotti.CommandType = CommandType.Text;
            alkalmazotti.CommandText = "SELECT * FROM Alkalmazottak WHERE Alkalmazottak.nev='" + alk + "'";
            alkalmazottID = adatbaz.SelectID(alkalmazotti);

            SqlCommand parancs2 = new SqlCommand();
            parancs2.CommandType = CommandType.Text;
            parancs2.CommandText = "INSERT INTO AlkalmazottakMunkaidok (AlkalmazottakID,MunkaidoID) VALUES (" + alkalmazottID + "," + munkaidoID + ")";
            int a = adatbaz.Insert(parancs2);
            if (a == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Visszaadja a munkaidő adatait!
        /// </summary>
        /// <returns>DataTable: a munkaidő névvel, a szükséges munkaórával, és az eddig eltöltött órával!</returns>
        public DataTable MunkaidokGirdViewba()
        {
            DataTable dt = new DataTable();
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT munkaidonev AS MunkaidoNeve, n_erteke AS TeljesitendoMunkaido,teljesitettorakszama AS TeljesitettOrak FROM Munkaidok INNER JOIN AlkalkamazottakMunkaidok ON Munkaidok.id=AlkalmazottakMunkaidok.munkaidoID INNER JOIN Alkalmazottak ON AlkalmaztottakID =AlkalmazottakMunkaidok.AlkalmazottakID WHERE Alkalmazottak.nev = '" + Alkalmazottak.AlkalmazottNeve + "'";
            dt = adatbaz.Select(parancs);
            return ell.Szerializalo(dt, "teljesített");
        }
        /// <summary>
        /// Visszaadja a nem teljesített munkaidő nevét!
        /// </summary>
        /// <returns>DataTable: a munkaidők nevével!</returns>
        public DataTable NemletoltottmunkaidoDataGirdViewba()
        {
            string munkaidonev = ell.MunkaidoNevetAd();
            DataTable dt = new DataTable();
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT munkaidonev FROM Munkaidok WHERE munkaidonev != '" + munkaidonev + "'";
            dt = adatbaz.Select(parancs);
            return ell.Szerializalo(dt, "nemteljesített");
        }
        /// <summary>
        /// Munkaidő teljesítése az adott időben!
        /// </summary>
        /// <param name="ora">teljesített órák!</param>
        /// <param name="munkaidonev">A munkaidő neve, amelyiken dolgozik a user!</param>
        /// <returns>BOOL érték; true: sikerült a munka, false: nem sikerült a munka!</returns>
        public bool MunkaidoLedolgozas(int ora, string munkaidonev)
        {
            SqlCommand orakiolvas = new SqlCommand();
            orakiolvas.CommandType = CommandType.Text;
            orakiolvas.CommandText = "SELECT beepitettorakszama FROM Munkaorak WHERE munkaidonev = '" + munkaidonev + "'";
            DataTable dt = adatbaz.Select(orakiolvas);
            int kiolvasottora;
            int.TryParse(dt.Rows[0]["teljesitettorakszama"].ToString(), out kiolvasottora);
            int ujertek = kiolvasottora + ora;

            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "UPDATE Munkaorak SET teljesitettorakszama = " + ujertek + " WHERE munkaidonev = '" + munkaidonev + "'";
            Alkalmazottak.Munkaora= -ora;
            Random r = new Random();
            double bonus = ora * (1 + r.NextDouble());
            Alkalmazottak.Munkaora = bonus;
            return adatbaz.Update(parancs);
        }
        /// <summary>
        /// A paraméterben adott munkaidő teljesítettségi szintjét mutatja!
        /// </summary>
        /// <param name="munkaidonev">A kérdéses munkaidő neve!</param>
        /// <returns>DOUBE: a teljesítettség szintje %-ban!</returns>
        public double MunkaidoStatusz(string munkaidonev)
        {
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT n_erteke, teljesitettorakszama FROM Munkaidok WHERE munkaidonev='" + munkaidonev+ "'";
            DataTable dt = adatbaz.Select(parancs);
            int nerteke;
            int.TryParse(dt.Rows[0]["n_erteke"].ToString(), out nerteke);
            double telorsz;
            double.TryParse(dt.Rows[0]["teljesitettorakszama"].ToString(), out telorsz);

            return (telorsz * 100) / nerteke;
        }
        /// <summary>
        /// Tudatja, hogy a teljesitett munkaórák kikhez tartozik, azaz ki kapja a bónuszt!
        /// </summary>
        /// <param name="munkaidonev">A munkaido neve, amibe dolgozunk!</param>
        /// <returns>DataTable: A bónuszt kapó nevével!</returns>
        public DataTable Bonusz(string munkaidonev)
        {
            DataTable dt = new DataTable();
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT nev FROM Alkalmazottak INNER JOIN AlkalmazottakMunkaorak ON Alkalmazottak.ID=AlkalmazottakMunkaidok.AlkalmazottakID INNER JOIN Varak ON Munkaidok.ID=AlkalmazottakMunkaidok.MunkaidokID WHERE Munkaidok.munkaidonev = '" + munkaidonev + "'";
            dt = adatbaz.Select(parancs);
            return ell.Szerializalo(dt, "Bonusz");
        }
        public void MunkaidoketLenullaz()
        {
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "UPDATE Munkaidok SET teljesitettorakszama = " + 0 + " WHERE teljesitettorakszama != " + 0 + "";
            adatbaz.Update(parancs);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            ServiceHost host = new ServiceHost(typeof(Munkaidok));
            host.Open();
            Console.WriteLine("Az Alkalmazott Bonusz nevű szerver elindult!");
            Console.WriteLine("A leállításához nyomj meg egy billentyűt!");
            Console.ReadKey();
            host.Close();
        }
    }
}
