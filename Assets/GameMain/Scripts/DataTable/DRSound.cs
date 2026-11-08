//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-01-07 14:37:40.467
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BurnicleFramework.Runtime;


namespace BurnicleFramework.Generated
{
    /// <summary>
    /// Sound表。
    /// </summary>
    public class DRSound : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取优先级（默认0，128最高，-128最低）。
        /// </summary>
        public int Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否循环。
        /// </summary>
        public bool Loop
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取音量（0~1）。
        /// </summary>
        public float Volume
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableParser.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableParser.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
			AssetName = columnStrings[index++];
			int.TryParse(columnStrings[index++],out var tempPriority); Priority = tempPriority;
			bool.TryParse(columnStrings[index++],out var tempLoop); Loop = tempLoop;
			float.TryParse(columnStrings[index++],out var tempVolume); Volume = tempVolume;
            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    AssetName = binaryReader.ReadString();
                    Priority = binaryReader.Read7BitEncodedInt32();
                    Loop = binaryReader.ReadBoolean();
                    Volume = binaryReader.ReadSingle();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
