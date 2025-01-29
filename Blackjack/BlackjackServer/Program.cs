using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BlackjackServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(System.Net.IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server started. Waiting for players...");

            var networkManager = new NetworkManager();

            while (true)
            {
                if (networkManager.Clients.Count < 2)
                {
                    TcpClient client = server.AcceptTcpClient();
                    networkManager.AddClient(client);
                    Console.WriteLine($"Player {networkManager.Clients.Count} connected.");

                    Task.Run(() => networkManager.HandleClient(client));
                }

                if (networkManager.Clients.Count == 2 && networkManager.AllPlayersReady())
                {
                    var gameLogic = new GameLogic(networkManager);
                    gameLogic.StartGame();
                }
            }
        }
    }
}