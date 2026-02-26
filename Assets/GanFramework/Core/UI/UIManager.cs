using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using GanFramework.Core.Patterns;

namespace GanFramework.Core.UI
{
    // UI管理器，负责管理所有UI的打开、关闭和层级关系
    public class UIManager : GlobalMonoSingleton<UIManager>, IUIManager
    {
        // 检测是否应该锁定鼠标
        public bool IsShouldLockCursor()
        {
            foreach (var layer in unLockedCursorLayers)
            {
                if (IsLayerHasUIActive(layer))
                {
                    return false;
                }
            }

            return true;
        }

        // 检测某个层级下是否有UI处于激活状态
        public bool IsLayerHasUIActive(UILayer layer)
        {
            return layerRoots.FirstOrDefault(lr => lr.layer == layer)?.IsHasUIActive ?? false;
        }
        
        // 检查某个UI是否处于激活状态
        public bool IsUIActive<T>() where T : Component
        {
            string uiName = typeof(T).Name;
            var viewerBase = GetViewer(uiName);
            return viewerBase != null && viewerBase.IsActive;
        }

        // 允许解锁鼠标的UI层级
        [SerializeField] private List<UILayer> unLockedCursorLayers = new List<UILayer>
        {
            UILayer.Popup,
            UILayer.Top
        };
        
        // 所有注册的UI层级根节点
        public List<UILayerCanvas> layerRoots;

        protected override void Awake()
        {
            base.Awake();
            layerRoots = new List<UILayerCanvas>();
        }

        // 注册层级根节点, 提供给UILayerCanvas调用
        public void RegisterLayer(UILayerCanvas uiLayerCanvas)
        {
            if (!layerRoots.Contains(uiLayerCanvas))
            {
                layerRoots.Add(uiLayerCanvas);
            }
            else
            {
                Debug.LogError("UILayerRoot " + uiLayerCanvas.name + " is already registered.");
            }
        }

        // 注销层级根节点, 提供给UILayerCanvas调用
        public void UnRegisterLayer(UILayerCanvas uiLayerCanvas)
        {
            // 然后移除层级根节点
            if (layerRoots.Contains(uiLayerCanvas))
            {
                layerRoots.Remove(uiLayerCanvas);
            }
        }

        // 打开UI
        // 若UI已打开则直接返回该UI实例
        // 若UI未打开则加载预制体并实例化
        public T OpenUI<T>() where T : Component
        {
            string uiName = typeof(T).Name;
            ViewerBase viewerBase = null;
            viewerBase = GetViewer(uiName);
            if (viewerBase != null)
            {
                viewerBase.Open();
                return viewerBase as T;
            }

            // 若未打开则加载并实例化
            // 加载UI预制体
            GameObject prefab = Resources.Load<GameObject>("UI/" + uiName); // 从 Resources/UI 文件夹加载UI预制体
            if (prefab == null)
            {
                Debug.LogError("UI Prefab not found: " + uiName);
                return null;
            }

            // 检测UI预制体是否有UIBase组件
            var prefabUIBase = prefab.GetComponent<ViewerBase>();
            if (prefabUIBase == null)
            {
                Debug.LogError("UI Prefab does not have UIBase component: " + uiName);
                return null;
            }

            // 获取UI层级
            UILayer layer = prefabUIBase.GetComponent<ViewerBase>().Layer;

            // 确保层级根节点已注册,不存在则自己生成
            UILayerCanvas targetLayerCanvas = layerRoots.FirstOrDefault(lr => lr.layer == layer);
            if (targetLayerCanvas == null)
            {
                GameObject layerRootObj = new GameObject(layer.ToString() + "_Auto");
                layerRootObj.AddComponent<Canvas>();
                layerRootObj.AddComponent<CanvasScaler>();
                layerRootObj.AddComponent<CanvasRenderer>();
                layerRootObj.AddComponent<GraphicRaycaster>();
                targetLayerCanvas = layerRootObj.AddComponent<UILayerCanvas>();
                targetLayerCanvas.layer = layer;
                layerRoots.Add(targetLayerCanvas);
            }

            // 实例化UI
            GameObject uiObj = Instantiate(prefab, targetLayerCanvas.gameObject.transform); // 实例化
            viewerBase = uiObj.GetComponent<ViewerBase>(); // 获取UIBase组件
            viewerBase.Init(); // 初始化UI
            targetLayerCanvas.UIBasesDic.TryAdd(viewerBase.name, viewerBase); // 注册UI到层级根节点

            viewerBase.Open(); // 显示UI
            UpdateCursorState(); // 更新鼠标状态
            return viewerBase as T;
        }

        // 关闭UI
        // 若UI未打开则不做任何操作
        // UI类名与Hierarchy中的UI对象名必须一致
        public void CloseUI<T>() where T : Component
        {
            string uiName = typeof(T).Name;
            // 遍历所有LayerRoot，查找并关闭该UI
            GetViewer(uiName)?.Close();
            UpdateCursorState();
        }

        // 根据UI名称在所有层级中查找UI实例
        private ViewerBase GetViewer(string viewerName)
        {
            foreach (var layerRoot in layerRoots)
            {
                if (layerRoot.UIBasesDic.TryGetValue(viewerName, out ViewerBase existingUI))
                {
                    return existingUI;
                }
            }

            return null;
        }

        // 根据当前UI状态更新鼠标锁定状态
        public void UpdateCursorState()
        {
            bool shouldLockCursor = IsShouldLockCursor();
            Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}