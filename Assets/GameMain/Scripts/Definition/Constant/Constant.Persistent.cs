namespace MergeSword
{
    public static partial class Constant
    {
        public static class Persistent
        {
            public const string FirstEnterGameFlag = "FirstEnterGameFlag";
            public const string PassRewardAcquiredTimes = "PassRewardAcquiredTimes";
            public const string ShopRainbowPityTimes = "ShopRainbowPityTimes"; //商店彩虹箱保底计数
            public const string ShopRainbowSPityTimes = "ShopRainbowSPityTimes"; //商店彩虹箱S级装备保底计数
            public const string ShopObsidianPityTimes = "ShopObsidianPityTimes";//商店黑曜石箱保底计数
            public const string ShopRelicEpicPityTimes = "ShopRelicEpicPityTimes"; //商店epic遗物保底计数
            public const string ShopRelicMythicPityTimes = "ShopRelicMythicPityTimes"; //商店mythic遗物保底计数
            public const string SideBarRewardFlag = "SideBarRewardFlag";
            public const string SideBarClickedToday = "SideBarClickedToday"; //侧边栏入口红点：今日已点击过
            public const string ShortcutGiftAdded = "ShortcutGiftAdded"; //永久：已成功添加桌面快捷方式
            public const string ShortcutGiftDailyAcquiredFlag = "ShortcutGiftDailyAcquiredFlag"; //每日：今日已从快捷方式进入并领取
            public const string ShortcutGiftClickedToday = "ShortcutGiftClickedToday"; //快捷方式入口红点：今日已点击过
            public const string TotalVideoAdViews = "TotalVideoAdViews";
            public const string DailyRewardProgressKey = "DailyRewardProgress";
            public const string DailyBlessingRewardKey = "DailyBlessingReward";
            public const string SweepDailyTimes = "SweepDailyTimes";
            public const string FirstRareChestFreeFlag = "FirstRareChestFreeFlag";
            public const string NewbieGiftAcquiredFlag = "NewbieGiftUnlockedFlag";
            public const string NewbieGiftAcquiredTimes = "NewbieGiftAcquiredTimes";
            public const string NewbieGiftFormPopupFlag = "NewbieGiftFormPopupFlag";
            public const string GearWishlistId = "GearWishlistId"; //心愿装备ID（0=无）
            public const string GearWishlistProgress = "GearWishlistProgress"; //心愿累计进度
            public const string ShopSliverChestDrawSeed = "ShopSliverChestDrawSeed"; //商店银宝箱抽取持久种子
            public const string ShopObsidianChestDrawSeed = "ShopObsidianChestDrawSeed"; //商店黑曜石宝箱抽取持久种子
            public const string ShopRainbowChestDrawSeed = "ShopRainbowChestDrawSeed"; //商店彩虹宝箱抽取持久种子
            public const string ShopRelicDrawSeed = "ShopRelicDrawSeed"; //商店遗物抽取持久种子
            
            public const string FirstEnterGameFlag_Report = "FirstEnterGameFlag_Report";
            public const string FirstWatchAdFlag_Report = "FirstWatchAdFlag_Report";
            
            // 游戏圈任务
            private const string GameClubTaskClaimedHead = "GameClubTaskClaimed_{0}";
            public static string GameClubTaskClaimedFlag(int typeId)
            {
                return string.Format(GameClubTaskClaimedHead, typeId);
            }
            
            private const string GameClubTaskDailyHead = "GameClubTaskDaily_{0}";
            public static string GameClubTaskDailyFlag(int typeId)
            {
                return string.Format(GameClubTaskDailyHead, typeId);
            }
            
            public const string GameClubClickedToday = "GameClubClickedToday";
            
            // 庄园商店购买次数（每日重置，存于 DailyIntValues）
            private const string ManorShopPurchasedHead = "ManorShopPurchased_{0}";
            public static string ManorShopPurchasedKey(int productId)
            {
                return string.Format(ManorShopPurchasedHead, productId);
            }
            

            private const string TutorialFinishFlagHead = "TutorialFinishFlag_{0}";
            public static string TutorialFinishFlag(int id)
            {
                return string.Format(TutorialFinishFlagHead, id);
            }

            /// <summary>
            /// 金券首次刷新时间（本地时间 2026-07-27 00:00）
            /// </summary>
            public static readonly System.DateTime GoldenTicketFirstRefreshLocal = new System.DateTime(2026, 7, 27, 0, 0, 0, System.DateTimeKind.Local);
            /// <summary>
            /// 金券刷新间隔（14天）
            /// </summary>
            public const long GoldenTicketIntervalSeconds = 14 * 24 * 60 * 60;
        }
    }
}
