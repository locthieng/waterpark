//using FunGlobalSDK.Public;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BundleID
{
    NO_ADS = 7,
    NO_ADS_BUNDLE = 8,
    FAIL_OFFER = 15,
    SMALL_BUNDLE = 9,
    BIG_BUNDLE = 10,
    GREAT_BUNDLE = 11,
    ULTRA_BUNDLE = 12,
    LEGENDARY_BUNDLE = 13,
    SPECIAL_PACK = 14,
    COIN_1 = 1,
    COIN_2 = 2,
    COIN_3 = 3,
    COIN_4 = 4,
    COIN_5 = 5,
    COIN_6 = 6
}

public enum ConsumableType
{
    Coin,
    Booster,
    NoAds,
    Life,
    Full,
    None
}

public enum ConsumableCostType
{
    Coin,
    Ads,
    Cash
}

[Serializable]
public struct ConsumableBundle
{
    public ConsumableType Type;
    public ConsumableCostType CostType;
    //[HideIfEnumValue("Type", HideIf.NotEqual, (int)ConsumableType.Coin) ]
    public int Amount;
    //[HideIfEnumValue("Type", HideIf.NotEqual, (int)ConsumableType.Booster)]
    public int Value;
    //[HideIfEnumValue("Type", HideIf.NotEqual, (int)ConsumableType.Life)]
    public int AmountPlus;
    //[HideIfEnumValue("Type", HideIf.NotEqual, (int)ConsumableType.Life)]
    public bool IsInfinityLife;
}

public class ConsumableShopUI : MonoBehaviour
{
    [SerializeField] private ConsumableShopBundle[] bundles;
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject btnSetting;
    private bool isBtnSettingActive;
    private Dictionary<BundleID, ConsumableShopBundle> dictBundles = new Dictionary<BundleID, ConsumableShopBundle>();
    public Action OnBundlePurchaseSuccess;

    private void Start()
    {
        SetUp();
    }

    private void SetUp()
    {
        //scrollRect.content.localPosition = new Vector2(0, -scrollRect.content.rect.size.y / 2f);
        for (int i = 0; i < bundles.Length; i++)
        {
            bundles[i].SetUp();
            dictBundles[bundles[i].BundleID] = bundles[i];
            bundles[i].OnPurchaseSuccess.RemoveAllListeners();
            bundles[i].OnPurchaseSuccess.AddListener(OnPurchaseSuccess);
            //bundles[i].buttonScript.draggableRoot = scrollRect;
        }
        //IAPController.Instance.onBundlePurchaseSuccess = OnPurchaseSuccess;
        //dictBundles[BundleID.STARTER].transform.parent.gameObject.SetActive(!DataController.Instance.Stats.StarterActivated);
        
        if(dictBundles.TryGetValue(BundleID.NO_ADS, out ConsumableShopBundle item))
        {
            item.transform.parent.gameObject.SetActive(!DataController.Instance.Stats.NoAds);
        }
    }

    public void LogIAPShowEvent()
    {
        for (int i = 0; i < bundles.Length; i++)
        {
            bundles[i].LogIAPShowEvent("shop");
            
        }
    }

    public void ShowUp()
    {
        gameObject.SetActive(true);
        canvas.LeanAlpha(1, 0.2f).setOnComplete(() =>
        {
            canvas.blocksRaycasts = true;
        });
        //scrollRect.verticalNormalizedPosition = 1;
        isBtnSettingActive = btnSetting.activeInHierarchy;
        btnSetting.SetActive(false);
        //GameUIController.Instance.SetButtonCoinPlusActive(false);
        //dictBundles[BundleID.STARTER].gameObject.SetActive(!DataController.Instance.Stats.StarterActivated);
    }

    public void Hide()
    {
        canvas.blocksRaycasts = false;
        canvas.LeanAlpha(0, 0.2f).setOnComplete(() =>
        {
            gameObject.SetActive(false);
        });
        btnSetting.SetActive(isBtnSettingActive);
        //GameUIController.Instance.SetButtonCoinPlusActive(true);
    }

    private void OnPurchaseSuccess(ConsumableShopBundle item)
    {
        if (GlobalController.Instance.ForTesting)
        {
            // Logic
            ClaimReward(item);
        }
        // UI
        OnGet(item);

    }

