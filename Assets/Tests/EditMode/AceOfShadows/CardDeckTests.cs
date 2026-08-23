using System;
using AceOfShadows.Logic;
using NUnit.Framework;

namespace AceOfShadows.Tests
{
    public class CardDeckTests
    {
        [Test]
        public void Constructor_CreatesFullSource_EmptyTarget()
        {
            var deck = new CardDeck(144);

            Assert.AreEqual(144, deck.Source.Count);
            Assert.AreEqual(0, deck.Target.Count);
        }

        [Test]
        public void Constructor_NonPositiveTotalCards_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CardDeck(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CardDeck(-1));
        }

        [Test]
        public void MoveNext_MovesTopCard_SourceToTarget()
        {
            var deck = new CardDeck(3);
            var expectedCard = deck.Source.PeekTop();

            deck.MoveNext();

            Assert.AreEqual(2, deck.Source.Count);
            Assert.AreEqual(1, deck.Target.Count);
            Assert.AreSame(expectedCard, deck.Target.PeekTop());
        }

        [Test]
        public void MoveNext_RaisesCardMoved_WithCorrectData()
        {
            var deck = new CardDeck(3);
            var expectedCard = deck.Source.PeekTop();
            CardMove? observed = null;
            deck.CardMoved += move => observed = move;

            deck.MoveNext();

            Assert.IsTrue(observed.HasValue);
            Assert.AreEqual(0, observed.Value.MoveIndex);
            Assert.AreSame(expectedCard, observed.Value.Card);
            Assert.AreSame(deck.Source, observed.Value.From);
            Assert.AreSame(deck.Target, observed.Value.To);
        }

        [Test]
        public void MoveNext_CalledTotalCardsTimes_EmptiesSource_SetsAllMovesTriggered()
        {
            var deck = new CardDeck(5);

            for (var i = 0; i < 5; i++)
                deck.MoveNext();

            Assert.AreEqual(0, deck.Source.Count);
            Assert.AreEqual(5, deck.Target.Count);
            Assert.IsTrue(deck.AllMovesTriggered);
        }

        [Test]
        public void MoveNext_AfterAllMovesTriggered_Throws()
        {
            var deck = new CardDeck(1);
            deck.MoveNext();

            Assert.Throws<InvalidOperationException>(() => deck.MoveNext());
        }

        [Test]
        public void IsFinished_FalseUntilAllMovesLanded_EvenIfAllTriggered()
        {
            var deck = new CardDeck(2);
            deck.MoveNext();
            deck.MoveNext();

            Assert.IsTrue(deck.AllMovesTriggered);
            Assert.IsFalse(deck.IsFinished);
        }

        [Test]
        public void NotifyCardLanded_ForEveryMove_SetsIsFinished()
        {
            var deck = new CardDeck(3);
            deck.MoveNext();
            deck.MoveNext();
            deck.MoveNext();

            deck.NotifyCardLanded();
            deck.NotifyCardLanded();
            Assert.IsFalse(deck.IsFinished);

            deck.NotifyCardLanded();
            Assert.IsTrue(deck.IsFinished);
        }

        [Test]
        public void AllAnimationsFinished_FiresExactlyOnce_OnLastLanding()
        {
            var deck = new CardDeck(2);
            deck.MoveNext();
            deck.MoveNext();

            var fireCount = 0;
            deck.AllAnimationsFinished += () => fireCount++;

            deck.NotifyCardLanded();
            deck.NotifyCardLanded();

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void NotifyCardLanded_CanArriveBeforeAllMovesTriggered_WithoutFinishingEarly()
        {
            var deck = new CardDeck(3);
            deck.MoveNext();

            deck.NotifyCardLanded();

            Assert.IsFalse(deck.IsFinished);

            deck.MoveNext();
            deck.MoveNext();
            deck.NotifyCardLanded();
            deck.NotifyCardLanded();

            Assert.IsTrue(deck.IsFinished);
        }
    }
}
