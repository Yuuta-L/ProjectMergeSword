namespace MergeSword
{
    using System;
    using System.Collections.Generic;
    using GameFramework;

    /// <summary>
    /// 引用类型参数, 用于传递参数, 类似黑板
    /// </summary>
    public class RefParams : IReference
    {
        private VariableDic _variableDic;
        public VariableDic VariableDic {
            get
            {
                if (_variableDic == null)
                {
                    _variableDic = VariableDic.Create();
                }
                return _variableDic;
            } 
        }
    
        private int _serialId = 0;
        /// <summary>
        /// 序列号，自动获取
        /// </summary>
        public int Id {
            get
            {
                if (_serialId == 0)
                {
                    _serialId = GenerateSerialId();
                }
                return _serialId;
            } 
        }

        public static RefParams Create()
        {
            var refParams = ReferencePool.Acquire<RefParams>();
            return refParams;
        }

        public void Set<T>(int key ,T value)
        {
            VariableDic.Set(key, value);
        }
    
        /// <summary>
        /// 获取引用类型的参数
        /// </summary>
        public T Get<T>(int key, T defaultValue)
        {
            return VariableDic.Get(key, defaultValue);
        }
    
        public T Get<T>(int key)
        {
            return VariableDic.Get<T>(key);
        }
    
        public bool TryGet<T>(int key, out T value)
        {
            return VariableDic.TryGet(key, out value);
        }

        public virtual void Clear()
        {
            _serialId = 0;
            if (_variableDic != null)
            {
                ReferencePool.Release(_variableDic);
                _variableDic = null;
            }
        }

        private static int _serialIdCache;
        private static int GenerateSerialId()
        {
            return _serialIdCache++;
        }
    }
}
