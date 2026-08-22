using System;
using NUnit.Framework;

public class FpsCounterTest
{
    [Test]
    public void CurrentFps_BeforeAnySample_IsZero()
    {
        var calculator = new FpsCalculator(0.5f);

        Assert.AreEqual(0f, calculator.CurrentFps);
    }

    [Test]
    public void Sample_BelowUpdateInterval_DoesNotUpdateCurrentFps()
    {
        var calculator = new FpsCalculator(0.5f);

        calculator.Sample(0.1f);
        calculator.Sample(0.1f);
        calculator.Sample(0.1f);
        calculator.Sample(0.1f);

        Assert.AreEqual(0f, calculator.CurrentFps);
    }

    [Test]
    public void Sample_ReachingUpdateInterval_ComputesAverageFps()
    {
        var calculator = new FpsCalculator(0.5f);

        for (var i = 0; i < 5; i++)
            calculator.Sample(0.1f);

        Assert.AreEqual(10f, calculator.CurrentFps, 0.0001f);
    }

    [Test]
    public void Sample_AfterWindowResets_NextWindowComputesIndependently()
    {
        var calculator = new FpsCalculator(0.5f);

        for (var i = 0; i < 5; i++)
            calculator.Sample(0.1f);

        calculator.Sample(0.25f);
        calculator.Sample(0.25f);

        Assert.AreEqual(4f, calculator.CurrentFps, 0.0001f);
    }

    [Test]
    public void Constructor_NonPositiveUpdateInterval_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FpsCalculator(0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FpsCalculator(-1f));
    }
}
