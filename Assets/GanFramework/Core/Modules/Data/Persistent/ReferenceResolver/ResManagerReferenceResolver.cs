using GanFramework.Core.ResLoad;
using GanFramework.Odin.OdinSerializer;
using UnityEngine;

namespace GanFramework.Core.Data.Persistent.ReferenceResolver
{
    /// <summary>
    /// 使用 ResManager 的外部引用解析器
    /// 支持：
    /// id → 资源路径 → ResManager → 对象
    /// </summary>
    public class ResManagerReferenceResolver : IExternalStringReferenceResolver
    {
        public IExternalStringReferenceResolver NextResolver { get; set; }

        /// <summary>
        /// 对象 → ID（存档时）
        /// </summary>
        public bool CanReference(object value, out string id)
        {
            id = null;

            if (value == null)
                return false;

            // 只处理 UnityEngine.Object
            if (value is Object obj)
            {
                /*
                 * 推荐你项目统一约定：
                 * 所有可存档资源都必须实现接口
                 * 或挂一个组件提供ID
                 */

                if (obj is IResGuid guidObj)
                {
                    id = guidObj.Guid;
                    return true;
                }
            }

            return NextResolver?.CanReference(value, out id) ?? false;
        }

        /// <summary>
        /// ID → 对象（读档时）
        /// </summary>
        public bool TryResolveReference(string id, out object value)
        {
            value = null;

            if (string.IsNullOrEmpty(id))
                return false;

            // 直接交给 ResManager
            var obj = ResManager.Instance.Load<Object>(id);

            if (obj != null)
            {
                value = obj;
                return true;
            }

            return NextResolver?.TryResolveReference(id, out value) ?? false;
        }
    }

}