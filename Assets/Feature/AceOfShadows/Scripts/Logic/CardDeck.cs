using System;

namespace AceOfShadows.Logic
{
    public sealed class CardDeck
    {
        public CardStack Source { get; }
        public CardStack Target { get; }

        public event Action<CardMove> CardMoved;
        public event Action AllAnimationsFinished;

        private readonly int _totalCards;
        private int _triggeredCount;
        private int _acknowledgedCount;

        public bool AllMovesTriggered => _triggeredCount >= _totalCards;
        public bool IsFinished { get; private set; }

        public CardDeck(int totalCards)
        {
            if (totalCards <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalCards));

            _totalCards = totalCards;

            var cards = new Card[totalCards];
            for (var i = 0; i < totalCards; i++)
                cards[i] = new Card(i);

            Source = new CardStack(cards);
            Target = new CardStack();
        }

        public void MoveNext()
        {
            if (AllMovesTriggered)
                throw new InvalidOperationException("All cards have already been moved.");

            var card = Source.PopTop();
            Target.PushTop(card);
            _triggeredCount++;

            CardMoved?.Invoke(new CardMove(_triggeredCount - 1, card, Source, Target));
        }

        // Called by the presentation layer once a move's animation has
        // actually finished landing - this, not the trigger timer, is what
        // AllAnimationsFinished is tied to.
        public void NotifyCardLanded()
        {
            _acknowledgedCount++;

            if (_acknowledgedCount >= _totalCards && !IsFinished)
            {
                IsFinished = true;
                AllAnimationsFinished?.Invoke();
            }
        }
    }
}
