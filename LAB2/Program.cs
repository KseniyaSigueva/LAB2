﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketUdpClient
{
    class Program
    {
        /* Объявляем порты */
        static int localPort; // порт приема сообщений
        static int remotePort; // порт для отправки сообщений
        static Socket listeningSocket;

        static void Main(string[] args)
        {
            Console.Write("Введите порт для приема сообщений: ");
            localPort = Int32.Parse(Console.ReadLine());
            Console.Write("Введите порт для отправки сообщений: ");
            remotePort = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");
            Console.WriteLine();

            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Task listeningTask = new Task(Listen);
                listeningTask.Start();

                // отправка сообщений на разные порты
                while (true)
                {
                    string message = Console.ReadLine();

                    byte[] data = Encoding.Unicode.GetBytes(message);
                    EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort);
                    listeningSocket.SendTo(data, remotePoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        /* Здесь метод, который обрабатывает поток для приема подключений */
        private static void Listen()
        {
            try
            {
                //Адрес и порт, который мы прослушиваем
                IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localPort);
                listeningSocket.Bind(localIP);

                while (true)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    //Адрес отправителя
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (listeningSocket.Available > 0);

                    //Здесь получаем данные о подключении
                    IPEndPoint remoteFullIp = remoteIp as IPEndPoint;

                    //Сообщение-ответ 
                    Console.WriteLine("{0}:{1} - {2}", remoteFullIp.Address.ToString(),
                                                    remoteFullIp.Port, builder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        /* Здесь закрываем сокет */
        private static void Close()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
    }
}