    private void ClaimReward(ConsumableShopBundle item)
    {
        /*var packDatas = item.BundleData;
        
        for (int i = 0; i < packDatas.Length; i++)
        {
            var data = packDatas[i];
            var value = (int)data.Value;
            switch (data.Type)
            {
                case ConsumableType.Coin:
                    CoinSystem.Instance.AddCoin(value);
                    
                    break;
                case ConsumableType.Booster:
                    BoosterDataController.Instance.Data.AddAllBoosters(value);
                    break;
                case ConsumableType.NoAds:
                    //FGSDK.Instance.ConfirmRemoveAds();
                    DataController.Instance.Stats.NoAds = true;
                    DataController.Instance.SaveStats();
                    break;
                case ConsumableType.Life:
                    int infiniteDurationSeconds = value * 3600;
                    LifeSystem.Instance.ActivateInfiniteLife(infiniteDurationSeconds);
                    break;
                
                default:
                    break;
            }
        }*/
        DataController.Instance.SaveData();
    }

    private void OnGet(ConsumableShopBundle item)
    {
        //var packDatas = item.BundleData;
        //for (int i = 0; i < packDatas.Length; i++)
        //{
        //    var data = packDatas[i];
        //    switch (data.Type)
        //    {
        //        case ConsumableType.Coin:
        //            int amount = (int)data.Value;
        //            //CoinSystem.Instance.AddCoin(amount);
        //            // Log event tracking
        //            //AnalyticsController.Instance.LogResourceSource(GlobalController.CurrentLevelIndex, "currency", "coin", amount, "purchase", CoinSystem.Instance.coin);
        //            ChangeTransformItemBundle(item);
        //            UICoin.Instance.ShowFlyingClaim(amount, () =>
        //            {
        //                //UICoin.Instance.UpdateCoin(GameUIController.Instance.UICoinPlus.coinText, amount, 0.4f, null);
        //                //GameUIController.Instance.ShowCoin();
        //                UICoin.Instance.UpdateCoin(null, amount, 0.4f, null);
        //            });
        //            /*GameUIController.Instance.ShowFlyingCoinsFrom(GameUIController.Instance.transform, amount, () =>
        //            {
        //                int previous = DataController.Instance.Data.Coin - amount;
        //                int newValue = DataController.Instance.Data.Coin;

        //                GameUIController.Instance.UpdateCoin(null, previous, newValue);
        //            });*/
        //            //value = amount;
        //            //GameUIController.Instance.HideConsumableShop();
        //            break;
        //        case ConsumableType.Booster:
        //            ChangeTransformItemBundle(item);
        //            int coinAmount = (int)data.Value;
        //            UICoin.Instance.ShowFlyingBundlesFrom(() =>
        //            {
        //                BoosterController.Instance.SetUp();
        //               // GameUIController.Instance.ShowCoin();
        //                //GameUIController.Instance.uiLife.SetUp();
        //                UICoin.Instance.UpdateCoin(null, coinAmount, 0.4f, null);
        //            });
        //            //AnalyticsController.Instance.LogCustomEvent("get_booster_" + item.BundleData[i].BoosterType.ToString().ToLower() + "_" + item.BundleData[i].CostType.ToString().ToLower() + "_" + item.BundleData[i].Amount, "booster_type", item.BundleData[i].BoosterType.ToString().ToLower());
        //            break;
        //        case ConsumableType.NoAds:
        //            //GlobalController.Instance.RemoveAds();
        //            //GameUIController.Instance.HideRemoveAds();
        //            //AnalyticsController.Instance.LogCustomEvent("remove_ads", "", "");
        //            break;
        //        case ConsumableType.Life:
        //            //GameUIController.Instance.uiLife.SetUp();
        //            //if (item.BundleData[i].IsInfinityLife)
        //            //{
        //            //    //PageUI.Instance.OnPurchaseLifeBundle(item);
        //            //}
        //            //else
        //            //{
        //            //    //DataController.Instance.Data.Life += item.BundleData[i].AmountPlus;
        //            //    //PageUI.Instance.RefreshLifeInfo();
        //            //}
        //            break;
        //        case ConsumableType.Full:
        //            //int coinAmount = data.Amount;
        //            //int valueAmount = data.Value;
        //            //CoinSystem.Instance.AddCoin(coinAmount);
        //            //BoosterDataController.Instance.Data.AddAllBoosters(valueAmount);

        //            //int infiniteDurationSeconds = item.BundleData[i].AmountPlus * 3600;
        //            //LifeSystem.Instance.ActivateInfiniteLife(infiniteDurationSeconds);
        //            //GameUIController.Instance.uiLife.SetUp();
        //            //UICoin.Instance.ShowFlyingBundlesFrom(() =>
        //            //{
        //            //    UICoin.Instance.UpdateCoin(GameUIController.Instance.UICoinPlus.coinText, coinAmount, 0.4f, null);
        //            //});
        //            break;
        //        default:
        //            break;
        //    }
        //}
        ////DataController.Instance.Data.IAPCount++;
        //DataController.Instance.SaveData();
        ////GlobalSDKController.Instance.LogIAPRevenueSDK(IAPController.Instance.ProductInfo[IAPController.ItemIDs[(int)item.BundleID]].metadata.isoCurrencyCode, IAPController.ItemIDs[(int)item.BundleID], (float)IAPController.Instance.ProductInfo[IAPController.ItemIDs[(int)item.BundleID]].metadata.localizedPrice);
        ////GlobalSDKController.Instance.UpdateUserProperty();
        ////GameUIController.Instance.ShowUIGetItem(storeBooster, value);
        ////UIScrewJamController.Instance.RefreshUIBoosterItem();
        //// if (item.BundleID == BundleID.STARTER)
        //// {
        ////     //DataController.Instance.Stats.StarterActivated = true;
        ////     DataController.Instance.SaveStats();
        ////     //GameUIController.Instance.HideUIStarter();
        ////     //UIBannerPopup.Instance.OnStarterActive();
        //// }
        //// if (item.BundleID == BundleID.HEART_1 || item.BundleID == BundleID.HEART_24 || item.BundleID == BundleID.HEART_BOOSTER_2)
        //// {
        ////     //GameUIController.Instance.HideUILifePopup();
        //// }
        //if (item.IsNonConsumable)
        //{
        //    GameObject itemScale;
        //    if (item.BundleID == BundleID.NO_ADS || item.BundleID == BundleID.NO_ADS_BUNDLE)
        //    {
        //        itemScale = item.transform.parent.gameObject;
        //    }
        //    else
        //    {
        //        itemScale = item.gameObject;
        //        item.buttonScript.enabled = false;
        //    }
        //    scalingItem = itemScale.transform;
        //    scalingFactor = 1;
        //    LeanTween.value(itemScale, itemScale.transform.localScale.y, 0, 0.3f).setOnUpdate((float f) =>
        //    {
        //        scalingFactor = f;
        //    }).setOnComplete(() =>
        //    {
        //        scalingItem = null;
        //        itemScale.SetActive(false);
        //    });
        //}
    }

    private void ChangeTransformItemBundle(ConsumableShopBundle item)
    {
        if (GameUIController.Instance != null && UICoin.Instance != null)
        {
            Camera camUI = CameraController.Instance.UICamera;
            Camera camHighUI = CameraController.Instance.HighUICamera;

            if (camUI != null && camHighUI != null)
            {
                Vector2 screenPos = camUI.WorldToScreenPoint(item.transform.position);

                RectTransform parentRect = UICoin.Instance.flyingCoinsContainer.parent as RectTransform;
                RectTransform containerRect = UICoin.Instance.flyingCoinsContainer as RectTransform;

                if (parentRect != null && containerRect != null)
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect,
                        screenPos,
                        camHighUI,
                        out localPoint
                    );

                    containerRect.anchoredPosition = localPoint;

                    Vector3 localPos3D = containerRect.anchoredPosition3D;
                    localPos3D.z = 0f;
                    containerRect.anchoredPosition3D = localPos3D;
                }
            }
        }
    }    

    private Transform scalingItem;
    private float scalingFactor;

    private void OnGUI()
    {
        if (scalingItem != null)
        {
            scalingItem.localScale = Vector3.one * scalingFactor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }
    }
}
