using System;
using System.Collections.Generic;
using GanFramework.Odin.OdinSerializer;
using UnityEditor;
using UnityEngine;

namespace GanFramework.Core.Data.Persistent.ReferenceResolver
{
    public class ReferenceResolverManager
    {
        public readonly List<IExternalStringReferenceResolver> StringResolvers = new List<IExternalStringReferenceResolver>();
        public readonly List<IExternalGuidReferenceResolver> GuidResolvers = new List<IExternalGuidReferenceResolver>();
        public IExternalIndexReferenceResolver IndexResolver;

        public void AddStringResolver(IExternalStringReferenceResolver resolver)
        {
            if (StringResolvers.Count > 0)
            {
                StringResolvers[StringResolvers.Count - 1].NextResolver = resolver;
            }
            StringResolvers.Add(resolver);
        }

        public void AddGuidResolver(IExternalGuidReferenceResolver resolver)
        {
            if (GuidResolvers.Count > 0)
            {
                GuidResolvers[GuidResolvers.Count - 1].NextResolver = resolver;
            }
            GuidResolvers.Add(resolver);
        }
        
        public void SetIndexResolver(IExternalIndexReferenceResolver resolver)
        {
            IndexResolver = resolver;
        }
    }
}