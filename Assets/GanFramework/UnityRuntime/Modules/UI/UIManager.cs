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

        public GameObject UIRoot;

        public class LayerInfo
        {
            public GameObject Root { get; set; }
            public Dictionary<string, IViewer> UIBasesDic { get; } = new();
            public bool IsHasUIActive => UIBasesDic.Values.Any(ui => ui.IsActive);
        }

        public bool IsShouldLockCursor()
        {
            if (IsNeedAutoLockCursor == false)
                return false;
            foreach (var layer in UnLockedCursorLayers)
            {
                if (IsLayerHasUIActive(layer))
                    return false;
            }
            return true;
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

        // 控制在什么UILayer存在UI的情况下不锁定鼠标
        public HashSet<UILayer> UnLockedCursorLayers;
        public bool IsNeedAutoLockCursor = true;
        private Dictionary<UILayer, LayerInfo> _layerRoots = new();
        private static readonly Dictionary<string, Type> _viewerTypeCache = new();

        public IReadOnlyCollection<LayerInfo> layerRoots => _layerRoots.Values;
        public Dictionary<UILayer, LayerInfo> LayerRoots => _layerRoots;
        public float LastInteractiveUICloseTime { get; private set; } = -100f;

        public UIManager(bool isNeedAutoLockCursor = true)
        {
            _instance = this;
            IsNeedAutoLockCursor = isNeedAutoLockCursor;

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

            UnLockedCursorLayers = new HashSet<UILayer>
            {
                UILayer.Popup,
                UILayer.Top
            };

            UpdateCursorState();
        }

        #region IUIManager Implementation

        public T OpenUI<T>(bool show = true) where T : class, IViewer
        {
            return OpenUIInternal<T>(show) as T;
        }

        public IViewer OpenUI(string viewerName, bool show = true)
        {
            return OpenUIInternal(viewerName, show);
        }

        public void CloseUI<T>() where T : class, IViewer
        {
            string uiName = typeof(T).Name;
            IViewer viewer = GetViewer(uiName);
            if (viewer != null)
            {
                viewer.Close();
            }
            UpdateCursorState();
        }

        public void CloseUI(string viewerName)
        {
            IViewer viewer = GetViewer(viewerName);
            if (viewer != null)
            {
                viewer.Close();
            }
            UpdateCursorState();
        }
        #endregion

        private IViewer GetViewer(string viewerName)
        {
            foreach (var layerRoot in _layerRoots.Values)
            {
                if (layerRoot.UIBasesDic.TryGetValue(viewerName, out IViewer existingUI))
                    return existingUI;
            }
            return null;
        }

        public void SwitchUI<T>() where T : class, IViewer
        {
            if (IsUIActive<T>())
                CloseUI<T>();
            else
                OpenUI<T>();
        }

        public void CloseLayerUI(UILayer layer)
        {
            if (!_layerRoots.TryGetValue(layer, out var layerInfo)) return;
            foreach (var uiBase in layerInfo.UIBasesDic.Values)
            {
                uiBase.Close();
            }
            UpdateCursorState();
        }

        public bool TryCloseLayerUIByEscape(UILayer layer)
        {
            if (!_layerRoots.TryGetValue(layer, out var layerInfo)) return false;

            bool closedAny = false;
            foreach (var uiBase in layerInfo.UIBasesDic.Values)
            {
                if (uiBase.IsActive && uiBase.CloseableByEscape)
                {
                    uiBase.Close();
                    closedAny = true;
                }
            }

            if (closedAny) UpdateCursorState();
            return closedAny;
        }

        public void RecordInteractiveUIClose(IViewer viewer)
        {
            if (viewer == null) return;
            if (viewer.Layer != UILayer.Popup && viewer.Layer != UILayer.Top) return;

            LastInteractiveUICloseTime = Time.unscaledTime;
        }

        public void UpdateCursorState()
        {
            bool shouldLockCursor = IsShouldLockCursor();
            Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLockCursor;
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

        private IViewer OpenUIInternal<T>(bool show = true) where T : class, IViewer
        {
            string uiName = typeof(T).Name;
            IViewer viewerBase = GetViewer(uiName);
            if (viewerBase != null)
            {
                if (show) viewerBase.Open();
                else viewerBase.Close();
                UpdateCursorState();
                return viewerBase;
            }

            var attr = typeof(T).GetCustomAttribute<ViewerAttribute>();
            return CreateUI(uiName, attr, show);
        }

        private IViewer OpenUIInternal(string uiName, bool show = true)
        {
            IViewer viewerBase = GetViewer(uiName);
            if (viewerBase != null)
            {
                if (show) viewerBase.Open();
                else viewerBase.Close();
                UpdateCursorState();
                return viewerBase;
            }

            Type type = ResolveViewerType(uiName);
            var attr = type?.GetCustomAttribute<ViewerAttribute>();
            return CreateUI(uiName, attr, show);
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
            UpdateCursorState();
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
