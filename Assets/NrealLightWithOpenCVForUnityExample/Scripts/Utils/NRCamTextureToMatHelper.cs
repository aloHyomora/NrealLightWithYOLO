#if !OPENCV_DONT_USE_WEBCAMTEXTURE_API
#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using System;
using NRKernal;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System.Collections;
using UnityEngine;
using NrealLightWithOpenCVForUnityExample;
using CustomLogger;

namespace NrealLightWithOpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// NRCamTexture to mat helper.
    /// v 1.0.2
    /// Depends on NRSDK v 2.1.0 (https://nreal.gitbook.io/nrsdk/nrsdk-fundamentals/core-features).
    /// Depends on OpenCVForUnity version 2.4.1 (WebCamTextureToMatHelper v 1.1.2) or later.
    /// 
    /// By setting outputColorFormat to RGB, processing that does not include extra color conversion is performed.
    /// 
    /// </summary>
    public class NRCamTextureToMatHelper : WebCamTexture2MatHelper
    {

        protected NRRGBCamTexture nrRGBCamTexture = default;

        /// <summary>
        /// Returns the NRRGBCamTexture.
        /// </summary>
        /// <returns>The NRRGBCamTexture.</returns>
        public virtual NRRGBCamTexture GetNRRGBCamTexture()
        {
            return nrRGBCamTexture;
        }

        /// <summary>
        /// Pauses the CurrentFrame timeStamp.
        /// </summary>
        public virtual ulong GetCurrentFrameTimeStamp()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && !DISABLE_NRSDK_API
            return hasInitDone ? nrRGBCamTexture.CurrentFrame.timeStamp : ulong.MinValue;
#else            
            return ulong.MinValue;
#endif
        }

        /// <summary>
        /// Pauses the CurrentFrame gain.
        /// </summary>
        public virtual ulong GetCurrentFrameGain()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && !DISABLE_NRSDK_API
            return hasInitDone ? nrRGBCamTexture.CurrentFrame.gain : ulong.MinValue;
#else
            return ulong.MinValue;
#endif
        }

        /// <summary>
        /// Pauses the CurrentFrame exposureTime.
        /// </summary>
        public virtual ulong GetCurrentFrameExposureTime()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && !DISABLE_NRSDK_API
            return hasInitDone ? nrRGBCamTexture.CurrentFrame.exposureTime : ulong.MinValue;
#else            
            return ulong.MinValue;
#endif
        }

        /// <summary>
        /// Pauses the FrameCount.
        /// </summary>
        public virtual int GetFrameCount()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && !DISABLE_NRSDK_API
            return hasInitDone ? nrRGBCamTexture.FrameCount : -1;
#else
            return -1;
#endif
        }


#if UNITY_ANDROID && !UNITY_EDITOR && !DISABLE_NRSDK_API
        
        new protected Source2MatHelperColorFormat baseColorFormat = Source2MatHelperColorFormat.RGB;
        protected Matrix4x4 invertZM = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), new Vector3(1, 1, -1));

        protected Matrix4x4 rgbCameraPoseFromHeadMatrix = Matrix4x4.identity;

        protected Matrix4x4 centerEyePoseFromHeadMatrix = Matrix4x4.identity;

        protected Matrix4x4 projectionMatrix = Matrix4x4.identity;        

        #region DEBUG
        private bool previousIsPlaying = false;
        private int previousFrameCount = -1;
        private int currentFrameCount = 0;  // 자체 프레임 카운터 추가

        // Update is called once per frame
        protected override void Update() 
        {
            if (nrRGBCamTexture != null)
            {
                bool currentIsPlaying = nrRGBCamTexture.IsPlaying;
                
                // isPlaying이 true일 때만 프레임 카운트 증가
                if (currentIsPlaying)
                {
                    currentFrameCount++;
                }
                
                // 상태가 변경되었거나 처음 실행되는 경우에만 로그 기록
                if (previousIsPlaying != currentIsPlaying || previousFrameCount == -1)
                {
                    FileLogger.Log($"카메라 상태: isPlaying={currentIsPlaying}, FrameCount={currentFrameCount}");
                    FileLogger.Log("...");
                    if (previousIsPlaying && !currentIsPlaying)
                    {
                        FileLogger.Log("카메라 상태 변경 감지: Playing -> Stopped");
                        // 카메라 재시작 시도
                        RestartCamera();
                    }
                }
                
                previousIsPlaying = currentIsPlaying;
                previousFrameCount = currentFrameCount;
            }            
        }
        
        private void RestartCamera()
        {
            FileLogger.Log("카메라 재시작 시도");
            if (nrRGBCamTexture != null)
            {
                nrRGBCamTexture.Stop();
                nrRGBCamTexture.Play();
            }
        }

        private void OnDisable()
        {
            FileLogger.Log("NRCamTextureToMatHelper OnDisable 호출됨");   
            FileLogger.Log($"OnDisable 호출 스택: {Environment.StackTrace}");
            FileLogger.Log($"GameObject 활성화 상태: {gameObject.activeInHierarchy}");
            FileLogger.Log($"컴포넌트 활성화 상태: {this.enabled}");      
            if (nrRGBCamTexture != null)
            {
                FileLogger.Log($"카메라 상태: isPlaying={nrRGBCamTexture.IsPlaying}");
            }   
        }

        private void OnEnable()
        {
            FileLogger.Log("NRCamTextureToMatHelper OnEnable 호출됨");            
        }
        #endregion
        /// <summary>
        /// Initializes this instance by coroutine.
        /// </summary>
        protected override IEnumerator _Initialize()
        {

            if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }

            isInitWaiting = true;


