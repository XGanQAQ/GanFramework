using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using GanFramework.Core.UI;
using GanFramework.Runtime.Patterns;

namespace GanFramework.Runtime.UI
{
    // UI管理器，负责管理所有UI的打开、关闭和层级关系
    public class UIManager : GlobalMonoSingleton<UIManager>, IUIManager
    {
        public GameObject UIRoot; // 所有UI相关组件的根节点

        // 单个UI层级的数据
        public class LayerInfo
        {
            public GameObject Root { get; set; }
            public Dictionary<string, ViewerBase> UIBasesDic { get; } = new();
            public bool IsHasUIActive => UIBasesDic.Values.Any(ui => ui.IsActive);
        }

        // 检测是否应该锁定鼠标
        public bool IsShouldLockCursor()
        {
            foreach (var layer in unLockedCursorLayers)
            {
                if (IsLayerHasUIActive(layer))
                    return false;
            }
            return true;
        }

        // 检测某个层级下是否有UI处于激活状态
        public bool IsLayerHasUIActive(UILayer layer)
        {
            return _layerRoots.TryGetValue(layer, out var layerRoot) && layerRoot.IsHasUIActive;
        }

        // 检查某个UI是否处于激活状态
        public bool IsUIActive<T>() where T : Component
        {
            string uiName = typeof(T).Name;
            var viewerBase = GetViewer(uiName);
            return viewerBase != null && viewerBase.IsActive;
        }

        // 允许解锁鼠标的UI层级
        [SerializeField]
        private List<UILayer> unLockedCursorLayers = new List<UILayer>
        {
            UILayer.Popup,
            UILayer.Top
        };

        // UILayer 到层级数据的映射
        private Dictionary<UILayer, LayerInfo> _layerRoots = new();

        // 外部只读访问接口
        public IReadOnlyCollection<LayerInfo> layerRoots => _layerRoots.Values;
        public Dictionary<UILayer, LayerInfo> LayerRoots => _layerRoots;
        public float LastInteractiveUICloseTime { get; private set; } = -100f;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(UIRoot);

            // 在所有 Start 执行前创建好所有层级 Canvas，确保渲染顺序正确
            var layers = System.Enum.GetValues(typeof(UILayer));
            foreach (UILayer layer in layers)
            {
                CreateLayerCanvas(layer);
            }
        }

        void Start()
        {
        }

        // 创建单个层级 Canvas 并注册到字典
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

        /// <summary>
        /// 打开UI核心逻辑（内部方法）
        /// </summary>
        private ViewerBase OpenUIInternal(string uiName, bool show = true)
        {
            // 若UI已打开则直接返回
            ViewerBase viewerBase = GetViewer(uiName);
            if (viewerBase != null)
            {
                if (show) viewerBase.Open();
                else viewerBase.Close();
                UpdateCursorState();
                return viewerBase;
            }

            // 加载预制体
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

            // 层级 Canvas 应在 Start 时已全部创建，此处仅查找
            if (!_layerRoots.TryGetValue(layer, out var layerInfo))
            {
                Debug.LogError("[UIManager]: Layer " + layer + " has not been initialized.");
                return null;
            }

            // 实例化UI
            GameObject uiObj = Instantiate(prefab, layerInfo.Root.transform);
            viewerBase = uiObj.GetComponent<ViewerBase>();
            viewerBase.Init();
            layerInfo.UIBasesDic.TryAdd(uiName, viewerBase);

            // 初始化同一 GameObject 上的其他 IInitializable 组件（如 Presenter）
            // 顺序：ViewerBase.Init 先于其他组件，确保依赖关系
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

        public T OpenUI<T>(bool show = true) where T : ViewerBase
        {
            return OpenUIInternal(typeof(T).Name, show) as T;
        }

        public ViewerBase OpenUI(string viewerName, bool show = true)
        {
            return OpenUIInternal(viewerName, show);
        }

        public void CloseUI<T>() where T : ViewerBase
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

        // 根据UI名称在所有层级中查找UI实例
        private ViewerBase GetViewer(string viewerName)
        {
            foreach (var layerRoot in _layerRoots.Values)
            {
                if (layerRoot.UIBasesDic.TryGetValue(viewerName, out ViewerBase existingUI))
                    return existingUI;
            }
            return null;
        }

        // 来回切换UI状态
        public void SwitchUI<T>() where T : ViewerBase
        {
            if (IsUIActive<T>())
                CloseUI<T>();
            else
                OpenUI<T>();
        }

        // 关闭某个层级下的所有UI
        public void CloseLayerUI(UILayer layer)
        {
            if (!_layerRoots.TryGetValue(layer, out var layerInfo)) return;
            foreach (var uiBase in layerInfo.UIBasesDic.Values)
            {
                uiBase.Close();
            }
            UpdateCursorState();
        }

        /// <summary>
        /// 仅关闭指定层级中 CloseableByEscape 为 true 的 UI。
        /// 返回 true 表示至少关闭了一个 UI。
        /// </summary>
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
