using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using pr6;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace pr6
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;
        }
        bool testconnect;
        bool isCreator = false;
        private void serverk(object sender, RoutedEventArgs e)
        {
            if (Nick.Text == null || Nick.Text.Length == 0)
            {
                MessageBox.Show("Имя пользователя не должно быть пустым");
                return;
            }
            else
            {
                List<Socket> clients = new List<Socket>();
                List<String> users = new List<String>();
                Server server = new Server();
                server.start(clients, Messagebox, Users);// создание сервера
                isCreator = true;
                connect_server(); // подключение к серверу
                Main.Visibility = Visibility.Hidden;
                Chat.Visibility = Visibility.Visible;
                logBox.Visibility = Visibility.Visible;
                Messagebox.Visibility = Visibility.Visible;
                Nick.IsEnabled = false;
                window.Title = "Сервер";
                Senduser();
            }
        }

        private void user(object sender, RoutedEventArgs e)
        {
            if (Nick.Text == null || Nick.Text.Length == 0)
            {
                MessageBox.Show("Имя пользователя не должно быть пустым");
                return;
            }
            else
            {
                if (Ip.Text == null || Ip.Text.Length == 0)
                {
                    MessageBox.Show("Ip адрес не должен быть пустым");
                    return;
                }
                else
                {
                    connect_server(); // подключение к серверу
                    if (Ip.Text == "127.0.0.1" && testconnect == true)
                    {
                        window.Title = "Подключено к локальному серверу";
                        Main.Visibility = Visibility.Hidden;
                        Chat.Visibility = Visibility.Visible;
                        logBox.Visibility = Visibility.Collapsed;
                        Messagebox.Visibility = Visibility.Collapsed;
                        Nick.IsEnabled = false;
                        Senduser();
                    }
                    else if (testconnect == true)
                    {
                        window.Title = "Подключено к " + Ip.Text;
                        Main.Visibility = Visibility.Hidden;
                        Chat.Visibility = Visibility.Visible;
                        logBox.Visibility = Visibility.Collapsed;
                        Messagebox.Visibility = Visibility.Collapsed;
                        Nick.IsEnabled = false;
                        Senduser();
                    }
                }
            }


        }
        private void user_message(object sender, RoutedEventArgs e)
        {
            if (Message.Text == "/disconnect")
            {
                Senduser();
                Chat.Visibility = Visibility.Hidden;
                Main.Visibility = Visibility.Visible;
                Nick.IsEnabled = true;
            }
            else
            {
                SendMessage(Message.Text); // отправка сообщения
            }
        }


        // часть клиента
        private Socket server;
        private void connect_server()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                var ping = new Ping();
                var reply = ping.Send(Ip.Text, 1000);
                reply = ping.Send(Ip.Text, 3000);
                server.ConnectAsync(Ip.Text, 8888);
                testconnect = true;
                RecieveMessage();
            }
            catch
            {
                testconnect = false;
            }
        }
        private async Task RecieveMessage()
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await server.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes);
                if (message.IndexOf("clear") > -1)
                {
                    Users.Items.Clear();
                }
                else if (message.IndexOf("]:") > -1)
                {
                    MessageboxU.Items.Add(message);
                }
                else if (message.IndexOf("]:") < 0)
                {
                    Users.Items.Add(message);
                }

            }
        }

        private async Task SendMessage(string message)
        {
            string nick = Nick.Text;
            DateTime data = DateTime.Now;
            byte[] bytes = Encoding.UTF8.GetBytes($"{data}  [{nick}]: {message}");
            await server.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        }
        private async Task Senduser()
        {
            string nick = Nick.Text;
            byte[] bytes = Encoding.UTF8.GetBytes(nick);
            await server.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            Senduser();
            Chat.Visibility = Visibility.Hidden;
            Main.Visibility = Visibility.Visible;
            Nick.IsEnabled = true;
            if (isCreator)
            {
                server.Close();
            }
        }
            private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (Chat.Visibility == Visibility.Visible)
                {
                    e.Cancel = true;
                    Senduser();
                    Chat.Visibility = Visibility.Hidden;
                    Main.Visibility = Visibility.Visible;
                    Nick.IsEnabled = true;

                    if (isCreator)
                    {
                        server.Close();
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
