﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/
using CustomLogger;
namespace NRKernal
{
    using UnityEngine;

    /// <summary> A camera model view used to receive camera image. </summary>
    public class CameraModelView
    {
        /// <summary> Gets the width. </summary>
        /// <value> The width. </value>
        public int Width
        {
            get
            {
                return m_NativeCameraProxy.Resolution.width;
            }
        }

        /// <summary> Gets the height. </summary>
        /// <value> The height. </value>
        public int Height
        {
            get
            {
                return m_NativeCameraProxy.Resolution.height;
            }
        }

        /// <summary> Gets a value indicating whether this object is playing. </summary>
        /// <value> True if this object is playing, false if not. </value>
        public bool IsPlaying
        {
            get
            {
                FileLogger.Log($"[카메라 프레임] IsPlaying, m_State={m_State}");
                return m_State == State.Playing;
            }
        }

        /// <summary> Values that represent states. </summary>
        public enum State
        {
            /// <summary> An enum constant representing the playing option. </summary>
            Playing,
            /// <summary> An enum constant representing the paused option. </summary>
            Paused,
            /// <summary> An enum constant representing the stopped option. </summary>
            Stopped
        }
        /// <summary> The state. </summary>
        protected State m_State = State.Stopped;

        /// <summary> Gets a value indicating whether the did update this frame. </summary>
        /// <value> True if did update this frame, false if not. </value>
        public virtual bool DidUpdateThisFrame
        {
            get
            {
                return m_NativeCameraProxy.HasFrame();
            }
        }

        /// <summary> Gets or sets the number of frames. </summary>
        /// <value> The number of frames. </value>
        public int FrameCount { get; protected set; }
        /// <summary> The native camera proxy. </summary>
        protected NativeCameraProxy m_NativeCameraProxy;

        /// <summary> Gets or sets the native camera proxy. </summary>
        /// <value> The native camera proxy. </value>
        public NativeCameraProxy NativeCameraProxy
        {
            get
            {
                return this.m_NativeCameraProxy;
            }
            set
            {
                this.m_NativeCameraProxy = value;
            }
        }

        public CameraImageFormat ImageFormat { get; protected set; }

        /// <summary> Default constructor. </summary>
        public CameraModelView() { }

        /// <summary> Constructor. </summary>
        /// <param name="format"> Camera image format.</param>
        public CameraModelView(CameraImageFormat format)
        {
            ImageFormat = format;
            this.CreateRGBCameraProxy(format);
            m_NativeCameraProxy.Regist(this);
        }

        /// <summary> Use RGB_888 format default. </summary>
        /// <param name="format"> (Optional) Camera image format.</param>
        protected void CreateRGBCameraProxy(CameraImageFormat format = CameraImageFormat.RGB_888)
        {
            if (m_NativeCameraProxy != null)
            {
                return;
            }

            m_NativeCameraProxy = CameraProxyFactory.CreateRGBCameraProxy();
            m_NativeCameraProxy.SetImageFormat(format);
        }

        /// <summary> Plays this object. </summary>
        public virtual void Play()
        {
            if (m_State == State.Playing)
            {
                return;
            }
            NRKernalUpdater.OnUpdate += UpdateTexture;
            m_NativeCameraProxy.Play();
            m_NativeCameraProxy.Regist(this);
            m_State = State.Playing;
        }

        /// <summary> Pauses this object. </summary>
        public virtual void Pause()
        {
            if (m_State == State.Paused || m_State == State.Stopped)
            {
                return;
            }
            m_NativeCameraProxy.Pause();
            NRKernalUpdater.OnUpdate -= UpdateTexture;
            m_State = State.Paused;
        }

        /// <summary> Resume this object. </summary>
        public virtual void Resume()
        {
            if (m_State == State.Paused)
            {
                m_NativeCameraProxy.Resume();
                NRKernalUpdater.OnUpdate += UpdateTexture;
                m_State = State.Playing;
            }
        }

        /// <summary> Updates the texture. </summary>
        protected virtual void UpdateTexture()
        {
            if (!DidUpdateThisFrame || !IsPlaying)
            {
                // FileLogger.Log($"[카메라 프레임] 프레임 스킵됨, DidUpdateThisFrame={DidUpdateThisFrame}, IsPlaying={IsPlaying}");    
                return;
            }

            FrameRawData frame = m_NativeCameraProxy.GetFrame();
            FileLogger.Log($"[카메라 프레임] Exist={frame.data != null}, FrameCount={FrameCount}");
            if (frame.data == null)
            {
                FileLogger.Log("[카메라 프레임] 프레임 데이터 획득 실패");
                NRDebugger.Error("[카메라 프레임] Get camera raw data faild...");
                return;
            }
            FrameCount++;
            OnRawDataUpdate(frame);
        }

        /// <summary> Stops this object. </summary>
        public virtual void Stop()
        {
            FileLogger.Log("=== CameraModelView Stop 시작 ===");
            
            if (m_State == State.Stopped)
            {
                FileLogger.Log("카메라가 이미 정지 상태임");
                return;
            }

            FileLogger.Log($"현재 카메라 상태: FrameCount={FrameCount}, State={m_State}");
            
            m_NativeCameraProxy.Remove(this);
            FileLogger.Log("NativeCameraProxy에서 제거됨");
            
            m_NativeCameraProxy.Stop();
            FileLogger.Log("NativeCameraProxy 정지됨");
            
            NRKernalUpdater.OnUpdate -= UpdateTexture;
            FileLogger.Log("UpdateTexture 이벤트 핸들러 제거됨");

            FrameCount = 0;
            m_State = State.Stopped;
            FileLogger.Log($"카메라 상태 변경: FrameCount={FrameCount}, State={m_State}");
            
            this.OnStopped();
            FileLogger.Log("=== CameraModelView Stop 종료 ===");
        }

        /// <summary> Load raw texture data. </summary>
        /// <param name="frame"> .</param>
        protected virtual void OnRawDataUpdate(FrameRawData frame) { }

        /// <summary> On texture stopped. </summary>
        protected virtual void OnStopped() { }
    }
}
