//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-05-08 09:24:35.031
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
    /// 界面可解锁项表。
    /// </summary>
    public class DRUnlockable : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取可解锁项编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取解锁项名称。
        /// </summary>
        public string ItemName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取显示图标资源名。
        /// </summary>
        public string IconAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取显示功能名。
        /// </summary>
        public string LocalizationKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取解锁方式。
        /// </summary>
        public string UnlockType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取解锁参数。
        /// </summary>
        public float[] UnlockValue
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
			ItemName = columnStrings[index++];
			IconAssetName = columnStrings[index++];
			LocalizationKey = columnStrings[index++];
			UnlockType = columnStrings[index++];
			UnlockValue = columnStrings[index++].ParseSingleArray();
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
                    ItemName = binaryReader.ReadString();
                    IconAssetName = binaryReader.ReadString();
                    LocalizationKey = binaryReader.ReadString();
                    UnlockType = binaryReader.ReadString();
					UnlockValue = binaryReader.ReadSingleArray();
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
