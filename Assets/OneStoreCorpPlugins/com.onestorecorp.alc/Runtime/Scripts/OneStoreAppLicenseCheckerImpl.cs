#if UNITY_ANDROID || !UNITY_EDITOR

using System.ComponentModel;
using Microsoft.VisualBasic;
using System;
using System.Net.Mime;
using OneStore.Core;
using OneStore.Alc.Internal;
using UnityEngine;

namespace OneStore.Alc
{
    public class OneStoreAppLicenseCheckerImpl
    {
        private AndroidJavaObject _appLicenseChecker;
        private readonly OneStoreLogger _logger;

        private readonly string _licenseKey;

        private LicenseCheckerListener _listener;

        private ILicenseCheckCallback _callback;

        public OneStoreAppLicenseCheckerImpl(string licenseKey) {
            if (Application.platform != RuntimePlatform.Android) {
                throw new PlatformNotSupportedException("Operation is not supported on this platform.");
            }

            _logger = new OneStoreLogger();
            _licenseKey = licenseKey;
        }

        // ALC 연결 초기화
        public void Initialize(ILicenseCheckCallback callback) {
            _callback = callback;
            _appLicenseChecker = new AndroidJavaObject(Constants.AppLicenseChecker, _licenseKey);

            var context = JniHelper.GetUnityAndroidActivity();
            _listener = new LicenseCheckerListener();
            _listener.Granted += OnGranted;
            _listener.Denied += OnDenied;
            _listener.Error += OnError;

            _appLicenseChecker.Call(
                Constants.AppLicenseCheckerSetupMethod,
                context,
                _listener
            );
        }

        // Cached API 호출
        // 캐시된 라이센스를 이용할 경우 사용한다.
        public void QueryLicense() {
            _logger.Log("do queryLicense");
            _appLicenseChecker.Call(Constants.AppLicenseCheckerQueryLicenseMethod);
        }

        // Non-Cached API 호출
        // 캐시된 라이센스를 이용하지 않고 사용할 경우 사용한다.
        public void StrictQueryLicense() {
            _logger.Log("do strictQueryLicense");
            _appLicenseChecker.Call(Constants.AppLicenseCheckerStrickQueryLicenseMethod);
        }

        // ALC 연결 해제
        public void Destroy() {
            _logger.Log("do destroy");
            _appLicenseChecker.Call(Constants.AppLicenseCheckerDestroy);
        }
        private void OnGranted(string license, string signature) {
            _callback.OnGranted(license, signature);
        }

        private void OnDenied() {
            _callback.OnDenied();
        }

        private void OnError(int code, string message) {
            _callback.OnError(code, message);
        }
    }
    
}
#endif
