using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GanFramework.Core
{
    public interface ISceneManager
    {
        event Action<string> OnSceneLoaded;
        event Action<string> OnSceneUnloaded;
        event Action OnSceneGroupLoaded;

        bool IsLoading { get; }
        float Progress { get; }

        Task LoadScenesAsync(IReadOnlyList<SceneLoadTask> scenes, IProgress<float> progress = null, bool reloadDupScenes = false);
        Task UnloadScenesAsync();
    }
}
