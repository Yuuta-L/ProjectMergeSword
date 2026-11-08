using System;
using BurnicleFramework.Runtime;
using Cysharp.Threading.Tasks;

namespace MergeSword
{
    public static class ProcedureExtension
    {
        public static void LoadAsset<T>(this ProcedureComponent component, string assetPath, Action<T> callback)
        {
            if (component.CurrentProcedure == null)
            {
                callback?.Invoke(default);
                return;
            }
            ((ProcedureBase)component.CurrentProcedure).LoadAsset(assetPath,callback);
        }
        
        public static UniTask<T> LoadAsset<T>(this ProcedureComponent component, string assetPath)
        {
            if (component.CurrentProcedure == null)
            {
                return UniTask.FromResult<T>(default);
            }
            return ((ProcedureBase)component.CurrentProcedure).LoadAsset<T>(assetPath);
        }
    }
}