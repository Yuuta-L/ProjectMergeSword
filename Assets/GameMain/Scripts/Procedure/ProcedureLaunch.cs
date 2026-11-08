using System.Globalization;
using UnityEngine;
using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using BurnicleFramework.Runtime;
using Obfuz;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    [ObfuzIgnore]
    public class ProcedureLaunch : ProcedureBase
    {
#if ENABLE_OBFUZ
        /// <summary>
        /// 初始化Obfuz
        /// </summary>
        [Obfuz.ObfuzIgnore]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetUpStaticSecretKey()
        {
            Obfuz.EncryptionService<Obfuz.DefaultStaticEncryptionScope>.Encryptor = new Obfuz.EncryptionVM.GeneratedEncryptionVirtualMachine(Resources.Load<TextAsset>("Obfuz/defaultStaticSecretKey").bytes);
        }
#endif
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            AwaitExtension.SubscribeEvent();
            // 构建信息：发布版本时，把一些数据以 Json 的格式写入 Assets/GameMain/Configs/BuildInfo.txt，供游戏逻辑读取
            Burnicle.BuiltinData.InitVersionInfo();
            InitLanguageSettings();
            
            InitCurrentVariant();

            InitSettings();
            Burnicle.BuiltinData.InitDefaultDictionary();
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds,float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            
            if (Burnicle.Base.EditorResourceMode)
            {
                // 编辑器模式
                Log.Info("Editor resource mode detected.");
                ChangeState<ProcedureMain>(procedureOwner);
            }
            else if (Burnicle.Resource.ResourceMode == ResourceMode.Package)
            {
                // 单机模式
                Log.Info("Package resource mode detected.");
                ChangeState<ProcedureInitResources>(procedureOwner);
            }
            else
            {
                // 可更新模式 — 使用本地 VersionInfo，经 CheckVersion → UpdateVersion → VerifyResources → CheckResources → UpdateResources → Preload
                Log.Info("Updatable resource mode detected.");
                ChangeState<ProcedureCheckVersion>(procedureOwner); 
            }
        }
        
        private void InitLanguageSettings()
        {
            if (Burnicle.Base.EditorResourceMode && Burnicle.Base.EditorLanguage != Language.Unspecified)
            {
                // 编辑器资源模式直接使用 Inspector 上设置的语言
                Burnicle.Setting.SetInt(Constant.Setting.Language, (int)Burnicle.Base.EditorLanguage);
                Burnicle.Setting.Save();
            }
            else
            {
                //只有中文
                Burnicle.Localization.Language = Language.ChineseSimplified;
            }
            Log.Info("Init language settings complete, current language is '{0}'.", Burnicle.Localization.Language.ToString());
        }

        private void InitCurrentVariant()
        {
            if (Burnicle.Base.EditorResourceMode)
            {
                // 编辑器资源模式不使用 AssetBundle，也就没有变体了
                return;
            }

            string currentVariant = null;
            switch (Burnicle.Localization.Language)
            {
                case Language.English:
                    currentVariant = "en-us";
                    break;

                case Language.ChineseSimplified:
                    currentVariant = "zh-cn";
                    break;

                case Language.ChineseTraditional:
                    currentVariant = "zh-tw";
                    break;

                default:
                    currentVariant = "en-us";
                    break;
            }

            Burnicle.Resource.SetCurrentVariant(currentVariant);
            Log.Info("Init current variant complete.current variant :{0}.", currentVariant);
        }
        
        private void InitSettings()
        {
            Burnicle.Sound.SetVolume(Constant.Sound.SoundGroupMusic, Burnicle.Setting.GetFloat(Constant.Setting.MusicVolume, 1f));
            Burnicle.Sound.SetVolume(Constant.Sound.SoundGroupSound, Burnicle.Setting.GetFloat(Constant.Setting.SoundVolume, 1f));
            Burnicle.Base.FrameRate = Burnicle.Setting.GetBool(Constant.Setting.HFR,false) ? 60 : 40;
            Log.Info("Init settings complete.");
        }
            }
}