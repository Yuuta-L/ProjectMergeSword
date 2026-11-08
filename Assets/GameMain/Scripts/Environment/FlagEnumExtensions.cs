using System;
using System.Collections.Generic;

public static class FlagEnumRefExtensions
{

    /// <summary>
    /// 添加多个标志（原地修改）
    /// </summary>
    public static void Add<T>(this ref T value, T flags) where T : struct, Enum
    {
        var result = Convert.ToInt32(value);
        result |= Convert.ToInt32(flags);
        value = (T)Enum.ToObject(typeof(T), result);
    }

    /// <summary>
    /// 移除一个标志（原地修改）
    /// </summary>
    public static void Remove<T>(this ref T value, T flag) where T : struct, Enum
    {
        var result = Convert.ToInt32(value);
        result &= ~Convert.ToInt32(flag);
        value = (T)Enum.ToObject(typeof(T), result);
    }


    /// <summary>
    /// 切换标志状态（如果存在则移除，不存在则添加，原地修改）
    /// </summary>
    public static void Toggle<T>(this ref T value, T flag) where T : struct, Enum
    {
        if (value.HasFlag(flag))
            value.Remove(flag);
        else
            value.Add(flag);
    }

    /// <summary>
    /// 反转所有标志（原地修改）
    /// </summary>
    public static void Invert<T>(this ref T value) where T : struct, Enum
    {
        var result = Convert.ToInt32(value);
        var mask = 0;

        // 获取所有有效标志位的掩码
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToInt32(flag);
            if (flagValue != 0)
            {
                mask |= flagValue;
            }
        }

        // 反转掩码内的位
        result ^= mask;
        value = (T)Enum.ToObject(typeof(T), result);
    }

    /// <summary>
    /// 清空所有标志（原地修改）
    /// </summary>
    public static void Clear<T>(this ref T value) where T : struct, Enum
    {
        value = (T)Enum.ToObject(typeof(T), 0);
    }

    /// <summary>
    /// 检查枚举值是否包含目标中任意一个指定的标志
    /// </summary>
    public static bool Any<T>(this ref T value, T flags) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);
        var flagsInt = Convert.ToInt32(flags);

        return (valueInt & flagsInt) != 0;
    }


    /// <summary>
    /// 检查枚举值是否完全包含目标所有指定标志
    /// </summary>
    public static bool All<T>(this ref T value, T flags) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);
        var flagsInt = Convert.ToInt32(flags);

        return (valueInt & flagsInt) == flagsInt;
    }

    /// <summary>
    /// 检查两个枚举值是否包含完全相同的标志集
    /// </summary>
    public static bool Equals<T>(this ref T value, T other) where T : struct, Enum
    {
        return Convert.ToInt32(value) == Convert.ToInt32(other);
    }

    /// <summary>
    /// 检查枚举值是否为空（没有任何标志被设置）
    /// </summary>
    public static bool IsEmpty<T>(this ref T value) where T : struct, Enum
    {
        return Convert.ToInt32(value) == 0;
    }

    /// <summary>
    /// 检查是否是单个标志（只设置了一个标志）
    /// </summary>
    public static bool IsSingle<T>(this ref T value) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);
        return valueInt != 0 && (valueInt & (valueInt - 1)) == 0;
    }

    /// <summary>
    /// 将枚举拆分为单个标志的列表
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要拆分的枚举值</param>
    /// <returns>包含所有单个标志的列表</returns>
    public static List<T> ToFlagList<T>(this ref T value) where T : struct, Enum
    {
        var result = new List<T>();
        var valueInt = Convert.ToInt32(value);

        if (valueInt == 0)
        {
            // 如果是0，返回空列表（或者可以根据需求返回包含None的列表）
            return result;
        }

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToInt32(flag);
            if (flagValue != 0 && (valueInt & flagValue) == flagValue)
            {
                result.Add(flag);
            }
        }

        return result;
    }

    /// <summary>
    /// 将枚举拆分为单个标志的数组
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要拆分的枚举值</param>
    /// <returns>包含所有单个标志的数组</returns>
    public static T[] ToFlagArray<T>(this ref T value) where T : struct, Enum
    {
        return value.ToFlagList().ToArray();
    }

    /// <summary>
    /// 遍历枚举中的每个单个标志
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要遍历的枚举值</param>
    /// <param name="action">对每个单个标志执行的操作</param>
    public static void ForEach<T>(this ref T value, Action<T> action) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);

        if (valueInt == 0)
            return;

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToInt32(flag);
            if (flagValue != 0 && (valueInt & flagValue) == flagValue)
            {
                action(flag);
            }
        }
    }

    /// <summary>
    /// 获取枚举中单个标志的数量
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要计数的枚举值</param>
    /// <returns>单个标志的数量</returns>
    public static int Count<T>(this ref T value) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);
        var count = 0;

        if (valueInt == 0)
            return 0;

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToInt32(flag);
            if (flagValue != 0 && (valueInt & flagValue) == flagValue)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 获取枚举中所有单个标志的名称
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="value">要拆分的枚举值</param>
    /// <returns>标志名称的数组</returns>
    public static string[] GetFlagNames<T>(this ref T value) where T : struct, Enum
    {
        var valueInt = Convert.ToInt32(value);
        var names = new List<string>();

        if (valueInt == 0)
            return new[] { Enum.GetName(typeof(T), 0) ?? "None" };

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToInt32(flag);
            if (flagValue != 0 && (valueInt & flagValue) == flagValue)
            {
                names.Add(flag.ToString());
            }
        }

        return names.ToArray();
    }
}