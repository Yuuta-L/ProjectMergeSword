using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Resource;
using BurnicleFramework.Runtime;
using Object = UnityEngine.Object;

namespace MergeSword
{
    /// <summary>
    /// 资源加载器（回调为真实 API，await 只是语法糖）
    /// - 自动缓存已加载资源
    /// - 同名资源并发只加载一次
    /// - await 与 callback 完全共用同一套逻辑
    /// </summary>
    public sealed class ResourceLoader
    {
        private readonly Dictionary<string, object> _assetCache = new();
        private readonly Dictionary<string, List<Action<object>>> _loading = new();
        private readonly LoadAssetCallbacks _loadAssetCallbacks;

        public ResourceLoader()
        {
            _loadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public void LoadAsset<T>(string assetName, Action<T> callback)
        {
            // ① 已缓存
            if (_assetCache.TryGetValue(assetName, out var cached))
            {
                callback?.Invoke((T)cached);
                return;
            }

            // ② 正在加载
            if (_loading.TryGetValue(assetName, out var handle))
            {
                handle.Add(o => callback?.Invoke((T)o));
                return;
            }

            // ③ 首次加载
            handle = new List<Action<object>>();
            handle.Add(o => callback?.Invoke((T)o));
            _loading.Add(assetName, handle);

            Burnicle.Resource.LoadAsset(assetName, typeof(T), _loadAssetCallbacks, this);
        }
        
        public UniTask<T> LoadAsset<T>(string assetName)
        {
            var tcs = new UniTaskCompletionSource<T>();

            LoadAsset<T>(assetName, asset =>
            {
                tcs.TrySetResult(asset);
            });

            return tcs.Task;
        }

        public bool HasAsset(string assetName)
        {
            return _assetCache.ContainsKey(assetName);
        }

        public T GetAsset<T>(string assetName) where T : Object
        {
            return _assetCache.TryGetValue(assetName, out var asset) ? (T)asset : null;
        }

        public void ReleaseAsset(string assetName)
        {
            if (_assetCache.TryGetValue(assetName, out var asset))
            {
                Burnicle.Resource.UnloadAsset(asset);
                _assetCache.Remove(assetName);
            }
        }

        public void Clear()
        {
            foreach (var asset in _assetCache.Values)
            {
                if (asset != null)
                    Burnicle.Resource.UnloadAsset(asset);
            }

            _assetCache.Clear();
            _loading.Clear();
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            if (!_loading.TryGetValue(assetName, out var handle))
                return;

            _assetCache[assetName] = asset;

            foreach (var cb in handle)
                cb?.Invoke(asset);

            _loading.Remove(assetName);
        }

        private void OnLoadAssetFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error($"Load Asset Failed: {assetName}, {errorMessage}");
            _loading.Remove(assetName);
        }
    }
}