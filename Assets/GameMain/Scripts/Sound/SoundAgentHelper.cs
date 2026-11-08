using System;
using System.Collections;
using GameFramework;
using GameFramework.Sound;
using UnityEngine;
using UnityEngine.Audio;
using BurnicleFramework.Runtime;

namespace UGFExtensions.Helper
{
    /// <summary>
    /// 声音代理辅助器。
    ///
    /// WebGL（含各小游戏平台）下 Unity 的音频通道会失效（clip 被释放、播放结束、音频上下文状态变化等），
    /// 此时再读 isPlaying 或写 time/loop/pitch/clip 等属性，FMOD 会刷
    /// “An invalid object handle was used. (FMOD_ERR_INVALID_HANDLE)”。
    /// 而框架 SoundAgent.Reset() 每次要写十来个属性（SetSoundAsset 之前还会先 Reset 一次），
    /// 因此这里把播放参数先缓存在本地，只有确认通道活着（m_HasChannel）时才下发给 AudioSource；
    /// Play() 里先 Play() 让 Unity 建出通道、再一次性下发参数，避免对失效通道的读写把日志刷满。
    /// </summary>
    public class SoundAgentHelper: SoundAgentHelperBase
    {
        private Transform m_CachedTransform = null;
        private AudioSource m_AudioSource = null;
        private float m_VolumeWhenPause = 0f;
        private bool m_ApplicationPauseFlag = false;
        private EventHandler<ResetSoundAgentEventArgs> m_ResetSoundAgentEventHandler = null;
        private bool m_IsPaused;

        /// <summary> 是否已开始播放、AudioSource 上存在活通道。 </summary>
        private bool m_HasChannel;

        /// <summary> 已发起播放且尚未上报播完：决定何时通知框架回收代理（与通道是否还活着无关）。 </summary>
        private bool m_Playing;

