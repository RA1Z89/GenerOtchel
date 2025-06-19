using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace KP.GenerOtchel
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем логин и пароль из текстовых полей
            string login = textBox1.Text;
            string password = textBox2.Text;

            // Проверяем, что оба поля не пустые
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми!");
                return;
            }

            // Строка подключения к базе данных
            string connectionString = "Server = DAMIR;" +
                                      "Database = KP; " +
                                      "User Id = kplogin; " +
                                      "Password = 12345;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Запрос на добавление нового логина и пароля в таблицу Логины_Пароли
                    string query = "INSERT INTO Логины_Пароли (Логин, Пароль) VALUES (@login, @password)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Параметры для предотвращения SQL инъекций
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        // Выполняем запрос
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Новый логин и пароль добавлены!");

                            Form3 form2 = new Form3(); // форма авторизации
                            form2.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении логина и пароля.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form2 = new Form1(); // Возвращение к форме авторизации
            form2.Show();
            this.Hide();
        }
    }
}

