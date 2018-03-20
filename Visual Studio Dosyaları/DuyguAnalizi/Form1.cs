using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TweetSharp;
using System.Data.SqlClient;

namespace DuyguAnalizi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            kelimelrim();
        }

        SqlConnection baglanti;
        SqlCommand komut;
        DataTable dt = new DataTable();


        public void kelimelrim()
        {
            baglanti = new SqlConnection("Data Source = YUNUS; Initial Catalog = kelimeler; Integrated Security = True");
            baglanti.Open();
            komut = new SqlCommand("Select *From kelime", baglanti);
            SqlDataAdapter da = new SqlDataAdapter(komut);
            da.Fill(dt);
        }



        TwitterService twitterservis = new TwitterService(
            "SyZRe887viChj8ij77oGOp72t",
            "xL8Z8GLB0eieUc9KjAwZCmI2OsWA3PmXeMKm6q8J2HVhj2NAjR",
            "3891446608-Nyq0BsKgGSIikUL7M9v3XXNynHc8pdiFsVIyISj",
            "GddnEoNUpFGXDZNIxzRWhitIlIdlk8MD81p3H6N7pG05P");


        public int sayısı;
        List<string> listtweet = new List<string>();
        List<string> listtweet3 = new List<string>();
        private void Form1_Load(object sender, EventArgs e)
        { }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != ""&& textBox1.Text.Substring(0,1)!=" ")
            {
                artıpuan = 0;
                eksipuan = 0;
                tweetpuanı = 0;
                dataGridView1.Rows.Clear();
                listtweet.Clear();
                listtweet3.Clear();
                foreach (var series in chart1.Series)
                {
                    series.Points.Clear();
                }
                if (textBox1.Text != "")
                {
                    if (radioButton1.Checked == true)
                        KullanıcıTweetleri();
                    else
                        HastagTweetleri();
                    tweettemizle();
                    MessageBox.Show("Analiz Tamamlandı.");
                }
            }
            else
                MessageBox.Show("Boş Alan Bırakmayın!");

        }

        public void HastagTweetleri()
        {
            int count = Convert.ToInt16(numericUpDown1.Value);
            var veriler1 = twitterservis.Search(new SearchOptions { Q = "#" +textBox1.Text, Count = 1, Resulttype = TwitterSearchResultType.Recent });
            int b = 0;
            for (b = 0; b < count / 10 + 1; b++)
            {
                try
                {
                    veriler1 = twitterservis.Search(new SearchOptions { Q = "#" + textBox1.Text, Count = 10, Resulttype = TwitterSearchResultType.Recent, MaxId = veriler1.Statuses.Last().Id });
                }
                catch{}
                try
                {
                    foreach (var item in veriler1.Statuses)
                    {
                        listtweet.Add(item.Text);
                    }
                    if (b != count / 10 && listtweet.Count > 0 && veriler1.Statuses.Count() > 0)
                    {
                        listtweet.RemoveAt(listtweet.Count - 1);
                    }
                }
                catch { }

                
            }
        }
        public void KullanıcıTweetleri()
        {
            int count = Convert.ToInt16(numericUpDown1.Value);
            var veriler2 = twitterservis.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions() { Count = 1, ScreenName = textBox1.Text });//kullanıcı adı ile tweet çek           
            int b = 0;
            for (b = 0; b < count / 10+1; b++)
            {
                try
                {
                    veriler2 = twitterservis.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions() { Count = 10, ScreenName = textBox1.Text, MaxId = veriler2.Last().Id });
                }
                catch
                {
                }
                try
                {
                    foreach (var item in veriler2)
                    {
                        listtweet.Add(item.Text);
                    }
                    if (b != count / 10 && listtweet.Count > 0 && veriler2.Count() > 0)
                    {
                        listtweet.RemoveAt(listtweet.Count - 1);
                    }
                }
                catch { }

            }
        }
        public void tweettemizle()
        {
            Regex rgx = new Regex(@"[^0-9a-zA-Z ğüşıöçĞÜŞİÖÇ@]");
            int i = 0;
            foreach (string tweet in listtweet)
            {
                try
                {
                    string[] words = rgx.Replace(tweet, "").Split(' ');
                    i++;
                    foreach (string a in words)
                    {
                        if (a.Length < 18 && a.Length > 2 && a != "RT" && "htt" != a.Substring(0, 3) && "@" != a.Substring(0, 1))
                        {
                            listtweet3.Add(a.ToLower());
                        }
                    }
                    puanlandır(tweet);
                    listtweet3.Clear();
                }
                catch { }
            }
            grafik();
            label5.Text = artıpuan.ToString();
            label7.Text = eksipuan.ToString();
        }
        public void grafik()
        {
            chart1.Series["Series3"].Points.Add(artıpuan);
            chart1.Series["Series3"].Points.Add(eksipuan);
            chart1.Series["Series3"].Points[0].Color = Color.Blue;
            chart1.Series["Series3"].Points[1].Color = Color.Red;
            chart1.Series["Series3"].Points[0].AxisLabel = "Pozitif";
            chart1.Series["Series3"].Points[1].AxisLabel = "Negatif";
        }
        int artıpuan = 0;
        int eksipuan = 0;
        int tweetpuanı = 0;
        public void puanlandır(string a)
        {
            tweetpuanı = 0;
            int t = 0;
            foreach (var item in listtweet3)
            {
                if (item == "değil"|| item == "ama")
                    t = 1;
                DataRow[] rows = dt.Select("kelime ='" + item + "'");
                if (rows.Length > 0)
                {
                    tweetpuanı += Convert.ToInt16(rows[0][1]);
                    if ((Convert.ToInt16(rows[0][1]) < 0 && t == 1 && tweetpuanı < 0)||
                        (Convert.ToInt16(rows[0][1]) > 0 && t == 1 && tweetpuanı > 0))
                        t = 0;
                }
            }
            if (t == 1)
            {
                tweetpuanı *= -1;
            }
            if (tweetpuanı < 0)
                eksipuan += tweetpuanı;
            else
                artıpuan += tweetpuanı;
            dataGridView1.Rows.Add(a, tweetpuanı);
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = false;
            label3.Visible = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = true;
            label3.Visible = false;
        }
    }
}

