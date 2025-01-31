using System;

namespace BlackjackServer
{
    public static class CardDeck
    {
        public static int GetRandomCardValue()
        {
            Random random = new Random();
            return random.Next(1, 11);
        }
    }
}