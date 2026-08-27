using NUnit.Framework;
using SceneServices.Logic;

namespace SceneServices.Tests
{
    public class SceneServiceStateTests
    {
        [Test]
        public void Constructor_DefaultsCurrentSceneToNull()
        {
            var state = new SceneServiceState();

            Assert.IsNull(state.CurrentScene);
        }

        [Test]
        public void TryBeginNavigation_ToNewScene_UpdatesCurrentScene_ReturnsTrue()
        {
            var state = new SceneServiceState();

            var result = state.TryBeginNavigation("MainMenu", out var previousScene);

            Assert.IsTrue(result);
            Assert.IsNull(previousScene);
            Assert.AreEqual("MainMenu", state.CurrentScene);
        }

        [Test]
        public void TryBeginNavigation_ToCurrentScene_ReturnsFalse_LeavesStateUnchanged()
        {
            var state = new SceneServiceState("MainMenu");

            var result = state.TryBeginNavigation("MainMenu", out var previousScene);

            Assert.IsFalse(result);
            Assert.AreEqual("MainMenu", previousScene);
            Assert.AreEqual("MainMenu", state.CurrentScene);
        }

        [Test]
        public void TryBeginNavigation_ToDifferentScene_ReturnsPreviousScene()
        {
            var state = new SceneServiceState("MainMenu");

            var result = state.TryBeginNavigation("AceOfShadows", out var previousScene);

            Assert.IsTrue(result);
            Assert.AreEqual("MainMenu", previousScene);
            Assert.AreEqual("AceOfShadows", state.CurrentScene);
        }

        [TestCase(null)]
        [TestCase("")]
        public void TryBeginNavigation_ToNullOrEmptyScene_ReturnsFalse(string targetScene)
        {
            var state = new SceneServiceState("MainMenu");

            var result = state.TryBeginNavigation(targetScene, out var previousScene);

            Assert.IsFalse(result);
            Assert.AreEqual("MainMenu", previousScene);
            Assert.AreEqual("MainMenu", state.CurrentScene);
        }

        [Test]
        public void TryBeginNavigation_WhileAlreadyTransitioning_ReturnsFalse_LeavesStateUnchanged()
        {
            var state = new SceneServiceState("MainMenu");
            state.TryBeginNavigation("AceOfShadows", out _);

            var result = state.TryBeginNavigation("MagicWords", out var previousScene);

            Assert.IsFalse(result);
            Assert.AreEqual("AceOfShadows", previousScene);
            Assert.AreEqual("AceOfShadows", state.CurrentScene);
        }

        [Test]
        public void CompleteNavigation_AllowsANewNavigationToBegin()
        {
            var state = new SceneServiceState("MainMenu");
            state.TryBeginNavigation("AceOfShadows", out _);

            state.CompleteNavigation();
            var result = state.TryBeginNavigation("MagicWords", out var previousScene);

            Assert.IsTrue(result);
            Assert.AreEqual("AceOfShadows", previousScene);
            Assert.AreEqual("MagicWords", state.CurrentScene);
        }
    }
}
