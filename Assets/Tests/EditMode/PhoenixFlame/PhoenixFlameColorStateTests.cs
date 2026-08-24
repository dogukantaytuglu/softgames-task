using System;
using NUnit.Framework;
using PhoenixFlame.Logic;

namespace PhoenixFlame.Tests
{
    public class PhoenixFlameColorStateTests
    {
        [Test]
        public void Constructor_DefaultsToIndexZero()
        {
            var state = new PhoenixFlameColorState(3);

            Assert.AreEqual(0, state.CurrentIndex);
        }

        [Test]
        public void Constructor_NonPositiveOptionCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PhoenixFlameColorState(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PhoenixFlameColorState(-1));
        }

        [Test]
        public void Constructor_StartIndexOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PhoenixFlameColorState(3, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PhoenixFlameColorState(3, -1));
        }

        [Test]
        public void TrySelect_DifferentIndex_UpdatesCurrentIndex_ReturnsTrue()
        {
            var state = new PhoenixFlameColorState(3);

            var result = state.TrySelect(2);

            Assert.IsTrue(result);
            Assert.AreEqual(2, state.CurrentIndex);
        }

        [Test]
        public void TrySelect_SameIndexAsCurrent_ReturnsFalse_DoesNotChange()
        {
            var state = new PhoenixFlameColorState(3, 1);

            var result = state.TrySelect(1);

            Assert.IsFalse(result);
            Assert.AreEqual(1, state.CurrentIndex);
        }

        [Test]
        public void TrySelect_IndexOutOfRange_Throws()
        {
            var state = new PhoenixFlameColorState(3);

            Assert.Throws<ArgumentOutOfRangeException>(() => state.TrySelect(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.TrySelect(-1));
        }
    }
}
