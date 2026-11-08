using System;
using System.Collections.Generic;
using BurnicleFramework.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MergeSword
{
    /// <summary>
    /// 养成数据
    /// </summary>
    public class ProgressionData : PersistentData
    {
        /// <summary>
        /// 正在探索的关卡编号（未通过的）
        /// </summary>
        public int CurrentLevelIndex = 1;
        /// <summary>
        /// 最远到达的房间数
        /// </summary>
        public int LatestRoomIndex;
    }
}

/// <summary>
/// 安全反序列化 Dictionary{Enum, TValue} 的转换器。
/// 当 JSON 中出现无法解析的 enum key 时，跳过该条目而非抛异常，
/// 防止整个数据类反序列化失败导致存档被重置。
/// </summary>
public class SafeEnumDictConverter<TEnum, TValue> : JsonConverter<Dictionary<TEnum, TValue>>
    where TEnum : struct
{
    public override Dictionary<TEnum, TValue> ReadJson(JsonReader reader, Type objectType,
        Dictionary<TEnum, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var dict = new Dictionary<TEnum, TValue>();
        var obj = JObject.Load(reader);

        foreach (var prop in obj.Properties())
        {
            try
            {
                var key = (TEnum)System.Enum.Parse(typeof(TEnum), prop.Name, true);
                var value = prop.Value.ToObject<TValue>(serializer);
                dict[key] = value;
            }
            catch
            {
                // 跳过无法解析的条目（如已删除的 enum 值或格式异常），不阻塞整个反序列化
                UnityEngine.Debug.LogWarning(
                    $"[SafeEnumDictConverter] 跳过无法解析的 enum key '{prop.Name}' (类型 {typeof(TEnum).Name})，该条目数据将被忽略");
                continue;
            }
        }

        return dict;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<TEnum, TValue> value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            serializer.Serialize(writer, kvp.Value);
        }

        writer.WriteEndObject();
    }
}