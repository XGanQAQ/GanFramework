using System;
using UnityEngine;
namespace Core
{
    // 挂在 NormalCanvas 上的脚本
    public class UILayerRoot : MonoBehaviour
    {
        public UILayer layer;

        void Awake()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterLayer(layer, transform);
            }
        }

        private void OnDestroy()
        {
            UIManager.Instance.UnRegisterLayer(layer);
        }
    }

}
