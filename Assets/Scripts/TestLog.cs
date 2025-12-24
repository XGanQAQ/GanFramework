using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GanFramework.Core.Utils;

public class TestLog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LogUtil.Info("TestLog", "This is a test log message.");
        LogUtil.Error("TestLog", "This is a test error message.");
        LogUtil.Success("TestLog", "This is a test success message.");
        LogUtil.Warn("TestLog", "This is a test warning message.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
