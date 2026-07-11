using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GanFramework.Core;
using GanFramework.Core.UI;
using GanFramework.Core.Resource;

namespace GanFramework.UnityRuntime.UI
{
    public partial class UIManager : IUIManager
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        private ICursorController _cursorController;

        private GameObject UIRoot;

        public class LayerInfo
        {
            public GameObject Root { get; set; }
            public Dictionary<string, IViewer> UIBasesDic { get; } = new();
            public bool IsHasUIActive => UIBasesDic.Values.Any(ui => ui.IsActive);
        }

        public bool IsLayerHasUIActive(UILayer layer)
        {
            return _layerRoots.TryGetValue(layer, out var layerRoot) && layerRoot.IsHasUIActive;
        }

        public bool IsUIActive<T>() where T : class, IViewer
        {
            string uiName = typeof(T).Name;
            var viewerBase = GetViewer(uiName);
            return viewerBase != null && viewerBase.IsActive;
        }

        private Dictionary<UILayer, LayerInfo> _layerRoots = new();
        private static readonly Dictionary<string, Type> _viewerTypeCache = new();

        public UIManager(HashSet<UILayer> unLockedCursorLayers, bool isNeedAutoLockCursor = true)
        {
            _instance = this;

            if (UIRoot == null)
            {
                UIRoot = new GameObject("[UIRoot]");
                UnityEngine.Object.DontDestroyOnLoad(UIRoot);
            }

            _layerRoots.Clear();

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                CreateLayerCanvas(layer);
            }

            EnsureEventSystem();

            _cursorController = new CursorController(this, unLockedCursorLayers, isNeedAutoLockCursor);
            _cursorController.UpdateCursorState();
        }

        public IViewer OpenUI(string viewerName, bool show = true)
        {
            IViewer viewerBase = GetViewer(viewerName);
            if (viewerBase != null)
            {
                if (show) viewerBase.Open();
                else viewerBase.Close();
                _cursorController.UpdateCursorState();
                return viewerBase;
            }

            Type type = ResolveViewerType(viewerName);
            var attr = type?.GetCustomAttribute<ViewerAttribute>();
            return CreateUI(viewerName, attr, show);
        }

        public void CloseUI(string viewerName)
        {
            IViewer viewer = GetViewer(viewerName);
            if (viewer != null)
            {
                viewer.Close();
            }
            _cursorController.UpdateCursorState();
        }

        public void CloseLayerUI(UILayer layer)
        {
            if (!_layerRoots.TryGetValue(layer, out var layerInfo) || layerInfo == null)
                return;

            bool hasClosed = false;
            foreach (var viewer in layerInfo.UIBasesDic.Values)
            {
                if (viewer != null && viewer.IsActive)
                {
                    viewer.Close();
                    hasClosed = true;
                }
            }

            if (hasClosed)
                _cursorController.UpdateCursorState();
        }

        public bool TryCloseLayerUIByEscape(UILayer layer)
        {
            if (!_layerRoots.TryGetValue(layer, out var layerInfo) || layerInfo == null)
                return false;

            var target = layerInfo.UIBasesDic.Values
                .Where(viewer => viewer != null && viewer.IsActive && viewer.CloseableByEscape)
                .OrderByDescending(viewer => (viewer as Component)?.transform.GetSiblingIndex() ?? int.MinValue)
                .FirstOrDefault();

            if (target == null)
                return false;

            target.Close();
            _cursorController.UpdateCursorState();
            return true;
        }

        private IViewer GetViewer(string viewerName)
        {
            foreach (var layerRoot in _layerRoots.Values)
            {
                if (layerRoot.UIBasesDic.TryGetValue(viewerName, out IViewer existingUI))
                    return existingUI;
            }
            return null;
        }

        private void CreateLayerCanvas(UILayer layer)
        {
            GameObject layerRootObj = new GameObject(layer.ToString() + "_Canvas");
            layerRootObj.transform.SetParent(UIRoot.transform);
            var canvas = layerRootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)layer * 100;
            layerRootObj.AddComponent<CanvasScaler>();
            layerRootObj.AddComponent<CanvasRenderer>();
            layerRootObj.AddComponent<GraphicRaycaster>();
            _layerRoots.Add(layer, new LayerInfo { Root = layerRootObj });
        }

        private void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObj = new GameObject("[EventSystem]");
            eventSystemObj.transform.SetParent(UIRoot.transform);
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }


        private UIViewer CreateUI(string uiName, ViewerAttribute attr, bool show)
        {
            string assetKey = (attr != null && !string.IsNullOrEmpty(attr.AssetKey))
                ? attr.AssetKey
                : "UI/" + uiName;

            UILayer layer = attr?.Layer ?? UILayer.Normal;

            GameObject prefab = Framework.GetModule<IResManager>()?.Load<GameObject>(assetKey);
            if (prefab == null)
            {
                Debug.LogError("[UIManager]: UI Prefab not found: " + assetKey);
                return null;
            }

            if (!_layerRoots.TryGetValue(layer, out var layerInfo))
            {
                Debug.LogError("[UIManager]: Layer " + layer + " has not been initialized.");
                return null;
            }

            GameObject uiObj = UnityEngine.Object.Instantiate(prefab, layerInfo.Root.transform);
            var viewerBase = uiObj.GetComponent<UIViewer>();
            viewerBase.Init();
            layerInfo.UIBasesDic.TryAdd(uiName, viewerBase);

            var initializables = uiObj.GetComponents<IInitializable>();
            foreach (var init in initializables)
            {
                if (!ReferenceEquals(init, viewerBase))
                    init.Init();
            }

            if (show) viewerBase.Open();
            else viewerBase.Close();
            _cursorController.UpdateCursorState();
            return viewerBase;
        }

        private static Type ResolveViewerType(string uiName)
        {
            if (_viewerTypeCache.TryGetValue(uiName, out var cached))
                return cached;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == uiName && typeof(IViewer).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        _viewerTypeCache[uiName] = type;
                        return type;
                    }
                }
            }

            _viewerTypeCache[uiName] = null;
            return null;
        }
    }
}
