using System;
using System.Collections.Generic;

namespace MagicMayhem.Rules
{
    /// <summary>All 30 cards (three elements, values 0 to 9), drawn at random without
    /// replacement, so no card is ever dealt twice.</summary>
    public class Deck
    {
        private readonly List<Card> cards = new List<Card>();
        private readonly Random random;

        public Deck(Random random)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            foreach (Element element in Enum.GetValues(typeof(Element)))
                for (int value = Card.MinValue; value <= Card.MaxValue; value++)
                    cards.Add(new Card(element, value));
        }

        public int Count => cards.Count;

        public Card Draw()
        {
            if (cards.Count == 0)
                throw new InvalidOperationException("The deck is empty.");
            int i = random.Next(cards.Count);
            Card card = cards[i];
            cards.RemoveAt(i);
            return card;
        }

        public List<Card> Deal(int count)
        {
            var hand = new List<Card>(count);
            for (int i = 0; i < count; i++) hand.Add(Draw());
            return hand;
        }
    }
}
