using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConsumableShopBundle : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI txtPrice;
    [SerializeField] private TMPro.TextMeshProUGUI txtAmount;
    [SerializeField] private string prefix;
    [SerializeField] private string postfix;
    public UIButton buttonScript;
    //public IAPBundle[] BundleData;
    public UnityEvent<ConsumableShopBundle> OnPurchaseSuccess;
    public float price; // Will be overriden by IAP package's price
    public BundleID BundleID;
    public bool IsNonConsumable;
    public Image imageCoin;
    private string location = "home_shop";
    private string showType = "shop";

    [Header("Text amount")]
    [SerializeField] private TMPro.TextMeshProUGUI TextAmount;
    [SerializeField] private TMPro.TextMeshProUGUI[] TextValues;
    [SerializeField] private TMPro.TextMeshProUGUI TextAmountPlus;


    private void OnValidate()
    {
        if (!Application.isPlaying && buttonScript == null)
        {
            buttonScript = GetComponent<UIButton>();
        }
    }

    public void LogIAPShowEvent(string type)
    {
        var level = GlobalController.CurrentLevelIndex;
        //var productID = IAPController.Instance.GetProductId((int)BundleID);
        //AnalyticsController.Instance.LogIAPShow(level, type, productID);
    }

    public void SetParameterForLogEvent(string location, string showType)
    {
        this.location = location;
        this.showType = showType;
    }

    public void SetUp()
    {
        //BundleData = IAPController.Instance.GetRewardData((int)BundleID);

        if (txtAmount != null) // there's only one bundle
        {
            //string amount = BundleData[0].Type == ConsumableType.Coin ? BundleData[0].Value.ToString("#,##0") : BundleData[0].Value.ToString();
            //txtAmount.text = prefix + amount + postfix;
        }
        if (txtPrice != null)
        {
            string priceString = "";
            //priceString = IAPController.Instance.GetProductPrice((int)BundleID);
            if (string.IsNullOrEmpty(priceString))
            {
                priceString = "$" + price;
            }
            priceString = priceString == "" ? "$" + price : priceString;
            txtPrice.text = priceString != null ? priceString : price.ToString();
        }
        SetUpTextBundleData();
    }

    public void SetUpTextBundleData()
    {
        /*if (TextAmount != null) { TextAmount.text = BundleData[0].Value.ToString(); }
        if (TextAmountPlus != null)
        {
            var amount = Array.Find(BundleData, e => e.Type == ConsumableType.Life).Value;
            TextAmountPlus.text = amount < 1 ? (amount * 60f).ToString() + "min" : amount.ToString() + "h"; 
        }
        if (TextValues.Length > 0)
        {
            var amount = Array.Find(BundleData, e => e.Type == ConsumableType.Booster).Value;
            for (int i = 0; i < TextValues.Length; i++)
            {
                TextValues[i].text = amount.ToString() + "x";
            }    
        }*/
    }    

    public void OnTapPurchase()
    {
        if (GlobalController.Instance.ForTesting)
        {
            // For testing only
            OnGetBundle();
        }
        else
        {
            GlobalController.Instance.BuyProduct((int)BundleID, GlobalController.CurrentLevelIndex, HandlePurchaseResult);
        }

        //Log event
        var level = GlobalController.CurrentLevelIndex;
        //var productID = IAPController.Instance.GetProductId((int)BundleID);
        //AnalyticsController.Instance.LogIAPClick(level, productID);
    }

    private void HandlePurchaseResult(bool success)
    {
        if (success)
        {
            OnGetBundle();
        }
        
    }

    // private void OnGetMoreCoin(ConsumableShopBundle arg0)
    // {
    //     SetUp();
    // }

    private void OnGetBundle()
    {
        OnPurchaseSuccess?.Invoke(this);
        //OnPurchaseSuccess = null;
    }
}
