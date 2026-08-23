using Feature.AceOfShadows.Scripts.Monobehaviour;
using NUnit.Framework;
using UnityEngine;

public class CardStackLayoutTests
{
    [Test]
    public void GetOffset_AtBottom_IsZero()
    {
        var offset = CardStackLayout.GetOffset(0);

        Assert.AreEqual(Vector3.zero, offset);
    }

    [Test]
    public void GetOffset_IncreasesLinearly_BelowCapDepth()
    {
        var offsetAtOne = CardStackLayout.GetOffset(1);
        var offsetAtTwo = CardStackLayout.GetOffset(2);

        Assert.Greater(offsetAtTwo.y, offsetAtOne.y);
        Assert.AreEqual(offsetAtOne.y * 2f, offsetAtTwo.y, 0.0001f);
    }

    [Test]
    public void GetOffset_IsFlat_AtAndBeyondCapDepth()
    {
        var atCap = CardStackLayout.GetOffset(12);
        var wellBeyondCap = CardStackLayout.GetOffset(143);

        Assert.AreEqual(atCap.y, wellBeyondCap.y, 0.0001f);
    }

    [Test]
    public void GetOffset_ZDecreases_AsDistanceFromBottomIncreases()
    {
        // Higher cards (closer to the top) get a smaller/more negative Z, so
        // they render in front under the camera's distance-based transparency
        // sorting - draw order comes from this, not SpriteRenderer.sortingOrder.
        var bottom = CardStackLayout.GetOffset(0);
        var top = CardStackLayout.GetOffset(143);

        Assert.Less(top.z, bottom.z);
    }

    [Test]
    public void GetOffset_ZIsNotCapped_UnlikeTheVisualFan()
    {
        // Y flattens out past the visual fan cap, but Z must stay unique per
        // card past that point too, or draw order among the "hidden" cards
        // becomes undefined.
        var atCap = CardStackLayout.GetOffset(12);
        var wellBeyondCap = CardStackLayout.GetOffset(143);

        Assert.AreNotEqual(atCap.z, wellBeyondCap.z);
    }
}
