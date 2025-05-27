using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static TcpListener server;
    static List<TcpClient> clients = new List<TcpClient>();

    
    static float ballX = 0f, ballY = 0f;
    static float player1Y = 0f, player2Y = 0f;
    static int score1 = 0, score2 = 0;
    static bool isPaused = false;

    static void Main(string[] args)
    {
        
        server = new TcpListener(IPAddress.Any, 7777);
        server.Start();
        Console.WriteLine("Сервер запущен на порту 7777. Ожидание подключений...");

        
        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();

        
        while (true)
        {
            Console.ReadLine(); 
        }
    }

    static void AcceptClients()
    {
        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);
                Console.WriteLine($"Игрок подключён. Всего подключений: {clients.Count}");

                
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при принятии клиента: {ex.Message}");
            }
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (client.Connected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"Получено сообщение от клиента: {message}");

                string[] parts = message.Split(':');

                switch (parts[0])
                {
                    case "BALL":
                        if (parts.Length == 3)
                        {
                            ballX = float.Parse(parts[1]);
                            ballY = float.Parse(parts[2]);
                            Broadcast($"BALL:{ballX:F2}:{ballY:F2}");
                        }
                        else
                        {
                            Console.WriteLine($"Неверный формат сообщения BALL: {message}");
                        }
                        break;

                    case "P1":
                        if (parts.Length == 2)
                        {
                            player1Y = float.Parse(parts[1]);
                            Broadcast($"P1:{player1Y:F2}");
                            Console.WriteLine($"Обработано P1: {player1Y:F2}");
                        }
                        else
                        {
                            Console.WriteLine($"Неверный формат сообщения P1: {message}");
                        }
                        break;

                    case "P2":
                        if (parts.Length == 2)
                        {
                            player2Y = float.Parse(parts[1]);
                            Broadcast($"P2:{player2Y:F2}");
                            Console.WriteLine($"Обработано P2: {player2Y:F2}");
                        }
                        else
                        {
                            Console.WriteLine($"Неверный формат сообщения P2: {message}");
                        }
                        break;

                    case "SCORE":
                        if (parts.Length == 3)
                        {
                            score1 = int.Parse(parts[1]);
                            score2 = int.Parse(parts[2]);
                            Broadcast($"SCORE:{score1}:{score2}");
                        }
                        break;

                    case "PAUSE":
                        isPaused = true;
                        Broadcast("PAUSE");
                        break;

                    case "UNPAUSE":
                        isPaused = false;
                        Broadcast("UNPAUSE");
                        break;

                    case "GAMEOVER":
                        Broadcast("GAMEOVER");
                        break;

                    case "RESET_SCORES":
                        score1 = 0;
                        score2 = 0;
                        Broadcast("SCORE:0:0");
                        break;

                    case "MAINMENU":
                        Broadcast("MAINMENU");
                        break;

                    default:
                        Console.WriteLine($"Неизвестное сообщение: {message}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
                clients.Remove(client);
                client.Close();
                break;
            }
        }

        Console.WriteLine("Клиент отключён. Осталось подключений: " + clients.Count);
        clients.Remove(client);
        client.Close();
    }

    static void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var c in clients.ToArray()) 
        {
            try
            {
                if (c.Connected)
                {
                    NetworkStream stream = c.GetStream();
                    if (stream.CanWrite)
                    {
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine($"Отправлено всем: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки клиенту: {ex.Message}");
                clients.Remove(c);
                c.Close();
            }
        }
    }
}