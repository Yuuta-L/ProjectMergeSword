//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2026-11-07 14:42:26.141
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BurnicleFramework.Runtime;
using UnityEngine;

namespace BurnicleFramework.Generated
{
    public static class ConfigExtension
    {
        #region 公共解析方法

        private static T[] ParseArray<T>(string rawValue, Func<string, T> parser)
        {
            if (string.IsNullOrEmpty(rawValue))
                return new T[0];

            string[] items = rawValue.Split(',');
            T[] result = new T[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                result[i] = parser(items[i].Trim());
            }

            return result;
        }
        
        private static Vector2 ParseVector2(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return Vector2.zero;

            string[] items = rawValue.Split('-');
            var item1 = float.Parse(items[0]);
            if (items.Length >= 2)
            {
                return new Vector2(item1, float.Parse(items[1]));
            }
            return new Vector2(item1, item1);
        }
        
        private static Vector2Int ParseVector2Int(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return Vector2Int.zero;

            string[] items = rawValue.Split('-');
            var item1 = int.Parse(items[0]);
            if (items.Length >= 2)
            {
                return new Vector2Int(item1, int.Parse(items[1]));
            }
            return new Vector2Int(item1, item1);
        }
        
        private static Vector3 ParseVector3(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return Vector3.zero;

            string[] items = rawValue.Split('-');
            var item1 = float.Parse(items[0]);
            if (items.Length >= 3)
            {
                return new Vector3(item1, float.Parse(items[1]), float.Parse(items[2]));
            }
            return new Vector3(item1, item1, item1);
        }
        
        private static Vector3Int ParseVector3Int(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return Vector3Int.zero;

            string[] items = rawValue.Split('-');
            var item1 = int.Parse(items[0]);
            if (items.Length >= 3)
            {
                return new Vector3Int(item1, int.Parse(items[1]), int.Parse(items[2]));
            }
            return new Vector3Int(item1, item1, item1);
        }
        
        private static Vector4 ParseVector4(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return Vector4.zero;

            string[] items = rawValue.Split('-');
            var x = int.Parse(items[0]);
            var y = int.Parse(items[1]);
            var z = int.Parse(items[2]);
            var w = int.Parse(items[3]);
            return new Vector4(x, y, z, w);
        }
        
        private static Vector4Int ParseVector4Int(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
                return new Vector4Int(0, 0, 0, 0);

            string[] items = rawValue.Split('-');
            return new Vector4Int(
                int.Parse(items[0]),
                int.Parse(items[1]),
                int.Parse(items[2]),
                int.Parse(items[3]));
        }

        private static Dictionary<TKey, TValue> ParseDictionary<TKey, TValue>(string rawValue, Func<string, TKey> keyParser, Func<string, TValue> valueParser)
        {
            if (string.IsNullOrEmpty(rawValue))
                return new Dictionary<TKey, TValue>();

            string[] pairs = rawValue.Split(',');
            var dict = new Dictionary<TKey, TValue>(pairs.Length);

            foreach (string pair in pairs)
            {
                string trimmedPair = pair.Trim();
                if (string.IsNullOrEmpty(trimmedPair))
                    continue;

                string[] keyValue = trimmedPair.Split('|');
                if (keyValue.Length != 2)
                    continue;

                TKey key = keyParser(keyValue[0].Trim());
                TValue value = valueParser(keyValue[1].Trim());
                dict[key] = value;
            }

            return dict;
        }

        #endregion

        #region 公式解析辅助方法

        /// <summary>
        /// 提取公式中的参数
        /// </summary>
        private static List<string> ExtractFormulaParameters(string formula)
        {
            var parameters = new List<string>();
            if (string.IsNullOrEmpty(formula))
                return parameters;

            // 匹配公式中的参数（单个小写字母）
            var matches = Regex.Matches(formula, @"\b[a-z]\b");
            foreach (Match match in matches)
            {
                string param = match.Value;
                if (!parameters.Contains(param))
                    parameters.Add(param);
            }

            return parameters;
        }

        /// <summary>
        /// 缓存已解析的表达式，提高性能
        /// </summary>
        private static readonly Dictionary<string, MathExpression> _expressionCache = new Dictionary<string, MathExpression>();

        /// <summary>
        /// 执行公式计算（使用参数赋值方式）
        /// </summary>
        private static float EvaluateFormula(string formula, Dictionary<string, float> parameters)
        {
            if (string.IsNullOrEmpty(formula))
                return 0f;

            // 尝试从缓存获取
            if (!_expressionCache.TryGetValue(formula, out var expression))
            {
                expression = MathExpressionHelper.TryEvaluate(formula);
                if (expression != null)
                {
                    _expressionCache[formula] = expression;
                }
                else
                {
                    return 0f;
                }
            }

            // 为表达式参数赋值
            foreach (var param in parameters)
            {
                if (expression.Parameters.ContainsKey(param.Key))
                {
                    expression.Parameters[param.Key].Value = param.Value;
                }
                else
                {
                    // 如果公式中不包含该参数，可能是动态生成的表达式
                    // 重新解析并添加参数
                    _expressionCache.Remove(formula);
                    return EvaluateFormula(formula, parameters);
                }
            }

            return (float)expression.Value;
        }
        
        #endregion

        #region 配置常量定义
        private const string DefaultEnergyMaxKey = "DefaultEnergyMax";
        private const string RecoverTimePerEnergyKey = "RecoverTimePerEnergy";
        #endregion

        #region 配置获取方法
        /// <summary>
        /// 获取配置：DefaultEnergyMax
        /// 原始值：40
        /// </summary>
        public static int GetDefaultEnergyMax(this ConfigComponent component)
        {
            return component.GetInt(DefaultEnergyMaxKey);
        }

        /// <summary>
        /// 获取配置：RecoverTimePerEnergy
        /// 原始值：900
        /// </summary>
        public static int GetRecoverTimePerEnergy(this ConfigComponent component)
        {
            return component.GetInt(RecoverTimePerEnergyKey);
        }

        #endregion

    }
}