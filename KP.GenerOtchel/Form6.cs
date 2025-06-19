using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace KP.GenerOtchel
{
    public partial class Form6 : Form
    {
        private string connectionString = "Server=DAMIR; Database=KP; User Id=kplogin; Password=12345;";
        private Dictionary<string, List<Dictionary<string, object>>> _buffer;
        private Form8 _parentForm; // ссылка на Form8
        private string lastSavedReportPath = ""; // путь к отчету для автозапуска

        public Form6(Dictionary<string, List<Dictionary<string, object>>> incomingBuffer, Form8 parentForm)
        {
            InitializeComponent();
            _buffer = incomingBuffer;
            _parentForm = parentForm;
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            label3.Text = DateTime.Now.ToString("dd.MM.yyyy");

            for (int i = 1; i <= 10; i++)
                comboBox1.Items.Add(i);

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите название отчета.");
                return;
            }

            if (!int.TryParse(comboBox1.SelectedItem.ToString(), out int teacherID))
            {
                MessageBox.Show("Некорректный ID преподавателя.");
                return;
            }

            string reportTitle = textBox1.Text.Trim();
            DateTime reportDate = DateTime.Now;

            // Сохраняем отчет в базу
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string insertQuery = "INSERT INTO Отчеты (ID_Преподавателя, Название, Дата_Оформления) VALUES (@ID, @Title, @Date)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", teacherID);
                    cmd.Parameters.AddWithValue("@Title", reportTitle);
                    cmd.Parameters.AddWithValue("@Date", reportDate);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            GenerateReport(reportTitle);

            if (checkBox1.Checked && !string.IsNullOrEmpty(lastSavedReportPath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = lastSavedReportPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось открыть отчет: " + ex.Message);
                }
            }

            this.Close();
        }

        private void GenerateReport(string reportTitle)
        {
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            worksheet.Name = "Отчет";

            worksheet.Cells[1, 1] = "Название отчета:";
            worksheet.Cells[1, 2] = reportTitle;
            worksheet.Cells[2, 1] = "Дата:";
            worksheet.Cells[2, 2] = DateTime.Now.ToString("dd.MM.yyyy");
            worksheet.Range["A1", "B2"].Font.Bold = true;

            int currentRow = 4;

            foreach (var entry in _buffer)
            {
                string tableName = entry.Key;
                var rows = entry.Value;

                if (rows.Count == 0)
                    continue;

                // Название таблицы
                worksheet.Cells[currentRow, 1] = tableName;
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, rows[0].Count]].Merge();
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                currentRow++;

                // Заголовки колонок
                int colIndex = 1;
                foreach (var key in rows[0].Keys)
                {
                    worksheet.Cells[currentRow, colIndex++] = key;
                }
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, rows[0].Count]].Font.Bold = true;
                currentRow++;

                // Данные
                foreach (var row in rows)
                {
                    colIndex = 1;
                    foreach (var val in row.Values)
                    {
                        worksheet.Cells[currentRow, colIndex++] = val;
                    }
                    currentRow++;
                }

                // Пустая строка между таблицами
                currentRow++;
            }

            string folderPath = @"D:\УНИК\-ДИПЛОМ";
            string safeTitle = string.Join("_", reportTitle.Split(System.IO.Path.GetInvalidFileNameChars()));
            string fileName = "Отчет_" + safeTitle + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
            string fullPath = System.IO.Path.Combine(folderPath, fileName);
            lastSavedReportPath = fullPath;

            try
            {
                workbook.SaveAs(fullPath);
                workbook.Close(false);
                excelApp.Quit();
                MessageBox.Show("Отчет успешно сохранен: " + fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении отчета: " + ex.Message);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Очистка буфера
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string deleteQuery = "DELETE FROM Временные_Выбранные_Строки";
                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _parentForm.Show(); // Возврат к исходной форме
            this.Close();       // Закрытие текущей
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // логика при изменении checkbox (если нужно)
        }
    }
}
