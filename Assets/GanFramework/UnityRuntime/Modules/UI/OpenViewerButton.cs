using System;
using UnityEngine;
using UnityEngine.UI;

namespace GanFramework.Runtime.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class OpenViewerButton : MonoBehaviour
    {
        [SerializeField] private ViewerBase TargetViewer;
        [SerializeField] private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(OpenTargetViewer);
            }
            else
            {
                Debug.LogWarning("OpenViewerButton: No Button component found.");
            }
        }

        public void OpenTargetViewer()
        {
            if (TargetViewer != null)
            {
                TargetViewer.Open();
            }
            else
            {
                Debug.LogWarning("OpenViewerButton: TargetViewer is not assigned.");
            }
        }
    }
}