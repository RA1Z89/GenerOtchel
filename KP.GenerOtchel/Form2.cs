using System;
using System.Windows.Forms;

namespace KP.GenerOtchel
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form5 form1 = new Form5();
            form1.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Form8 form8 = new Form8();
                form8.Show();
                this.Hide();

                MessageBox.Show("Переход выполнен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form7 form1 = new Form7();
            form1.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Закрытие программы
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
    }
}
