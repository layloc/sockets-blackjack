using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace BlackjackServer
{
    public class GameLogic
    {
        private readonly NetworkManager _networkManager;
        private Dictionary<TcpClient, int> playerScores = new Dictionary<TcpClient, int>();

        public GameLogic(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void StartGame()
        {
            InitializeGame();

            bool player1Turn = true;
            bool player2Turn = true;

            while (player1Turn || player2Turn)
            {
                foreach (var client in _networkManager.Clients)
                {
                    if (!_networkManager.PlayerNames.ContainsKey(client)) continue;

                    string playerName = _networkManager.PlayerNames[client];
                    int score = playerScores[client];

                    if (score > 21)
                    {
                        _networkManager.SendData(client.GetStream(), $"You busted with a score of {score}. Your turn is over.");
                        if (_networkManager.PlayerNames[_networkManager.Clients[0]] == playerName) player1Turn = false;
                        if (_networkManager.PlayerNames[_networkManager.Clients[1]] == playerName) player2Turn = false;
                        continue;
                    }

                    _networkManager.SendData(client.GetStream(), $"Your current score is {score}. Type 'hit' to take a card or 'stand' to stop.");
                    string choice = _networkManager.ReceiveData(client.GetStream());

                    if (choice.ToLower() == "hit")
                    {
                        int cardValue = CardDeck.GetRandomCardValue();
                        playerScores[client] += cardValue;
                        _networkManager.SendData(client.GetStream(), $"You drew a card with value {cardValue}. Your new score is {playerScores[client]}.");
                    }
                    else if (choice.ToLower() == "stand")
                    {
                        _networkManager.SendData(client.GetStream(), $"You chose to stand with a score of {score}.");
                        if (_networkManager.PlayerNames[_networkManager.Clients[0]] == playerName) player1Turn = false;
                        if (_networkManager.PlayerNames[_networkManager.Clients[1]] == playerName) player2Turn = false;
                    }
                    else
                    {
                        _networkManager.SendData(client.GetStream(), "Invalid command. Please type 'hit' or 'stand'.");
                    }
                }
            }

            DetermineWinner();
            ResetGame();
        }

        private void InitializeGame()
        {
            Random random = new Random();
            foreach (var client in _networkManager.Clients)
            {
                int initialScore = random.Next(2, 12); // Начальная карта
                playerScores[client] = initialScore;
                _networkManager.SendData(client.GetStream(), $"Welcome, {_networkManager.PlayerNames[client]}! Your starting card is {initialScore}.");
            }
        }

        private void DetermineWinner()
        {
            int player1Score = playerScores[_networkManager.Clients[0]];
            int player2Score = playerScores[_networkManager.Clients[1]];

            string winnerMessage;
            if (player1Score > 21 && player2Score > 21)
            {
                winnerMessage = "Both players busted! It's a draw.";
            }
            else if (player1Score > 21 || (player2Score <= 21 && player2Score > player1Score))
            {
                winnerMessage = $"{_networkManager.PlayerNames[_networkManager.Clients[1]]} wins with a score of {player2Score}!";
            }
            else if (player2Score > 21 || (player1Score <= 21 && player1Score > player2Score))
            {
                winnerMessage = $"{_networkManager.PlayerNames[_networkManager.Clients[0]]} wins with a score of {player1Score}!";
            }
            else
            {
                winnerMessage = "It's a draw!";
            }

            foreach (var client in _networkManager.Clients)
            {
                _networkManager.SendData(client.GetStream(), winnerMessage);
            }

            Console.WriteLine(winnerMessage);
        }

        private void ResetGame()
        {
            playerScores.Clear();
            foreach (var client in _networkManager.Clients)
            {
                _networkManager.SendData(client.GetStream(), "Game over. Type 'new' to start a new game.");
                string response = _networkManager.ReceiveData(client.GetStream());
                if (response.ToLower() != "new")
                {
                    client.Close();
                    _networkManager.Clients.Remove(client);
                }
            }

            if (_networkManager.Clients.Count == 2)
            {
                StartGame();
            }
        }
    }
}