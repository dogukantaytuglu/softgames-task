namespace AceOfShadows.Logic
{
    public readonly struct CardMove
    {
        public readonly int MoveIndex;
        public readonly Card Card;
        public readonly CardStack From;
        public readonly CardStack To;

        public CardMove(int moveIndex, Card card, CardStack from, CardStack to)
        {
            MoveIndex = moveIndex;
            Card = card;
            From = from;
            To = to;
        }
    }
}
