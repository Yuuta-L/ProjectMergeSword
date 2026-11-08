using System.Collections.Generic;
using System.Linq;
using GameFramework.Event;
using BurnicleFramework.Runtime;
using BurnicleFramework.Generated;
using GameFramework;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedurePreload : ProcedureBase
    {
        private readonly LoadCollector _loadFlags = ReferencePool.Acquire<LoadCollector>();
        private int _preloadTotalCount;
        private int _preloadCompletedCount;
        private bool　_isUnstableNetwork;
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            Burnicle.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            Burnicle.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            Burnicle.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            Burnicle.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            Burnicle.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadLocalizationSuccess);
            Burnicle.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
            Burnicle.Event.Subscribe(LoadDataObjectSuccessEventArgs.EventId, OnLoadDataObjectSuccess);
            Burnicle.Event.Subscribe(LoadDataObjectFailureEventArgs.EventId, OnLoadDataObjectFailure);
            
            _preloadCompletedCount = 0;
            _preloadTotalCount = 0;
            _isUnstableNetwork = false;

            // 平台初始化：OnUpdate 要等 PlatformInitialized，没人调用就会一直卡在预加载
            if (!Burnicle.CrossPlatform.PlatformInitialized)
            {
                Burnicle.CrossPlatform.Init(result => Log.Info("[Launch] 平台初始化结果: {0}", result));
            }

            PreloadResources();
            
            if (Launch.View != null)
            {
                Launch.View.ShowProgress(0f, Burnicle.Localization.GetString("Launch.Tip.LoadingResources"));
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds,float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_loadFlags.Check())
            {
                UpdatePreloadProgress();
                return;
            }

            if (!Burnicle.DateTime.GotTime || !Burnicle.CrossPlatform.PlatformInitialized)
            {
                if (!_isUnstableNetwork)
                {
                    var des = Burnicle.Localization.GetString("Launch.Tip.UnstableNetwork");
                    Launch.View?.SetTip(des);
                    _isUnstableNetwork = true;
                }
                return;
            }

            ChangeState<ProcedureMain>(procedureOwner);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Burnicle.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            Burnicle.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            Burnicle.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            Burnicle.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            Burnicle.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadLocalizationSuccess);
            Burnicle.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
            Burnicle.Event.Unsubscribe(LoadDataObjectSuccessEventArgs.EventId, OnLoadDataObjectSuccess);
            Burnicle.Event.Unsubscribe(LoadDataObjectFailureEventArgs.EventId, OnLoadDataObjectFailure);
        }
        
        private void PreloadResources()
        {
            _loadFlags.Clear();
            _preloadTotalCount = 0;
            // Preload configs
            LoadConfigs();
            LoadLocalization();
            LoadDataTables();
            LoadDataObject();
        }
        
        
        private void LoadDataTables()
        {
            foreach (var dataTableName in DataSetting.Instance.DataTables)
            {
                var assetName = AssetsPath.GetDataTableFilePath(dataTableName, DataSetting.Instance.LoadFromBytes);
                _loadFlags.Add(assetName);
                _preloadTotalCount++;
                Burnicle.DataTable.LoadDataTable(dataTableName, assetName, this);
            }
        }
        
        private void LoadConfigs()
        {
            foreach (var configName in DataSetting.Instance.Configs)
            {
                var assetName = AssetsPath.GetConfigFilePath(configName, DataSetting.Instance.LoadFromBytes);
                _loadFlags.Add(assetName);
                _preloadTotalCount++;
                Burnicle.Config.ReadData(assetName,this);
            }
        }
        
        private void LoadLocalization()
        {
            var languageName = Burnicle.Localization.Language.ToString();
            var assetName = AssetsPath.GetLocalizationFilePath(languageName, DataSetting.Instance.LoadFromBytes);
            _loadFlags.Add(assetName);
            _preloadTotalCount++;
            Burnicle.Localization.ReadData(assetName, this);
        }

        private void LoadDataObject()
        {
            foreach (var dataObjectSetting in DataSetting.Instance.DataObjects)
            {
                _loadFlags.Add(dataObjectSetting.Path);
                _preloadTotalCount++;
                Burnicle.DataObject.LoadDataObject(dataObjectSetting,this);
            }
        }
        
        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            _loadFlags.Complete(ne.ConfigAssetName);
            _preloadCompletedCount++;
            UpdatePreloadProgress();
            Log.Info("Load config '{0}' OK.", ne.ConfigAssetName);
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName, ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            if (ne.DataTableAssetName.Equals(""))
            {
                
            }
            _loadFlags.Complete(ne.DataTableAssetName);
            _preloadCompletedCount++;
            UpdatePreloadProgress();
            Log.Info("Load data table '{0}' OK.", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadLocalizationSuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            _loadFlags.Complete(ne.DictionaryAssetName);
            _preloadCompletedCount++;
            UpdatePreloadProgress();
            Log.Info("Load dictionary '{0}' OK.", ne.DictionaryAssetName);
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName, ne.DictionaryAssetName, ne.ErrorMessage);
        }
        
        
        private void OnLoadDataObjectSuccess(object sender, GameEventArgs e)
        {
            LoadDataObjectSuccessEventArgs ne = (LoadDataObjectSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            _loadFlags.Complete(ne.DataObjectAssetName);
            _preloadCompletedCount++;
            UpdatePreloadProgress();
            Log.Info("Load data object '{0}' OK.", ne.DataObjectAssetName);
        }

        private void OnLoadDataObjectFailure(object sender, GameEventArgs e)
        {
            LoadDataObjectFailureEventArgs ne = (LoadDataObjectFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data object '{0}' from '{1}' with error message '{2}'.", ne.DataObjectAssetName, ne.DataObjectAssetName, ne.ErrorMessage);
        }

        private void UpdatePreloadProgress()
        {
            if (Launch.View == null || _preloadTotalCount <= 0) return;
            float p = (float)_preloadCompletedCount / _preloadTotalCount;
            Launch.View.ShowProgress(Mathf.Clamp01(p), Burnicle.Localization.GetString("Launch.Tip.LoadingResources"));
        }
    }
}