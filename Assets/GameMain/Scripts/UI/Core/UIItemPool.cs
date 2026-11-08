using System.Collections.Generic;
using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace MergeSword
{
    /// <summary>
    /// 由uiform演变出来的池，其实可以优化为通用GameObject池
    /// </summary>
    public class UIItemPool
    {
        private readonly string _poolName;
        
        private IList<IObjectPool<UIItemObject>> _itemPools = null;
        
        public UIItemPool(string poolName)
        {
            _poolName = poolName;
        }
        
        public void UnspawnAllItemObjects()
        {
            if (_itemPools == null) return;
            foreach (var item in _itemPools)
            {
                item.ReleaseAllUnused();

                item.UnspawnAll();
            }
        }

        public void DestroyAllItemPool()
        {
            if (_itemPools == null) return;

            for (int i = 0; i < _itemPools.Count; i++)
            {
                var item = _itemPools[i];
                Burnicle.ObjectPool.DestroyObjectPool(item);
            }

            _itemPools.Clear();
        }
        
        /// <summary>
        /// 从对象池获取一个Item (界面关闭时会自动Unspawn)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemTemple">Item实例化模板</param>
        /// <param name="instanceRoot">Item实例化到根节点</param>
        /// <param name="autoReleaseInterval"></param>
        /// <param name="capacity">对象池容量</param>
        /// <param name="expireTime">对象过期时间(过期后自动销毁)</param>
        /// <returns></returns>
        public T SpawnItem<T>(GameObject itemTemple, Transform instanceRoot, float autoReleaseInterval = 5f,
            int capacity = 50, float expireTime = 50) where T : UIItemBase
        {
            var itemTempleId = GetItemPoolId(itemTemple);
            IObjectPool<UIItemObject> pool;
            if (Burnicle.ObjectPool.HasObjectPool<UIItemObject>(itemTempleId))
            {
                pool = Burnicle.ObjectPool.GetObjectPool<UIItemObject>(itemTempleId);
                pool.AutoReleaseInterval = autoReleaseInterval;
                pool.Capacity = capacity;
                pool.ExpireTime = expireTime;
            }
            else
            {
                pool = Burnicle.ObjectPool.CreateSingleSpawnObjectPool<UIItemObject>(itemTempleId, autoReleaseInterval, capacity, expireTime, 0);
                if (_itemPools == null) _itemPools = new List<IObjectPool<UIItemObject>>();
                _itemPools.Add(pool);
            }

            var spawn = pool.Spawn();
            if (spawn == null)
            {
                var itemInstance = Object.Instantiate(itemTemple, instanceRoot);
                spawn = UIItemObject.Create(itemTempleId, itemInstance);
                pool.Register(spawn, true);
            }
            else
            {
                spawn.ItemLogic.SetParent(instanceRoot);
            }

            return (T)spawn.ItemLogic;
        }

        private string GetItemPoolId(GameObject itemTemple)
        {
            return Utility.Text.Format("{0}.{1}", _poolName, itemTemple.GetInstanceID());
        }
        
        /// <summary>
        /// 从对象池回收Item
        /// </summary>
        public void UnspawnItem<T>(T item) where T : UIItemBase
        {
            var itemTempleId = item.Container.PoolName;
            if (!Burnicle.ObjectPool.HasObjectPool<UIItemObject>(itemTempleId)) return;

            var pool = Burnicle.ObjectPool.GetObjectPool<UIItemObject>(itemTempleId);
            pool.Unspawn(item.Container); 
        }

        /// <summary>
        /// 回收所有item
        /// </summary>
        public void UnspawnAllItem(GameObject itemTemple)
        {
            var itemTempleId = GetItemPoolId(itemTemple);
            if (!Burnicle.ObjectPool.HasObjectPool<UIItemObject>(itemTempleId)) return;

            var pool = Burnicle.ObjectPool.GetObjectPool<UIItemObject>(itemTempleId);
            pool.ReleaseAllUnused();
            pool.UnspawnAll();
        }
    }
}