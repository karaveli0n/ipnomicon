using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Net.NetworkInformation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading.Tasks;
using System.Threading;

namespace ipnomicon
{
    public partial class Form1 : Form
    {
        public string serverAddress;
        public string username;
        public string password;
        public int s1, s2;
        public bool ekle_sil;
        private DateTime baslangicZamani;
        private bool kronometreCalisiyor = false;
        int acik_kapali = 0;

        private static string FolderPath => Path.Combine(Directory.GetCurrentDirectory(), "VPN");

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(new[] { "ENG", "GER", "CND", "JPN", "CHI", "HOL", "FRA", "USA" });
            for (int i = 0; i <= 60; i++)
            {
                comboBox2.Items.Add(i);
            }
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
                e.Cancel = true;
                StopVPNConnection();
                Thread.Sleep(10000);
                this.Close();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            textBox1.Focus();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                textBox1.Text = selectedItem.SubItems[0].Text; // Server Address
                textBox2.Text = selectedItem.SubItems[1].Text; // Username
                textBox3.Text = selectedItem.SubItems[2].Text; // Password
                comboBox1.Text = selectedItem.SubItems[3].Text; // Language
            }

            if (ekle_sil == false)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Seçili öğeyi silmek istediğinizden emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        listView1.SelectedItems[0].Remove();
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ekle_sil = true;
            serverAddress = textBox1.Text;
            username = textBox2.Text;
            password = textBox3.Text;
            string selectedLanguage = comboBox1.Text;

            bool isDuplicate = listView1.Items.Cast<ListViewItem>().Any(item =>
                serverAddress == item.SubItems[0].Text &&
                username == item.SubItems[1].Text &&
                password == item.SubItems[2].Text &&
                selectedLanguage == item.SubItems[3].Text);

            if (!isDuplicate && !string.IsNullOrEmpty(textBox1.Text))
            {
                var lv = new ListViewItem(textBox1.Text);
                lv.SubItems.AddRange(new[] { textBox2.Text, textBox3.Text, comboBox1.Text });
                listView1.Items.Add(lv);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            acik_kapali = (acik_kapali + 1) % 2;
            if (acik_kapali == 1)
            {
                button5.ForeColor = Color.Red;
                button5.BackColor = Color.Black;
                ekle_sil = false;
            }
            else
            {
                button5.ForeColor = Color.Black;
                button5.BackColor = Color.White;
                ekle_sil = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartVPNConnection(serverAddress, username, password);
            if (!kronometreCalisiyor)
            {
                baslangicZamani = DateTime.Now;
                timer1.Stop();
                timer1.Start();
                kronometreCalisiyor = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count>=1)
            {   
                if (!kronometreCalisiyor)
                {
                    baslangicZamani = DateTime.Now;
                    timer1.Stop();
                    timer1.Start();
                    timer2.Stop();
                    timer2.Start();
                    kronometreCalisiyor = true;
                }
                StartVPNConnection(serverAddress, username, password);
            }
            else
            {
                MessageBox.Show("Sıçramalar için Yeterli Sayıda VPN ağı Kayıtlı Değil");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = false;
            StopVPNConnection();
            if (kronometreCalisiyor)
            {
                timer1.Stop();
                timer2.Stop();
                kronometreCalisiyor = false;
                label1.Text = "00:00:00:00";
                string bilgisayarAdi = Dns.GetHostName();
                label2.Text = "Bilgisayar Adı: " + bilgisayarAdi;
                string localipAdresi = Dns.GetHostByName(bilgisayarAdi).AddressList[0].ToString();
                label3.Text = "Local IP: " + localipAdresi;
                string modemIPAddress = new WebClient().DownloadString("https://api64.ipify.org?format=text");
                label4.Text = "Internet IP: " + modemIPAddress;
                label5.Text = "Mac Adress: " + GetMacAddress();
            }
        }

        private void StartVPNConnection(string connectionName, string username, string password)
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);

                var vpnConfigPath = Path.Combine(FolderPath, "VpnConnection.pbk");
                var batFilePath = Path.Combine(FolderPath, "VpnConnection.bat");
                File.WriteAllText(vpnConfigPath, $"[VPN]\r\nMEDIA=rastapi\r\nPort=VPN2-0\r\nDevice=WAN Miniport (IKEv2)\r\nDEVICE=vpn\r\nPhoneNumber={connectionName}");
                File.WriteAllText(batFilePath, $"rasdial \"VPN\" {username} {password} /phonebook:\"{vpnConfigPath}\"");

                var newProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = batFilePath,
                        Arguments = $"/C {batFilePath}",  // /C argümanı, cmd'nin işlemleri tamamladıktan sonra kapanmasını sağlar
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                newProcess.Start();
                newProcess.WaitForExit();

                if (newProcess.ExitCode != 0)
                {
                    MessageBox.Show("VPN bağlantısı başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button2.Enabled = true;
                    button1.Enabled = true;
                    button3.Enabled = false;
                }
                else
                {
                    button2.Enabled = false;
                    button1.Enabled = false;
                    button3.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button2.Enabled = true;
                button3.Enabled = false;
            }
        }

        private void StopVPNConnection()
        {
            var batFilePath = Path.Combine(FolderPath, "VpnDisconnect.bat");
            File.WriteAllText(batFilePath, "rasdial /d");

            var newProcess = new Process
            {
                StartInfo =
                 {
                     FileName = batFilePath,
                     Arguments = $"/C {batFilePath}",  // /C argümanı, cmd'nin işlemleri tamamladıktan sonra kapanmasını sağlar
                     WindowStyle = ProcessWindowStyle.Hidden,
                     CreateNoWindow = true,
                     UseShellExecute = false,
                     RedirectStandardOutput = true,
                     RedirectStandardError = true
                 }
            };

            newProcess.Start();
            newProcess.WaitForExit();          
        }

        private string GetMacAddress()
        {
            string macAddress = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Sadece fiziksel, çalışan ve Ethernet tipindeki ağ arabirimlerini kontrol et
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = nic.GetPhysicalAddress().ToString();
                    break; // İlk bulduğumuz geçerli MAC adresini alırız
                }
            }

