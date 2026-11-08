using System;
using BurnicleFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace MergeSword
{
    public static class ResourceExtension
    {
        private static ResourceLoader _commonResLoader = null;
        public static void LoadAsset<T>(this ResourceComponent resourceComponent,string assetPath,Action<T> callback)
        {
            if (_commonResLoader == null)
            {
                _commonResLoader = new ResourceLoader();
            }
            
            _commonResLoader.LoadAsset(assetPath,callback);
        }
        
        public static UniTask<T> LoadAsset<T>(this ResourceComponent resourceComponent,string assetPath)
        {
            if (_commonResLoader == null)
            {
                _commonResLoader = new ResourceLoader();
            }
            
            return _commonResLoader.LoadAsset<T>(assetPath);
        }
    }
}