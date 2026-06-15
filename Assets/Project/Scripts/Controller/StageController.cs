using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class StageController : MonoBehaviour
{
    public static StageController Instance { get; set; }
    public LevelController Level { get; set; }
    public int CurrentLevel { get; set; }
    public bool IsOver { get; set; }
    private float playTimeInSeconds;

    [SerializeField] private bool noHomeScreen;
    [SerializeField] private bool useSavedLevel;
    [SerializeField] public TMPro.TextMeshProUGUI textCoinWin;
    public int LevelLimit;
    private int mapIndex;
    private int numGoalsDone;
    private int numTotalGoals;
    public static bool IsStart { get; set; }
    private const string PlayerEnteredKey = "PlayerEntered";

    private void Awake()
    {
        if (GlobalController.StartSceneName == SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene("Splash");
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        showingInter = true;
        StartCoroutine("CoStart");
    }

    IEnumerator CoStart()
    {
        bool hasEnteredBefore = PlayerPrefs.GetInt(PlayerEnteredKey, 0) == 1;
        yield return new WaitForSeconds(0);
        IsOver = false;
        if (!noHomeScreen)
        {
            if (hasEnteredBefore)
            {
                //Debug.Log("Player entered the game for the second time");
                if (GlobalController.CurrentStage == StageScreen.InGame)
                {
                    StartLevel();
                }
                else
                {
                    GlobalController.CurrentStage = StageScreen.Home;
                }
                GameUIController.Instance.SwitchStageUI();
            }
            else
            {
                //Debug.Log("Player entered the game for the first time");
                IsStart = true;
                PlayerPrefs.SetInt(PlayerEnteredKey, 1);
                PlayerPrefs.Save();

                GlobalController.CurrentStage = StageScreen.InGame;
                GameUIController.Instance.SwitchStageUI();
                StartLevel();
            }
        }
        else
        {

            GlobalController.CurrentStage = StageScreen.InGame;
            GameUIController.Instance.SwitchStageUI();
            StartLevel();
        }
    }

    private void StartLevel()
    {
        //AdsController.Instance.ShowBanner();
        if (useSavedLevel)
        {
            if (DataController.Instance != null)
            {
                if (GlobalController.ReplayingLevel > 0)
                {
                    GlobalController.CurrentLevelIndex = GlobalController.ReplayingLevel;
                }
            }
            else
            {
                GlobalController.CurrentLevelIndex = 1;
            }
        }
        //mapIndex = LevelController.Instance.LoadLevel(GlobalController.CurrentLevelIndex, LevelLimit);
        StartCoroutine(CoStartLevel());
    }

    IEnumerator CoStartLevel()
    {
        // Setup level
        //LevelController.Instance.SetUpLevel();
        yield return new WaitForSeconds(0.02f);
        GameUIController.Instance.ShowInGameUI(GlobalController.CurrentLevelIndex);
        playTimeInSeconds = Time.realtimeSinceStartup;
        /*if (LevelController.Instance.Level != null)
        {
            LevelController level = LevelController.Instance;
            LevelController.Instance.Level.SetUp();
            numGoalsDone = 0;
            yield return new WaitForSeconds(0.02f);
            yield return new WaitForSeconds(0.02f);
            level.Level.StartLevel();
        }*/
    }

    public void StartGame()
    {
        IsStart = true;
        GlobalController.CurrentStage = StageScreen.InGame;
        GameUIController.Instance.ShowLevelBreak(StartLevel);
    }


    public void PrepareToStartGame()
    {
        if (LifeSystem.Instance.currentLife == 0)
        {
            return;
        }    
        else
        {
            StartGame();
        }
    }    

    public void GetBackHome()
    {
        GlobalController.CurrentStage = StageScreen.Home;
        LifeSystem.Instance.ConsumeLife();
        ReloadScene();
        if (showingInter)
        {
            //AdsController.Instance.ShowInter("get_back_home");
        }
    }

    public void GetBackHomeWhenWin()
    {
        GlobalController.CurrentStage = StageScreen.Home;
        ReloadScene();
        if (showingInter)
        {
            //AdsController.Instance.ShowInter("get_back_home_win");
        }
    }



    public void ButtonTryAgain()
    {
        if (LifeSystem.Instance.currentLife == 0)
        {
            //pop up mua life
            GetBackHome();
        }
        else
        {
            Restart();
        }    
    }

    public void End(bool win)
    {
        if (IsOver) return;
        HapticController.TriggerHaptic(win ? HapticType.Success : HapticType.Failure);
        IsOver = true;
        StartCoroutine(CoEnd(win));
    }

    IEnumerator CoEnd(bool win)
    {
        if (win)
        {
            //yield return new WaitForSeconds(1f);
            if (!DataController.Instance.Data.Levels.Contains(GlobalController.CurrentLevelIndex) || GlobalController.CurrentLevelIndex == 1)
            {
                DataController.Instance.Data.Levels.Add(GlobalController.CurrentLevelIndex);
                //AnalyticsController.Instance.LogLevelComplete(GlobalController.CurrentLevelIndex, (int)(Time.realtimeSinceStartup - playTimeInSeconds), GlobalController.ReplayCount);
                //AppsflyerController.Instance.LogCustomEvent(AFInAppEvents.LEVEL_ACHIEVED, AFInAppEvents.LEVEL, (GlobalController.CurrentLevelIndex).ToString("000"));
            }
            DataController.Instance.Data.LevelIndex = GlobalController.CurrentLevelIndex = LevelController.Instance.GetNextLevelInOrder();
            DataController.Instance.SaveData();
            GameUIController.Instance.SetInteractBoosterUI(true);
            StartCoroutine(CoShowEndGameUI(true));
            GlobalController.ReplayCount = 0;
        }
        else
        {
            yield return new WaitForSeconds(0f);
            //AnalyticsController.Instance.LogLevelFail(GlobalController.CurrentLevelIndex, (int)(Time.realtimeSinceStartup - playTimeInSeconds), GlobalController.ReplayCount);
            StartCoroutine(CoShowEndGameUI(false));
        }
    }

    public void AfterClose(bool win)
    {
        //LevelController.Instance.Level.BlockCanMove(true);
        StartCoroutine(CoEnd(win));
    }

    public void UseCoinToContinue()
    {
        GameUIController.Instance.SetInteractBoosterUI(true);
        float bonusTime = 20f;
        //GameUIController.Instance.BonusTime(bonusTime);
    }
    
    public void UseCoinToRefill()
    {
        LifeSystem.Instance.AddLife(5, () =>
        {
        });

    }

    int earning = 40;
    int earningX;
    public bool showingInter { get; set; }
    public bool isCollectCoin;
    public int BonusEarning;

    public void WatchAdsEarnX()
    {
        if (GlobalController.Instance.ForTesting)
        {
            EarnRewardX();
            return;
        }    
        //AdsController.Instance.ShowRewardedVideo(EarnRewardX, "BonusCoin");
    }

    private void EarnRewardX()
    {
        if (isCollectCoin) return;
        isCollectCoin = true;
        earningX = earning * 2;
        CoinSystem.Instance.AddCoin(earningX);
        UICoin.Instance.ShowFlyingClaim(earningX, () =>
        {
            UICoin.Instance.UpdateCoin(textCoinWin, earningX, 0.4f, AfterCollectCoinWin);
        });
        //AnalyticsController.Instance.LogEarnCurrency("cash", earningX, "x5watchads");
        DataController.Instance.SaveData();
        showingInter = false;
    }

    IEnumerator CoShowEndGameUI(bool win)
    {
        //GameUIController.Instance.SetEndResultLabelActive(win, true);
        if (win)
        {
            yield return new WaitForSeconds(0.5f);
            GameUIController.Instance.ShowGameEnd(win, earning);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            GameUIController.Instance.ShowGameEnd(win, 0);
        }
        CameraController.Instance.EndCam(ShowEndAds);
    }

    private void ShowEndAds()
    {
        //AdsController.Instance.ShowMREC("end_screen");
    }

    public void Next()
    {
        //DataController.Instance.Data.Coin += earning;

        if (isCollectCoin) return;

        isCollectCoin = true;
        CoinSystem.Instance.AddCoin(earning);
        UICoin.Instance.ShowFlyingClaim(earning, () =>
        {
            UICoin.Instance.UpdateCoin(textCoinWin, earning, 0.4f, AfterCollectCoinWin);
        });
        DataController.Instance.SaveData();
        /*if (DataController.Instance.Data.LevelIndex % 4 == 0 && !GlobalController.IsRated)
        {
            GameUIController.Instance.ShowRateUs(NextLevel, NextLevelAfterRate);
        }*/
        //else
        //{
            //NextLevel();
        //}
    }

    private void NextLevelAfterRate()
    {
        //showingInter = false;
        NextLevel();
    }

    private int levelBackToHome = 10;

    private void AfterCollectCoinWin()
    {
        if (GlobalController.CurrentLevelIndex > levelBackToHome)
        {
            GlobalController.ReplayingLevel = 0;
            GetBackHomeWhenWin();
            return;
        }
        NextLevel();
    }

    public void NextLevel()
    {
        GlobalController.ReplayingLevel = 0;
        GameUIController.Instance.ShowLevelBreak(ReloadScene);
        if (showingInter)
        {
            //AdsController.Instance.ShowInter("next_level");
        }
    }

    public void ForceStartLevel(int level)
    {
        IsStart = true;
        GlobalController.CurrentStage = StageScreen.InGame;
        GlobalController.ReplayingLevel = 0;
        GlobalController.CurrentLevelIndex = level;
        GameUIController.Instance.ShowLevelBreak(() =>
        {
            mapIndex = LevelController.Instance.LoadLevel(GlobalController.CurrentLevelIndex, LevelLimit);
            StartCoroutine(CoStartLevel());
        });
    }

    public void Restart()
    {
        GlobalController.ReplayingLevel = mapIndex;
        GlobalController.ReplayCount++;
        LifeSystem.Instance.ConsumeLife();
        GameUIController.Instance.ShowLevelBreak(ReloadScene);
        if (showingInter)
        {
            //AdsController.Instance.ShowInter("restart");
        }
    }
    
    public void RetryRestart()
    {
        if (LifeSystem.Instance.currentLife == 0)
        {
        }   
        else
        {
            Restart();
        }    
    }

    private void ReloadScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