            return macAddress;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (kronometreCalisiyor)
            {
                TimeSpan gecenSure = DateTime.Now - baslangicZamani;
                label1.Text = gecenSure.ToString(@"dd\.hh\:mm\:ss");
                string bilgisayarAdi = Dns.GetHostName();
                label2.Text = "Bilgisayar Adı: " + bilgisayarAdi;
                string localipAdresi = Dns.GetHostByName(bilgisayarAdi).AddressList[0].ToString();
                label3.Text = "Local IP: " + localipAdresi;
                string modemIPAddress = new WebClient().DownloadString("https://api64.ipify.org?format=text");
                label4.Text = "Internet IP: " + modemIPAddress;
                label5.Text = "Mac Adress: " + GetMacAddress();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            TimeSpan gecenSure = DateTime.Now - baslangicZamani;

            if (int.TryParse(comboBox2.Text, out int minutes)/* && int.TryParse(comboBox3.Text, out int seconds)*/)
            {
                if (minutes == 0/* && seconds < 10*/)
                {
                    MessageBox.Show("Süre 10 saniye ve aşağısında olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Timer'ı durdur ve işleme devam etme
                    timer2.Stop();
                    return;
                }

                int cmb_s1 = minutes * 60/* + seconds*/;
                label6.Text = $"Jump To: {gecenSure:mm\\:ss}";

                if (gecenSure.TotalSeconds >= cmb_s1)
                {
                    Random rnd = new Random();
                    int s1 = rnd.Next(0, listView1.Items.Count);
                    ListViewItem selectedItem = listView1.Items[s1];
                    textBox1.Text = selectedItem.SubItems[0].Text; // Server Address
                    textBox2.Text = selectedItem.SubItems[1].Text; // Username
                    textBox3.Text = selectedItem.SubItems[2].Text; // Password

                    // Bağlantıyı durdurma işlemini sadece eğer bağlıysa yap
                    if (kronometreCalisiyor)
                    {
                        button2.Enabled = true;
                        button1.Enabled = true;
                        button3.Enabled = false;
                        StopVPNConnection();
                    }

                    StartVPNConnection(textBox1.Text, textBox2.Text, textBox3.Text);

                    baslangicZamani = DateTime.Now; // Zamanı sıfırla
                }
            }
            else
            {
                MessageBox.Show("Geçersiz süre formatı. Lütfen geçerli bir sayı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Timer'ı durdur ve işleme devam etme
                timer2.Stop();
            }
        }
    }
}