using UnityEngine;
namespace Core
{
    public static class GlobalBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {

        }
    }
}