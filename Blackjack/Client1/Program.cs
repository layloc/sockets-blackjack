using System;
using System.Net.Sockets;
using System.Text;

namespace BlackjackClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    Console.WriteLine(message);

                    if (message.Contains("Enter your name:") || message.Contains("Type") || message.Contains("start a new game"))
                    {
                        Console.Write("-> ");
                        string input = Console.ReadLine();
                        byte[] data = Encoding.UTF8.GetBytes(input);
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    break;
                }
            }

            client.Close();
        }
    }
}