using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace LoginWithTimerApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string login = "korkem";
        private const string password = "schogetten";
        private const int seconds = 30;

        private DispatcherTimer timer;
        private TimeSpan time;

        public MainWindow()
        {
            InitializeComponent();
            TextTimer.Text = "";

            if (Properties.Settings.Default.ErrorSeconds != 0)
            {
                BlockApp();
            }
        }

        /// <summary>
        /// Сброс настроек
        /// </summary>
        private void ResetSettings()
        {
            Properties.Settings.Default.ErrorCount = 0;
            Properties.Settings.Default.ErrorSeconds = 0;
            Properties.Settings.Default.Save();

            BtnLogin.IsEnabled = true;
            TextTimer.Text = "";
            TextLogin.Text = "";
            TextPass.Password = "";
        }

        /// <summary>
        /// Блокировка приложения
        /// </summary>
        private void BlockApp()
        {
            if (Properties.Settings.Default.ErrorSeconds == 0)
            {
                Properties.Settings.Default.ErrorSeconds = seconds;
            }

            MessageBox.Show($"Вход заблокирован на {Properties.Settings.Default.ErrorSeconds} сек.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);

            BtnLogin.IsEnabled = false;

            time = TimeSpan.FromSeconds(Properties.Settings.Default.ErrorSeconds);
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TextTimer.Text = "До следующей попытки осталось: " + time.ToString();

                if (time == TimeSpan.Zero)
                {
                    timer.Stop();
                    BtnLogin.IsEnabled = true;
                    Properties.Settings.Default.ErrorSeconds = 0;
                }
                else
                {
                    time = time.Add(TimeSpan.FromSeconds(-1));
                    Properties.Settings.Default.ErrorSeconds = time.Seconds;
                }

            }, Application.Current.Dispatcher);

            timer.Start();
            Properties.Settings.Default.Save();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            /// проверка на заполнение полей
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TextLogin.Text))
                errors.AppendLine("Введите логин");
            if (string.IsNullOrWhiteSpace(TextPass.Password))
                errors.AppendLine("Введите пароль");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /// проверка на правильность введенных данных
            /// 
            if (TextLogin.Text != login || TextPass.Password != password)
            {
                MessageBox.Show("Неверное соответствие логина и пароля!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);

                if (Properties.Settings.Default.ErrorCount >= 3)
                {
                    BlockApp();
                }
                else
                {
                    Properties.Settings.Default.ErrorCount++;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                MessageBox.Show($"Добро пожаловать, {login}!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetSettings();
            }
        }

        /// <summary>
        /// При закрытии приложения
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
