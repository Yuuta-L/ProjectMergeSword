//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class Burnicle : MonoBehaviour
    {
        /// <summary>
        /// 获取游戏基础组件。
        /// </summary>
        public static BaseComponent Base {get; private set; }
        

        /// <summary>
        /// 获取配置组件。
        /// </summary>
        public static ConfigComponent Config {get; private set; }

        /// <summary>
        /// 获取数据表组件。
        /// </summary>
        public static DataTableComponent DataTable {get; private set; }

        /// <summary>
        /// 获取内置数据组件（版本信息、默认字典）。
        /// </summary>
        public static BuiltinDataComponent BuiltinData {get; private set; }
        
        /// <summary>
        /// 获取数据体组件。
        /// </summary>
        public static DataObjectComponent DataObject {get; private set; }

        /// <summary>
        /// 获取调试组件。
        /// </summary>
        public static DebuggerComponent Debugger {get; private set; }

        /// <summary>
        /// 获取下载组件。
        /// </summary>
        public static DownloadComponent Download {get; private set; }

        /// <summary>
        /// 获取事件组件。
        /// </summary>
        public static EventComponent Event {get; private set; }
        
        /// <summary>
        /// 获取实体组件
        /// </summary>
        public static EntityComponent Entity {get; private set; }
        
        /// <summary>
        /// 获取文件系统组件。
        /// </summary>
        public static FileSystemComponent FileSystem {get; private set; }

        /// <summary>
        /// 获取有限状态机组件。
        /// </summary>
        public static FsmComponent Fsm {get; private set; }

        /// <summary>
        /// 获取本地化组件。
        /// </summary>
        public static LocalizationComponent Localization {get; private set; }

        /// <summary>
        /// 获取网络组件。
        /// </summary>
        public static NetworkComponent Network {get; private set; }

        /// <summary>
        /// 获取对象池组件。
        /// </summary>
        public static ObjectPoolComponent ObjectPool {get; private set; }

        /// <summary>
        /// 获取流程组件。
        /// </summary>
        public static ProcedureComponent Procedure {get; private set; }
        
        /// <summary>
        /// 获取持久化数据组件。
        /// </summary>
        public static PersistentComponent Persistent {get; private set; }

        /// <summary>
        /// 获取资源组件。
        /// </summary>
        public static ResourceComponent Resource {get; private set; }

        /// <summary>
        /// 获取场景组件。
        /// </summary>
        public static SceneComponent Scene {get; private set; }
        /// <summary>
        /// 获取配置组件。
        /// </summary>
        public static SettingComponent Setting {get; private set; }

        /// <summary>
        /// 获取声音组件。
        /// </summary>
        public static SoundComponent Sound {get; private set; }

        /// <summary>
        /// 获取界面组件。
        /// </summary>
        public static UIComponent UI {get; private set; }

        /// <summary>
        /// 获取网络组件。
        /// </summary>
        public static WebRequestComponent WebRequest {get; private set; }
        
        /// <summary>
        /// 获取网络组件。
        /// </summary>
        public static CrossPlatformComponent CrossPlatform {get; private set; }
        
        /// <summary>
        /// 获取网络组件。
        /// </summary>
        public static DateTimeComponent DateTime {get; private set; }

        private static void RegisterBuiltinComponents()
        {
            Base = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<BaseComponent>();
            Config = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<ConfigComponent>();
            DataTable = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<DataTableComponent>();
            BuiltinData = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<BuiltinDataComponent>();
            BuiltinData = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<BuiltinDataComponent>();
            DataObject = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<DataObjectComponent>();
            Debugger = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<DebuggerComponent>();
            Download = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<DownloadComponent>();
            Event = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<EventComponent>();
            Entity = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<EntityComponent>();
            FileSystem = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<FileSystemComponent>();
            Fsm = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<FsmComponent>();
            Localization = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<LocalizationComponent>();
            Network = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<NetworkComponent>();
            ObjectPool = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<ObjectPoolComponent>();
            Procedure = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<ProcedureComponent>();
            Persistent = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<PersistentComponent>();
            Resource = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<ResourceComponent>();
            Scene = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<SceneComponent>();
            Setting = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<SettingComponent>();
            Sound = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<SoundComponent>();
            UI = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<UIComponent>();
            WebRequest = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<WebRequestComponent>();
            CrossPlatform = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<CrossPlatformComponent>();
            DateTime = BurnicleFramework.Runtime.BurnicleEntry.GetComponent<DateTimeComponent>();
        }
    }
}
