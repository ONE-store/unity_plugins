using OneStore.Common;
using OneStore.Alc;
using OneStore.Sample.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LicenseUIManager : MonoBehaviour, ILicenseCheckCallback
{
    private OneStoreLogger _logger;
    private OneStoreAppLicenseCheckerImpl _appLicenseChecker;
    private bool _doubleBackToExitPressedOnce = false;

    private string _publicKey = Constants.PublicKey;    // input your license key

    void Awake() {
        _logger = new OneStoreLogger();
        _logger.Log("Awake");
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
        _logger.Log("onDestroy");
        _appLicenseChecker.Destroy();
        _appLicenseChecker = null;
    }

    private void InstantiateAppLicenseChecker() {
        _logger.Log("InstantiateAppLicenseChecker");
        _appLicenseChecker = new OneStoreAppLicenseCheckerImpl(_publicKey);
        _appLicenseChecker.Initialize(this);
    }

    public void OnGranted(string license, string signature) {
        _logger.Log("granted");
        // Utils.ShowAndroidToast("Granted!!");
    }

    public void OnDenied() {
        _logger.Log("denied");
        // Utils.ShowAndroidToast("Denied!!");
    }

    public void OnError(int code, string message) {
        _logger.Log("error => {0} : {1}", code, message);
        // Utils.ShowAndroidToast("[Error] " + message + "(" + code + ")!!");
    }

    public void BackSceneBtn() {
        _logger.Log("back");
        SceneManager.LoadScene(SampleScene.Main);
    }

    public void QueryLicense() {
        _logger.Log("queryLicense");
        _appLicenseChecker.QueryLicense();
    }

    public void StrickLicense() {
        _logger.Log("strickLicense");
        _appLicenseChecker.StrictQueryLicense();
    }
}