        // 本地缓存的播放参数（默认值与 AudioSource 默认值一致）
        private float m_Time = 0f;
        private bool m_Mute = false;
        private bool m_Loop = false;
        private int m_Priority = 128;
        private float m_Volume = 1f;
        private float m_Pitch = 1f;
        private float m_PanStereo = 0f;
        private float m_SpatialBlend = 0f;
        private float m_MaxDistance = 500f;
        private float m_DopplerLevel = 1f;
        private AudioMixerGroup m_AudioMixerGroup = null;
        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public override bool IsPlaying
        {
            get
            {
                // 通道不存在时不要去问 Unity（失效句柄会让 FMOD 报错）
                return m_HasChannel && (m_AudioSource.isPlaying || m_IsPaused);
            }
        }

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        public override float Length
        {
            get
            {
                return m_AudioSource.clip != null ? m_AudioSource.clip.length : 0f;
            }
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public override float Time
        {
            get
            {
                return m_HasChannel ? m_AudioSource.time : m_Time;
            }
            set
            {
                m_Time = value;
                if (m_HasChannel)
                {
                    m_AudioSource.time = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置是否静音。
        /// </summary>
        public override bool Mute
        {
            get
            {
                return m_Mute;
            }
            set
            {
                m_Mute = value;
                if (m_HasChannel)
                {
                    m_AudioSource.mute = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public override bool Loop
        {
            get
            {
                return m_Loop;
            }
            set
            {
                m_Loop = value;
                if (m_HasChannel)
                {
                    m_AudioSource.loop = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public override int Priority
        {
            get
            {
                // m_Priority 存的是 Unity 侧优先级（数值越小优先级越高）
                return 128 - m_Priority;
            }
            set
            {
                m_Priority = 128 - value;
                if (m_HasChannel)
                {
                    m_AudioSource.priority = m_Priority;
                }
            }
        }

        /// <summary>
        /// 获取或设置音量大小。
        /// </summary>
        public override float Volume
        {
            get
            {
                return m_Volume;
            }
            set
            {
                m_Volume = value;
                if (m_HasChannel)
                {
                    m_AudioSource.volume = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public override float Pitch
        {
            get
            {
                return m_Pitch;
            }
            set
            {
                m_Pitch = value;
                if (m_HasChannel)
                {
                    m_AudioSource.pitch = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public override float PanStereo
        {
            get
            {
                return m_PanStereo;
            }
            set
            {
                m_PanStereo = value;
                if (m_HasChannel)
                {
                    m_AudioSource.panStereo = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public override float SpatialBlend
        {
            get
            {
                return m_SpatialBlend;
            }
            set
            {
                m_SpatialBlend = value;
                if (m_HasChannel)
                {
                    m_AudioSource.spatialBlend = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public override float MaxDistance
        {
            get
            {
                return m_MaxDistance;
            }

            set
            {
                m_MaxDistance = value;
                if (m_HasChannel)
                {
                    m_AudioSource.maxDistance = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public override float DopplerLevel
        {
            get
            {
                return m_DopplerLevel;
            }
            set
            {
                m_DopplerLevel = value;
                if (m_HasChannel)
                {
                    m_AudioSource.dopplerLevel = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置声音代理辅助器所在的混音组。
        /// </summary>
        public override AudioMixerGroup AudioMixerGroup
        {
            get
            {
                return m_AudioMixerGroup;
            }
            set
            {
                m_AudioMixerGroup = value;
                if (m_HasChannel)
                {
                    m_AudioSource.outputAudioMixerGroup = value;
                }
            }
        }

        /// <summary>
        /// 重置声音代理事件。
        /// </summary>
        public override event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent
        {
            add
            {
                m_ResetSoundAgentEventHandler += value;
            }
            remove
            {
                m_ResetSoundAgentEventHandler -= value;
            }
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public override void Play(float fadeInSeconds)
        {
            StopAllCoroutines();

            m_IsPaused = false;

            // 先 Play 让 Unity 建出音频通道，再下发参数：通道不存在时写属性会报 FMOD_ERR_INVALID_HANDLE
            m_AudioSource.Play();
            m_HasChannel = true;
            m_Playing = true;
            ApplyCachedSettings();

            if (fadeInSeconds > 0f)
            {
                m_AudioSource.volume = 0f;
                StartCoroutine(FadeToVolume(m_AudioSource, m_Volume, fadeInSeconds));
            }
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public override void Stop(float fadeOutSeconds)
        {
            StopAllCoroutines();

            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy && m_HasChannel)
            {
                StartCoroutine(StopCo(fadeOutSeconds));
            }
            else
            {
                StopChannel();
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public override void Pause(float fadeOutSeconds)
        {
            StopAllCoroutines();

            m_VolumeWhenPause = m_Volume;
            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy && m_HasChannel)
            {
                StartCoroutine(PauseCo(fadeOutSeconds));
            }
            else
            {
                m_IsPaused = true;
                if (m_HasChannel)
                {
                    m_AudioSource.Pause();
                }
            }
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public override void Resume(float fadeInSeconds)
        {
            StopAllCoroutines();
            m_IsPaused = false;

            if (!m_HasChannel)
            {
                return;
            }

            m_AudioSource.UnPause();
            if (fadeInSeconds > 0f)
            {
                StartCoroutine(FadeToVolume(m_AudioSource, m_VolumeWhenPause, fadeInSeconds));
            }
            else
            {
                m_AudioSource.volume = m_VolumeWhenPause;
            }
        }

        /// <summary>
        /// 重置声音代理辅助器。
        /// </summary>
        public override void Reset()
        {
            m_CachedTransform.localPosition = Vector3.zero;

            // 必须在资源被释放（AudioClip 被销毁）之前把通道停掉，
            // 并断开 AudioSource 对 clip 的引用，否则会留下一堆指向死 clip 的通道。
            StopChannel();
            m_AudioSource.clip = null;
            m_Playing = false;

            m_VolumeWhenPause = 0f;
            m_IsPaused = false;
        }

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源。</param>
        /// <returns>是否设置声音资源成功。</returns>
        public override bool SetSoundAsset(object soundAsset)
        {
            AudioClip audioClip = soundAsset as AudioClip;
            if (audioClip == null)
            {
                return false;
            }

            // clip 相同时不重复写 AudioSource（失效通道上的写会报 FMOD_ERR_INVALID_HANDLE）
            if (m_AudioSource.clip != audioClip)
            {
                m_AudioSource.clip = audioClip;
            }
            return true;
        }

        /// <summary>
        /// 设置声音所在的世界坐标。
        /// </summary>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        public override void SetWorldPosition(Vector3 worldPosition)
        {
            m_CachedTransform.position = worldPosition;
        }

        private void Awake()
        {
            m_CachedTransform = transform;
            m_AudioSource = gameObject.GetOrAddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            m_AudioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        private void Update()
        {
            // m_Playing 只表示“已发起播放、还没上报播完”，与通道是否还活着解耦：
            // 显式 Stop/淡出结束后通道已经停了，也必须上报，否则框架不会 Reset 代理，
            // clip 就永远不会释放（与之前“回收时抢走仍在用的数据”对称的另一个坑）。
            if (m_Playing && !m_ApplicationPauseFlag && !IsPlaying && m_ResetSoundAgentEventHandler != null)
            {
                // 先落状态再回调：回调会走到 SoundAgent.Reset() → 本类 Reset()
                m_Playing = false;
                ResetSoundAgentEventArgs resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                m_ResetSoundAgentEventHandler(this, resetSoundAgentEventArgs);
                ReferencePool.Release(resetSoundAgentEventArgs);
            }
        }

        /// <summary>
        /// 停止当前通道（通道未建立/已停时不做任何 AudioSource 调用）。
        /// </summary>
        private void StopChannel()
        {
            if (!m_HasChannel)
            {
                return;
            }

            m_AudioSource.Stop();
            m_HasChannel = false;
        }

        /// <summary>
        /// 把本地缓存的播放参数一次性下发给 AudioSource（必须在通道已建立之后调用）。
        /// </summary>
        private void ApplyCachedSettings()
        {
            m_AudioSource.loop = m_Loop;
            m_AudioSource.mute = m_Mute;
            m_AudioSource.priority = m_Priority;
            m_AudioSource.volume = m_Volume;
            m_AudioSource.pitch = m_Pitch;
            m_AudioSource.panStereo = m_PanStereo;
            m_AudioSource.spatialBlend = m_SpatialBlend;
            m_AudioSource.maxDistance = m_MaxDistance;
            m_AudioSource.dopplerLevel = m_DopplerLevel;
            m_AudioSource.outputAudioMixerGroup = m_AudioMixerGroup;
            m_AudioSource.time = m_Time;
        }

        private void OnApplicationPause(bool pause)
        {
            m_ApplicationPauseFlag = pause;
        }

        private IEnumerator StopCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(m_AudioSource, 0f, fadeOutSeconds);
            StopChannel();
        }

        private IEnumerator PauseCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(m_AudioSource, 0f, fadeOutSeconds);
            m_AudioSource.Pause();
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            float time = 0f;
            float originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }
    }
}