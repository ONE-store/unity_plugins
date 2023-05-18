
#if UNITY_ANDROID || !UNITY_EDITOR

using System;
using OneStore.Core;
using OneStore.Auth.Internal;
using UnityEngine;

namespace OneStore.Auth
{
    public class OneStoreAuthClientImpl
    {
        private AndroidJavaObject _signInClient;
        private readonly OneStoreLogger _logger;
        private readonly AuthHelper _authHelper;
        public OneStoreAuthClientImpl()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                throw new PlatformNotSupportedException("Operation is not supported on this platform.");
            }

            _logger = new OneStoreLogger();
            _authHelper = new AuthHelper(_logger);
        }

        public void SilentSignIn(Action<SignInResult> callback)
        {
            SignInInternal(true, callback);
        }

        public void LaunchSignInFlow(Action<SignInResult> callback)
        {
            SignInInternal(false, callback);
        }

        private void SignInInternal(bool isSilent, Action<SignInResult> callback)
        {
            var context = JniHelper.GetApplicationContext();
            _signInClient = new AndroidJavaObject(Constants.SignInClient, Constants.Version);
            _signInClient.Call(Constants.SignInClientSetupMethod, context);

            var authListener = new OnAuthListener();
            authListener.OnAuthResponse += (javaSignInResult) => {
                var signInResult = _authHelper.ParseJavaSignInpResult(javaSignInResult);
                var responseCode = _authHelper.GetResponseCodeFromSignInResult(signInResult);
                if (responseCode != ResponseCode.RESULT_OK)
                {
                    _logger.Error("Failed to signIn with error code {0} and message: {1}", signInResult.Code, signInResult.Message);
                }

                RunOnMainThread(() => callback?.Invoke(signInResult));
            };

            if (isSilent)
            {
                _signInClient.Call(
                    Constants.SignInClientSilentSignInMethod,
                    authListener
                );
            }
            else
            {
                _signInClient.Call(
                    Constants.SignInClientLaunchSignInFlowMethod,
                    JniHelper.GetUnityAndroidActivity(),
                    authListener
                );
            }
        }

        // public void LaunchUpdateOrInstallFlow()
        // {
        //     var resultListener = new ResultListener();
        //     resultListener.OnResponse += (code, message) =>
        //     {
        //         if (code == 0)
        //         {
        //             LaunchSignInFlow();
        //         }
        //         else
        //         {
        //             var signInResult = new SignInResult(code, message);
        //             if (!HandleErrorCode(signInResult))
        //             {
        //                 RunOnMainThread(() => _callback.OnFailed(signInResult));
        //             }
        //         }
        //     };

        //     _signInClient.Call(
        //         Constants.SignInClientLaunchUpdateOrInstallMethod,
        //         JniHelper.GetUnityAndroidActivity(),
        //         resultListener
        //     );
        // }

        private void RunOnMainThread(Action action)
        {
            OneStoreDispatcher.RunOnMainThread(() => action());
        }
    }
}

#endif
