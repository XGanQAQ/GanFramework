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
    public partial class UIManager : IModules
    {
        public void OnInit()
        {

        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public void OnLateUpdate(float deltaTime)
        {
        }

        public void OnDestroy()
        {
            foreach (var layerInfo in _layerRoots.Values)
            {
                if (layerInfo?.Root != null)
                    UnityEngine.Object.Destroy(layerInfo.Root);
            }

            _layerRoots.Clear();

            if (UIRoot != null)
            {
                UnityEngine.Object.Destroy(UIRoot);
                UIRoot = null;
            }

            if (ReferenceEquals(_instance, this))
                _instance = null;
        }
    }
}