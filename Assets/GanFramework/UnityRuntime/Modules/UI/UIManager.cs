using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using GanFramework.Core.Modules.UI;
using GanFramework.Runtime.Patterns;

namespace GanFramework.Runtime.Modules.UI
{
    public class UIManager : GlobalMonoSingleton<UIManager>, IUIManager
    {
        public GameObject UIRoot;

        public class LayerInfo
        {
            public GameObject Root { get; set; }
            public Dictionary<string, ViewerBase> UIBasesDic { get; } = new();
            public bool IsHasUIActive => UIBasesDic.Values.Any(ui => ui.IsActive);
        }

        public bool IsShouldLockCursor()
        {
            foreach (var layer in unLockedCursorLayers)
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

        [SerializeField]
        private List<UILayer> unLockedCursorLayers = new List<UILayer>
        {
            UILayer.Popup,
            UILayer.Top
        };

        private Dictionary<UILayer, LayerInfo> _layerRoots = new();

        public IReadOnlyCollection<LayerInfo> layerRoots => _layerRoots.Values;
        public Dictionary<UILayer, LayerInfo> LayerRoots => _layerRoots;
        public float LastInteractiveUICloseTime { get; private set; } = -100f;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(UIRoot);

            var layers = System.Enum.GetValues(typeof(UILayer));
            foreach (UILayer layer in layers)
            {
                CreateLayerCanvas(layer);
            }
        }

        void Start()
        {
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

        private ViewerBase OpenUIInternal(string uiName, bool show = true)
        {
            ViewerBase viewerBase = GetViewer(uiName);
            if (viewerBase != null)
            {
                if (show) viewerBase.Open();
                else viewerBase.Close();
                UpdateCursorState();
                return viewerBase;
            }

            GameObject prefab = Resources.Load<GameObject>("UI/" + uiName);
            if (prefab == null)
            {
                Debug.LogError("[UIManager]: UI Prefab not found: " + uiName);
                return null;
            }

            var prefabViewerBase = prefab.GetComponent<ViewerBase>();
            if (prefabViewerBase == null)
            {
                Debug.LogError("[UIManager]: UI Prefab does not have ViewerBase component: " + uiName);
                return null;
            }

            UILayer layer = prefabViewerBase.Layer;

            if (!_layerRoots.TryGetValue(layer, out var layerInfo))
            {
                Debug.LogError("[UIManager]: Layer " + layer + " has not been initialized.");
                return null;
            }

            GameObject uiObj = Instantiate(prefab, layerInfo.Root.transform);
            viewerBase = uiObj.GetComponent<ViewerBase>();
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

        public T OpenUI<T>(bool show = true) where T : class, IViewer
        {
            return OpenUIInternal(typeof(T).Name, show) as T;
        }

        public IViewer OpenUI(string viewerName, bool show = true)
        {
            return OpenUIInternal(viewerName, show);
        }

        public void CloseUI<T>() where T : class, IViewer
        {
            string uiName = typeof(T).Name;
            ViewerBase viewer = GetViewer(uiName);
            if (viewer != null)
            {
                viewer.Close();
            }
            UpdateCursorState();
        }

        public void CloseUI(string viewerName)
        {
            ViewerBase viewer = GetViewer(viewerName);
            if (viewer != null)
            {
                viewer.Close();
            }
            UpdateCursorState();
        }

        private ViewerBase GetViewer(string viewerName)
        {
            foreach (var layerRoot in _layerRoots.Values)
            {
                if (layerRoot.UIBasesDic.TryGetValue(viewerName, out ViewerBase existingUI))
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

        public void RecordInteractiveUIClose(ViewerBase viewer)
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
    }
}
