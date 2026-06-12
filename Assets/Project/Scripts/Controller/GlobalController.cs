using System;
using UnityEngine;
using UnityEngine.Events;
//using static MaxSdkCallbacks;

#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID
//using Google.Play.Review;
#endif

public class MissionSuccessEvent : UnityEvent<int> { }

public class GlobalController : MonoBehaviour
{
    public static GlobalController Instance { get; set; }
    public static Vector2 ScreenResolution = new Vector2(1080, 1920);
    public static float StageCameraSize = 5;
    public static Vector2 FixedStageResolution = new Vector2(1080, 1920);
    public static float ScreenRatio { get { return Screen.width / (float)Screen.height; } }

    public static bool IsTutDone { get; set; }
    public static bool IsHapticOn { get; set; }
    public static bool IsSoundOn { get; set; }
    public static bool IsMusicOn { get; set; }
    public static bool IsDailyShown { get; set; }
    public static int LoginDay { get; set; }
    public static int RewardDay { get; set; }

    public static bool IsRated { get; set; }
    public static int PlayTimes { get; set; }
    public static Vector2 ScreenSize
    {
        get
        {
            return new Vector2(Screen.width * StageCameraSize * 2 / Screen.height, StageCameraSize * 2);
        }
    }
    public static int TotalLoseTimes;
    public static int TotalDistance;
    public static int CurrentLevelIndex;
    public static int CurrentLevelSpecialIndex;
    public static int CurrentLevelInGame;
    public static string StartSceneName = "Game";
    public static int ReplayingLevel;
    public static Action OnLoadSceneComplete;
    public static int ReplayCount { get; internal set; }
    public static StageScreen CurrentStage { get; internal set; }
    public static LevelType CurrentLevelType;

    public static bool IsBgmOn { get; internal set; }
    public bool ForTesting = false;
    public bool HaveCheatUI = false;
    public static int NumLevelsTillSpecial;
    public static int SpecialLevelGap = 8;
    [HideInInspector] public int bonusTime = 10;

    public static float SoundVolume { get; set; } = 1f;
    public static float BgmVolume { get; set; } = 1f;
    [HideInInspector]
    //public float adDuration;
    public bool IsInfiniteLife { get; set; }
    //public int home_after_win_milestone;

    public static int MaxLife = 5;
    public static int LifeRefillTime = 900; // second(s)

    public static bool IsRu
    {
        get
        {
            return System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName == "RU";
        }
    }

    private void Awake()
    {
        Instance = this;
        // Re-adjust stage camera
        /*LeanTween.init(2000);
        Application.targetFrameRate = 60;
        StartSceneName = "Game";
        CurrentStage = StageScreen.Home;*/
    }

    private void Start()
    {
        Debug.Log("Country Code: " + System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
        if (DataController.Instance.Data.LevelIndex <= 0)
        {
            DataController.Instance.Data.LevelIndex = 1;
        }
        CurrentLevelIndex = DataController.Instance.Data.LevelIndex;

    }

    /*public void PurchaseIAPBundle(BundleID id)
    {
#if UNITY_EDITOR
        IAPController.Instance.onBundlePurchaseSuccess.Invoke(id);
#else
        if (ForTesting)
        {
            IAPController.Instance.onBundlePurchaseSuccess.Invoke(id);
        }
        else
        {
            IAPController.Instance.BuyProductID(id);
        }
#endif
    }*/

    public void BuyProduct(int id, int level, Action<bool> onFinish)
    {
        //IAPController.Instance.BuyProduct(id, level, onFinish);
    }

//    public void PurchaseNoAds(Action<bool, IAPReward> e)
//    {
//#if UNITY_EDITOR || UNITY_STANDALONE
//        e.Invoke(true, );
//#else
//        IAPController.Instance.BuyProduct(7, CurrentLevelIndex, e);
//#endif
//    }

    public void RestorePurchase()
    {
        //IAPController.Instance.RestorePurchases();
        
    }


    public static string GetCurrentStage()
    {

        switch (CurrentStage)
        {
            case StageScreen.Home:
                return "home";
            case StageScreen.Shop:
                return "shop";
            case StageScreen.InGame:
                return "gameplay";
            default:
                return "gameplay";
        }
    }
}
