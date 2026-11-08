//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-01-07 14:37:40.473
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
    /// 界面配置表。
    /// </summary>
    public class DRUIForm : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取界面编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取界面名称。
        /// </summary>
        public string AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面位于组中深度。
        /// </summary>
        public int Depth
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否允许多个界面实例。
        /// </summary>
        public bool AllowMultiInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否暂停被其覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否显示屏幕遮罩。
        /// </summary>
        public bool ShowScreenDim
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否允许返回关闭。
        /// </summary>
        public bool AllowEscapeClose
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
			UIGroupName = columnStrings[index++];
			int.TryParse(columnStrings[index++],out var tempDepth); Depth = tempDepth;
			bool.TryParse(columnStrings[index++],out var tempAllowMultiInstance); AllowMultiInstance = tempAllowMultiInstance;
			bool.TryParse(columnStrings[index++],out var tempPauseCoveredUIForm); PauseCoveredUIForm = tempPauseCoveredUIForm;
			bool.TryParse(columnStrings[index++],out var tempShowScreenDim); ShowScreenDim = tempShowScreenDim;
			bool.TryParse(columnStrings[index++],out var tempAllowEscapeClose); AllowEscapeClose = tempAllowEscapeClose;
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
                    UIGroupName = binaryReader.ReadString();
                    Depth = binaryReader.Read7BitEncodedInt32();
                    AllowMultiInstance = binaryReader.ReadBoolean();
                    PauseCoveredUIForm = binaryReader.ReadBoolean();
                    ShowScreenDim = binaryReader.ReadBoolean();
                    AllowEscapeClose = binaryReader.ReadBoolean();
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
