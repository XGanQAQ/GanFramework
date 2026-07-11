using System;
using System.Collections.Generic;
using System.Reflection;
using GanFramework.Core.Resource;

namespace GanFramework.Core.UI
{
    public abstract class UIManager : IUIManager, IModules
    {
        private readonly Dictionary<UILayer, Dictionary<string, IViewer>> _layerViewers = new();
        private static readonly Dictionary<string, Type> _viewerTypeCache = new();

        protected ICursorController CursorController;

        protected UIManager()
        {
            InitializeLayerViewers();
        }

        public bool IsActive(string viewerName)
        {
            var viewer = GetViewer(viewerName);
            return viewer != null && viewer.IsActive;
        }

        public IViewer OpenUI(string viewerName, bool show = true)
        {
            IViewer viewer = GetViewer(viewerName);
            if (viewer != null)
            {
                if (show) viewer.Open();
                else viewer.Close();
                CursorController?.UpdateCursorState();
                return viewer;
            }

            Type type = ResolveViewerType(viewerName);
            var attr = type?.GetCustomAttribute<ViewerAttribute>();
            string assetKey = attr != null && !string.IsNullOrEmpty(attr.AssetKey)
                ? attr.AssetKey
                : "UI/" + viewerName;
            UILayer layer = attr?.Layer ?? UILayer.Normal;

            viewer = CreateViewer(viewerName, assetKey, layer);
            if (viewer == null)
                return null;

            if (!_layerViewers.TryGetValue(layer, out var viewers))
            {
                viewers = new Dictionary<string, IViewer>();
                _layerViewers[layer] = viewers;
            }

            viewers.TryAdd(viewerName, viewer);

            if (show) viewer.Open();
            else viewer.Close();

            CursorController?.UpdateCursorState();
            return viewer;
        }

        public void CloseUI(string viewerName)
        {
            IViewer viewer = GetViewer(viewerName);
            if (viewer != null)
            {
                viewer.Close();
            }

            CursorController?.UpdateCursorState();
        }

        public IEnumerable<KeyValuePair<string, IViewer>> GetLayerViewers(UILayer layer)
        {
            if (!_layerViewers.TryGetValue(layer, out var viewers) || viewers == null)
                return Array.Empty<KeyValuePair<string, IViewer>>();

            return viewers;
        }

        protected bool TryGetLayerViewers(UILayer layer, out Dictionary<string, IViewer> viewers)
        {
            return _layerViewers.TryGetValue(layer, out viewers);
        }

        protected void SetCursorController(ICursorController cursorController)
        {
            CursorController = cursorController;
        }

        protected abstract IViewer CreateViewer(string viewerName, string assetKey, UILayer layer);

        protected IResManager GetResManager()
        {
            return Framework.GetModule<IResManager>();
        }

        private void InitializeLayerViewers()
        {
            _layerViewers.Clear();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _layerViewers[layer] = new Dictionary<string, IViewer>();
            }
        }

        private IViewer GetViewer(string viewerName)
        {
            foreach (var layer in _layerViewers.Values)
            {
                if (layer.TryGetValue(viewerName, out IViewer existingUI))
                    return existingUI;
            }

            return null;
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

        #region IModules Implementation

        public virtual void OnInit()
        {
        }

        public virtual void OnUpdate(float deltaTime)
        {
        }

        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public virtual void OnLateUpdate(float deltaTime)
        {
        }

        public virtual void OnDestroy()
        {
            _layerViewers.Clear();
        }
        #endregion
    }
}