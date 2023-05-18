using System;
using OneStore.Purchasing;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    private static DialogManager _instance = null;
    public static DialogManager Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(DialogManager)) as DialogManager;
                if (_instance == null)
                    Debug.Log("There's no active ManagerClass object");
                
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    [SerializeField] public GameObject _canvas;
    [SerializeField] public GameObject _consumable;
    [SerializeField] public GameObject _subscription;
    [SerializeField] public GameObject _btnConfirm;
    [SerializeField] public GameObject _number;

    [SerializeField] private int _prorationMode = 1;
    [SerializeField] private int _quantity = 1;

    Animator _fadeAnim;
    Animator _scaleAnim;
    private bool _isAnimationPlaying = false;

    void Awake()
    {
        Instance = this;
        var bg = _canvas.gameObject.transform.GetChild(0).gameObject;
        _fadeAnim = bg.GetComponent<Animator>();
        _scaleAnim = bg.transform.GetChild(0).GetComponent<Animator>();
        _canvas.SetActive(false);
    }

    void Update()
    {
        if (_isAnimationPlaying)
        {
            if (_scaleAnim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.ScaleOut") &&
                _scaleAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                _isAnimationPlaying = false;
                _canvas.SetActive(false);
            }
        }
    }

    public void Show()
    {
        _isAnimationPlaying = true;
        _canvas.SetActive(true);
    }

    public void Dismiss()
    {
        _fadeAnim.SetTrigger("Dismiss");
        _scaleAnim.SetTrigger("Dismiss");
    }

    public bool IsShowing()
    {
        return _canvas.activeSelf;
    }
    

    #region Purchase popup

    public void ShowPurchase(ProductItem product, Action<string, int> confirm)
    {
        _consumable.SetActive(true);
        _subscription.SetActive(false);
        _btnConfirm.SetActive(true);

        _quantity = 1;

        _consumable.transform.Find("Icon").GetChild(0).GetComponent<Image>().sprite = product.icon;
        _consumable.transform.Find("Title").GetComponent<Text>().text = product.title;
        _consumable.transform.Find("Price").GetComponent<Text>().text = product.price;

        _number.SetActive(ProductType.INAPP == ProductType.Get(product.productType));
        _number.GetComponent<InputField>().text = _quantity.ToString();
        _number.GetComponent<InputField>().onEndEdit.AddListener((value) => _quantity = int.Parse(value));

        _btnConfirm.GetComponent<Button>().onClick.RemoveAllListeners();
        _btnConfirm.GetComponent<Button>().onClick.AddListener(() => {
            confirm?.Invoke(product.productId, _quantity);
            Dismiss();
        });
        Show();
    }

    public void OnClickIncrease()
    {
        _quantity++;
        if (_quantity > 10) _quantity = 10;
        _number.GetComponent<InputField>().text = _quantity.ToString();
    }

    public void OnClickDecrease()
    {
        _quantity--;
        if (_quantity < 1) _quantity = 1;
        _number.GetComponent<InputField>().text = _quantity.ToString();
    }

    #endregion

    #region UpdateSubscription popup

    public void ShowUpdateSubscription(ProductItem product, Action<string, OneStoreProrationMode> confirm)
    {
        _subscription.SetActive(true);
        _consumable.SetActive(false);
        _btnConfirm.SetActive(true);

        _subscription.transform.Find("Icon").GetChild(0).GetComponent<Image>().sprite = product.icon;
        _subscription.transform.Find("Title").GetComponent<Text>().text = product.title;
        _subscription.transform.Find("Price").GetComponent<Text>().text = product.price;

        _btnConfirm.GetComponent<Button>().onClick.RemoveAllListeners();
        _btnConfirm.GetComponent<Button>().onClick.AddListener(() => {
            confirm?.Invoke(product.productId, (OneStoreProrationMode) _prorationMode);
            Dismiss();
        });
        Show();
    }

    public void SetCheck(int prorationMode)
    {
        _prorationMode = prorationMode;
    }

    #endregion
}
