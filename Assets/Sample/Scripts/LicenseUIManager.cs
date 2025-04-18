using OneStore.Alc;
using OneStore.Sample.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LicenseUIManager : MonoBehaviour, ILicenseCheckCallback
{
    private OneStoreAppLicenseCheckerImpl _appLicenseChecker;
    private bool _doubleBackToExitPressedOnce = false;

    private string _publicKey = Constants.PublicKey;    // input your license key

    void Awake() {
        Debug.Log("Awake");
        InstantiateAppLicenseChecker();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!_doubleBackToExitPressedOnce) {
                _doubleBackToExitPressedOnce = true;
                BackSceneBtn();
            }
        }
    }

    void OnDestroy() {
        Debug.Log("onDestroy");
        _appLicenseChecker.Destroy();
        _appLicenseChecker = null;
    }

    private void InstantiateAppLicenseChecker() {
        Debug.Log("InstantiateAppLicenseChecker");
        _appLicenseChecker = new OneStoreAppLicenseCheckerImpl(_publicKey);
        _appLicenseChecker.Initialize(this);
    }

    public void OnGranted(string license, string signature) {
        Debug.Log("granted");
        // Utils.ShowAndroidToast("Granted!!");
    }

    public void OnDenied() {
        Debug.Log("denied");
        // Utils.ShowAndroidToast("Denied!!");
    }

    public void OnError(int code, string message) {
        Debug.LogErrorFormat("error => {0} : {1}", code, message);
        // Utils.ShowAndroidToast("[Error] " + message + "(" + code + ")!!");
    }

    public void BackSceneBtn() {
        Debug.Log("back");
        SceneManager.LoadScene(SampleScene.Main);
    }

    public void QueryLicense() {
        Debug.Log("queryLicense");
        _appLicenseChecker.QueryLicense();
    }

    public void StrickLicense() {
        Debug.Log("strickLicense");
        _appLicenseChecker.StrictQueryLicense();
    }
}
