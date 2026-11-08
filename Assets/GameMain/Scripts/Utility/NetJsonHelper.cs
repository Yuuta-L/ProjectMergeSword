using System;
using GameFramework;
using Newtonsoft.Json;
using Obfuz;

namespace MergeSword
{
    [ObfuzIgnore]
    public class NetJsonHelper : Utility.Json.IJsonHelper
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        
        public string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj,_settings);
        }

        public T ToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json,_settings);
        }

        public object ToObject(Type objectType, string json)
        {
            return JsonConvert.DeserializeObject(json, objectType,_settings);
        }
    }
}