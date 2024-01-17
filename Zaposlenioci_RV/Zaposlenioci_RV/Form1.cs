using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace Zaposlenioci_RV {
    public partial class Form1 : Form {
        DB nasabaza;
        public Form1()
        {
            InitializeComponent();
            nasabaza = DB.Instance;
            label1.Text = dateTimePicker2.Value.ToString("yyyy-MM-dd");
            Refreshliste();
            datumstart();
            nasabaza.zaposlenicirf();


        }



        public class Zaposlenik {
            public int ID_zaposlenika { get; private set; }
            public string Ime { get; private set; }
            public string Prezimeime { get; private set; }
            public int ID_firme { get; private set; }
            public int satitjedan;

            

          

            public Zaposlenik(int ID_zaposlenika, string Ime, string Prezimeime, int ID_firme) {
                this.ID_zaposlenika = ID_zaposlenika;
                this.Ime = Ime;
                this.Prezimeime = Prezimeime;
                this.ID_firme = ID_firme;
               
            }


            public string vrativirjeme() {

                int hours = satitjedan / 3600;
                int minutes = (satitjedan % 3600) / 60;
                int seconds = satitjedan % 60;

                string outs = hours + ":" + minutes + ":" + seconds;
                return outs;


            }



        }

        public class DB {
            private static DB instance;
            private SqlConnection connection;
            public List<Zaposlenik> Zaposlenici { get; private set; }

            private DB()
            {
                string conn_string = @"Server=localhost\SQLEXPRESS;Database=pracenjeradjnogvremena;Trusted_Connection=True;User Id=Ivort;Password=sqlknjiznica";
                connection = new SqlConnection(conn_string);
                connection.Open();
                Zaposlenici = new List<Zaposlenik>();
            }

            public static DB Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new DB();
                    }
                    return instance;
                }
            }

            public List<string> Select(string sql)
            {
                List<string> output = new List<string>();
                SqlDataReader reader;
                SqlCommand komanda = new SqlCommand(sql, connection);
                reader = komanda.ExecuteReader();
                string row;
                while (reader.Read())
                {
                    int count = reader.FieldCount;
                    row = "";
                    for (int i = 0; i < count; i++)
                    {
                        row += reader.GetValue(i).ToString() + " ";
                    }
                    output.Add(row);

                }
                reader.Close();

                return output;
            }

            public void Insert(string sql)
            {

                SqlDataReader reader;
                SqlCommand komanda = new SqlCommand(sql, connection);
                reader = komanda.ExecuteReader();
                reader.Close();
            }



            public void Update(string sql)
            {

                SqlDataReader reader;
                SqlCommand komanda = new SqlCommand(sql, connection);
                komanda.ExecuteNonQuery();

            }


            public void Delete(string sql)
            {

                SqlDataReader reader;
                SqlCommand komanda = new SqlCommand(sql, connection);
                reader = komanda.ExecuteReader();
                reader.Close();
            }

            public void zaposlenicirf()
            {
                Zaposlenici.Clear();
                List<string> zaptemp = Select("select [ID_Zaposlenika] from Zaposlenici");
                foreach (string zaposlenik in zaptemp) {
                    int id = Int32.Parse(zaposlenik.Substring(0, zaposlenik.IndexOf(" ")));
                    string ime = Select("select ime from Zaposlenici WHERE ID_Zaposlenika=" + id).ElementAt(0);
                    string prezime = Select("select prezime from Zaposlenici WHERE ID_Zaposlenika=" + id).ElementAt(0);
                    int firma = Int32.Parse(Select("select ID_firme from Zaposlenici WHERE ID_Zaposlenika=" + id).ElementAt(0));
                    Zaposlenik z = new Zaposlenik(id, ime, prezime, firma);
                    Zaposlenici.Add(z);
                }

            }

        }

        public void datumstart() {
            
            List<string> templista =nasabaza.Select("select Datum_prijave from prijave");
            List<DateTime> dateTimes= new List<DateTime>();
            foreach (string a in templista) {
                dateTimes.Add(DateTime.Parse(a));
            }
            dateTimes.Sort();
            label1.Text = dateTimes.ElementAt(templista.Count-1).Date.ToString("yyyy-MM-dd");
            dateTimePicker2.Value = dateTimes.ElementAt(templista.Count - 1);

        }

        public void Refreshliste() {
            comboBoxfirme.Items.Clear();

            List<string> firme = nasabaza.Select("select [ID_firme],[Naziv_firme] from Firme");
            foreach (string firma in firme)
            {
                comboBoxfirme.Items.Add(firma);
            }



        }
        public void tjsati()
        {
            foreach (Zaposlenik zaposlenik in nasabaza.Zaposlenici)
            {
                try
                {
                    DateTime pocetnorv = DateTime.Parse(nasabaza.Select("select vrijeme_prijave from prijave where ID_zaposlenika=" + zaposlenik.ID_zaposlenika + " AND  datum_prijave='" + getdatum() + "'  AND vrsta_prijave='Prijava'").ElementAt(0));      
                    DateTime zavrsnovr = DateTime.Parse(nasabaza.Select("select vrijeme_prijave from prijave where ID_zaposlenika=" + zaposlenik.ID_zaposlenika + " AND  datum_prijave='" + getdatum() + "'  AND vrsta_prijave='Odjava'").ElementAt(0));
                    TimeSpan timeDifference =   zavrsnovr - pocetnorv;

                    int razlika= Convert.ToInt32(timeDifference.TotalSeconds.ToString());
                    zaposlenik.satitjedan += razlika;
                }
                catch { }
            }
        }
        public string getdatum() { if (debugToolStripMenuItem.Checked) return label1.Text;
            else
            {
                DateTime currentDateTime = DateTime.Now;
                string a =currentDateTime.ToString("yyyy-MM-dd");
                return a;
            }
        }
        public string getvrijeme() {
            if (debugToolStripMenuItem.Checked) return dateTimePicker1.Value.TimeOfDay.ToString();
            else { DateTime currentDateTime = DateTime.Now;
                return currentDateTime.ToString("HH:mm:ss");     }
        }

        public string getzaposlenik() { return comboBoxZaposlenici.Text.Substring(0, comboBoxfirme.Text.IndexOf(" ")).ToString(); }
        public string getfirma() { return comboBoxfirme.Text.Substring(0, comboBoxfirme.Text.IndexOf(" ")).ToString(); }

        public bool subotach() {
            if (debugToolStripMenuItem.Checked)
            {
                if (dateTimePicker2.Value.DayOfWeek.ToString() == "Saturday") return true;
                else return false;
            }
            else
            {
                DateTime currentDateTime = DateTime.Now;
                if (currentDateTime.DayOfWeek.ToString() == "Saturday") return true;
                else return false;
            }

        }

        public bool nedeljach()
        {
            if (debugToolStripMenuItem.Checked)
            {
                if (dateTimePicker2.Value.DayOfWeek.ToString() == "Sunday") return true;
                else return false;
            }
            else
            {
                DateTime currentDateTime = DateTime.Now;
                if (currentDateTime.DayOfWeek.ToString() == "Sunday") return true;
                else return false;
            }
        }

        public void zaposlenici_kasne()
        {
            string mindatum = dateTimePicker2.Value.AddDays(-5).ToString("yyyy-MM-dd");
            listBox1.Items.Clear();

            List<string> zap_kasne = nasabaza.Select("SELECT Prijave.ID_Zaposlenika, Prijave.Vrsta_prijave,  Prijave.Vrijeme_prijave , Prijave.Datum_prijave FROM Firme, Zaposlenici, Prijave WHERE Zaposlenici.ID_firme = Firme.ID_firme AND Prijave.ID_Zaposlenika = Zaposlenici.ID_zaposlenika AND Prijave.Vrijeme_prijave > Firme.rv_poc_kasno AND Prijave.Vrsta_prijave = 'Prijava' AND Prijave.Datum_prijave>'" + mindatum + "' AND Prijave.Datum_prijave<'" + label1.Text + "'");
            foreach (string zaposlnik in zap_kasne)
            {
                listBox1.Items.Add(zaposlnik.Substring(0, zaposlnik.Length - 8));
            }

        }

        public void zaposlenici_raniodlazak()
        {
            string mindatum = dateTimePicker2.Value.AddDays(-5).ToString("yyyy-MM-dd");
            List<string> zap_kasne = nasabaza.Select("SELECT Prijave.ID_Zaposlenika, Prijave.Vrsta_prijave,  Prijave.Vrijeme_prijave , Prijave.Datum_prijave FROM Firme, Zaposlenici, Prijave WHERE Zaposlenici.ID_firme = Firme.ID_firme AND Prijave.ID_Zaposlenika = Zaposlenici.ID_zaposlenika AND Prijave.Vrijeme_prijave < Firme.rv_zav_rano AND Prijave.Vrsta_prijave = 'Odjava' AND Prijave.Datum_prijave>'" + mindatum + "' AND Prijave.Datum_prijave<'" + label1.Text + "'");
            foreach (string zaposlnik in zap_kasne)
            {
                listBox1.Items.Add(zaposlnik.Substring(0, zaposlnik.Length - 8));
            }

        }

        public string getrvfirme() {

            List<string> firmarv= nasabaza.Select("SELECT rv_zav_rano FROM Firme WHERE ID_firme="+ getfirma());
            return firmarv.ElementAt(0);

        }

        public int otvorenich()
        {
            List<string> vrijemeradi = nasabaza.Select("SELECT Firme.rv_poc_rano FROM Firme WHERE Firme.ID_firme = " + getfirma());
            TimeSpan time1 = new TimeSpan(Int32.Parse(vrijemeradi.ElementAt(0).Substring(0, 2)), (Int32.Parse(vrijemeradi.ElementAt(0).Substring(3, 2))), (Int32.Parse(vrijemeradi.ElementAt(0).Substring(6, 2))));
            string trvrijeme = dateTimePicker1.Value.TimeOfDay.ToString();
            TimeSpan time2 = new TimeSpan(Int32.Parse(trvrijeme.Substring(0, 2)), Int32.Parse(trvrijeme.Substring(3, 2)), Int32.Parse(trvrijeme.Substring(6, 2)));

            int usporedba = TimeSpan.Compare(time1, time2);

            return usporedba;
        }

        private void comboBoxfirme_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxZaposlenici.ResetText();
            List<string> zaposlenici = nasabaza.Select("select [ID_zaposlenika],[Ime],[Prezime] from Zaposlenici WHERE  ID_firme=" + comboBoxfirme.Text.Substring(0, comboBoxfirme.Text.IndexOf(" ")).ToString() + "");
            comboBoxZaposlenici.Items.Clear();
            foreach (string zaposlnik in zaposlenici)
            {
                comboBoxZaposlenici.Items.Add(zaposlnik);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!subotach() && !nedeljach())
            {
          
                try
                {
                    List<string> prijavljenich = nasabaza.Select("select [ID_prijave] from Prijave WHERE  ID_zaposlenika =" + getzaposlenik() + " AND Datum_prijave='" + getdatum() + "'   AND Vrsta_prijave='Prijava'");
                    List<string> odjavljenich = nasabaza.Select("select [ID_prijave] from Prijave WHERE  ID_zaposlenika =" + getzaposlenik() + " AND Datum_prijave='" + getdatum() + "'   AND Vrsta_prijave='Odjava'");
                    if (odjavljenich.Count == 0 && prijavljenich.Count == 1)
                    {
                        nasabaza.Insert("INSERT INTO [dbo].[Prijave] ([ID_Zaposlenika],[Datum_prijave],[Vrijeme_prijave],[Vrsta_prijave]) VALUES (" + getzaposlenik() + ",'" + getdatum() + "','" + getrvfirme() + "' ,'Odjava');");
                    }

                }
                catch { }
                tjsati();

            }





            dateTimePicker2.Value = dateTimePicker2.Value.AddDays(1);
            label1.Text = dateTimePicker2.Value.ToString("yyyy-MM-dd");


            if (subotach())
            {
                label2.Text = "Subota";
                zaposlenici_kasne();
                zaposlenici_raniodlazak();

                foreach (Zaposlenik zaposlenik in nasabaza.Zaposlenici) {
                    if (zaposlenik.satitjedan < 144.000) {
                        listBox2.Items.Add(zaposlenik.ID_zaposlenika + " " + zaposlenik.Ime + " " + zaposlenik.Prezimeime);
                    
                    }

                }


                comboBoxfirme.Visible = false;
                comboBoxZaposlenici.Visible = false;
                button1.Visible = false;
                button2.Visible = false;
                label3.Visible = false;

            }
            else if (nedeljach())
            {
                label2.Text = "Nedelja";

            }
            else {
                
                
                    label2.Text = "Radni dan";
                    comboBoxfirme.Visible = true;
                    comboBoxZaposlenici.Visible = true;
                    button1.Visible = true;
                    button2.Visible = false;
                    label3.Visible = true;
                }
           

            
            comboBoxfirme.ResetText();
            comboBoxZaposlenici.ResetText();



        }

        private void button1_Click(object sender, EventArgs e)

        {


            if (otvorenich() > 0)
            {
                label3.Text = "Još ne radimo";
            }
            else
            {
                nasabaza.Insert("INSERT INTO [dbo].[Prijave] ([ID_Zaposlenika],[Datum_prijave],[Vrijeme_prijave],[Vrsta_prijave]) VALUES (" + getzaposlenik() + ",'" + getdatum() + "','" + getvrijeme() + "' ,'Prijava');");


                button2.Visible = true;
                button1.Visible = false;
                label3.Text = "";
            }




        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {

                nasabaza.Insert("INSERT INTO [dbo].[Prijave] ([ID_Zaposlenika],[Datum_prijave],[Vrijeme_prijave],[Vrsta_prijave]) VALUES (" + getzaposlenik() + ",'" + getdatum() + "','" + getvrijeme() + "' ,'Odjava');");
                button2.Visible = false;
            }
            catch { }
        }

        private void comboBoxZaposlenici_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Visible = false;
            List<string> prijavljenich = nasabaza.Select("select [ID_prijave] from Prijave WHERE  ID_zaposlenika =" + getzaposlenik() + " AND Datum_prijave='" + getdatum() + "'");
            if (prijavljenich.Count == 0)
            {
                button1.Visible = true;
            }



        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (debugToolStripMenuItem.Checked)
            {
                button3.Visible = false;
                label1.Visible = false;
                dateTimePicker1.Visible = false;
                debugToolStripMenuItem.Checked = false;
            }
            else
            {

                button3.Visible = true;
                label1.Visible = true;
                dateTimePicker1.Visible = true;


                debugToolStripMenuItem.Checked = true;
            }

        }

        private void tOPListaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Zaposlenik> templista = nasabaza.Zaposlenici.OrderByDescending(p => p.satitjedan).ToList();
            listBox3.Items.Clear();
            foreach (Zaposlenik zaposlenik in templista) {
                listBox3.Items.Add(zaposlenik.vrativirjeme());
            }
            
        }
    }

}