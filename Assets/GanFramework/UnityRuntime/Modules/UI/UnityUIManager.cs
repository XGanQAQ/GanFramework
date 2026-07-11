using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GanFramework.Core.UI;

namespace GanFramework.UnityRuntime.UI
{
    public class UnityUIManager : UIManager
    {
        private readonly Dictionary<UILayer, GameObject> _layerRoots = new();
        private GameObject _uiRoot;

        public UnityUIManager(HashSet<UILayer> unLockedCursorLayers, bool isNeedAutoLockCursor = true)
        {
            CreateUIRoot();
            CreateLayerCanvases();
            EnsureEventSystem();

            SetCursorController(new CursorController(this, unLockedCursorLayers, isNeedAutoLockCursor));
            CursorController.UpdateCursorState();
        }

        public override void OnDestroy()
        {
            foreach (var layerRoot in _layerRoots.Values)
            {
                if (layerRoot != null)
                    Object.Destroy(layerRoot);
            }

            _layerRoots.Clear();

            if (_uiRoot != null)
            {
                Object.Destroy(_uiRoot);
                _uiRoot = null;
            }

            base.OnDestroy();
        }

        protected override IViewer CreateViewer(string viewerName, string assetKey, UILayer layer)
        {
            GameObject prefab = GetResManager()?.Load<GameObject>(assetKey);
            if (prefab == null)
            {
                Debug.LogError("[UnityUIManager]: UI Prefab not found: " + assetKey);
                return null;
            }

            if (!_layerRoots.TryGetValue(layer, out var layerRoot))
            {
                Debug.LogError("[UnityUIManager]: Layer " + layer + " has not been initialized.");
                return null;
            }

            GameObject uiObj = Object.Instantiate(prefab, layerRoot.transform);
            var viewer = uiObj.GetComponent<UIViewer>();
            if (viewer == null)
            {
                Debug.LogError("[UnityUIManager]: UIViewer component is missing on prefab: " + assetKey);
                Object.Destroy(uiObj);
                return null;
            }

            viewer.Init();

            var initializables = uiObj.GetComponents<IInitializable>();
            foreach (var init in initializables)
            {
                if (!ReferenceEquals(init, viewer))
                    init.Init();
            }

            return viewer;
        }

        private void CreateUIRoot()
        {
            if (_uiRoot != null)
                return;

            _uiRoot = new GameObject("[UIRoot]");
            Object.DontDestroyOnLoad(_uiRoot);
        }

        private void CreateLayerCanvases()
        {
            _layerRoots.Clear();

            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                GameObject layerRootObj = new GameObject(layer + "_Canvas");
                layerRootObj.transform.SetParent(_uiRoot.transform);
                var canvas = layerRootObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = (int)layer * 100;
                layerRootObj.AddComponent<CanvasScaler>();
                layerRootObj.AddComponent<CanvasRenderer>();
                layerRootObj.AddComponent<GraphicRaycaster>();
                _layerRoots[layer] = layerRootObj;
            }
        }

        private void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObj = new GameObject("[EventSystem]");
            eventSystemObj.transform.SetParent(_uiRoot.transform);
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }
}
