using System;
using System.Collections.Generic;
using MagicWords.Logic;
using NUnit.Framework;

namespace MagicWords.Tests
{
    public class DialogueSequenceTests
    {
        private static DialogueLine Line(string name = "A") =>
            new(name, "text", null, DialoguePosition.Left);

        [Test]
        public void Current_BeforeAnyMoveNext_IsNull()
        {
            var sequence = new DialogueSequence(new List<DialogueLine> { Line() });

            Assert.IsNull(sequence.Current);
            Assert.IsFalse(sequence.HasStarted);
        }

        [Test]
        public void MoveNext_AdvancesCurrent_InOrder()
        {
            var first = Line("First");
            var second = Line("Second");
            var sequence = new DialogueSequence(new List<DialogueLine> { first, second });

            var returnedFirst = sequence.MoveNext();
            Assert.AreSame(first, returnedFirst);
            Assert.AreSame(first, sequence.Current);

            var returnedSecond = sequence.MoveNext();
            Assert.AreSame(second, returnedSecond);
            Assert.AreSame(second, sequence.Current);
        }

        [Test]
        public void IsFinished_FalseUntilLastLineShown()
        {
            var sequence = new DialogueSequence(new List<DialogueLine> { Line(), Line() });

            sequence.MoveNext();
            Assert.IsFalse(sequence.IsFinished);

            sequence.MoveNext();
            Assert.IsTrue(sequence.IsFinished);
        }

        [Test]
        public void MoveNext_PastLastLine_Throws()
        {
            var sequence = new DialogueSequence(new List<DialogueLine> { Line() });
            sequence.MoveNext();

            Assert.Throws<InvalidOperationException>(() => sequence.MoveNext());
        }

        [Test]
        public void Constructor_NullLines_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DialogueSequence(null));
        }

        [Test]
        public void CurrentNumber_IsZeroBeforeStart_ThenOneBasedPosition()
        {
            var sequence = new DialogueSequence(new List<DialogueLine> { Line(), Line(), Line() });

            Assert.AreEqual(0, sequence.CurrentNumber);

            sequence.MoveNext();
            Assert.AreEqual(1, sequence.CurrentNumber);

            sequence.MoveNext();
            sequence.MoveNext();
            Assert.AreEqual(sequence.Count, sequence.CurrentNumber);
        }

        [Test]
        public void Count_ReflectsConstructorLineCount()
        {
            var sequence = new DialogueSequence(new List<DialogueLine> { Line(), Line(), Line() });

            Assert.AreEqual(3, sequence.Count);
        }
    }
}
