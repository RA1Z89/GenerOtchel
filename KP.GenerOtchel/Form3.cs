using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace KP.GenerOtchel
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        // Обработчик нажатия на кнопку "Войти"
        private void button3_Click(object sender, EventArgs e)
        {
            string login = textBox3.Text; // Получаем логин из textBox3
            string password = textBox4.Text; // Получаем пароль из textBox4

            // Подключение к базе данных
            string connectionString = "Server = DAMIR;" +
                                      "Database = KP; " +
                                      "User Id = kplogin; " +
                                      "Password = 12345;";

            // SQL запрос для проверки логина и пароля
            string query = "SELECT COUNT(*) FROM Логины_Пароли WHERE Логин = @login AND Пароль = @password";

            try
            {
                // Создаем подключение к базе данных
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Открываем соединение с БД
                    connection.Open();

                    // Создаем команду с параметрами для безопасности
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Добавляем параметры к запросу
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        // Выполняем запрос и получаем количество найденных записей
                        int result = (int)cmd.ExecuteScalar();

                        // Если результат равен 1, то логин и пароль верны
                        if (result == 1)
                        {
                            MessageBox.Show("Авторизация успешна!");
                            Form2 form2 = new Form2(); // Открытие следующей формы
                            form2.Show();
                            this.Hide(); // Скрытие текущей формы
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Обработчик кнопки "Назад" (возвращаемся на форму авторизации)
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
         
        }

        private void Form3_Load_1(object sender, EventArgs e)
        {

        }
    }
}
