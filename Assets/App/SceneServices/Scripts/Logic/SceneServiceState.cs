namespace SceneServices.Logic
{
    public sealed class SceneServiceState
    {
        public string CurrentScene { get; private set; }
        public bool IsTransitioning { get; private set; }

        public SceneServiceState(string initialScene = null)
        {
            CurrentScene = initialScene;
        }

        public bool TryBeginNavigation(string targetScene, out string previousScene)
        {
            previousScene = CurrentScene;

            if (IsTransitioning || string.IsNullOrEmpty(targetScene) || targetScene == CurrentScene)
                return false;

            CurrentScene = targetScene;
            IsTransitioning = true;
            return true;
        }

        public void CompleteNavigation()
        {
            IsTransitioning = false;
        }
    }
}
