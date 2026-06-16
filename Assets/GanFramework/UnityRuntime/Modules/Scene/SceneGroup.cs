using System;
using System.Collections.Generic;
using System.Linq;
using GanFramework.Core;

namespace GanFramework.Runtime.Modules.Scene
{
    [Serializable]
    public class SceneGroup
    {
        public string GroupName = "New Scene Group";
        public List<SceneData> Scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Reference.Name;
        }

        public List<SceneLoadTask> ToLoadTasks()
        {
            return Scenes.Select(s => new SceneLoadTask
            {
                ScenePath = s.Reference.Path,
                SceneName = s.Reference.Name,
                SceneType = s.SceneType,
#if USE_ADDRESSABLES
                IsAddressable = s.Reference.IsAddressable,
#endif
            }).ToList();
        }
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference Reference;
        public SceneType SceneType;
    }
}