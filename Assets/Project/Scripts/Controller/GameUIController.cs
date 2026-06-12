using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public enum StageScreen
{
    None,
    Home,
    Shop,
    InGame,
}

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; set; }
    [SerializeField] private CanvasScaler[] canvasScalers;
    [SerializeField] private Image cover;
    [Header("Shop UI")]
    [Header("Start UI")]
    [SerializeField] private CanvasGroup startUI;
    
    [Header("In Game UI")]
    [SerializeField] public CanvasGroup inGameUI;
    [SerializeField] private TMPro.TextMeshProUGUI levelCurrent;
    [Header("End UI")]
    private bool isUION;

    public static bool IsUIMatchWidth
    {
        get
        {
            return GlobalController.ScreenRatio < GlobalController.FixedStageResolution.x / GlobalController.FixedStageResolution.y;
        }
    }

    private void Start()
    {
        Instance = this;
        isUION = !GlobalController.Instance.ForTesting;
        //btnToggleUI.SetActive(GlobalController.Instance.ForTesting);
        for (int i = 0; i < canvasScalers.Length; i++)
        {
            canvasScalers[i].matchWidthOrHeight = IsUIMatchWidth ? 0 : 1;
        }
        UpdateNoAdsButtons();
    }

    public void SwitchStageUI()
    {
        switch (GlobalController.CurrentStage)
        {
            case StageScreen.None:
                break;
            case StageScreen.Home:
                inGameUI.alpha = 0;
                inGameUI.blocksRaycasts = false;
                startUI.alpha = 1;
                startUI.blocksRaycasts = true;
                break;
            case StageScreen.InGame:
                inGameUI.alpha = 1;
                inGameUI.blocksRaycasts = true;
                startUI.alpha = 0;
                startUI.blocksRaycasts = false;
                break;
            default:
                break;
        }
        LeanTween.alpha(cover.rectTransform, 0, 0.4f).setOnComplete(() =>
        {
            cover.raycastTarget = false;
            if (GlobalController.CurrentStage == StageScreen.Home)
            {
                //uiDailyBonus.CheckAndShow();
            }
        });
    }

    public void ShowInGameUI(int level)
    {
        SwitchStageUI();
        levelCurrent.text = "Level " + level.ToString();
        SetUp();
    }

    public void SetUp()
    {
        
    }

    public void ToggleSound(bool isOn)
    {
        GlobalController.IsSoundOn = !GlobalController.IsSoundOn;
        if (GlobalController.IsSoundOn)
        {
            SoundController.Instance.Unmute();
        }
        else
        {
            SoundController.Instance.Mute();
        }
        PlayerPrefs.SetInt("IsSoundOn", GlobalController.IsSoundOn ? 1 : 0);
    }

    public void ShowLevelBreak(Action callback, float duration = 0.3f, float delay = 0f, bool hideCover = false)
    {
        cover.raycastTarget = true;
        LeanTween.alpha(cover.rectTransform, 1, duration).setOnComplete(() =>
        {
            levelCurrent.text = "";
            if (hideCover)
            {
                LeanTween.alpha(cover.rectTransform, 0, duration).setOnComplete(() =>
                {
                    cover.raycastTarget = false;
                }).setDelay(delay);
            }
            callback?.Invoke();
        });
    }
    public void ShowGameEnd(bool win, int numCoin)
    {
        inGameUI.blocksRaycasts = false;
        LeanTween.alphaCanvas(inGameUI, 0, 0.1f);
        if (win)
        {

        }
        else
        {

        }
    }

    public void SetInteractBoosterUI(bool isActive)
    {

    }

    [SerializeField] private GameObject[] btnNoAds;

    internal void UpdateNoAdsButtons()
    {
        for (int i = 0; i < btnNoAds.Length; i++)
        {
            //btnNoAds[i].SetActive(!DataController.Instance.Stats.NoAds);
        }
    }
}
