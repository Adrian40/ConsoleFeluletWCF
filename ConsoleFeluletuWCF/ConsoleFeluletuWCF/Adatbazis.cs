using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.Serialization;

namespace ConsoleFeluletuWCF
{
    /// <Adatbázis>
    /// Adatbázis kezelés
    /// </Adatbázis>
    class Adatbazis
    {
        public Logger hibafuzet = new Logger();
        private static readonly Adatbazis adatBazis = new Adatbazis();
        public static SqlConnection kapcsolat;
        public static Adatbazis AdatBazis
        {
            get { return adatBazis; }
        }
        public static SqlConnection Kapcsolat
        {
            get { return kapcsolat; }
        }
        public Adatbazis()
        {
            kapcsolat = new SqlConnection();
            kapcsolat.ConnectionString = ConfigurationManager.ConnectionStrings["connstring"].ConnectionString;
        }
        ///<summary>
        /// SELECT parancs
        /// </summary>
        /// <param name="parancs">Egy SELECT parancs</param>
        /// <returns>A parancsban szereplő tábla ID értékét adja vissza (azaz egy egész számot)</returns>
        public int SelectID(SqlCommand parancs)
        {
            DataTable adattabla = new DataTable();
            parancs.Connection = kapcsolat;
            try
            {
                if (parancs.Connection.State != ConnectionState.Open)
                {
                    parancs.Connection.Open();
                }
                SqlDataAdapter sda = new SqlDataAdapter(parancs);
                sda.Fill(adattabla);
                parancs.Connection.Close();
            }
            catch (Exception e)
            {
                if (parancs.Connection.State == ConnectionState.Open)
                {
                    hibafuzet.error("Hiba a SELECTID metódusban: " + DateTime.Now + e.Message);
                    parancs.Connection.Close();
                }
            }
            int a = 0;
            int.TryParse(adattabla.Rows[0]["id"].ToString(), out a);
            return a;
        }
        /// <summary>
        /// SELECT parancs végrehajtása!
        /// </summary>
        /// <param name="parancs">SELECT parancs!</param>
        /// <returns>Az eredmény egy adattábla (DataTable)!</returns>
        public DataTable Select(SqlCommand parancs)
        {
            DataTable adattabla = new DataTable();
            parancs.Connection = kapcsolat;
            try
            {
                if (parancs.Connection.State != ConnectionState.Open)
                {
                    kapcsolat.Open();
                }
                SqlDataAdapter sda = new SqlDataAdapter(parancs);
                sda.Fill(adattabla);
                parancs.Connection.Close();
            }
            catch (Exception e)
            {
                hibafuzet.error("Hiba a belső SELECT-ben" + DateTime.Now + " Hibakód: " + e.Message);
                if (kapcsolat.State == ConnectionState.Open)
                {
                    kapcsolat.Close();
                }
            }
            return adattabla;
        }
        /// <summary>
        /// UPDATE parancs
        /// </summary>
        /// <param name="parancs">UPDATE parancs!</param>
        /// <returns>BOOL érték, attól függően, hogy sikerült-e, vagy sem!</returns>
        public bool Update(SqlCommand parancs)
        {
            int eredmeny = Futtat(kapcsolat, parancs);
            if (eredmeny == 1)
            {
                hibafuzet.log("UPDATE parancs végrehajtva!" + "(" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            else if (eredmeny == -1)
            {
                hibafuzet.error("Nem sikerült végrehatjani az UPDATE parancsot!" + ")" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            else
            {
                hibafuzet.error("Váratlan hiba az UPDATE parancs végrehatjásakor!" + ")" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            if (eredmeny == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Egy INSERT parancsot hajt végre!
        /// </summary>
        /// <param name="parancs">INSERT parancs!</param>
        /// <returns>INT-tel tér vissza; 1: ha sikerült, -1: ha nem sikerült, egyéb: nem várt hiba lépett fel!</returns>
        public int Insert(SqlCommand parancs)
        {
            int eredmeny = Futtat(kapcsolat, parancs);
            if (eredmeny == 1)
            {
                hibafuzet.log("INSERT parancs végrehajtva!" + "(" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            else if (eredmeny == -1)
            {
                hibafuzet.error("Nem sikerült végrehatjani az INSERT parancsot!" + ")" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            else
            {
                hibafuzet.error("Váratlan hiba az INSERT parancs végrehatjásakor!" + ")" + parancs.CommandText + ") Ekkor: " + DateTime.Now);
            }
            return eredmeny;
        }
        /// <summary>
        /// INSERT és UPDATE parancshoz
        /// </summary>
        /// <param name="kapcsolat"></param>
        /// <param name="parancs"></param>
        /// <returns></returns>
        public int Futtat(SqlConnection kapcsolat, SqlCommand parancs)
        {
            parancs.Connection = kapcsolat;
            try
            {
                lock (typeof(Adatbazis))
                {
                    if (parancs.Connection.State != ConnectionState.Open)
                    {
                        parancs.Connection.Open();
                    }
                    parancs.ExecuteNonQuery();
                    parancs.Connection.Close();
                    return 1;
                }
            }
            catch (Exception e)
            {
                hibafuzet.error("Hiba lépett fel a Futtat metódus végrehajtásakor!. Ekkor: " + DateTime.Now + "A hiba oka: " + e.Message);
                {
                    return -1;
                }
            }
        }
        /// <summary>
        /// Adatbázis kapcsolótáblájába író privát metódus!
        /// </summary>
        /// <param name="userid">Az aktuális felhasználó SelectID() által visszaadott ID-je!</param>
        /// <param name="munkaid">Az aktuális vár SelectID() által visszaadott ID-je!</param>
        /// <returns>BOOL érték; true: sikerült, false: nem sikerült!</returns>
        private bool ElkezdDolgozni(int userid, int munkaid)
        {
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "INSERT INTO DolgozoMunkaido (AlkalmazottID, MunkaidoID) VALUES ('" + userid + "','" + munkaid + "')";
            if (Insert(parancs) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// SELECT parancsról megmondja, hogy ad-e vissza eredményt, vagy sem!
        /// </summary>
        /// <param name="kapcsolat">Adatbáziskapcsolat!</param>
        /// <param name="parancs">SELECT parancs!</param>
        /// <returns>BOOL érték; true: ad vissza eredményt, false: nem ad vissza eredményt!</returns>
        public bool SzerepelazAdatBazisban(SqlConnection kapcsolat, SqlCommand parancs)
        {
            DataSet ds = new DataSet();
            int rows = 0;
            parancs.Connection = kapcsolat;
            bool letezik = false;
            try
            {
                if (parancs.Connection.State != ConnectionState.Open)
                {
                    parancs.Connection.Open();
                }
                lock (typeof(Adatbazis))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = parancs;
                    adapter.Fill(ds);
                    parancs.Connection.Close();
                    rows = ds.Tables[0].Rows.Count;
                }
                if (rows > 0)
                {
                    letezik = true;
                }
            }
            catch (Exception e)
            {
                hibafuzet.error("Hiba lépett fel a SzerepelazAdatBazisban nevű metód hívásakor! Ekkor: " + DateTime.Now + "A hiba oka: " + e.Message);
            }
            return letezik;
        }
    }
    /// <summary>
    /// Az adatbázis motorhoz közvetlenül nem kapcsolódó, de azt használó ellenőrzésre való metódusok gyűjtőhelye!
    /// </summary>
    class Ellenorzes
    {
        Adatbazis adatbazis = Adatbazis.AdatBazis;
        SqlConnection kapcsolat = Adatbazis.kapcsolat;
        /// <summary>
        /// A beírt munkaidőről megmondja, hogy szerepel-e már az adatbázisban!
        /// </summary>
        /// <param name="n">A munkaidő a neve!</param>
        /// <returns>BOOL érték; true: szerepel, false: nem szerepel!</returns>
        public bool MunkaidoEllenorzo(string n)
        {
            SqlCommand parancs = new SqlCommand();
            parancs.Connection = kapcsolat;
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT * FROM Munkaidok WHERE Munkaidok.munkaidonev='" + n + "'";
            return adatbazis.SzerepelazAdatBazisban(kapcsolat, parancs);
        }
        /// <summary>
        /// A beírt usernévről megmondja, hogy szerepel-e már az adatbázisban!
        /// </summary>
        /// <param name="n">A user-nek a neve!</param>
        /// <returns>BOOL érték; true: szerepel, false: nem szerepel!</returns>
        public bool AlkalmazottEllenorzo(string n)
        {
            SqlCommand parancs = new SqlCommand();
            parancs.Connection = kapcsolat;
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT * FROM Alkalmazottak WHERE Alkalmazottak.nev='" + n + "'";
            return adatbazis.SzerepelazAdatBazisban(kapcsolat, parancs);

        }
        /// <summary>
        /// Megmondja, hogy egy létező user megfelelő jelszót írt-e be!
        /// </summary>
        /// <param name="n">A NevEllenorzo() által visszaadott usernév!</param>
        /// <param name="j">A kliens jelszó mezőjébe írt karakterlánc!</param>
        /// <returns>BOOL érték; true: jó a jelszó, false: hibás a jelszó!</returns>
        public bool JelszoEllenorzo(string n, string j)
        {
            DataTable adattabla = new DataTable();
            SqlCommand parancs = new SqlCommand();
            parancs.Connection = kapcsolat;
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT nev, jelszo FROM Alkalmazottak WHERE (nev='" + n + "') AND (jelszo='" + j + "')";
            return adatbazis.SzerepelazAdatBazisban(kapcsolat, parancs);
        }
        /// <summary>
        /// A user által töltött munkaidő nevét mondja meg!
        /// </summary>
        /// <returns>STRING: A user munkaidejének a neve!</returns>
        public string MunkaidoNevetAd()
        {
            string nev = Alkalmazottak.AlkalmazottNeve;

            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT Munkaido FROM Munkaidok INNER JOIN AlkalmazottakMunkaido ON Munkaidok.id=MunkidokAlkalmazottak.MunkaidokID INNER JOIN Alkalmazottak ON AlkalmazottakMunkaidok.AlkalmazottakID=AlkalmazottakID WHERE AlkalmazottakNev = '" + nev + "'";

            DataTable dt = adatbazis.Select(parancs);
            string munkaidonev = dt.Rows[0]["munkaidonev"].ToString();
            return munkaidonev;
        }
        /// <summary>
        /// Megmondja, hogy az alkalmazottnak van-e már munkaideje!
        /// </summary>
        /// <returns>BOOL érték; true: van munkaideje, false: nincs munkaideje!</returns>
        public bool VanMunkaideje()
        {
            string alkalmazottnev = Alkalmazottak.AlkalmazottNeve;
            SqlCommand kerdes = new SqlCommand();
            kerdes.CommandType = CommandType.Text;
            kerdes.CommandText = "SELECT * FROM Alkalmazottak WHERE Alkalmazottak.nev = '" + alkalmazottnev + "'";

            int eredmeny = adatbazis.SelectID(kerdes);

            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT * FROM AlkalmazottakMunkaidok WHERE AlkalmazottakID= " + eredmeny + "";

            if (adatbazis.SzerepelazAdatBazisban(kapcsolat, parancs) != true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Van munkaidő az adatbázisban?
        /// </summary>
        /// <returns>BOOL érték; true: van munkaidő, false: nincs munkaidő!</returns>
        public bool VanMunkaido()
        {
            SqlCommand parancs = new SqlCommand();
            parancs.CommandType = CommandType.Text;
            parancs.CommandText = "SELECT * FROM Munkaidok";
            return adatbazis.SzerepelazAdatBazisban(kapcsolat, parancs);
        }
        /// <summary>
        /// Ahhoz, hogy a táblát a WCF át tudja adni a kliensnek, segítenünk kell a Szerializálásban! Gyakorlatilag rákényszerítjük, hogy elvégezze, mert magától nem teszi meg! Ha nem csináljuk, kivételt dob!
        /// </summary>
        /// <param name="dt">A szerializálni kívánt DataTable példány, és a TableName metódus szövege (egyedi táblanév)</param>
        /// <returns>Szerializált DataTable példány!</returns>
        public DataTable Szerializalo(DataTable dt, string adattablanev)
        {
            dt.TableName = adattablanev;
            new DataContractSerializer(typeof(DataTable)).WriteObject(new System.IO.MemoryStream(), dt);
            return dt;
        }
    }
}