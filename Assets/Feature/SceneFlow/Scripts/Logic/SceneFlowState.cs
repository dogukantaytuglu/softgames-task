namespace SceneFlow.Logic
{
    public sealed class SceneFlowState
    {
        public string CurrentScene { get; private set; }

        public SceneFlowState(string initialScene = null)
        {
            CurrentScene = initialScene;
        }

        public bool TryNavigate(string targetScene, out string previousScene)
        {
            previousScene = CurrentScene;

            if (string.IsNullOrEmpty(targetScene) || targetScene == CurrentScene)
                return false;

            CurrentScene = targetScene;
            return true;
        }
    }
}
