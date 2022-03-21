using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MySqlConnector;
using System.Data;
using System.Threading;

namespace XFSohbet
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        MySqlConnection baglanti;
        String kime,ad;       
        public ChatPage(String ad,String kime)//Sayfaya girişte var olan mesajları listeler
        {
            InitializeComponent();
            this.kime = kime;
            this.ad = ad;            
            kisiLbl.Text = kime + " ile Sohbet";
            listele();
        }

        public async void listele()//mesajları periyodik olarak veritabanından kontrol eder ve listeler
        {
            while (true)
            {
                Device.BeginInvokeOnMainThread(() =>
                {                 
                    mesajListele(ad, kime);                   
                });
                await Task.Delay(1000);
            }
        }
        public void baglan()//mysql bağlantısı
        {
            try
            {
                MySqlConnectionStringBuilder msb = new MySqlConnectionStringBuilder();
                msb.Server = "sunucu adresi";
                msb.Database = "veritabanı adı";
                msb.UserID = "veritabanı kullanıcısı";
                msb.Password = "veritabanı parola";
                baglanti = new MySqlConnection();
                baglanti.ConnectionString = msb.ConnectionString;
                baglanti.Open();                
            }
            catch
            {
                kisiLbl.Text = "Hata oluştu";
            }
        }
        public void mesajListele(String ad,String kime)//mesajları veritabanından sorgulayıp listeler
        {
            try
            {                
                    if (baglanti == null)
                    {
                        baglan();
                    }
                kisiLbl.Text = kime + " ile Sohbet";
                String cumle = "select * from mesajdb where ad='"+ad+"' or kime='"+ad+"'";
                    MySqlDataAdapter mda = new MySqlDataAdapter(cumle, baglanti);
                    DataTable dt = new DataTable();
                    mda.Fill(dt);
                    List<Mesaj> liste = new List<Mesaj>();
                    foreach (var satir in dt.Rows)
                    {
                        Mesaj m = new Mesaj();
                        DataRow dr = (DataRow)satir;
                        if(dr[1].ToString()==ad&& dr[3].ToString()==kime)
                    {
                        m.kimden = LayoutOptions.EndAndExpand;
                        m.renk = Color.LightGreen;
                        m.mesaj = dr[2].ToString();
                        liste.Add(m);
                    }
                    if (dr[3].ToString() == ad&&dr[1].ToString()==kime)
                    {
                        m.kimden = LayoutOptions.StartAndExpand;
                        m.renk = Color.WhiteSmoke;
                        m.mesaj = dr[2].ToString();
                        liste.Add(m);
                    }
                }
                    listeLV.BindingContext = liste;
                listeLV.ScrollTo(liste[liste.Count - 1], ScrollToPosition.End, true);
            }
            catch
            {
                kisiLbl.Text = "Mesaj yok";
            }
        }

        private void gonderBtn_Clicked(object sender, EventArgs e)//mesajları veritabanına kaydeden buton
        {
            try
            {
                if (String.IsNullOrWhiteSpace(mesajTxt.Text) == false)
                {
                    String cumle = "insert into mesajdb(ad,mesaj,kime) values('" + ad + "','"+mesajTxt.Text+"','"+kime+"')";
                    MySqlCommand mcom = new MySqlCommand();
                    mcom.CommandText = cumle;
                    mcom.Connection = baglanti;
                    mcom.ExecuteNonQuery();
                    mesajTxt.Text = "";
                    mesajListele(ad, kime);
                }
            }
            catch
            {
                kisiLbl.Text = "Hata oluştu";
            }
        }
    }
}