using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ipnomicon
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public string username, password;
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(textBox2.Text))
            {
                username = textBox1.Text;
                password = textBox2.Text;
                frm.s1 = Convert.ToInt32(comboBox1.Text);
                frm.s2 = Convert.ToInt32(comboBox2.Text);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            for (int i = 0; i <= 60; i++)
            {
                comboBox1.Items.Add(i);
                comboBox2.Items.Add(i);
            }
        }
    }
}
