using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MergeSword
{
    public static class MathUtility
    {
        /// <summary>
        /// 随机分配指定数量项到容器中,
        /// 例如将10分配到3个容器，总和总是10
        /// </summary>
        /// <param name="total"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] RandomAllotValues(int total, int count)
        {
            var result = new int[count];
            for (int i = 0; i < total; i++)
            {
                var index = Random.Range(0, count);
                result[index] += 1;
            }

            return result;
        }

        /// <summary>使用指定 Random 实例</summary>
        public static int[] RandomAllotValues(int total, int count, System.Random rng)
        {
            var result = new int[count];
            for (int i = 0; i < total; i++)
            {
                var index = rng.Next(0, count);
                result[index] += 1;
            }

            return result;
        }
        
        /// <summary>
        /// 随机分配指定数量项到容器中,
        /// 例如将10分配到3个容器，总和总是10，先保证平均分配
        /// </summary>
        /// <param name="total"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] RandomAllotValuesEvenly(int total, int count)
        {
            if (count <= 0) return new int[0];
            if (total <= 0) return new int[count];

            int avg = total / count;
            int remainder = total % count;

            var result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = avg;

            // 创建索引数组并随机打乱
            var indices = new int[count];
            for (int i = 0; i < count; i++)
                indices[i] = i;
            indices.Shuffle();
    
            // 前 remainder 个各加 1
            for (int i = 0; i < remainder; i++)
            {
                result[indices[i]]++;
            }
    
            return result;
        }
        
        /// <summary>
        /// m个下标中取出n个
        /// </summary>
        public static int[] RandomSelect(int m, int n)
        {
            if (n > m)
                throw new ArgumentException("m cannot be greater than n");
            if (m < 0)
                throw new ArgumentException("n cannot be negative");
            if (n < 0)
                throw new ArgumentException("m cannot be negative");
    
            // 创建包含 0 到 n-1 的整数列表
            var indices = new List<int>();
            for (var i = 0; i < m; i++)
            {
                indices.Add(i);
            }

            return indices.RandomPick(n);
        }
        
        /// <summary>
        /// 去除整数的第一位
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int RemoveFirstDigit(int number)
        {
            // 将整数转换为字符串
            string numberString = number.ToString();

            // 如果数字只有一位，则返回0
            if (numberString.Length == 1)
            {
                return 0;
            }

            // 去除字符串的第一位，并将其转换回整数
            string resultString = numberString.Substring(1);
            int result = int.Parse(resultString);

            return result;
        }
        
        
        public struct Weight
        {
            public int Flag;
            public int Value;

            public Weight(int flag, int value)
            {
                Flag = flag;
                Value = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// 根据权重随机选取一个（使用指定 Random 实例）
        /// </summary>
        public static int RandomPickOneByWeight(Dictionary<int,int> items, System.Random random)
        {
            var weights = new List<Weight>();
            foreach (var (key, value) in items)
            {
                weights.Add(new Weight(key, value));
            }

            return RandomPickOneByWeight(weights, random);
        }

        /// <summary>
        /// 根据权重随机选取一个
        /// </summary>
        public static int RandomPickOneByWeight(IList<Vector2Int> items)
        {
            var weights = new List<Weight>();
            foreach (var item in items)
            {
                weights.Add(new Weight(item.x, item.y));
            }

            return RandomPickOneByWeight(weights);
        }

        /// <summary>使用指定 Random 实例</summary>
        public static int RandomPickOneByWeight(IList<Vector2Int> items, System.Random random)
        {
            var weights = new List<Weight>();
            foreach (var item in items)
            {
                weights.Add(new Weight(item.x, item.y));
            }

            return RandomPickOneByWeight(weights, random);
        }

        /// <summary>
        /// 根据权重随机选取一个
        /// </summary>
        public static int RandomPickOneByWeight(IList<Vector3Int> items)
        {
            var weights = new List<Weight>();
            foreach (var item in items)
            {
                weights.Add(new Weight(item.x, item.y));
            }

            return RandomPickOneByWeight(weights);
        }

        /// <summary>使用指定 Random 实例</summary>
        public static int RandomPickOneByWeight(IList<Vector3Int> items, System.Random random)
        {
            var weights = new List<Weight>();
            foreach (var item in items)
            {
                weights.Add(new Weight(item.x, item.y));
            }

            return RandomPickOneByWeight(weights, random);
        }
        
        /// <summary>
        /// 根据权重随机选取一个
        /// </summary>
        public static int RandomPickOneByWeight(IList<Weight> weights)
        {
            var index = Pick(weights);
            return weights[index].Flag;
        }

        /// <summary>
        /// 根据权重随机选取一个（使用指定 Random 实例）
        /// </summary>
        public static int RandomPickOneByWeight(IList<Weight> weights, System.Random random)
        {
            var index = Pick(weights, random);
            return weights[index].Flag;
        }
        
        /// <summary>
        /// 根据权重随机选取一个
        /// </summary>
        public static int[] RandomPickByWeight(Dictionary<int,int> items, int count)
        {
            var weights = new List<Weight>();
            foreach (var (key, value) in items)
            {
                weights.Add(new Weight(key, value));
            }

            return RandomPickByWeight(weights, count);
        }

        /// <summary>
        /// 根据权重随机选取一个（使用指定 Random 实例）
        /// </summary>
        public static int[] RandomPickByWeight(Dictionary<int,int> items, int count, System.Random random)
        {
            var weights = new List<Weight>();
            foreach (var (key, value) in items)
            {
                weights.Add(new Weight(key, value));
            }

            return RandomPickByWeight(weights, count, random);
        }
        
        /// <summary>
        /// 根据权重随机选取一个
        /// </summary>
        public static int[] RandomPickByWeight(IList<Vector2Int> items, int count)
        {
            var weights = new List<Weight>();
            foreach (var item in items)
            {
                weights.Add(new Weight(item.x, item.y));
            }

            return RandomPickByWeight(weights, count);
        }
        
        /// <summary>
        /// 根据权重随机选取(不重复)
        /// </summary>
        public static int[] RandomPickByWeight(IList<Weight> weights, int count)
        {
            count = Mathf.Min(count, weights.Count);
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                var index = Pick(weights);
                var target = weights[index];
                result[i] = target.Flag;
                weights[index] = new Weight(target.Flag, 0);
            }

            return result;
        }

        /// <summary>
        /// 根据权重随机选取(不重复，使用指定 Random 实例)
        /// </summary>
        public static int[] RandomPickByWeight(IList<Weight> weights, int count, System.Random random)
        {
            count = Mathf.Min(count, weights.Count);
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                var index = Pick(weights, random);
                var target = weights[index];
                result[i] = target.Flag;
                weights[index] = new Weight(target.Flag, 0);
            }

            return result;
        }
        
        /// <summary>
        /// 根据权重随机选取一个,返回元素在数组中的下标
        /// </summary>
        public static int Pick(IList<Weight> items)
        {
            if (items == null || items.Count <= 0)
            {
                return 0;
            }

            var totalWeight = items.Sum(item => item.Value);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            for (var i = 0; i < items.Count; i++)
            {
                var value = items[i].Value;
                //0权重被跳过
                if (value <= 0)
                {
                    continue;
                }

                currentWeight += value;
                if (randomValue < currentWeight)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 根据权重随机选取一个,返回元素在数组中的下标（使用指定 Random 实例）
        /// </summary>
        public static int Pick(IList<Weight> items, System.Random random)
        {
            if (items == null || items.Count <= 0)
            {
                return 0;
            }

            var totalWeight = items.Sum(item => item.Value);
            int randomValue = random.Next(0, totalWeight);
            int currentWeight = 0;

            for (var i = 0; i < items.Count; i++)
            {
                var value = items[i].Value;
                //0权重被跳过
                if (value <= 0)
                {
                    continue;
                }

                currentWeight += value;
                if (randomValue < currentWeight)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}