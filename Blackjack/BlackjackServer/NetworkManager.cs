using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BlackjackServer
{
    public class NetworkManager
    {
        public List<TcpClient> Clients { get; private set; } = new List<TcpClient>();
        public Dictionary<TcpClient, string> PlayerNames { get; private set; } = new Dictionary<TcpClient, string>();

        public bool AllPlayersReady()
        {
            return PlayerNames.Count == 2;
        }

        public void AddClient(TcpClient client)
        {
            lock (Clients)
            {
                Clients.Add(client);
            }
        }

        public void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                SendData(stream, "Enter your name:");
                string playerName = ReceiveData(stream);

                lock (PlayerNames)
                {
                    PlayerNames[client] = playerName;
                }

                Console.WriteLine($"{playerName} has joined the game.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        public void SendData(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public string ReceiveData(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
        }
    }
}