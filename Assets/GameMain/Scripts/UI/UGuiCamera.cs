using BurnicleFramework.Runtime;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MergeSword
{
    public class UGuiCamera : MonoBehaviour
    {
        private Camera m_Camera;
        private Camera m_MainCamera;
        private EventComponent m_EventComponent;
        private UniversalAdditionalCameraData m_CameraData;
        
        public static Camera CurrentUICamera;
        
        private void Awake()
        {
            CurrentUICamera = m_Camera = GetComponent<Camera>();
            m_CameraData = m_Camera.GetUniversalAdditionalCameraData();
        }
        
        private void Start()
        {
            m_EventComponent = BurnicleEntry.GetComponent<EventComponent>();
            m_EventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            CheckCamera();
        }
        
        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            if (e is LoadSceneSuccessEventArgs)
            {
                CheckCamera();
            }
        }
        
        private void CheckCamera()
        {
            if (m_Camera == null) return;
            
            m_MainCamera = Camera.main;
            
            if (m_MainCamera != null && m_MainCamera != m_Camera)
            {
                // 关键：使用Camera Stack，但要确保正确配置
                m_CameraData.renderType = CameraRenderType.Overlay;
                m_Camera.clearFlags = CameraClearFlags.Depth;
                m_Camera.depth = m_MainCamera.depth;
                
                var mainCameraData = m_MainCamera.GetUniversalAdditionalCameraData();
                if (mainCameraData != null)
                {
                    mainCameraData.renderType = CameraRenderType.Base;
                    if (!mainCameraData.cameraStack.Contains(m_Camera))
                    {
                        mainCameraData.cameraStack.Add(m_Camera);
                    }
                }
            }
            else
            {
                m_CameraData.renderType = CameraRenderType.Base;
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
                m_Camera.depth = -1;
            }
        }
        
        private void OnDestroy()
        {
            if (m_EventComponent != null)
            {
                m_EventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            }
        }
    }
}