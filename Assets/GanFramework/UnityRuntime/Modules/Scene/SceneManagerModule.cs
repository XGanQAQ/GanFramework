using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif
using UnityEngine.SceneManagement;
using GanFramework.Core;

namespace GanFramework.Runtime.Modules.Scene
{
    public class SceneManagerModule : ISceneManager, IModules
    {
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };

        public bool IsLoading { get; private set; }
        public float Progress => (regularGroup.Progress + addressableProgress) / 2f;

        readonly AsyncOperationGroup regularGroup = new AsyncOperationGroup(10);

#if USE_ADDRESSABLES
        readonly AsyncOperationHandleGroup addressableGroup = new AsyncOperationHandleGroup(10);
        float addressableProgress => addressableGroup.Progress;
#else
        float addressableProgress => 0f;
#endif

        public async Task LoadScenesAsync(IReadOnlyList<SceneLoadTask> scenes, IProgress<float> progress = null, bool reloadDupScenes = false)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[SceneManagerModule] Already loading scenes, skipping request.");
                return;
            }

            IsLoading = true;

            await UnloadScenesAsync();

            var loadedSceneNames = new List<string>();
            int sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
                loadedSceneNames.Add(SceneManager.GetSceneAt(i).name);

            regularGroup.Operations.Clear();
#if USE_ADDRESSABLES
            addressableGroup.Handles.Clear();
#endif

            int totalToLoad = scenes.Count;
            var tempRegularOps = new List<AsyncOperation>(totalToLoad);

            for (var i = 0; i < totalToLoad; i++)
            {
                var sceneData = scenes[i];
                if (!reloadDupScenes && loadedSceneNames.Contains(sceneData.SceneName))
                    continue;

                if (!sceneData.IsAddressable)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.ScenePath, LoadSceneMode.Additive);
                    if (operation != null)
                        tempRegularOps.Add(operation);
                }
#if USE_ADDRESSABLES
                else
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.ScenePath, LoadSceneMode.Additive);
                    addressableGroup.Handles.Add(sceneHandle);
                }
#else
                else
                {
                    Debug.LogWarning($"[SceneManagerModule] Scene '{sceneData.SceneName}' is addressable but Addressables are not enabled. Skipping.");
                }
#endif

                OnSceneLoaded.Invoke(sceneData.SceneName);
            }

            regularGroup.Operations.AddRange(tempRegularOps);

            // Wait for all operations
            while (!regularGroup.IsDone || !IsAddressableGroupDone())
            {
                progress?.Report(Progress);
                await Task.Delay(100);
            }
            progress?.Report(1f);

            // Set active scene
            var activeSceneData = scenes.FirstOrDefault(s => s.SceneType == SceneType.ActiveScene);
            if (!string.IsNullOrEmpty(activeSceneData.SceneName))
            {
                var activeScene = SceneManager.GetSceneByName(activeSceneData.SceneName);
                if (activeScene.IsValid())
                    SceneManager.SetActiveScene(activeScene);
            }

            IsLoading = false;
            OnSceneGroupLoaded.Invoke();
        }

        public async Task UnloadScenesAsync()
        {
            var scenesToUnload = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;

            int sceneCount = SceneManager.sceneCount;
            for (var i = sceneCount - 1; i > 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;

                var sceneName = sceneAt.name;
                if (sceneName.Equals(activeScene) || sceneName == "Bootstrapper") continue;

#if USE_ADDRESSABLES
                if (addressableGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName)) continue;
#endif

                scenesToUnload.Add(sceneName);
            }

            var unloadOps = new AsyncOperationGroup(scenesToUnload.Count);
            foreach (var scene in scenesToUnload)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;
                unloadOps.Operations.Add(operation);
                OnSceneUnloaded.Invoke(scene);
            }

#if USE_ADDRESSABLES
            foreach (var handle in addressableGroup.Handles)
            {
                if (handle.IsValid())
                    Addressables.UnloadSceneAsync(handle);
            }
            addressableGroup.Handles.Clear();
#endif

            while (!unloadOps.IsDone)
                await Task.Delay(100);

            var unloadOp = Resources.UnloadUnusedAssets();
            while (!unloadOp.isDone)
                await Task.Delay(100);
        }

        bool IsAddressableGroupDone()
        {
#if USE_ADDRESSABLES
            return addressableGroup.IsDone;
#else
            return true;
#endif
        }

        public void OnInit() { }

        public void OnUpdate(float deltaTime) { }

        public void OnFixedUpdate(float fixedDeltaTime) { }

        public void OnLateUpdate(float deltaTime) { }

        public void OnDestroy()
        {
            regularGroup.Operations.Clear();
#if USE_ADDRESSABLES
            addressableGroup.Handles.Clear();
#endif
            OnSceneLoaded = delegate { };
            OnSceneUnloaded = delegate { };
            OnSceneGroupLoaded = delegate { };
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.Count == 0 || Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

#if USE_ADDRESSABLES
    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
#endif
}
