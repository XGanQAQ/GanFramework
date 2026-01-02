using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GanFramework.Editor.Tools
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class AutoColliderFitter : MonoBehaviour
    {
        [Header("Collider 生成在哪个物体下（可空）")] [Tooltip("Collider 会挂在这个 GameObject 上。如果为空则挂在当前节点。")]
        public Transform colliderRoot;

        [Header("渲染器设置（可留空自动查找）")] public Renderer[] targetRenderers;

        [Header("Collider 设置")] public ColliderType colliderType = ColliderType.Auto;
        public Vector3 sizeMultiplier = Vector3.one;
        public Vector3 positionOffset = Vector3.zero;

        [Header("更新方式")] public bool applyOnAwake = true;
        public bool continuousUpdateInEditor = true;
        public bool updateInPlayMode = false;

        [Header("Gizmos")] public bool drawGizmos = true;

        private Collider attachedCollider;

        public enum ColliderType
        {
            Auto,
            Box,
            Sphere,
            Capsule
        }

        // -----------------------
        // Unity Callbacks
        // -----------------------

        private void Reset()
        {
            AutoAssignRenderers();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (targetRenderers == null || targetRenderers.Length == 0)
                AutoAssignRenderers();

            if (!Application.isPlaying && continuousUpdateInEditor)
                Apply();
#endif
        }

        private void Awake()
        {
            if (applyOnAwake)
                Apply();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (continuousUpdateInEditor)
                    Apply();
                return;
            }
#endif

            if (Application.isPlaying && updateInPlayMode)
                Apply();
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            if (TryCalculateCombinedBounds(out Bounds bounds))
            {
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        // -----------------------
        // Main Logic
        // -----------------------

        public void Apply()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                AutoAssignRenderers();

            if (!TryCalculateCombinedBounds(out Bounds bounds))
                return;

            // --- 确定挂在哪里 ---
            Transform root = colliderRoot != null ? colliderRoot : transform;

            attachedCollider = root.GetComponent<Collider>();
            if (attachedCollider == null)
            {
                var type = colliderType == ColliderType.Auto ? GuessColliderType() : colliderType;
                attachedCollider = type switch
                {
                    ColliderType.Box => root.gameObject.AddComponent<BoxCollider>(),
                    ColliderType.Sphere => root.gameObject.AddComponent<SphereCollider>(),
                    ColliderType.Capsule => root.gameObject.AddComponent<CapsuleCollider>(),
                    _ => root.gameObject.AddComponent<BoxCollider>()
                };
            }

            // --- world bounds 转 local bounds（基于 colliderRoot） ---
            Vector3 localCenter = root.InverseTransformPoint(bounds.center) + positionOffset;
            Vector3 localSize = root.InverseTransformVector(bounds.size);

            if (attachedCollider is BoxCollider box)
            {
                box.center = localCenter;
                box.size = Vector3.Scale(localSize, sizeMultiplier);
            }
            else if (attachedCollider is SphereCollider sphere)
            {
                float radiusWorld = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                float radiusLocal = root.InverseTransformVector(new Vector3(radiusWorld, 0, 0)).magnitude;
                sphere.center = localCenter;
                sphere.radius = radiusLocal * sizeMultiplier.x;
            }
            else if (attachedCollider is CapsuleCollider capsule)
            {
                capsule.center = localCenter;

                Vector3 ext = bounds.extents;
                int axis = GetLongestAxis(ext);
                capsule.direction = axis;

                float heightWorld = (axis == 0 ? ext.x : axis == 1 ? ext.y : ext.z) * 2f;
                float radiusWorld = axis switch
                {
                    0 => Mathf.Min(ext.y, ext.z),
                    1 => Mathf.Min(ext.x, ext.z),
                    _ => Mathf.Min(ext.x, ext.y)
                };

                float heightLocal = root.InverseTransformVector(new Vector3(heightWorld, 0, 0)).magnitude;
                float radiusLocal = root.InverseTransformVector(new Vector3(radiusWorld, 0, 0)).magnitude;

                capsule.height = heightLocal * sizeMultiplier.y;
                capsule.radius = radiusLocal * sizeMultiplier.x;
            }
        }

        // -----------------------
        // Helper Functions
        // -----------------------

        private bool TryCalculateCombinedBounds(out Bounds combined)
        {
            combined = new Bounds();
            var rs = GetEffectiveRenderers();
            if (rs.Length == 0) return false;

            bool init = false;
            foreach (var r in rs)
            {
                if (!init)
                {
                    combined = r.bounds;
                    init = true;
                }
                else
                    combined.Encapsulate(r.bounds);
            }

            return true;
        }

        private Renderer[] GetEffectiveRenderers()
        {
            if (targetRenderers != null && targetRenderers.Length > 0)
                return targetRenderers.Where(r => r != null).ToArray();

            Transform model = transform.Find("Model");
            if (model != null)
            {
                var rs = model.GetComponentsInChildren<Renderer>(true);
                if (rs.Length > 0) return rs;
            }

            return GetComponentsInChildren<Renderer>(true);
        }

        private void AutoAssignRenderers()
        {
            if (targetRenderers != null && targetRenderers.Length > 0) return;

            Transform model = transform.Find("Model");
            if (model != null)
            {
                var rs = model.GetComponentsInChildren<Renderer>(true);
                if (rs.Length > 0)
                {
                    targetRenderers = rs;
                    return;
                }
            }

            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private ColliderType GuessColliderType()
        {
            var rs = GetEffectiveRenderers();
            if (rs.Any(r => r is SkinnedMeshRenderer))
                return ColliderType.Capsule;

            return ColliderType.Box;
        }

        private int GetLongestAxis(Vector3 v)
        {
            if (v.x > v.y && v.x > v.z) return 0;
            if (v.y > v.z) return 1;
            return 2;
        }
    }
}