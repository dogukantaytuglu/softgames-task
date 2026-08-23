using NUnit.Framework;
using SceneFlow.Logic;

public class SceneFlowStateTests
{
    [Test]
    public void Constructor_DefaultsCurrentSceneToNull()
    {
        var state = new SceneFlowState();

        Assert.IsNull(state.CurrentScene);
    }

    [Test]
    public void TryNavigate_ToNewScene_UpdatesCurrentScene_ReturnsTrue()
    {
        var state = new SceneFlowState();

        var result = state.TryNavigate("MainMenu", out var previousScene);

        Assert.IsTrue(result);
        Assert.IsNull(previousScene);
        Assert.AreEqual("MainMenu", state.CurrentScene);
    }

    [Test]
    public void TryNavigate_ToCurrentScene_ReturnsFalse_LeavesStateUnchanged()
    {
        var state = new SceneFlowState("MainMenu");

        var result = state.TryNavigate("MainMenu", out var previousScene);

        Assert.IsFalse(result);
        Assert.AreEqual("MainMenu", previousScene);
        Assert.AreEqual("MainMenu", state.CurrentScene);
    }

    [Test]
    public void TryNavigate_ToDifferentScene_ReturnsPreviousScene()
    {
        var state = new SceneFlowState("MainMenu");

        var result = state.TryNavigate("AceOfShadows", out var previousScene);

        Assert.IsTrue(result);
        Assert.AreEqual("MainMenu", previousScene);
        Assert.AreEqual("AceOfShadows", state.CurrentScene);
    }

    [TestCase(null)]
    [TestCase("")]
    public void TryNavigate_ToNullOrEmptyScene_ReturnsFalse(string targetScene)
    {
        var state = new SceneFlowState("MainMenu");

        var result = state.TryNavigate(targetScene, out var previousScene);

        Assert.IsFalse(result);
        Assert.AreEqual("MainMenu", previousScene);
        Assert.AreEqual("MainMenu", state.CurrentScene);
    }
}
