using System;
using System.Collections.Generic;

namespace AceOfShadows.Logic
{
    public sealed class CardStack
    {
        private readonly List<Card> _cards = new();

        public int Count => _cards.Count;

        // Bottom-to-top order, exposed read-only for the presentation layer's
        // initial layout pass - not a mutation surface.
        public IReadOnlyList<Card> Cards => _cards;

        public event Action<CardStack> CountChanged;

        public CardStack()
        {
        }

        public CardStack(IEnumerable<Card> initialCards)
        {
            _cards.AddRange(initialCards);
        }

        public Card PeekTop()
        {
            if (_cards.Count == 0)
                throw new InvalidOperationException("Cannot peek an empty stack.");

            return _cards[^1];
        }

        public Card PopTop()
        {
            var card = PeekTop();
            _cards.RemoveAt(_cards.Count - 1);
            CountChanged?.Invoke(this);
            return card;
        }

        public void PushTop(Card card)
        {
            _cards.Add(card);
            CountChanged?.Invoke(this);
        }
    }
}
