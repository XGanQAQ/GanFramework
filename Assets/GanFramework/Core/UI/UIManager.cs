using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GanFramework.Core.Patterns;

namespace GanFramework.Core.UI
{
    /// <summary>
    /// 单例UI管理者，控制UI的层级关系，UI的开关
    /// </summary>
    public class UIManager : PersistentSingleton<UIManager>
    {
        public bool IsHasBackgroundUIActive
        {
            get
            {
                return openedUIs.Values.Any(ui =>
                    ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Background]);
            }
        }

        public bool IsHasNormalUIActive
        {
            get
            {
                return openedUIs.Values.Any(ui =>
                    ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Normal]);
            }
        }

        public bool IsHasPopupUIActive
        {
            get
            {
                return openedUIs.Values.Any(ui =>
                    ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Popup]);
            }
        }

        public bool IsHasTopUIActive
        {
            get
            {
                return openedUIs.Values.Any(ui =>
                    ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Top]);
            }
        }

        private Dictionary<UILayer, Transform> layerRoots = new Dictionary<UILayer, Transform>();
        private Dictionary<string, UIBase> openedUIs = new Dictionary<string, UIBase>();

        public void RegisterLayer(UILayer layer, Transform root)
        {
            if (!layerRoots.ContainsKey(layer))
            {
                layerRoots[layer] = root;

                // 注册场景中已存在的UI
                // 遍历子对象
                foreach (Transform child in root)
                {
                    var uiName = child.gameObject.name;
                    UIBase uiBase = child.GetComponent<UIBase>();
                    if (uiBase != null && !openedUIs.ContainsKey(uiName))
                    {
                        openedUIs[uiName] = uiBase;
                    }
                }
            }
        }

        public void UnRegisterLayer(UILayer layer)
        {
            // 先移除该层级下的UI注册
            var uiToRemove = openedUIs.Values.Where(ui => ui.transform.parent == layerRoots[layer]).ToList();
            foreach (var ui in uiToRemove)
            {
                openedUIs.Remove(ui.gameObject.name);
            }

            // 然后移除层级根节点
            if (layerRoots.ContainsKey(layer))
            {
                layerRoots.Remove(layer);
            }
        }

        public T OpenUI<T>(UILayer layer = UILayer.Normal) where T : Component
        {
            string uiName = typeof(T).Name;
            if (openedUIs.ContainsKey(uiName))
            {
                openedUIs[uiName].Show();
                UpdateCursorState();
                return openedUIs[uiName] as T;
            }

            // 加载UI预制体
            GameObject prefab = Resources.Load<GameObject>("UI/" + uiName); // 从 Resources/UI 文件夹加载UI预制体
            if (prefab == null)
            {
                Debug.LogError("UI Prefab not found: " + uiName);
                return null;
            }

            GameObject uiObj = Instantiate(prefab, layerRoots[layer]); // 实例化并设置父对象
            UIBase uiBase = uiObj.GetComponent<UIBase>(); // 获取UIBase组件
            openedUIs[uiName] = uiBase; // 注册到字典中
            uiBase.Show(); // 显示UI
            UpdateCursorState(); // 更新鼠标状态
            return uiBase as T;
        }

        public void CloseUI<T>() where T : Component
        {
            string uiName = typeof(T).Name;
            if (openedUIs.ContainsKey(uiName))
            {
                openedUIs[uiName].Hide();
                UpdateCursorState();
            }
        }

        public void ClosePopupUI()
        {
            var popupUIs = openedUIs.Values.Where(ui =>
                ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Popup]);
            foreach (var ui in popupUIs)
            {
                ui.Hide();
            }

            UpdateCursorState();
        }

        public void CloseTopUI()
        {
            var topUIs = openedUIs.Values.Where(ui =>
                ui.gameObject.activeSelf && ui.transform.parent == layerRoots[UILayer.Top]);
            foreach (var ui in topUIs)
            {
                ui.Hide();
            }

            UpdateCursorState();
        }

        // 来回切换UI状态，按同一个键实现UI的开关
        public void SwitchUI<T>(UILayer layer = UILayer.Normal) where T : Component
        {
            string uiName = typeof(T).Name;
            if (openedUIs.ContainsKey(uiName) && openedUIs[uiName].gameObject.activeSelf)
            {
                CloseUI<T>();
            }
            else
            {
                OpenUI<T>(layer);
            }
        }

        public Dictionary<UILayer, List<GameObject>> GetExistingUIsByLayer()
        {
            var result = new Dictionary<UILayer, List<GameObject>>();
            foreach (var kvp in layerRoots)
            {
                var layer = kvp.Key;
                var root = kvp.Value;
                var uiList = new List<GameObject>();
                foreach (Transform child in root)
                {
                    UIBase uiBase = child.GetComponent<UIBase>();
                    if (uiBase != null)
                        uiList.Add(uiBase.gameObject);
                }

                result[layer] = uiList;
            }

            return result;
        }


        private void UpdateCursorState()
        {
            bool shouldLockCursor = !(IsHasPopupUIActive || IsHasTopUIActive); 
            Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}