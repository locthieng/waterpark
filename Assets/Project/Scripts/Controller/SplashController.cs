using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
    [SerializeField]
    private string sceneToLoad = "Game";
    [SerializeField] private UIProgress loadingBar;
    [SerializeField] private float loadTime = 6f;
    [SerializeField] private CanvasScaler[] canvasScalers;

    private AsyncOperation loadSceneAsync;
    private bool isDoneLoading;
    private bool isAOAClosed;
    private bool showAOA;
    int appOpen;

    // Use this for initialization
    void Start()
    {
        appOpen = PlayerPrefs.GetInt("AppOpenCount", 0);
        appOpen++;
        PlayerPrefs.SetInt("AppOpenCount", appOpen);
        loadSceneAsync = SceneManager.LoadSceneAsync(sceneToLoad);
        showAOA = appOpen >= 1;// && GoogleAdsController.Instance != null;
        loadSceneAsync.allowSceneActivation = !showAOA;
        GlobalController.StartSceneName = "Splash";
        loadingBar.SetProgress(1, 0, loadTime, OnLoadingDone);
        for (int i = 0; i < canvasScalers.Length; i++)
        {
            canvasScalers[i].matchWidthOrHeight = GlobalController.ScreenRatio < GlobalController.FixedStageResolution.x / GlobalController.FixedStageResolution.y ? 0 : 1;
        }
    }

    private void OnLoadingDone()
    {
        isDoneLoading = true;
        //AnalyticsController.Instance.LogLoadingStart("app_open");
        //Log event
        GlobalController.OnLoadSceneComplete = () =>
        {
            //AnalyticsController.Instance.LogLoadingFinish("app_open", true);
        };
        //AdsController.Instance.ShowBanner(GlobalController.CurrentLevelIndex);
    }

    private void OnAOAClosed()
    {
        loadSceneAsync.allowSceneActivation = true;
    }

    private void Update()
    {
        if (loadSceneAsync != null && isDoneLoading/* && GoogleAdsController.Instance != null && !GoogleAdsController.Instance.isShowingAOAAd*/)
        {
            loadSceneAsync.allowSceneActivation = true;
        }
        //else if (GoogleAdsController.Instance != null && showAOA)
        //{
        //    showAOA = !GlobalController.Instance.ShowAOA(OnAOAClosed);
        //}
    }

}
