using UnityEngine;

namespace GanFramework.Core.Patterns
{
    /// <summary>
    /// MonoBehavior抽象单例基类。确保继承类在场景中只有一个实例，并提供全局访问点。
    /// 不会在场景切换时保持存在
    /// </summary>
    /// <typeparam name="T">必须是继承自MonoBehaviour的类型。</typeparam>
    public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 存储单例实例
        private static T instance;

        /// <summary>
        /// 静态公共属性，用于获取单例实例（延迟加载）。
        /// </summary>
        public static T Instance 
        {
            get
            {
                if (instance == null)
                {
                    // 尝试在场景中查找现有实例
                    instance = FindObjectOfType<T>();
                    
                    if (instance == null)
                    {
                        // 如果没有，则创建新的游戏对象并附加该组件
                        GameObject gameObject = new GameObject(typeof(T).Name); 
                        instance = gameObject.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            // 将当前对象设置为单例实例
            if (instance == null)
            {
                instance = this as T;
                return;
            }
            
            // 如果场景中已存在其他实例，则销毁当前重复的对象
            if (instance != this as T)
            {
                Destroy(gameObject);
            }
        }
        
        //清除静态实例，以防止在特定情况下出现“幽灵”实例。
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}