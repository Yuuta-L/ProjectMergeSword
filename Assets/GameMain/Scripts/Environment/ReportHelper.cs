using System.Collections.Generic;

namespace MergeSword
{
    public static class ReportHelper
    {
        private const string PropName_Action = "action";
        private const string PropName_Index = "index";
        private const string PropName_Result = "result";
        private const string PropName_Scene = "scene";
        
        private const string EventName_Common = "common";
        private const string EventName_Level = "level";
        private const string EventName_Lobby = "lobby";
        private const string EventName_Ad = "ad";
        
        private const string Common_ActionName_GameLaunchFirst = "game_luanch_first";
        private const string Common_ActionName_GameLaunch = "game_luanch";
        private const string Common_ActionName_GameLoaded = "game_loaded";
        private const string Common_ActionName_LaunchFormClose = "launchform_close";
        private const string Common_ActionName_ComicOver = "comic_over";
        
        private const string Level_ActionName_LevelEntry = "level_entry";
        private const string Level_ActionName_LevelFinish = "level_finish";
        private const string Level_ActionName_LevelEnterLayer = "level_enterlayer";
        
        private const string Lobby_ActionName_Unlock = "lobby_unlock";
        
        private const string Ad_ActionName_Display = "ad_display";
        private const string Ad_ActionName_Watch = "ad_watch";
        private const string Ad_ActionName_WatchFirst = "ad_watch_first";
#region 常规
        ///------------------------常规-------------------------------
        
        public static void GameLaunch()
        {
            if (!Burnicle.Persistent.GetConstFlag(Constant.Persistent.FirstEnterGameFlag_Report))
            {
                var data = new Dictionary<string, object>()
                {
                    {PropName_Action, Common_ActionName_GameLaunchFirst } 
                };
                ReportCommon(data);
                Burnicle.Persistent.SetConstFlag(Constant.Persistent.FirstEnterGameFlag_Report);
            }
            
            var data1 = new Dictionary<string, object>()
            {
                {PropName_Action, Common_ActionName_GameLaunch } 
            };
            ReportCommon(data1);
        }

        public static void GameLoaded()
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Common_ActionName_GameLoaded } 
            };
            ReportCommon(data);
        }

        /// <summary>
        /// 启动界面关闭
        /// </summary>
        public static void LaunchFormClose()
        {
            if (!IsLessLevel(2))
            {
                return;
            }
            
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Common_ActionName_LaunchFormClose } 
            };
            ReportCommon(data);
        }
        
        /// <summary>
        /// 播放剧情
        /// </summary>
        public static void PlayComic(int index)
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Common_ActionName_ComicOver } ,
                {PropName_Index, index }
            };
            ReportCommon(data);
        }

        private static bool IsLessLevel(int target)
        {
            var level = Burnicle.Persistent.GetLatestLevelIndex();
            return level <= target;
        }
        /// <summary>
        /// 上报常规事件
        /// </summary>
        private static void ReportCommon(Dictionary<string, object> data)
        {
            Burnicle.CrossPlatform.Report(EventName_Common, data);
        }
#endregion
        
#region 关卡内
        ///------------------------关卡内-------------------------------

        public static void LevelStart(int levelIndex)
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Level_ActionName_LevelEntry} ,
                {PropName_Index, levelIndex} 
            };
            ReportLevel(data);
        }

        public static void EnterLayer(int layer,int roomId)
        {
            if (!IsLessLevel(2))
            {
                return;
            }
            
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Level_ActionName_LevelEnterLayer} ,
                {PropName_Index, layer}, 
                {PropName_Result, roomId} 
            };
            ReportLevel(data);
        }
        
        public static void LevelFinish(int levelIndex,bool result)
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Level_ActionName_LevelFinish},
                {PropName_Index, levelIndex} ,
                {PropName_Result, result?1:0} 
            };
            ReportLevel(data);
        }
        
        private static void ReportLevel(Dictionary<string, object> data)
        {
            Burnicle.CrossPlatform.Report(EventName_Level, data);
        }
#endregion

#region 城镇
        public static void LobbyUnlock(int unlockId)
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Lobby_ActionName_Unlock},
                {PropName_Index, unlockId}
            };
            ReportLobby(data);
        }
        
        private static void ReportLobby(Dictionary<string, object> data)
        {
            Burnicle.CrossPlatform.Report(EventName_Lobby, data);
        }
#endregion

#region 广告
        //系统展示广告
        public static void DisplayVideoAd(string scene)
        {
            var data = new Dictionary<string, object>()
            {
                {PropName_Action, Ad_ActionName_Display},
                {PropName_Scene, scene}
            };
            ReportAd(data);
        }
        //玩家观看广告
        public static void WatchVideoAd(string scene,bool result)
        {
            if (!Burnicle.Persistent.GetConstFlag(Constant.Persistent.FirstWatchAdFlag_Report))
            {
                var data = new Dictionary<string, object>()
                {
                    {PropName_Action, Ad_ActionName_WatchFirst},
                    {PropName_Scene, scene},
                    {PropName_Result, result?1:0}
                };
                ReportAd(data);
                Burnicle.Persistent.SetConstFlag(Constant.Persistent.FirstWatchAdFlag_Report);
            }
            
            var data1 = new Dictionary<string, object>()
            {
                {PropName_Action, Ad_ActionName_Watch},
                {PropName_Scene, scene},
                {PropName_Result, result?1:0}
            };
            ReportAd(data1);
        }
                
        private static void ReportAd(Dictionary<string, object> data)
        {
            Burnicle.CrossPlatform.Report(EventName_Ad, data);
        }
#endregion
    }
}