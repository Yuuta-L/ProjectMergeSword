using System;
using UnityEngine;

namespace MergeSword
{
    /// <summary>
    /// 可序列化的 Vector3 替代类型。仅包含 x/y/z 字段，无计算属性（normalized 等），
    /// 避免 Newtonsoft.Json 序列化时发生无限递归。
    /// </summary>
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(SerializableVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator SerializableVector3(Vector3 v) => new SerializableVector3(v.x, v.y, v.z);

        public override string ToString() => $"({x}, {y}, {z})";
    }
}
