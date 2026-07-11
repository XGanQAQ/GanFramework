using UnityEngine;
using GanFramework.Core;
using GanFramework.Core.UI;

public class ViewerSceneOpenest : MonoBehaviour
{
    public string ViewerName = "MainMenuViewer";

    void Start()
    {
        var uiManager = Framework.GetModule<IUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[ViewerSceneOpenest]: IUIManager module is not registered.");
            return;
        }

        uiManager.OpenUI(ViewerName);
    }
}
