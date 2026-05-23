using System;
using UnityEngine;
using System.Collections.Generic;
using GanFramework.Core.Data.Persistent;

namespace GanFramework.Editor.Data
{
    // 测试用的保存数据类，标记了 [SaveClass] 和 [SaveMember] 属性以供 SaveStore 使用
    [SaveClass("testdata")]
    [Serializable]
    public class TestSaveData
    {
        [SaveMember]
        public int Level = 1;

        [SaveMember] 
        public string PlayerName = "Player";
        
        // 也可以是属性，前提是标记了 [SaveMember] 属性，使用SaveMember方法保存
        // public string PlayerName { get; set; }

        [SaveMember("hp")]
        public float Health = 100f;
        
        // 私有字段也可以保存，前提是标记了 [SaveMember] 属性，使用SaveMember方法保存
        // private float health = 100f;
        
        // 支持 Unity 内置类型
        [SaveMember]
        public Vector3 position = new Vector3(1f, 2f, 3f);
        
        [SaveMember]
        public Color color = new Color(1f, 0f, 0f, 0.5f);

        // 字典类型测试
        [SaveMember]
        public Dictionary<string, int> DictStringInt = new Dictionary<string, int>()
        {
            {"one", 1},
            {"two", 2}
        };

        [SaveMember]
        public Dictionary<int, string> DictIntString = new Dictionary<int, string>()
        {
            {1, "one"},
            {2, "two"}
        };

        public override string ToString()
        {
            int sCount = DictStringInt != null ? DictStringInt.Count : 0;
            int iCount = DictIntString != null ? DictIntString.Count : 0;
            return $"Level={Level}, PlayerName={PlayerName}, Health={Health}, Position={position}, Color={color}, DictStringInt.Count={sCount}, DictIntString.Count={iCount}";
        }
    }
}

