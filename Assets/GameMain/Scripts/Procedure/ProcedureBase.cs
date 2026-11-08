using System;
using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace MergeSword
{
    public abstract class ProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        //流程资源加载器，离开流程则释放资源
        private readonly ResourceLoader _resLoader = new ResourceLoader();

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            _resLoader?.Clear();
        }

        public void LoadAsset<T>(string assetPath,Action<T> callback)
        {
            _resLoader.LoadAsset(assetPath,callback);
        }
        
        public UniTask<T> LoadAsset<T>(string assetName)
        {
            return _resLoader.LoadAsset<T>(assetName);
        }
    }
}