using System.Collections.Generic;
using System.Linq;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{
    public static partial class PersistentDataExtension
    {
        #region Base
        public static void CheckDailyTime(this PersistentComponent component)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            
        }
        
        private static void CheckDailyTime(BaseData baseData)
        {
            if (baseData.LastDailyRefreshTime > Burnicle.DateTime.UtcTimeStamp)
            {
                baseData.LastDailyRefreshTime = Burnicle.DateTime.UtcTimeStamp;
            }
            
            //转为本地时间对比
            var lastDataTime = baseData.LastDailyRefreshTime.ToLocalDateTime();
            var currentDataTime = Burnicle.DateTime.LocalDateTime;
            if (currentDataTime.TodayBeginning() > lastDataTime.TodayBeginning())
            {
                baseData.DailyBoolValues.Clear();
                baseData.DailyIntValues.Clear();
                baseData.LastDailyRefreshTime = Burnicle.DateTime.UtcTimeStamp;
                baseData.Save();
            }
        }
        
        /// <summary>
        /// 获取每日自动重置的flag
        /// </summary>
        /// <returns></returns>
        public static bool GetDailyFlag(this PersistentComponent component, string key)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            baseData.DailyBoolValues.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>
        /// 设置每日自动重置的flag
        /// </summary>
        /// <returns></returns>
        public static void SetDailyFlag(this PersistentComponent component, string key, bool value = true)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            baseData.DailyBoolValues[key] = value;
            baseData.Save();
        }
        
        /// <summary>
        /// 获取每日自动重置的times
        /// </summary>
        /// <returns></returns>
        public static int GetDailyTimes(this PersistentComponent component, string key)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            baseData.DailyIntValues.TryGetValue(key, out var result);
            return result;
        }
        
        /// <summary>
        /// 增加每日自动重置的times
        /// </summary>
        /// <returns></returns>
        public static void AddDailyTimes(this PersistentComponent component, string key, int value = 1)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            baseData.DailyIntValues.TryGetValue(key, out var result);
            baseData.DailyIntValues[key] = result + value;
            baseData.Save();
            Burnicle.Event.Fire(null,DailyTimesEventArgs.Create(key,result + value));
        }
        
        /// <summary>
        /// 设置每日自动重置的times
        /// </summary>
        /// <returns></returns>
        public static void SetDailyTimes(this PersistentComponent component, string key, int value ,bool compare = false)
        {
            var baseData = component.GetData<BaseData>();
            CheckDailyTime(baseData);
            baseData.DailyIntValues.TryGetValue(key, out var result);
            if (compare && result >= value)
            {
                return;
            }
            baseData.DailyIntValues[key] = value;
            baseData.Save();
        }
        
        /// <summary>
        /// 获取永久生效的flag
        /// </summary>
        /// <returns></returns>
        public static bool GetConstFlag(this PersistentComponent component, string key)
        {
            var baseData = component.GetData<BaseData>();
            baseData.ConstBoolValues.TryGetValue(key, out var result);
            return result;
        }
        
        /// <summary>
        /// 设置永久生效的flag
        /// </summary>
        /// <returns></returns>
        public static void SetConstFlag(this PersistentComponent component, string key, bool value = true)
        {
            var baseData = component.GetData<BaseData>();
            baseData.ConstBoolValues[key] = value;
            baseData.Save();
        }
        
        /// <summary>
        /// 获取永久生效的times
        /// </summary>
        /// <returns></returns>
        public static int GetConstTimes(this PersistentComponent component, string key)
        {
            var baseData = component.GetData<BaseData>();
            baseData.ConstIntValues.TryGetValue(key, out var result);
            return result;
        }
        
        /// <summary>
        /// 增加永久生效的times
        /// </summary>
        /// <returns></returns>
        public static void AddConstTimes(this PersistentComponent component, string key, int value = 1)
        {
            var baseData = component.GetData<BaseData>();
            baseData.ConstIntValues.TryGetValue(key, out var result);
            baseData.ConstIntValues[key] = result + value;
            baseData.Save();
        }
        
        /// <summary>
        /// 设置永久生效的times
        /// </summary>
        /// <returns></returns>
        public static void SetConstTimes(this PersistentComponent component, string key, int value ,bool compare = false)
        {
            var baseData = component.GetData<BaseData>();
            baseData.ConstIntValues.TryGetValue(key, out var result);
            if (compare && result >= value)
            {
                return;
            }
            baseData.ConstIntValues[key] = value;
            baseData.Save();
        }

        #endregion
    }
}