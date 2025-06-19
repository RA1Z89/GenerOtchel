using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KP.GenerOtchel
{
    public partial class Form7 : Form
    {
        private string reportsDirectory = @"D:\УНИК\-ДИПЛОМ";
        private List<string> allExcelFiles = new List<string>();

        public Form7()
        {
            InitializeComponent();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            comboBox1.TextChanged += comboBox1_TextChanged;
            LoadExcelFiles();
        }

        private void LoadExcelFiles()
        {
            listBox1.Items.Clear();
            allExcelFiles.Clear();

            if (Directory.Exists(reportsDirectory))
            {
                var excelFiles = Directory.GetFiles(reportsDirectory, "*.xls*");

                foreach (var file in excelFiles)
                {
                    allExcelFiles.Add(Path.GetFileName(file));
                }

                FilterFiles(comboBox1.Text); // Фильтруем при загрузке
            }
            else
            {
                MessageBox.Show("Папка с отчетами не найдена:\n" + reportsDirectory);
            }
        }

        private void FilterFiles(string filter)
        {
            listBox1.Items.Clear();

            string normalizedFilter = filter?.Trim().ToLowerInvariant() ?? "";

            var filtered = allExcelFiles
                .Where(f => f.ToLowerInvariant().Contains(normalizedFilter))
                .ToList();

            if (filtered.Count == 0)
            {
                listBox1.Items.Add("Ничего не найдено.");
            }
            else
            {
                foreach (var file in filtered)
                {
                    listBox1.Items.Add(file);
                }
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            FilterFiles(comboBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || listBox1.SelectedItem.ToString() == "Ничего не найдено.")
            {
                MessageBox.Show("Пожалуйста, выберите файл для открытия.");
                return;
            }

            string selectedFile = Path.Combine(reportsDirectory, listBox1.SelectedItem.ToString());

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = selectedFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии файла:\n" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadExcelFiles();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 form1 = new Form2();
            form1.Show();
            this.Hide();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void Form7_Load_1(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
