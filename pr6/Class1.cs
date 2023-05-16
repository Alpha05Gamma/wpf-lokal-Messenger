using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace pr6
{
    public class Server : MainWindow
    {

        private Socket socket;

        public object ConsoleView { get; private set; }
        public void start(List<Socket> clients, ListBox messagebox, ListBox us) {
            List<string> users = new List<string>();
            connect_server();
            void connect_server()
            {
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(ipPoint);
                    socket.Listen(1000);

                    ListenToClients();
                }
                catch
                {

                }
            }
            async Task ListenToClients()
            {
                while (true)
                {
                    var client = await socket.AcceptAsync();
                    clients.Add(client);
                    RecieveMessage(client);
                }
            }
            async Task RecieveMessage(Socket client)
            {   
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    await client.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                    string message = Encoding.UTF8.GetString(bytes);
                    messagebox.Items.Add($"Сообщение от [{client.RemoteEndPoint}]: {message}");
                    if (message.IndexOf("]:") < 0)
                    {
                        int a = 0;
                        for (int i = 0; i < users.Count; i++)
                        {
                            string usersend = string.Format(users[i].ToString());
                            usersend = usersend.Replace(" ", "");
                            string test = message;
                            if (usersend == test)
                            {
                                a = 1;
                                string messageFormatted = string.Format($"[{message}] вышел с чата");
                                messagebox.Items.Add(messageFormatted);
                                users.Remove(message);
                                foreach (var item in clients)
                                {
                                    clear(item);
                                    Sendusers(item);
                                }

                            }
                        }
                        if (a == 0)
                        {
                            users.Add(string.Format($"{message}"));
                            string messageFormatted = string.Format($"[{message}] зашёл в чат");
                            messagebox.Items.Add(messageFormatted);
                            foreach (var item in clients)
                            {
                                clear(item);
                                Sendusers(item);
                            }
                        }

                    }
                    else
                    {
                        foreach (var item in clients)
                        {
                            clear(item);
                            SendMessage(item, message);
                            Sendusers(item);
                        }
                    }
                }
            }

            async Task SendMessage(Socket client, string message)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
            }
            async Task Sendusers(Socket client)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    string usersend = users[i].ToString();
                    byte[] bytes = Encoding.UTF8.GetBytes(usersend);
                    await client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                }
            }
            async Task clear(Socket client)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("clear");
                await client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
            }
        }
    }
}
