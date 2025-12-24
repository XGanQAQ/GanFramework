// ===============================================
// 自动生成文件（请勿手动修改）
// 生成时间：2025-12-24 16:03:45
// 源数据文件：TestData.xlsx
// ===============================================

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Data.AutoGen.DataClass
{
    /// <summary>
    /// 自动生成的数据结构类
    /// 来自：TestData.xlsx
    /// </summary>
    [Serializable]
    public class TestData
    {
        /// <summary>唯一ID</summary>
        public int id;

        /// <summary>名称</summary>
        public string name;

        /// <summary>伤害数组</summary>
        public int[] damage;

        /// <summary>标签</summary>
        public string[] tags;

        /// <summary>位置</summary>
        public Vector3 pos;

        /// <summary>颜色</summary>
        public Color color;

        /// <summary>属性表</summary>
        public Dictionary<string,int> stats;

        /// <summary>坐标表</summary>
        public Dictionary<string,Vector3> extra;

        /// <summary>ID列表</summary>
        public List<int> items;

        /// <summary>字典数组</summary>
        public Dictionary<string,List<int>> dictList;
    }
}