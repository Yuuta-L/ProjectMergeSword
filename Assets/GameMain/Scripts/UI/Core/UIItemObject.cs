using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace MergeSword
{
    public sealed class UIItemObject : ObjectBase
    {
        public string PoolName { get; private set; }
        public UIItemBase ItemLogic { get; private set; }
        
        public static UIItemObject Create(string poolName, GameObject itemInstance)
        {
            var instance = ReferencePool.Acquire<UIItemObject>();
            instance.Initialize(itemInstance);
            instance.PoolName = poolName;
            instance.ItemLogic = itemInstance.GetComponent<UIItemBase>();
            instance.ItemLogic.SetContainer(instance);
            return instance;
        }
        
        protected override void Release(bool isShutdown)
        {
            if (ItemLogic == null)
            {
                return;
            }
            Object.Destroy(ItemLogic.gameObject);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (ItemLogic != null)
            {
                ItemLogic.OnSpawn();
            }
        }

        protected override void OnUnspawn()
        {
            base.OnUnspawn();
            if (ItemLogic != null)
            {
                ItemLogic.OnUnspawn();
            }
        }
    }
}
