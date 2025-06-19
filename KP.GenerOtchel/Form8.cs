using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace KP.GenerOtchel
{
    public partial class Form8 : Form
    {
        private string connectionString = "Server = DAMIR; Database = KP; User Id=kplogin; Password=12345;";

        private HashSet<string> validTables = new HashSet<string>
        {
            "Аудитории", "Группы", "Дисциплины", "Задолженности", "Занятия", "Занятия_Аудитории",
            "Кафедры", "Логины_Пароли", "Отчеты", "Оценки", "План_Дисциплин", "Практики",
            "Преподаватель", "Студенты", "Студенческие_Группы", "Учебные_Планы"
        };

        public Form8()
        {
            InitializeComponent();
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(validTables.ToArray());
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void LoadData(string tableName)
        {
            string query = $"SELECT * FROM [{tableName}]";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке таблицы {tableName}: {ex.Message}");
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBox1.SelectedItem.ToString();
            if (validTables.Contains(selectedTable))
            {
                LoadData(selectedTable);
            }
            else
            {
                MessageBox.Show($"Таблица '{selectedTable}' не существует.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну строку.");
                return;
            }

            string currentTable = comboBox1.SelectedItem.ToString();
            string username = "anonymous"; // Вместо Form2.CurrentUsername

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string columnName = dataGridView1.Columns[cell.ColumnIndex].HeaderText;
                    rowData[columnName] = cell.Value;
                }

                string jsonData = JsonConvert.SerializeObject(rowData);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string insertQuery = @"INSERT INTO Временные_Выбранные_Строки 
                (Имя_Таблицы, Данные_Строки, Имя_Пользователя)
                VALUES (@TableName, @RowData, @Username)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TableName", currentTable);
                        cmd.Parameters.AddWithValue("@RowData", jsonData);
                        cmd.Parameters.AddWithValue("@Username", username);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении строки: " + ex.Message);
                        }
                    }
                }
            }

            MessageBox.Show("Данные успешно добавлены в базу (временная таблица).");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = "anonymous"; // Вместо Form2.CurrentUsername

            Dictionary<string, List<Dictionary<string, object>>> buffer =
                new Dictionary<string, List<Dictionary<string, object>>>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Временные_Выбранные_Строки WHERE Имя_Пользователя = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string tableName = reader["Имя_Таблицы"].ToString();
                            string jsonData = reader["Данные_Строки"].ToString();

                            Dictionary<string, object> rowData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

                            if (!buffer.ContainsKey(tableName))
                                buffer[tableName] = new List<Dictionary<string, object>>();

                            buffer[tableName].Add(rowData);
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при чтении буфера: " + ex.Message);
                        return;
                    }
                }
            }

            Form6 form6 = new Form6(buffer, this);
            form6.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string username = "anonymous"; 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string deleteQuery = "DELETE FROM Временные_Выбранные_Строки WHERE Имя_Пользователя = @Username";
                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Буфер (временные строки) успешно очищен.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при очистке: " + ex.Message);
                    }
                }
            }

            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
    } 
}
