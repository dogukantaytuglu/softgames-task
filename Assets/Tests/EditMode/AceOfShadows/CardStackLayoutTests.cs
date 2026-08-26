using AceOfShadows.Monobehaviour;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AceOfShadows.Tests
{
    public class CardStackLayoutTests
    {
        // Arbitrary, test-local: GetOffset is a pure function, so its behaviour is
        // asserted independently of whatever the shipped config happens to hold.
        // Whether the *shipped* values are sane is a separate test, below.
        private const float PerCardOffset = 2f;
        private const float MaxPileRise = 340f;

        private static float OffsetY(int distanceFromBottom) =>
            CardStackLayout.GetOffset(distanceFromBottom, PerCardOffset, MaxPileRise).y;

        [Test]
        public void GetOffset_AtBottom_IsZero()
        {
            var offset = CardStackLayout.GetOffset(0, PerCardOffset, MaxPileRise);

            Assert.AreEqual(Vector2.zero, offset);
        }

        [Test]
        public void GetOffset_IncreasesLinearly_BelowCapDepth()
        {
            var offsetAtOne = OffsetY(1);
            var offsetAtTwo = OffsetY(2);

            Assert.Greater(offsetAtTwo, offsetAtOne);
            Assert.AreEqual(offsetAtOne * 2f, offsetAtTwo, 0.0001f);
        }

        [Test]
        public void GetOffset_IsFlat_AtAndBeyondCapDepth()
        {
            // Comfortably past MaxPileRise / PerCardOffset, so this keeps testing the
            // cap rather than accidentally testing the linear range.
            var atCap = OffsetY(400);
            var wellBeyondCap = OffsetY(900);

            Assert.AreEqual(atCap, wellBeyondCap, 0.0001f);
        }

        // Configuration test, not a logic test: the point of the retune is that a full
        // deck stays visibly taller than a half-empty one, which only holds if the cap
        // never binds within totalCards. Reads the shipped asset rather than mirroring
        // its numbers here, so retuning the config cannot silently invalidate this.
        [Test]
        public void ShippedConfig_CapNeverBinds_AcrossTheWholeDeck()
        {
            var config = AssetDatabase.LoadAssetAtPath<AceOfShadowsConfig>(
                "Assets/Feature/AceOfShadows/Configs/AceOfShadowsConfig.asset");

            Assert.IsNotNull(config, "AceOfShadowsConfig.asset not found at the expected path.");

            var topCard = config.TotalCards - 1;
            var atHalfDeck = CardStackLayout.GetOffset(topCard / 2, config.PerCardOffset, config.MaxPileRise).y;
            var atFullDeck = CardStackLayout.GetOffset(topCard, config.PerCardOffset, config.MaxPileRise).y;

            Assert.Less(topCard * config.PerCardOffset, config.MaxPileRise,
                "A full deck reaches MaxPileRise, so the tallest piles all look identical.");
            Assert.AreEqual(atHalfDeck * 2f, atFullDeck, atHalfDeck * 0.05f);
        }
    }
}
