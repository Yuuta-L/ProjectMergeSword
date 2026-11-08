using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{
    public static partial class PersistentDataExtension
    {
        /// <summary>
        /// 通过关卡
        /// </summary>
        public static void PassLevel(this PersistentComponent component,int selectLevelIndex)
        {
            var data = component.GetData<ProgressionData>();
            if (selectLevelIndex < data.CurrentLevelIndex)
            {
                return;
            }
            
            data.CurrentLevelIndex = selectLevelIndex + 1;
            data.LatestRoomIndex = 0;
            data.Save();
        }

        public static void PassRoom(this PersistentComponent component,int selectLevelIndex,int roomIndex)
        {
            var data = component.GetData<ProgressionData>();
            if (data.CurrentLevelIndex != selectLevelIndex)
            {
                return;
            }

            if (data.LatestRoomIndex <= roomIndex)
            {
                data.LatestRoomIndex = roomIndex;
                // 不立即保存，在游戏结束时随 PassLevel 一并存入
            }
        }

        public static int GetLatestLevelIndex(this PersistentComponent component)
        {
            var data = component.GetData<ProgressionData>();
            return Mathf.Max(1, data.CurrentLevelIndex);
        }
        
        public static int GetLatestRoomIndex(this PersistentComponent component)
        {
            var data = component.GetData<ProgressionData>();
            return data.LatestRoomIndex;
        }
    }
}