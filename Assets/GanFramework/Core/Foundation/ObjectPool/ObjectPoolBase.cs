using System.Collections.Generic;

namespace GanFramework.ObjectPool
{
    /// <summary>
    /// 对象池基类，适用于任意类型对象的池化管理。
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public abstract class ObjectPoolBase<T> where T : class
    {
        protected readonly Stack<T> pool = new Stack<T>();
        protected int maxSize = 100;

        /// <summary>
        /// 获取一个对象，如果池中没有则创建新对象。
        /// </summary>
        public T Get()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return CreateInstance();
        }

        /// <summary>
        /// 回收对象到池中。
        /// </summary>
        public void Release(T obj)
        {
            if (pool.Count < maxSize)
            {
                ResetInstance(obj);
                pool.Push(obj);
            }
            else
            {
                OnDestroyInstance(obj);
            }
        }

        /// <summary>
        /// 创建新对象实例，需由子类实现。
        /// </summary>
        protected abstract T CreateInstance();

        /// <summary>
        /// 重置对象状态，需由子类实现。
        /// </summary>
        protected abstract void ResetInstance(T obj);

        /// <summary>
        /// 销毁对象实例，需由子类实现。
        /// </summary>
        protected abstract void OnDestroyInstance(T obj);
    }
}
