using UnityEngine;

namespace GanFramework.Core.UI
{
    [RequireComponent(typeof(Camera))]
    public class UICamera : MonoBehaviour
    {
        public static UICamera Instance { get; private set; }
        private Camera uiCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            uiCamera = GetComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            uiCamera.orthographic = true;
            uiCamera.depth = 100; // 确保在主相机之后渲染
        }

        public Camera GetCamera()
        {
            return uiCamera;
        }
    }
}
