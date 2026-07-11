using UnityEngine;
using GanFramework.Modules.UI;
using System;
public class ViewerSceneOpenest : MonoBehaviour
{
    public string ViewerName = "MainMenuViewer";
    void Start()
    {
        UIManager.Instance.OpenUI(ViewerName);
    }
}