#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            // Checks camera permission state.
            IEnumerator coroutine = hasUserAuthorizedCameraPermission();//hasUserAuthorizedCameraPermission();
            yield return coroutine;

            if (!(bool)coroutine.Current)
            {
                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                {
                    onErrorOccurred.Invoke(Source2MatHelperErrorCode.CAMERA_PERMISSION_DENIED, string.Empty);
                    FileLogger.Log("ERROR: Camera permission denied");
                }

                yield break;
            }
#endif

            // Create an instance of NRRGBCamTexture
            nrRGBCamTexture = new NRRGBCamTexture();
            nrRGBCamTexture.Play();


            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {

                if (initFrameCount > timeoutFrameCount)
                {
                    FileLogger.Log($"초기화 시도 횟수: {initFrameCount}");
                    isTimeout = true;
                    break;
                }
                else if (nrRGBCamTexture.DidUpdateThisFrame)
                {
                    FileLogger.Log("카메라 프레임 업데이트 성공");

                    baseMat = new Mat(nrRGBCamTexture.Height, nrRGBCamTexture.Width, CvType.CV_8UC3);

                    if (baseColorFormat == outputColorFormat)
                    {
                        frameMat = baseMat;
                    }
                    else
                    {
                        frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Source2MatHelperUtils.Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));
                    }

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    if (rotate90Degree)
                        rotatedFrameMat = new Mat(frameMat.cols(), frameMat.rows(), CvType.CV_8UC(Source2MatHelperUtils.Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));

                    isInitWaiting = false;
                    hasInitDone = true;
                    initCoroutine = null;


                    // Get physical RGBCamera position (offset position from Head).
                    Pose camPos = NRFrame.GetDevicePoseFromHead(NativeDevice.RGB_CAMERA);
                    rgbCameraPoseFromHeadMatrix = Matrix4x4.TRS(camPos.position, camPos.rotation, Vector3.one);

                    // Get CenterEyePose (between left eye and right eye) position (offset position from Head).
                    var eyeposeFromHead = NRFrame.EyePoseFromHead;
                    Vector3 localPosition = (eyeposeFromHead.LEyePose.position + eyeposeFromHead.REyePose.position) * 0.5f;
                    Quaternion localRotation = Quaternion.Lerp(eyeposeFromHead.LEyePose.rotation, eyeposeFromHead.REyePose.rotation, 0.5f);
                    centerEyePoseFromHeadMatrix = Matrix4x4.TRS(localPosition, localRotation, Vector3.one);

                    // Get projection Matrix
                    //
                    //NativeMat3f mat = NRFrame.GetDeviceIntrinsicMatrix(NativeDevice.RGB_CAMERA);// Get the rgb camera's intrinsic matrix
                    //projectionMatrix = ARUtils.CalculateProjectionMatrixFromCameraMatrixValues(mat.column0.X, mat.column1.Y,
                    //    mat.column2.X, mat.column2.Y, NRFrame.GetDeviceResolution(NativeDevice.RGB_CAMERA).width, NRFrame.GetDeviceResolution(NativeDevice.RGB_CAMERA).height, 0.3f, 1000f);
                    //
                    // or
                    //
                    bool result;
                    EyeProjectMatrixData pm = NRFrame.GetEyeProjectMatrix(out result, 0.3f, 1000f);
                    while (!result)
                    {
                        yield return new WaitForEndOfFrame();
                        pm = NRFrame.GetEyeProjectMatrix(out result, 0.3f, 1000f);
                    }
                    projectionMatrix = pm.RGBEyeMatrix;
                    //


                    if (onInitialized != null)
                    {
                        onInitialized.Invoke();
                        FileLogger.Log("카메라 초기화 완료");
                    }
                    FileLogger.Log("카메라 초기화 완료: while 종료");
                    break;
                }
                else
                {
                    if (initFrameCount % 100 == 0)  // 100프레임마다 로그
                    {
                        FileLogger.Log($"카메라 초기화 대기 중... ({initFrameCount}/{timeoutFrameCount})");
                        // 카메라 상태 상세 로깅
                        if (nrRGBCamTexture != null)
                        {
                            FileLogger.Log($"카메라 상태: isPlaying={nrRGBCamTexture.IsPlaying}, FrameCount={nrRGBCamTexture.FrameCount}, Width={nrRGBCamTexture.Width}, Height={nrRGBCamTexture.Height}");
                        }
                        else
                        {
                            FileLogger.Log("경고: nrRGBCamTexture가 null임");
                        }
                    }
                    initFrameCount++;
                    yield return null;
                }
            }

            if (isTimeout)
            {
                FileLogger.Log($"타임아웃 발생 시 카메라 상태: isPlaying={nrRGBCamTexture?.IsPlaying}, FrameCount={nrRGBCamTexture?.FrameCount}");
                if (nrRGBCamTexture != null)
                {
                    FileLogger.Log("NRCamTextureToMatHelper에서 nrRGBCamTexture.Stop() 호출");
                    nrRGBCamTexture.Stop();
                    nrRGBCamTexture = null;
                }
                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                {
                    onErrorOccurred.Invoke(Source2MatHelperErrorCode.TIMEOUT, string.Empty);
                    FileLogger.Log("ERROR: Timeout");
                }
            }
        }

        /// <summary>
        /// Starts the camera.
        /// </summary>
        public override void Play()
        {
            if (hasInitDone)
            {
                nrRGBCamTexture.Play();
                FileLogger.Log("Camera Play 호출됨");
            }
        }

        /// <summary>
        /// Pauses the active camera.
        /// </summary>
        public override void Pause()
        {
            if (hasInitDone)    
            {
                nrRGBCamTexture.Pause();
                FileLogger.Log("Camera Pause 호출됨");
            }
        }

        /// <summary>
        /// Stops the active camera.
        /// </summary>
        public override void Stop()
        {
            if (hasInitDone)
            {
                nrRGBCamTexture.Stop(); 
                FileLogger.Log("Camera Stop 호출됨");
            }
        }

        /// <summary>
        /// Indicates whether the active camera is currently playing.
        /// </summary>
        /// <returns><c>true</c>, if the active camera is playing, <c>false</c> otherwise.</returns>
        public override bool IsPlaying()
        {
            return hasInitDone ? nrRGBCamTexture.IsPlaying : false;
        }

        /// <summary>
        /// Indicates whether the active camera device is currently front facng.
        /// </summary>
        /// <returns><c>true</c>, if the active camera device is front facng, <c>false</c> otherwise.</returns>
        public override bool IsFrontFacing()
        {
            return false;
        }

        /// <summary>
        /// Returns the active camera device name.
        /// </summary>
        /// <returns>The active camera device name.</returns>
        public override string GetDeviceName()
        {
            return "RGB_CAMERA";
        }

        /// <summary>
        /// Returns the active camera framerate.
        /// </summary>
        /// <returns>The active camera framerate.</returns>
        public override float GetFPS()
        {
            return -1f;
        }

        /// <summary>
        /// Returns the active WebcamTexture.
        /// </summary>
        /// <returns>The active WebcamTexture.</returns>
        public override WebCamTexture GetWebCamTexture()
        {
            return null;
        }

        /// <summary>
        /// Returns the camera to world matrix.
        /// </summary>
        /// <returns>The camera to world matrix.</returns>
        public override Matrix4x4 GetCameraToWorldMatrix()
        {
            //
            // RGB camera position is used. However, even if this correct value is used in the calculation, the projected AR object will appear slightly offset upward.
            // https://community.xreal.com/t/screen-to-world-point-from-centre-cam/1740/6
            Pose headPose = NRFrame.HeadPose;
            Matrix4x4 HeadPoseM = Matrix4x4.TRS(headPose.position, headPose.rotation, Vector3.one);
            Matrix4x4 localToWorldMatrix = HeadPoseM * rgbCameraPoseFromHeadMatrix;
            //
            // or
            //
            // Center eye position is used. The projected positions obtained with this method are generally consistent with reality, but are slightly off to the left.
            //Pose headPose = NRFrame.HeadPose;
            //Matrix4x4 HeadPoseM = Matrix4x4.TRS(headPose.position, headPose.rotation, Vector3.one);
            //Matrix4x4 localToWorldMatrix = HeadPoseM * centerEyePoseFromHeadMatrix;
            //

            // Transform localToWorldMatrix to cameraToWorldMatrix.
            return localToWorldMatrix * invertZM;
        }

        /// <summary>
        /// Returns the projection matrix matrix.
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public override Matrix4x4 GetProjectionMatrix()
        {
            return projectionMatrix;
        }

        /// <summary>
        /// Indicates whether the video buffer of the frame has been updated.
        /// </summary>
        /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
        public override bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

            return nrRGBCamTexture.DidUpdateThisFrame;
        }

        /// <summary>
        /// Gets the mat of the current frame.
        /// The Mat object's type is 'CV_8UC4' or 'CV_8UC3' or 'CV_8UC1' (ColorFormat is determined by the outputColorFormat setting).
        /// Please do not dispose of the returned mat as it will be reused.
        /// </summary>
        /// <returns>The mat of the current frame.</returns>
        public override Mat GetMat()
        {
            if (!hasInitDone || !nrRGBCamTexture.IsPlaying)
            {
                return (rotatedFrameMat != null) ? rotatedFrameMat : frameMat;
            }

            if (baseColorFormat == outputColorFormat)
            {
                Utils.fastTexture2DToMat(nrRGBCamTexture.GetTexture(), frameMat, false);
            }
            else
            {
                Utils.fastTexture2DToMat(nrRGBCamTexture.GetTexture(), baseMat, false);                
                Imgproc.cvtColor(baseMat, frameMat, Source2MatHelperUtils.ColorConversionCodes(baseColorFormat, outputColorFormat));
            }

            FlipMat(frameMat, flipVertical, flipHorizontal);
            if (rotatedFrameMat != null)
            {
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                return frameMat;
            }
        }

        /// <summary>
        /// Flips the mat.
        /// </summary>
        /// <param name="mat">Mat.</param>
        protected override void FlipMat(Mat mat, bool flipVertical, bool flipHorizontal)
        {
            int flipCode = int.MinValue;

            if (flipVertical)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 0;
                }
                else if (flipCode == 0)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == 1)
                {
                    flipCode = -1;
                }
                else if (flipCode == -1)
                {
                    flipCode = 1;
                }
            }

            if (flipHorizontal)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 1;
                }
                else if (flipCode == 0)
                {
                    flipCode = -1;
                }
                else if (flipCode == 1)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == -1)
                {
                    flipCode = 0;
                }
            }

            if (flipCode > int.MinValue)
            {
                Core.flip(mat, mat, flipCode);
            }
        }

        /// <summary>
        /// Gets the buffer colors.
        /// </summary>
        /// <returns>The buffer colors.</returns>
        public override Color32[] GetBufferColors()
        {
            if (!hasInitDone)
                return null;

            return nrRGBCamTexture.GetTexture().GetPixels32();
        }

        /// <summary>
        /// To release the resources.
        /// </summary>
        protected override void ReleaseResources()
        {
            FileLogger.Log("ReleaseResources 호출됨");
            isInitWaiting = false;
            hasInitDone = false;

            if (nrRGBCamTexture != null)
            {
                nrRGBCamTexture.Stop();
                nrRGBCamTexture = null;
            }
            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (baseMat != null)
            {
                baseMat.Dispose();
                baseMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebCamTextureToMatHelper"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="WebCamTextureToMatHelper"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="WebCamTextureToMatHelper"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="WebCamTextureToMatHelper"/> so
        /// the garbage collector can reclaim the memory that the <see cref="WebCamTextureToMatHelper"/> was occupying.</remarks>
        public override void Dispose()
        {
            FileLogger.Log("=== NRCamTextureToMatHelper Dispose 시작 ===");

            if (colors != null)
            {
                FileLogger.Log("colors 배열 해제");
                colors = null;
            }

            if (isInitWaiting)
            {
                FileLogger.Log("초기화 대기 중 Dispose 호출됨");
                CancelInitCoroutine();
                ReleaseResources();
            }
            else if (hasInitDone)
            {
                FileLogger.Log("초기화 완료 상태에서 Dispose 호출됨");
                ReleaseResources();

                if (onDisposed != null)
                {
                    FileLogger.Log("onDisposed 이벤트 호출");
                    onDisposed.Invoke();
                }
            }
            else
            {
                FileLogger.Log("초기화되지 않은 상태에서 Dispose 호출됨");
            }

            FileLogger.Log("=== NRCamTextureToMatHelper Dispose 종료 ===");
        }

#endif // UNITY_ANDROID && !DISABLE_NRSDK_API

    }
}

#endif
#endif