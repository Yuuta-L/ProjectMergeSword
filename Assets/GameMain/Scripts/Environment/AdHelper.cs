using System;
using UnityEngine;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    public static class AdHelper
    {
        private static float _videoAdNextCanShowTime;
        
        public static void ShowVideoAd(string sceneName,Action success,Action fail = null)
        {
            if (Time.unscaledTime < _videoAdNextCanShowTime)
            {
                Log.Info($"[AdHelper] 上次广告请求尚未返回，忽略本次调用, sceneName={sceneName}");
                fail?.Invoke();
                return;
            }
            
            //如果没有广告
            if (!Burnicle.CrossPlatform.HasVideoAd())
            {
                Debug.LogError("Common.AdNotYet");
                //Burnicle.Popup.ShowTitleMessage("Common.AdNotYet");
                fail?.Invoke();
                return;
            }
            
            Log.Info($"[AdHelper] call ShowVideoAd, sceneName={sceneName}");
            _videoAdNextCanShowTime = Time.unscaledTime + 30;
            // 进入静默模式：暂停游戏 + 释放缓存 + 降低渲染开销，降低广告加载时的内存压力
            //AdSilentMode.Enter();
            Burnicle.CrossPlatform.ShowVideoAd(sceneName, ()=>
            {
                Log.Info($"[AdHelper] ShowVideoAd success callback, sceneName={sceneName}");
                //AdSilentMode.Exit();
                _videoAdNextCanShowTime = 0;
                ReportHelper.WatchVideoAd(sceneName,true);
                OnVideoPlayComplete();
                success?.Invoke();
            }, ()=>
            {
                Log.Info($"[AdHelper] ShowVideoAd fail callback, sceneName={sceneName}");
                //AdSilentMode.Exit();
                _videoAdNextCanShowTime = 0;
                ReportHelper.WatchVideoAd(sceneName,false);
                fail?.Invoke();
            });
        }
        
        private static void OnVideoPlayComplete()
        {
            Burnicle.Persistent.AddConstTimes(Constant.Persistent.TotalVideoAdViews);
        }
        
        public static bool ShowIntersAd(string sceneName)
        {
            if (!Burnicle.CrossPlatform.HasInterstitialAd())
            {
                Log.Info("插屏未加载成功");
                return false;
            }

            Burnicle.CrossPlatform.ShowInterstitialAd(sceneName);
            Log.Info("已发送插屏请求");
            return true;
        }

        /// <summary>
        /// banner 广告开启所需的最小通关数：玩家最大通关数大于该值后才展示。
        /// </summary>
        private const int BannerUnlockPassedLevelCount = 3;

        /// <summary>
        /// banner 广告开启条件：玩家最大通关数大于 <see cref="BannerUnlockPassedLevelCount"/>。
        /// </summary>
        private static bool NeedShowBanner()
        {
            // GetLatestLevelIndex 返回当前待挑战关卡（1 起），减 1 即已通关关卡数
            var passedLevelCount = Burnicle.Persistent.GetLatestLevelIndex() - 1;
            return passedLevelCount > BannerUnlockPassedLevelCount;
        }

        /// <summary>
        /// banner 持有计数：界面打开时 AcquireBanner、关闭时 ReleaseBanner，计数大于 0 时保持展示。
        /// </summary>
        private static int _bannerRefCount;

        /// <summary>
        /// 静默模式（播放广告等）期间临时隐藏，保留持有计数，恢复后按计数重新展示。
        /// </summary>
        private static bool _bannerPaused;

        /// <summary>
        /// 申请展示 banner，必须与 <see cref="ReleaseBanner"/> 成对调用（界面 OnOpen 申请、OnClose 释放）。
        /// 多个界面同时申请时只请求一次，全部释放后才关闭。
        /// </summary>
        public static void AcquireBanner()
        {
            if (!NeedShowBanner())
            {
                return;
            }
            
            _bannerRefCount++;
            if (_bannerRefCount > 1)
            {
                return;
            }

            if (_bannerPaused)
            {
                return;
            }

            Burnicle.CrossPlatform.ShowBannerAd();
            Log.Info($"已发送显示banner请求，持有计数={_bannerRefCount}");
        }

        /// <summary>
        /// 释放 banner 展示申请，计数归零时关闭 banner。
        /// </summary>
        public static void ReleaseBanner()
        {
            if (!NeedShowBanner())
            {
                return;
            }
            
            if (_bannerRefCount <= 0)
            {
                return;
            }
            
            _bannerRefCount--;
            if (_bannerRefCount > 0)
            {
                return;
            }

            Burnicle.CrossPlatform.HideBannerAd();
            Log.Info("已发送关闭banner请求");
        }

        public static void PauseBanner()
        {
            if (!NeedShowBanner())
            {
                return;
            }

            _bannerPaused = true;
            if (_bannerRefCount > 0)
            {
                Burnicle.CrossPlatform.HideBannerAd();
            }
        }

        public static void ResumeBanner()
        {
            if (!NeedShowBanner())
            {
                return;
            }
            
            if (!_bannerPaused)
            {
                return;
            }

            _bannerPaused = false;
            if (_bannerRefCount > 0)
            {
                Burnicle.CrossPlatform.ShowBannerAd();
            }
        }
    }
}