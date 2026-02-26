using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GanFramework.Core.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class UILayerCanvas : MonoBehaviour
    {
        public UILayer layer = UILayer.Normal;

        public bool IsHasUIActive
        {
            get { return _uiBasesDic.Values.Any(ui => ui.IsActive); }
        }

        private Dictionary<string, ViewerBase> _uiBasesDic;

        public Dictionary<string, ViewerBase> UIBasesDic
        {
            get => _uiBasesDic;
        }

        private IUIManager _uiManager;

        private void Awake()
        {
            _uiBasesDic = new Dictionary<string, ViewerBase>();
            this._uiManager = UIManager.Instance;
            _uiManager.RegisterLayer(this);
            LoadUIs();
        }

        private void Start()
        {


        }

        private void OnDestroy()
        {
            if (_uiManager != null)
                _uiManager.UnRegisterLayer(this);
        }

        private void LoadUIs()
        {
            foreach (Transform child in gameObject.transform)
            {
                var list = child.GetComponents<IInitializable>();
                foreach (IInitializable i in list)
                {
                    // 仅处理ViewerBase类型的UI组件，其他类型的组件直接调用Init方法
                    if (i is ViewerBase viewerBase)
                    {
                        _uiBasesDic.TryAdd(viewerBase.UIName, viewerBase);
                        viewerBase.Layer = this.layer;
                        viewerBase.Init();
                        continue;
                    }
                    i.Init();
                }
            }
        }

        public void PrintAllUIs()
        {
            string result = "UIs in layer " + layer + ":\n";
            foreach (var kvp in _uiBasesDic)
            {
                result += "- " + kvp.Key + " IsActive: " + kvp.Value.IsActive + "\n";
            }

            Debug.Log(result);
        }
    }
}