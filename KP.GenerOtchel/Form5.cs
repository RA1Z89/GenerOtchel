using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace KP.GenerOtchel
{
    public partial class Form5 : Form
    {
        private string connectionString = "Server=DAMIR;" +
                                          "Database=KP;" +
                                          "User Id=kplogin;" +
                                          "Password=12345;";
        private DataTable currentTable;
        private string currentTableName;
        private bool isDataModified = false;
        private DataColumn primaryKeyColumn;

        public Form5()
        {
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(new string[] {
                "Дисциплины", "Аудитории", "Группы", "Задолженности", "Занятия",
                "Занятия_Аудитории", "Кафедры", "Логины_Пароли", "Отчеты", "Оценки",
                "План_Дисциплин", "Практики", "Преподаватель", "Студенты",
                "Студенческие_Группы", "Учебные_Планы"
            });

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedTable = comboBox1.SelectedItem.ToString();
                LoadData(selectedTable);
            }
        }

        private void LoadData(string tableName)
        {
            if (isDataModified)
            {
                var result = MessageBox.Show(
                    "Вы не сохранили изменения. Переключиться без сохранения?",
                    "Предупреждение", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = $"SELECT * FROM [{tableName}]";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

                    currentTable?.Dispose();
                    currentTable = new DataTable();
                    dataAdapter.Fill(currentTable);

                    if (currentTable.PrimaryKey.Length == 0 && currentTable.Columns.Count > 0)
                    {
                        primaryKeyColumn = currentTable.Columns[0];
                        currentTable.PrimaryKey = new DataColumn[] { primaryKeyColumn };
                    }
                    else
                    {
                        primaryKeyColumn = currentTable.PrimaryKey.FirstOrDefault();
                    }

                    dataGridView1.DataSource = currentTable;
                    dataGridView1.AutoGenerateColumns = true;
                    dataGridView1.AllowUserToAddRows = true;
                    dataGridView1.AllowUserToDeleteRows = true;
                    dataGridView1.ReadOnly = false;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    isDataModified = false;
                    currentTableName = tableName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && currentTable != null)
            {
                isDataModified = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentTable == null || !isDataModified) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataRow row in currentTable.Rows)
                    {
                        // Валидация: проверка NOT NULL полей (кроме ключа)
                        foreach (DataColumn column in currentTable.Columns)
                        {
                            if (!column.AllowDBNull && column != primaryKeyColumn)
                            {
                                if (row[column] == DBNull.Value || string.IsNullOrWhiteSpace(row[column].ToString()))
                                {
                                    throw new Exception($"Поле \"{column.ColumnName}\" не может быть пустым.");
                                }
                            }
                        }

                        if (row.RowState == DataRowState.Added)
                        {
                            var columns = currentTable.Columns.Cast<DataColumn>()
                                .Where(c => c != primaryKeyColumn);
                            string columnsList = string.Join(",", columns.Select(c => c.ColumnName));
                            string valuesList = string.Join(",", columns.Select(c => $"'{row[c].ToString().Replace("'", "''")}'"));

                            string insertQuery = $"INSERT INTO {currentTableName} ({columnsList}) VALUES ({valuesList})";

                            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            string setClause = string.Join(",", currentTable.Columns.Cast<DataColumn>()
                                .Where(c => c != primaryKeyColumn)
                                .Select(c => $"{c.ColumnName} = '{row[c].ToString().Replace("'", "''")}'"));

                            string updateQuery = $"UPDATE {currentTableName} SET {setClause} WHERE {primaryKeyColumn.ColumnName} = {row[primaryKeyColumn]}";

                            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Изменения успешно сохранены.");
                    isDataModified = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentTable != null)
            {
                try
                {
                    DataRow newRow = currentTable.NewRow();

                    foreach (DataColumn column in currentTable.Columns)
                    {
                        if (!column.AllowDBNull && column != primaryKeyColumn)
                        {
                            newRow[column] = GetDefaultValue(column.DataType);
                        }
                    }

                    currentTable.Rows.Add(newRow);
                    isDataModified = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении строки: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Текущая таблица не загружена.");
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return string.Empty;
            if (type == typeof(int) || type == typeof(long))
                return 0;
            if (type == typeof(bool))
                return false;
            if (type == typeof(DateTime))
                return DateTime.Now;
            if (type == typeof(decimal) || type == typeof(double))
                return 0.0;

            return DBNull.Value;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.");
                return;
            }

            if (currentTable != null)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                        {
                            if (!row.IsNewRow && row.Index < currentTable.Rows.Count)
                            {
                                object keyValue = currentTable.Rows[row.Index][primaryKeyColumn];
                                string deleteQuery = $"DELETE FROM {currentTableName} WHERE {primaryKeyColumn.ColumnName} = {keyValue}";

                                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        MessageBox.Show("Строки успешно удалены.");
                        isDataModified = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
