using UnityEngine;

namespace GanFramework.Core
{
    public interface IResManager
    {
        T Load<T>(string path) where T : Object;
        T GetFromCache<T>(string path) where T : Object;
    }
}
