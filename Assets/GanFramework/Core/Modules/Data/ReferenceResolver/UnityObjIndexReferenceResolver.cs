using System.Collections.Generic;
using GanFramework.Odin.OdinSerializer;

namespace GanFramework.Core.Data.Persistent.ReferenceResolver
{
    public class UnityObjIndexReferenceResolver : IExternalIndexReferenceResolver
    {
        public List<UnityEngine.Object> UnityObjReferenceList;
        
        public bool CanReference(object value, out int index)
        {
            if (value is UnityEngine.Object obj)
            {
                index = this.UnityObjReferenceList.Count;
                this.UnityObjReferenceList.Add(obj);
            }

            index = 0;
            return false;
        }
        public bool TryResolveReference(int index, out object value)
        {
            value = this.UnityObjReferenceList[index];
            return true;
        }
    }
}