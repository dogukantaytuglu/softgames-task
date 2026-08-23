using System;
using AceOfShadows.Logic;
using NUnit.Framework;

namespace AceOfShadows.Tests
{
    public class CardStackTests
    {
        [Test]
        public void NewStack_IsEmpty()
        {
            var stack = new CardStack();

            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void Constructor_WithInitialCards_PopulatesInOrder()
        {
            var cards = new[] { new Card(0), new Card(1), new Card(2) };

            var stack = new CardStack(cards);

            Assert.AreEqual(3, stack.Count);
            CollectionAssert.AreEqual(cards, stack.Cards);
        }

        [Test]
        public void PushTop_AddsCard_IncrementsCount()
        {
            var stack = new CardStack();
            var card = new Card(0);

            stack.PushTop(card);

            Assert.AreEqual(1, stack.Count);
            Assert.AreSame(card, stack.PeekTop());
        }

        [Test]
        public void PopTop_RemovesAndReturnsTopCard_DecrementsCount_LIFO()
        {
            var first = new Card(0);
            var second = new Card(1);
            var stack = new CardStack(new[] { first, second });

            var popped = stack.PopTop();

            Assert.AreSame(second, popped);
            Assert.AreEqual(1, stack.Count);
            Assert.AreSame(first, stack.PeekTop());
        }

        [Test]
        public void PopTop_OnEmptyStack_Throws()
        {
            var stack = new CardStack();

            Assert.Throws<InvalidOperationException>(() => stack.PopTop());
        }

        [Test]
        public void PeekTop_DoesNotRemove()
        {
            var card = new Card(0);
            var stack = new CardStack(new[] { card });

            var peeked = stack.PeekTop();

            Assert.AreSame(card, peeked);
            Assert.AreEqual(1, stack.Count);
        }
    }
}
