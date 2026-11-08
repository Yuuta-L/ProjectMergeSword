//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改
// 生成时间：2026-11-07 13:48:17.043
//------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BurnicleFramework.Runtime;

namespace BurnicleFramework.Generated
{
	public static partial class DataTableParserExtension
	{
		public static float[] ParseSingleArray(this string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			float[] array = new float[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Single.Parse(splitValue[i]);
			}

			return array;
		}
		public static int[] ParseInt32Array(this string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			int[] array = new int[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Int32.Parse(splitValue[i]);
			}

			return array;
		}
		public static string[] ParseStringArray(this string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			string[] array = new string[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = splitValue[i];
			}

			return array;
		}
	}
}
