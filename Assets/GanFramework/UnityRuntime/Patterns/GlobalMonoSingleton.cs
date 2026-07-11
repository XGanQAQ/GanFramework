using UnityEngine;

namespace GanFramework.Patterns
{
    /// <summary>
    /// 持久化单例基类，确保继承类在场景中只有一个实例，并在场景切换时保持存在。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GlobalMonoSingleton<T> : MonoBehaviour where T : Component
    {
        [Tooltip("if this is true, this singleton will auto detach if it finds itself parented on awake")]
        public bool UnparentOnAwake = true;

        public static bool HasInstance => instance != null;
        public static T Current => instance;

        protected static T instance;
        private static bool isShuttingDown;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                    if (instance == null)
                    {
                        if (isShuttingDown || !Application.isPlaying)
                        {
                            return null;
                        }

                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name + "AutoCreated";
                        instance = obj.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake() => InitializeSingleton();

        protected virtual void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                isShuttingDown = true;
                instance = null;
            }
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            isShuttingDown = false;

            if (UnparentOnAwake)
            {
                transform.SetParent(null);
            }

            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                enabled = true;
            }
            else
            {
                if (this != instance)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
