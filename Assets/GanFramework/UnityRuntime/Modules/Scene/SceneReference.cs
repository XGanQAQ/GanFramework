using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GanFramework.Runtime.Modules.Scene
{
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
#endif
        [SerializeField] private string scenePath;

        [NonSerialized] private string sceneName;

        public string Path => scenePath;

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(sceneName))
                    return sceneName;
                return System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
        }

#if USE_ADDRESSABLES
        public bool IsAddressable;
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (sceneAsset != null)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(scenePath))
                    sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
            else if (!string.IsNullOrEmpty(scenePath) && sceneAsset == null)
            {
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset != null && string.IsNullOrEmpty(sceneName))
                    sceneName = sceneAsset.name;
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
    }
}
