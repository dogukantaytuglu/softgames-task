using AceOfShadows.Monobehaviour;
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
    public void GetSortingOrder_IsBaseOrderPlusDistance_AndStacksNeverCollide()
    {
        var sourceTop = CardStackLayout.GetSortingOrder(stackSlot: 0, distanceFromBottom: 143);
        var targetBottom = CardStackLayout.GetSortingOrder(stackSlot: 1, distanceFromBottom: 0);

        Assert.Less(sourceTop, targetBottom);
    }
}
