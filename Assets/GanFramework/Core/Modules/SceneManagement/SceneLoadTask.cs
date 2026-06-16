namespace GanFramework.Core
{
    public struct SceneLoadTask
    {
        public string ScenePath;
        public string SceneName;
        public SceneType SceneType;
        public bool IsAddressable;

        public SceneLoadTask(string scenePath, string sceneName, SceneType sceneType, bool isAddressable = false)
        {
            ScenePath = scenePath;
            SceneName = sceneName;
            SceneType = sceneType;
            IsAddressable = isAddressable;
        }
    }
}
