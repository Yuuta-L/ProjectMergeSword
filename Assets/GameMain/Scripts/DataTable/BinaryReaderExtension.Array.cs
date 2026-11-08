//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改
// 生成时间：2026-11-07 13:48:17.049
//------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BurnicleFramework.Runtime;

namespace BurnicleFramework.Generated
{
	public static partial class BinaryReaderExtension
	{
		public static float[] ReadSingleArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			float[] array = new float[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadSingle();
			}
			return array;
		}
		public static int[] ReadInt32Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.Read7BitEncodedInt32();
			}
			return array;
		}
		public static string[] ReadStringArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadString();
			}
			return array;
		}
	}
}
