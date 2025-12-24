using UnityEngine;
namespace GanFramework.Core.Bootstrap
{
    public static class GlobalBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {

        }
    }
}