using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using MySqlConnector;
using System.Threading;
using System.Data;

namespace XFSohbet
{
    public partial class MainPage : ContentPage
    {
        MySqlConnection baglanti;
        String ad;
        int oturum = 0;
        public MainPage()
        {
            InitializeComponent();            
        }
        public void baglan()//mysql sunucu bağlantısı
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
                durumLbl.Text = "Oturum Açılmadı";

            }
            catch
            {
                durumLbl.Text = "Hata oluştu";
            }
        }
        public async void listele()//kullanıcıListele metodunu periyodik olarak sürekli çağıran asenkron metot
        {
            while (oturum==1)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    kullaniciListele(ad);
                });
                await Task.Delay(1000);
            }
        }
        public void kullaniciListele(String ad)//veritabanındaki kullanıcıları listeleyen metot
        {
            if (baglanti==null)
            {               
                 baglan();              
            }
            
                String cumle = "select * from sohbetdb where aktif=1";
                MySqlDataAdapter mda = new MySqlDataAdapter(cumle, baglanti);
                DataTable dt = new DataTable();
                mda.Fill(dt);
                List<Kisi> liste = new List<Kisi>();                
                foreach(var satir in dt.Rows)
                {
                    Kisi k = new Kisi();
                    DataRow dr = (DataRow)satir;
                    if ((int)dr[2] == 1&&dr[1].ToString()!=ad)
                    {
                        k.id = (int)dr[0];
                        k.ad = (String)dr[1];
                        liste.Add(k);
                    }
                }
                listeLV.BindingContext = liste;           
            }
        public int kullaniciVarmi(String ad)//Oturum Aç butonuna basıldığında girilen kullanıcı
        {                                                        //adını veritabanından sorgular. Eğer varsa aktif hale getirir
            try                                                
            {
                String cumle = "select * from sohbetdb where ad='"+ad+"'";
                MySqlDataAdapter mda = new MySqlDataAdapter(cumle, baglanti);
                DataTable dt = new DataTable();
                mda.Fill(dt);
                if(dt.Rows.Count>0)
                {
                    cumle = "update sohbetdb set aktif=1 where ad='" + ad + "'";
                    MySqlCommand mcom = new MySqlCommand();
                    mcom.Connection = baglanti;
                    mcom.CommandText = cumle;
                    mcom.ExecuteNonQuery();
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                durumLbl.Text = "Hata oluştu";
            }
            return 0;
        }
        public void oturumAc(String ad)//Oturum Aç butonuna basıldığında girilen kullanıcı adını
        {                                                   //kullaniciVarmi metodu ile sorgular, eğer yoksa kullanıcıyı ekler
            try
            {
                if (String.IsNullOrWhiteSpace(ad)==false)
                {
                    if (baglanti == null)
                    {
                        baglan();
                    }
                    int sonuc = kullaniciVarmi(ad);
                    if (sonuc == 0)
                    {
                        String cumle = "insert into sohbetdb(ad,aktif) values('" + ad + "',1)";
                        MySqlCommand mcom = new MySqlCommand();
                        mcom.CommandText = cumle;
                        mcom.Connection = baglanti;
                        mcom.ExecuteNonQuery();
                    }
                        durumLbl.Text = ad;
                        kullaniciTxt.IsEnabled = false;
                        oturumAcBtn.IsEnabled = false;
                        oturumKapatBtn.IsEnabled = true;
                    this.ad = ad;
                    oturum = 1;
                    listele();
                }
                else
                {
                    durumLbl.Text = "Kullanıcı Adı Gir";
                }
            }
            catch 
            {
                durumLbl.Text = "Hata oluştu";
            }
        }

        public void oturumKapat(String ad)//Oturumu kapatır.
        {
            try
            {
                String cumle = "update sohbetdb set aktif='0' where ad='"+ad+"'";
                MySqlCommand mcom = new MySqlCommand();
                mcom.CommandText = cumle;
                mcom.Connection = baglanti;
                mcom.ExecuteNonQuery();
                durumLbl.Text = "Oturum Açılmadı";
                kullaniciTxt.IsEnabled = true;
                baglanti.Close();
                baglanti = null;
                oturumAcBtn.IsEnabled = true;
                oturumKapatBtn.IsEnabled = false;
                listeLV.BindingContext = null;
                oturum = 0;
            }
            catch
            {
                durumLbl.Text = "Hata oluştu";
            }
        }
        private void oturumAcBtn_Clicked(object sender, EventArgs e)
        {
            oturumAc(kullaniciTxt.Text);
        }

        private void oturumKapatBtn_Clicked(object sender, EventArgs e)
        {
            oturumKapat(kullaniciTxt.Text);
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var lbl = sender as Label; 
           await Navigation.PushModalAsync(new ChatPage(kullaniciTxt.Text,lbl.Text));//kullanıcı listesinden bir kullanıcı seçildiğinde ChatPage sayfasına geçilir.
        }
    }
}
