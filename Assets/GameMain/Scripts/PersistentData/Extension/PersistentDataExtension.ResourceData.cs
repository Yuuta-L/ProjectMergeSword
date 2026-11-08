using System.Collections.Generic;
using BurnicleFramework.Generated;
using UnityEngine;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    public static partial class PersistentDataExtension
    {
        public static bool AddItem(this PersistentComponent component, EnumItem itemType, int number)
        {
            if (number == 0)
            {
                return true;
            }
            
            if (itemType == EnumItem.None ||number <= 0)
            {
                Log.Warning("货币类型错误或值无效");
                return false;
            }

            if (InternalChangeItem(component, itemType, number, false))
            {
                component.SaveData<ResourceData>();
                Burnicle.Event.Fire(null, BatchWealthChangeEventArgs.Create(WealthTypes.Item, (int)itemType, number));
                return true;
            }
            
            return false;
        }
        
        public static bool SetItem(this PersistentComponent component, EnumItem itemType, int number)
        {           
            if (number == 0)
            {
                return true;
            }
            
            if (itemType == EnumItem.None ||number <= 0)
            {
                Log.Warning("货币类型错误或值无效");
                return false;
            }

            if (InternalChangeItem(component, itemType, number, true))
            {
                component.SaveData<ResourceData>();
                Burnicle.Event.Fire(null, BatchWealthChangeEventArgs.Create(WealthTypes.Item, (int)itemType, -number));
                return true;
            }
            return false;
        }

        public static bool RemoveItem(this PersistentComponent component, EnumItem itemType, int number)
        {
            if (number == 0)
            {
                return true;
            }
            
            if (itemType == EnumItem.None || number < 0)
            {
                Log.Warning("货币类型错误或值无效");
                return false;
            }

            if (InternalChangeItem(component, itemType, -number, false))
            {
                component.SaveData<ResourceData>();
                Burnicle.Event.Fire(null, BatchWealthChangeEventArgs.Create(WealthTypes.Item, (int)itemType, -number));
                return true;
            }
            return false;
        }

        public static int GetItem(this PersistentComponent component, EnumItem itemType)
        {
            var currencyData = component.GetData<ResourceData>();
            if (currencyData.OwnedItems == null)
            {
                return 0;
            }
            
            return currencyData.OwnedItems.GetValueOrDefault(itemType, 0);
        }

#region Energy
        /// <summary>
        /// 检查并恢复体力，上限为Constant.Config.HealthPointMax
        /// </summary>
        public static void CheckEnergy(this PersistentComponent component)
        {
            var data = component.GetData<ResourceData>();
            var energyMax = Burnicle.Config.GetDefaultEnergyMax();
            var recoverTimePerEnergy = Burnicle.Config.GetRecoverTimePerEnergy();
            var currentEnergy = GetItem(component, EnumItem.Energy);
            if (currentEnergy >= energyMax || data.LastRecoverEnergyTimeStamp > Burnicle.DateTime.UtcTimeStamp)
            {
                data.LastRecoverEnergyTimeStamp = Burnicle.DateTime.UtcTimeStamp;
                return;
            }
            
            var timeOffset = (int)(Burnicle.DateTime.UtcTimeStamp - data.LastRecoverEnergyTimeStamp);
            var recoverNumber = timeOffset / recoverTimePerEnergy;
            if (recoverNumber > 0)
            {
                var result = Mathf.Clamp(currentEnergy + recoverNumber, 0, energyMax);
                data.LastRecoverEnergyTimeStamp += recoverNumber * recoverTimePerEnergy;
                SetItem(component, EnumItem.Energy, result);
            }
        }
        
        public static bool IsFullEnergy(this PersistentComponent component)
        {
            CheckEnergy(component);
            var energyMax = Burnicle.Config.GetDefaultEnergyMax();
            return GetItem(component,EnumItem.Energy) >= energyMax;
        }
        
        /// <summary>
        /// 是否有足够的体力
        /// </summary>
        /// <param name="component"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool HasEnoughEnergy(this PersistentComponent component, int number)
        {
            CheckEnergy(component);
            return GetItem(component,EnumItem.Energy) >= number;
        }
#endregion

#region Internal
        private static bool InternalChangeItem(PersistentComponent component, EnumItem itemType, int number, bool isSet)
        {
            var currencyData = component.GetData<ResourceData>();
            if (currencyData.OwnedItems == null)
            {
                if (number < 0)
                {
                    return false;
                }
                
                currencyData.OwnedItems = new Dictionary<EnumItem, int>();
                currencyData.OwnedItems[itemType] = number;
                return true;
            }

            if (isSet)
            {
                if (number < 0)
                {
                    return false;
                }
                currencyData.OwnedItems[itemType] = number;
            }
            else
            {
                currencyData.OwnedItems.TryGetValue(itemType, out var ownedCount);
                var result = ownedCount + number;
                if (result < 0)
                {
                    return false;
                }
                currencyData.OwnedItems[itemType] = result;
            }

            return true;
        }
#endregion
    }
}