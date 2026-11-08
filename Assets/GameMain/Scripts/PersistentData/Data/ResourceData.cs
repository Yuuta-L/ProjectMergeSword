using System.Collections.Generic;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    /// <summary>
    /// 资源存档
    /// </summary>
    public class ResourceData : PersistentData
    {
        /// <summary>
        /// 上次恢复体力的时间
        /// </summary>
        public long LastRecoverEnergyTimeStamp = 0;
        
        //已拥有的物品
        public Dictionary<EnumItem, int> OwnedItems;
    }
}