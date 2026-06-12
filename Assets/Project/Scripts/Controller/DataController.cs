using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : MonoBehaviour
{
    public static DataController Instance { get; set; }
    public GameData Data { get; set; }
    public GameStats Stats { get; set; }
    public int RateCancel { get; set; }
    public string LastLogin { get; set; }
    private string dataName = Constants.PackageName + ".gamedata";
    private string statsName = Constants.PackageName + ".gamestats";
    public DateTime FirstOpen;
    public DateTime LastOpen;

    public int RetentionDays { get; private set; }

    private void Awake()
    {
        Instance = this;
        LoadData();
    }

    public void LoadData()
    {
        // Data
        bool resetData = PlayerPrefs.GetInt("Data163920210421", 1) == 1;
        PlayerPrefs.SetInt("Data163920210421", 0);
        string data = resetData ? "" : PlayerPrefs.GetString(dataName, "");

        if (string.IsNullOrEmpty(data))
        {
            Data = new GameData()
            {
                BestScore = 0,
                LevelIndex = 0,
                Levels = new List<int>() { 1 },
                replayCount = 0,
            };
        }
        else
        {
            Data = JsonUtility.FromJson<GameData>(data);
        }
        if (Data.Levels.Count == 0)
        {
            Data.Levels = new List<int>();
            for (int i = 0; i < Data.LevelIndex - 1; i++)
            {
                Data.Levels.Add(i + 1);
            }
        }

        // Stats
        string stats = PlayerPrefs.GetString(statsName, "");

        if (string.IsNullOrEmpty(stats))
        {
            Stats = new GameStats();
        }
        else
        {
            Stats = JsonUtility.FromJson<GameStats>(stats);
        }

        // ===== HANDLE FIRST & LAST OPEN TIME =====

        if (string.IsNullOrEmpty(Stats.FirstOpenUtc))
        {
            Stats.FirstOpenUtc = DateTime.UtcNow.Ticks.ToString();
        }
        if (string.IsNullOrEmpty(Stats.LastOpenUtc))
        {
            Stats.LastOpenUtc = DateTime.UtcNow.Ticks.ToString();
        }
        FirstOpen = new DateTime(long.Parse(Stats.FirstOpenUtc), DateTimeKind.Utc);
        LastOpen = new DateTime(long.Parse(Stats.LastOpenUtc), DateTimeKind.Utc);

        RetentionDays = (int)(DateTime.UtcNow - LastOpen).TotalDays;


        Stats.LastOpenUtc = DateTime.UtcNow.Ticks.ToString();

        SaveStats();


        // Game settings
        RateCancel = PlayerPrefs.GetInt("CancelRateUs", 0);
        GlobalController.IsRated = PlayerPrefs.GetInt("RateUs", 0) == 1;
        GlobalController.IsTutDone = PlayerPrefs.GetInt("TutDone", 0) == 1;
        GlobalController.IsHapticOn = PlayerPrefs.GetInt("IsHapticOn", 1) == 1;
        GlobalController.IsSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        GlobalController.IsBgmOn = PlayerPrefs.GetInt("IsBgmOn", 0) == 1;
        GlobalController.LoginDay = PlayerPrefs.GetInt("LoginDay", 0);
        GlobalController.RewardDay = PlayerPrefs.GetInt("RewardDay", -1);

        GlobalController.SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        GlobalController.BgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1f);
        LastLogin = PlayerPrefs.GetString("LastLogin", "");
        if (!string.IsNullOrEmpty(LastLogin))
        {
            if ((DateTime.Today - DateTime.Parse(LastLogin)).Days > 0)
            {
                GlobalController.IsDailyShown = false;
                PlayerPrefs.SetInt("LoginDay", GlobalController.LoginDay + 1);
                PlayerPrefs.SetString("LastLogin", DateTime.Today.ToString());
            }
            else
            {
                GlobalController.IsDailyShown = true;
            }
        }
        else
        {
            PlayerPrefs.SetString("LastLogin", DateTime.Today.ToString());
        }

        // test
        //Data.Coin = 10000;
    }
    private int maxWatchAdsToRefillHeartCount = 5;
    public bool CanWatchAdsToday()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");

        // Nếu ngày lưu trong máy khác ngày hiện tại -> Reset số lượt
        if (Data.LastAdsViewDate != today)
        {
            Data.DailyAdsViewCount = 0;
            Data.LastAdsViewDate = today;
            SaveData();
        }

        return Data.DailyAdsViewCount < maxWatchAdsToRefillHeartCount; 
    }

    public void SaveData()
    {
        PlayerPrefs.SetString(dataName, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    public void SaveStats()
    {
        PlayerPrefs.SetString(statsName, JsonUtility.ToJson(Stats));
        PlayerPrefs.Save();
    }
}
