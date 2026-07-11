using UnityEngine;

namespace GanFramework.Core.Resource
{
    public interface IResManager
    {
        T Load<T>(string key, bool useResourcesOnly = false) where T : Object;
        T GetFromCache<T>(string key) where T : Object;
    }
}
